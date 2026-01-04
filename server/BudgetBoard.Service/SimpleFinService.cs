using System.Globalization;
using System.Security.Cryptography;
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

        userData.SimpleFinAccessToken = accessToken;

        userDataContext.Update(userData);
        await userDataContext.SaveChangesAsync();

        await RefreshAccountsAsync(userData.Id);
    }

    /// <inheritdoc />
    public async Task<IList<string>> RefreshAccountsAsync(Guid userGuid)
    {
        var errors = new List<string>();
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var simpleFinData = await GetSimpleFinAccountsDataAsync(
            userData.SimpleFinAccessToken,
            null,
            false
        );
        if (simpleFinData == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["SimpleFinDataNotFoundLog"]);
            return [responseLocalizer["SimpleFinDataNotFoundError"]];
        }

        errors.AddRange(simpleFinData.Errors);

        if (simpleFinData.Accounts.Any())
        {
            errors.AddRange(await RefreshSimpleFinAccountsAsync(userData, simpleFinData.Accounts));
        }

        return errors;
    }

    /// <inheritdoc />
    public async Task<IList<string>> SyncTransactionHistoryAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        logger.LogInformation("{LogMessage}", logLocalizer["SimpleFinTokenConfiguredLog"]);

        // Deleted accounts do not get updated during sync.
        long earliestBalanceTimestamp = GetOldestLastSyncTimestamp(
            userData.SimpleFinAccounts.Where(a =>
                a.LinkedAccountId != null
                && userData.Accounts.SingleOrDefault(ua => ua.ID == a.LinkedAccountId)?.Deleted
                    == null
            )
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
            var simpleFinData = await GetSimpleFinAccountsDataAsync(
                userData.SimpleFinAccessToken,
                syncStartDate
            );
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

    /// <inheritdoc />
    public async Task RemoveAccessTokenAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        userData.SimpleFinAccessToken = string.Empty;

        userDataContext.Update(userData);
        await userDataContext.SaveChangesAsync();

        await RemoveSimpleFinDataAsync(userGuid);
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

    private async Task<HttpResponseMessage> DecodeAccessToken(string setupToken)
    {
        // SimpleFin tokens are Base64-encoded URLs on which a POST request will
        // return the access URL for getting bank data.

        string decodedString;
        try
        {
            byte[] data = Convert.FromBase64String(setupToken);
            decodedString = Encoding.UTF8.GetString(data);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{LogMessage}", logLocalizer["SimpleFinDecodeTokenInvalidLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["SimpleFinDecodeTokenInvalidError"]
            );
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, decodedString);
            var client = clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{LogMessage}",
                logLocalizer["SimpleFinDecodeTokenRequestErrorLog", ex.Message]
            );
            throw new BudgetBoardServiceException(
                responseLocalizer["SimpleFinDecodeTokenRequestError"]
            );
        }
    }

    private async Task<bool> IsAccessTokenValid(string accessToken) =>
        (await QuerySimpleFinAccountDataAsync(accessToken, null, true)).IsSuccessStatusCode;

    private static SimpleFinData GetUrlCredentials(string accessToken)
    {
        string[] url = accessToken.Split("//");
        string[] data = url.Last().Split("@");
        var auth = data.First();
        var baseUrl = url.First() + "//" + data.Last();

        return new SimpleFinData(auth, baseUrl);
    }

    private long GetOldestLastSyncTimestamp(IEnumerable<SimpleFinAccount> simpleFinAccounts)
    {
        long oldestLastSyncTimestamp = ((DateTimeOffset)nowProvider.UtcNow).ToUnixTimeSeconds();

        foreach (var account in simpleFinAccounts)
        {
            if (!account.LastSync.HasValue)
            {
                return DateTimeOffset.UnixEpoch.ToUnixTimeSeconds();
            }

            if (
                ((DateTimeOffset)account.LastSync.Value).ToUnixTimeSeconds()
                < oldestLastSyncTimestamp
            )
            {
                oldestLastSyncTimestamp = (
                    (DateTimeOffset)account.LastSync.Value
                ).ToUnixTimeSeconds();
            }
        }

        return oldestLastSyncTimestamp;
    }

    private long GetSyncStartDate(int forceSyncLookbackMonths, long earliestBalanceTimestamp)
    {
        var nowUnix = ((DateTimeOffset)nowProvider.UtcNow).ToUnixTimeSeconds();

        if (forceSyncLookbackMonths > 0)
        {
            return nowUnix - (UNIX_MONTH * forceSyncLookbackMonths);
        }

        // SimpleFIN can lookback a maximum of 365 days (not inclusive).
        if (earliestBalanceTimestamp < nowUnix - MAX_SYNC_LOOKBACK_UNIX)
        {
            return nowUnix - MAX_SYNC_LOOKBACK_UNIX;
        }

        var oneMonthAgo = nowUnix - UNIX_MONTH;
        var lastSyncWithBuffer = earliestBalanceTimestamp - UNIX_WEEK;

        // Start date is the earlier of one month ago or last sync minus one week.
        return Math.Min(oneMonthAgo, lastSyncWithBuffer);
    }

    private async Task<ISimpleFinAccountsData?> GetSimpleFinAccountsDataAsync(
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
            return JsonSerializer.Deserialize<SimpleFinAccountsData>(jsonString, s_readOptions)
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
        if (!includeTransactions)
        {
            urlArgs += "balances-only=1";
        }
        var requestUrl = data.BaseUrl + "/accounts" + urlArgs;

        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

        var client = clientFactory.CreateClient();
        var byteArray = Encoding.ASCII.GetBytes(data.Auth);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(byteArray)
            );

        return await client.SendAsync(request);
    }

    private async Task<List<string>> RefreshSimpleFinAccountsAsync(
        ApplicationUser userData,
        IEnumerable<ISimpleFinAccountData> simpleFinAccounts
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
        IEnumerable<ISimpleFinAccountData> accountsData
    )
    {
        List<string> errors = [];
        foreach (var accountData in accountsData)
        {
            var simpleFinAccount = userData.SimpleFinAccounts.FirstOrDefault(a =>
                a.SyncID == accountData.Id
            );
            if (simpleFinAccount == null)
            {
                logger.LogError(
                    "{LogMessage}",
                    logLocalizer["SimpleFinAccountNotFoundForSyncLog", accountData.Name]
                );
                errors.Add(
                    responseLocalizer["SimpleFinAccountNotFoundForSyncError", accountData.Name]
                );

                continue;
            }

            var linkedAccount = userData.Accounts.FirstOrDefault(a =>
                a.ID == simpleFinAccount.LinkedAccountId
            );
            if (linkedAccount == null)
            {
                logger.LogInformation(
                    "{LogMessage}",
                    logLocalizer["SimpleFinAccountNotLinkedLog", accountData.Name]
                );
                continue;
            }

            // Deleted accounts do not get updated during sync.
            if (linkedAccount.Deleted.HasValue)
            {
                logger.LogInformation(
                    "{LogMessage}",
                    logLocalizer["SimpleFinAccountDeletedSkipLog", linkedAccount.Name]
                );
                continue;
            }

            var transactionErrors = await SyncTransactionsAsync(
                userData,
                simpleFinAccount.ID,
                accountData.Transactions
            );
            errors.AddRange(transactionErrors);

            var balanceSyncErrors = await SyncBalancesAsync(
                userData,
                simpleFinAccount.ID,
                accountData
            );
            errors.AddRange(balanceSyncErrors);

            if (transactionErrors.Count == 0 && balanceSyncErrors.Count == 0)
            {
                simpleFinAccount.LastSync = nowProvider.UtcNow;
            }
        }

        return errors;
    }

    private async Task<List<string>> SyncTransactionsAsync(
        ApplicationUser userData,
        Guid simpleFinAccountId,
        IEnumerable<ISimpleFinTransactionData> transactionsData
    )
    {
        List<string> errors = [];
        if (!transactionsData.Any())
            return errors;

        var userAccount = userData.Accounts.FirstOrDefault(a =>
            a.SimpleFinAccount != null && a.SimpleFinAccount.ID == simpleFinAccountId
        );
        if (userAccount == null)
        {
            logger.LogError(
                "{LogMessage}",
                logLocalizer["SimpleFinAccountNotFoundForTransactionLog", simpleFinAccountId]
            );
            errors.Add(
                responseLocalizer["SimpleFinAccountNotFoundForTransactionError", simpleFinAccountId]
            );
            return errors;
        }

        List<Transaction> userTransactions =
        [
            .. userAccount.Transactions.OrderByDescending(t => t.Date),
        ];
        foreach (var transactionData in transactionsData)
        {
            if (
                userTransactions.Any(t =>
                    (t.SyncID ?? string.Empty).Equals(
                        transactionData.Id,
                        StringComparison.InvariantCulture
                    )
                )
            )
            {
                // Transaction already exists.
                continue;
            }

            var newTransaction = new TransactionCreateRequest
            {
                SyncID = transactionData.Id,
                Amount = decimal.Parse(transactionData.Amount),
                Date = transactionData.Pending
                    ? DateTime.UnixEpoch.AddSeconds(transactionData.TransactedAt)
                    : DateTime.UnixEpoch.AddSeconds(transactionData.Posted),
                MerchantName = transactionData.Description,
                Source = TransactionSource.SimpleFin.Value,
                AccountID = userAccount.ID,
            };

            await transactionService.CreateTransactionAsync(userData.Id, newTransaction);
        }

        return errors;
    }

    private async Task<List<string>> SyncBalancesAsync(
        ApplicationUser userData,
        Guid simpleFinAccountId,
        ISimpleFinAccountData accountData
    )
    {
        List<string> errors = [];
        var userAccount = userData.Accounts.FirstOrDefault(a =>
            a.SimpleFinAccount != null && a.SimpleFinAccount.ID == simpleFinAccountId
        );
        if (userAccount == null)
        {
            logger.LogError(
                "{LogMessage}",
                logLocalizer["SimpleFinAccountNotFoundForBalanceLog", simpleFinAccountId]
            );
            errors.Add(
                responseLocalizer["SimpleFinAccountNotFoundForBalanceError", simpleFinAccountId]
            );
            return errors;
        }

        // We only want to create a balance if it is newer than the latest balance we have.
        var latestBalance = userAccount
            .Balances.OrderByDescending(b => b.DateTime)
            .FirstOrDefault();
        long latestBalanceTimestamp =
            latestBalance != null
                ? ((DateTimeOffset)latestBalance.DateTime).ToUnixTimeSeconds()
                : 0;
        if (accountData.BalanceDate > latestBalanceTimestamp)
        {
            var newBalance = new BalanceCreateRequest
            {
                Amount = decimal.Parse(
                    accountData.Balance,
                    CultureInfo.InvariantCulture.NumberFormat
                ),
                DateTime = DateTime.UnixEpoch.AddSeconds(accountData.BalanceDate),
                AccountID = userAccount.ID,
            };

            await balanceService.CreateBalancesAsync(userData.Id, newBalance);
        }

        return errors;
    }

    private async Task<List<string>> RemoveSimpleFinDataAsync(Guid userGuid)
    {
        List<string> errors = [];
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        foreach (var simpleFinAccount in userData.SimpleFinAccounts.ToList())
        {
            var linkedAccount = userData.Accounts.SingleOrDefault(a =>
                a.ID == simpleFinAccount.LinkedAccountId
            );
            if (linkedAccount != null)
            {
                await accountService.UpdateAccountSourceAsync(
                    userData.Id,
                    linkedAccount.ID,
                    AccountSource.Manual
                );
            }

            await simpleFinAccountService.DeleteAccountAsync(userData.Id, simpleFinAccount.ID);
        }

        foreach (var simpleFinOrganization in userData.SimpleFinOrganizations.ToList())
        {
            await simpleFinOrganizationService.DeleteSimpleFinOrganizationAsync(
                userData.Id,
                simpleFinOrganization.ID
            );
        }

        return errors;
    }
}
