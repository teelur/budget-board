using System.Globalization;
using System.Text;
using System.Text.Json;
using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class SimpleFinService(
    IHttpClientFactory clientFactory,
    UserDataContext userDataContext,
    ILogger<ISyncProvider> logger,
    INowProvider nowProvider,
    IAccountService accountService,
    IInstitutionService institutionService,
    ITransactionService transactionService,
    IBalanceService balanceService,
    ISimpleFinOrganizationService simpleFinOrganizationService,
    ISimpleFinAccountService simpleFinAccountService,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : ISimpleFinService
{
    private const int MAX_SYNC_LOOKBACK_UNIX = 31449600; // 364 days
    private const long UNIX_MONTH = 2629743;
    private const long UNIX_WEEK = 604800;

    private static readonly JsonSerializerOptions s_readOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <inheritdoc />
    public async Task ConfigureAccessTokenAsync(Guid userGuid, string setupToken)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var decodeAccessTokenResponse = await DecodeAccessToken(setupToken);
        if (!decodeAccessTokenResponse.IsSuccessStatusCode)
        {
            logger.LogError("{LogMessage}", logLocalizer["SimpleFinDecodeTokenErrorLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["SimpleFinDecodeTokenError"]);
        }

        var accessToken = await decodeAccessTokenResponse.Content.ReadAsStringAsync();

        if (!await IsAccessTokenValid(accessToken))
        {
            logger.LogError("{LogMessage}", logLocalizer["SimpleFinInvalidAccessTokenLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["SimpleFinInvalidAccessTokenError"]
            );
        }

        userData.AccessToken = accessToken;
        userDataContext.Update(userData);
        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IList<string>> UpdateDataAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var simpleFinData = await GetAccountDataAsync(userData.AccessToken, null, false);
        if (simpleFinData == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["SimpleFinDataNotFoundLog"]);
            return [responseLocalizer["SimpleFinDataNotFoundError"]];
        }

        List<string> errors = [.. simpleFinData.Errors];

        if (simpleFinData.Accounts.Any())
        {
            errors.AddRange(await SyncSimpleFinDataAsync(userData, simpleFinData.Accounts));
        }

        return errors;
    }

    /// <inheritdoc />
    public async Task<IList<string>> SyncDataAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        logger.LogInformation("{LogMessage}", logLocalizer["SimpleFinTokenConfiguredLog"]);

        // Deleted accounts do not get updated during sync.
        long earliestBalanceTimestamp = GetOldestLastSyncTimestamp(
            userData.Accounts.Where(a => !a.Deleted.HasValue)
        );

        long syncStartDate = GetSyncStartDate(
            userData.UserSettings?.ForceSyncLookbackMonths ?? 0,
            earliestBalanceTimestamp
        );

        logger.LogInformation(
            "{LogMessage}",
            logLocalizer[
                "SimpleFinSyncingTransactionsLog",
                DateTimeOffset.FromUnixTimeSeconds(syncStartDate).UtcDateTime,
                syncStartDate
            ]
        );

        try
        {
            var simpleFinData = await GetAccountDataAsync(userData.AccessToken, syncStartDate);
            if (simpleFinData == null)
            {
                logger.LogError("{LogMessage}", logLocalizer["SimpleFinDataNotFoundLog"]);
                return [responseLocalizer["SimpleFinDataNotFoundError"]];
            }

            List<string> errors = [.. simpleFinData.Errors];

            errors.AddRange(await SyncAccountsAsync(userData, simpleFinData.Accounts));

            userDataContext.Update(userData);
            await userDataContext.SaveChangesAsync();

            return errors;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{LogMessage}",
                logLocalizer["SimpleFinDataRetrievalErrorLog", ex.Message]
            );
            return [responseLocalizer["SimpleFinDataRetrievalError"]];
        }
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await userDataContext
                .ApplicationUsers.Include(u => u.Accounts)
                .ThenInclude(a => a.Transactions)
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Balances)
                .Include(u => u.Institutions)
                .Include(u => u.SimpleFinOrganizations)
                .ThenInclude(o => o.Accounts)
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{LogMessage}",
                logLocalizer["UserDataRetrievalErrorLog", ex.Message]
            );
            throw new BudgetBoardServiceException(responseLocalizer["UserDataRetrievalError"]);
        }

        if (foundUser == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["InvalidUserErrorLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["InvalidUserError"]);
        }

        return foundUser;
    }

    private static SimpleFinData GetUrlCredentials(string accessToken)
    {
        string[] url = accessToken.Split("//");
        string[] data = url.Last().Split("@");
        var auth = data.First();
        var baseUrl = url.First() + "//" + data.Last();

        return new SimpleFinData(auth, baseUrl);
    }

    private long GetOldestLastSyncTimestamp(IEnumerable<Account> accounts)
    {
        long oldestLastSyncTimestamp = ((DateTimeOffset)nowProvider.UtcNow).ToUnixTimeSeconds();

        foreach (var account in accounts)
        {
            var balanceTimestamps = account.Balances.Select(b => b.DateTime);
            if (!balanceTimestamps.Any())
            {
                // If an account has no balances, we need to sync everything.
                return DateTimeOffset.UnixEpoch.ToUnixTimeSeconds();
            }
            else
            {
                var accountMostRecentBalanceTimestamp = balanceTimestamps.Max();
                if (
                    ((DateTimeOffset)accountMostRecentBalanceTimestamp).ToUnixTimeSeconds()
                    < oldestLastSyncTimestamp
                )
                {
                    oldestLastSyncTimestamp = (
                        (DateTimeOffset)accountMostRecentBalanceTimestamp
                    ).ToUnixTimeSeconds();
                }
            }
        }

        return oldestLastSyncTimestamp;
    }

    private long GetSyncStartDate(int forceSyncLookbackMonths, long earliestBalanceTimestamp)
    {
        if (forceSyncLookbackMonths > 0)
        {
            return ((DateTimeOffset)nowProvider.UtcNow).ToUnixTimeSeconds()
                - (UNIX_MONTH * forceSyncLookbackMonths);
        }

        // SimpleFIN can lookback a maximum of 365 days (not inclusive).
        if (
            earliestBalanceTimestamp
            < ((DateTimeOffset)nowProvider.UtcNow).ToUnixTimeSeconds() - MAX_SYNC_LOOKBACK_UNIX
        )
        {
            return ((DateTimeOffset)nowProvider.UtcNow).ToUnixTimeSeconds()
                - MAX_SYNC_LOOKBACK_UNIX;
        }

        var oneMonthAgo = ((DateTimeOffset)nowProvider.UtcNow).ToUnixTimeSeconds() - UNIX_MONTH;
        var lastSyncWithBuffer = earliestBalanceTimestamp - UNIX_WEEK;

        // Start date is the earlier of one month ago or last sync minus one week.
        return Math.Min(oneMonthAgo, lastSyncWithBuffer);
    }

    private async Task<ISimpleFinAccountData?> GetAccountDataAsync(
        string accessToken,
        long? startDate,
        bool includeTransactions = true
    )
    {
        var response = await QuerySimpleFinAccountDataAsync(
            accessToken,
            startDate,
            includeTransactions
        );
        var jsonString = await response.Content.ReadAsStringAsync();

        try
        {
            return JsonSerializer.Deserialize<SimpleFinAccountData>(jsonString, s_readOptions)
                ?? null;
        }
        catch (JsonException jex)
        {
            logger.LogError(
                jex,
                "{LogMessage}",
                logLocalizer["SimpleFinDeserializationErrorLog", jex.Message]
            );
            return null;
        }
    }

    private async Task<HttpResponseMessage> QuerySimpleFinAccountDataAsync(
        string accessToken,
        long? startDate,
        bool includeTransactions
    )
    {
        SimpleFinData data = GetUrlCredentials(accessToken);

        var urlArgs = "?";
        if (startDate.HasValue)
        {
            urlArgs += "start-date=" + startDate.Value.ToString() + "&";
        }
        if (includeTransactions)
        {
            urlArgs += "balances-only=1";
        }

        var request = new HttpRequestMessage(HttpMethod.Get, data.BaseUrl + "/accounts" + urlArgs);

        var client = clientFactory.CreateClient();
        var byteArray = Encoding.ASCII.GetBytes(data.Auth);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(byteArray)
            );

        return await client.SendAsync(request);
    }

    private async Task<List<string>> SyncSimpleFinDataAsync(
        ApplicationUser userData,
        IEnumerable<ISimpleFinAccount> simpleFinAccounts
    )
    {
        List<string> errors = [];

        foreach (var simpleFinAccount in simpleFinAccounts)
        {
            var existingOrganization = userData.SimpleFinOrganizations.SingleOrDefault(o =>
                o.Domain == simpleFinAccount.Org.Domain
            );
            if (existingOrganization == null)
            {
                var simpleFinOrganization = simpleFinAccount.Org;
                await simpleFinOrganizationService.CreateSimpleFinOrganizationAsync(
                    userData.Id,
                    new SimpleFinOrganizationCreateRequest
                    {
                        Domain = simpleFinOrganization.Domain,
                        SimpleFinUrl = simpleFinOrganization.SimpleFinUrl,
                        Name = simpleFinOrganization.Name,
                        Url = simpleFinOrganization.Url,
                        SyncID = simpleFinOrganization.SyncID,
                    }
                );

                existingOrganization = userData.SimpleFinOrganizations.SingleOrDefault(o =>
                    o.Domain == simpleFinAccount.Org.Domain
                );
                if (existingOrganization == null)
                {
                    logger.LogError(
                        "{LogMessage}",
                        logLocalizer[
                            "SyncSimpleFinOrganizationCreationErrorLog",
                            simpleFinAccount.Name
                        ]
                    );
                    errors.Add(
                        responseLocalizer[
                            "SyncSimpleFinOrganizationCreationError",
                            simpleFinAccount.Name
                        ]
                    );
                    continue;
                }
            }

            var existingAccount = userData.SimpleFinAccounts.SingleOrDefault(a =>
                a.SyncID == simpleFinAccount.Id
                && a.Organization?.Domain == simpleFinAccount.Org.Domain
            );
            if (existingAccount == null)
            {
                await simpleFinAccountService.CreateSimpleFinAccountAsync(
                    userData.Id,
                    new SimpleFinAccountCreateRequest
                    {
                        SyncID = simpleFinAccount.Id,
                        Name = simpleFinAccount.Name,
                        Currency = simpleFinAccount.Currency,
                        Balance = decimal.Parse(
                            simpleFinAccount.Balance,
                            CultureInfo.InvariantCulture.NumberFormat
                        ),
                        BalanceDate = simpleFinAccount.BalanceDate,
                        OrganizationId = existingOrganization.ID,
                    }
                );
            }
            else
            {
                await simpleFinAccountService.UpdateAccountAsync(
                    userData.Id,
                    new SimpleFinAccountUpdateRequest
                    {
                        ID = existingAccount.ID,
                        SyncID = simpleFinAccount.Id,
                        Name = simpleFinAccount.Name,
                        Currency = simpleFinAccount.Currency,
                        Balance = decimal.Parse(
                            simpleFinAccount.Balance,
                            CultureInfo.InvariantCulture.NumberFormat
                        ),
                        BalanceDate = DateTimeOffset
                            .FromUnixTimeSeconds(simpleFinAccount.BalanceDate)
                            .UtcDateTime,
                    }
                );
            }
        }

        return errors;
    }

    private async Task<List<string>> SyncAccountsAsync(
        ApplicationUser userData,
        IEnumerable<ISimpleFinAccount> accountsData
    )
    {
        List<string> errors = [];
        // foreach (var accountData in accountsData)
        // {
        //     var existingAccount = userData.Accounts.SingleOrDefault(a =>
        //         a.SyncID == accountData.Id
        //     );
        //     if (existingAccount == null)
        //     {
        //         var institutionName = accountData.Org?.Name ?? accountData.Org?.Domain;

        //         if (string.IsNullOrEmpty(institutionName))
        //         {
        //             _logger.LogError(
        //                 "{LogMessage}",
        //                 _logLocalizer["SimpleFinOrganizationMissingIdLog", accountData.Name]
        //             );
        //             errors.Add(
        //                 _logLocalizer["SimpleFinOrganizationMissingIdError", accountData.Name]
        //             );

        //             continue;
        //         }

        //         var institutionId = await GetOrCreateInstitutionIdAsync(userData, accountData);
        //         if (institutionId == Guid.Empty)
        //         {
        //             _logger.LogError(
        //                 "{LogMessage}",
        //                 _logLocalizer["SyncInstitutionCreationErrorLog", accountData.Name]
        //             );
        //             errors.Add(
        //                 _responseLocalizer["SyncInstitutionCreationError", accountData.Name]
        //             );

        //             continue;
        //         }

        //         var newAccount = new AccountCreateRequest
        //         {
        //             SyncID = accountData.Id,
        //             Name = accountData.Name,
        //             InstitutionID = institutionId,
        //             Source = AccountSource.SimpleFIN,
        //         };

        //         await _accountService.CreateAccountAsync(userData.Id, newAccount);
        //     }
        //     else
        //     {
        //         // Deleted accounts do not get updated during sync.
        //         if (existingAccount.Deleted.HasValue)
        //         {
        //             _logger.LogInformation(
        //                 "{LogMessage}",
        //                 _logLocalizer["SimpleFinAccountDeletedSkipLog", existingAccount.Name]
        //             );
        //             continue;
        //         }
        //     }

        //     errors.AddRange(
        //         await SyncTransactionsAsync(userData, accountData.Id, accountData.Transactions)
        //     );
        //     errors.AddRange(await SyncBalancesAsync(userData, accountData.Id, accountData));
        // }

        return errors;
    }

    private async Task<Guid> GetOrCreateInstitutionIdAsync(
        ApplicationUser userData,
        ISimpleFinAccount accountData
    )
    {
        var institutionName = accountData.Org?.Name ?? accountData.Org?.Domain;
        if (string.IsNullOrEmpty(institutionName))
        {
            logger.LogError(
                "{LogMessage}",
                logLocalizer["SimpleFinOrganizationMissingIdLog", accountData.Name]
            );
            return Guid.Empty;
        }

        var existingInstitution = userData.Institutions.FirstOrDefault(i =>
            i.Name == institutionName
        );
        if (existingInstitution != null)
        {
            return existingInstitution.ID;
        }

        var newInstitution = new InstitutionCreateRequest { Name = institutionName };

        await institutionService.CreateInstitutionAsync(userData.Id, newInstitution);

        var createdInstitution = userData.Institutions.FirstOrDefault(i =>
            i.Name == institutionName
        );
        return createdInstitution != null ? createdInstitution.ID : Guid.Empty;
    }

    private async Task<List<string>> SyncTransactionsAsync(
        ApplicationUser userData,
        string syncId,
        IEnumerable<ISimpleFinTransaction> transactionsData
    )
    {
        List<string> errors = [];
        // if (!transactionsData.Any())
        //     return errors;

        // var userAccount = userData.Accounts.FirstOrDefault(a =>
        //     (a.SyncID ?? string.Empty).Equals(syncId)
        // );
        // if (userAccount == null)
        // {
        //     _logger.LogError(
        //         "{LogMessage}",
        //         _logLocalizer["SimpleFinAccountNotFoundForTransactionLog", syncId]
        //     );
        //     errors.Add(_responseLocalizer["SimpleFinAccountNotFoundForTransactionError", syncId]);
        //     return errors;
        // }

        // List<Transaction> userTransactions =
        // [
        //     .. userAccount.Transactions.OrderByDescending(t => t.Date),
        // ];
        // foreach (var transactionData in transactionsData)
        // {
        //     if (
        //         userTransactions.Any(t =>
        //             (t.SyncID ?? string.Empty).Equals(
        //                 transactionData.Id,
        //                 StringComparison.InvariantCulture
        //             )
        //         )
        //     )
        //     {
        //         // Transaction already exists.
        //         continue;
        //     }

        //     var newTransaction = new TransactionCreateRequest
        //     {
        //         SyncID = transactionData.Id,
        //         Amount = decimal.Parse(transactionData.Amount),
        //         Date = transactionData.Pending
        //             ? DateTime.UnixEpoch.AddSeconds(transactionData.TransactedAt)
        //             : DateTime.UnixEpoch.AddSeconds(transactionData.Posted),
        //         MerchantName = transactionData.Description,
        //         Source = TransactionSource.SimpleFin.Value,
        //         AccountID = userAccount.ID,
        //     };

        //     await _transactionService.CreateTransactionAsync(userData.Id, newTransaction);
        // }

        return errors;
    }

    private async Task<List<string>> SyncBalancesAsync(
        ApplicationUser userData,
        string syncId,
        ISimpleFinAccount accountData
    )
    {
        List<string> errors = [];
        // var foundAccount = userData.Accounts.SingleOrDefault(a => a.SyncID == syncId);
        // if (foundAccount == null)
        // {
        //     _logger.LogError(
        //         "{LogMessage}",
        //         _logLocalizer["SimpleFinAccountNotFoundForBalanceLog", syncId]
        //     );
        //     errors.Add(_responseLocalizer["SimpleFinAccountNotFoundForBalanceError", syncId]);
        //     return errors;
        // }

        // // We only want to create a balance if it is newer than the latest balance we have.
        // var latestBalance = foundAccount
        //     .Balances.OrderByDescending(b => b.DateTime)
        //     .FirstOrDefault();
        // long latestBalanceTimestamp =
        //     latestBalance != null
        //         ? ((DateTimeOffset)latestBalance.DateTime).ToUnixTimeSeconds()
        //         : 0;
        // if (accountData.BalanceDate > latestBalanceTimestamp)
        // {
        //     var newBalance = new BalanceCreateRequest
        //     {
        //         Amount = decimal.Parse(
        //             accountData.Balance,
        //             CultureInfo.InvariantCulture.NumberFormat
        //         ),
        //         DateTime = DateTime.UnixEpoch.AddSeconds(accountData.BalanceDate),
        //         AccountID = foundAccount.ID,
        //     };

        //     await _balanceService.CreateBalancesAsync(userData.Id, newBalance);
        // }

        return errors;
    }

    private async Task<HttpResponseMessage> DecodeAccessToken(string setupToken)
    {
        // SimpleFin tokens are Base64-encoded URLs on which a POST request will
        // return the access URL for getting bank data.

        byte[] data = Convert.FromBase64String(setupToken);
        string decodedString = Encoding.UTF8.GetString(data);

        using var request = new HttpRequestMessage(HttpMethod.Post, decodedString);
        var client = clientFactory.CreateClient();
        var response = await client.SendAsync(request);

        return response;
    }

    private async Task<bool> IsAccessTokenValid(string accessToken) =>
        (await QuerySimpleFinAccountDataAsync(accessToken, null, true)).IsSuccessStatusCode;
}
