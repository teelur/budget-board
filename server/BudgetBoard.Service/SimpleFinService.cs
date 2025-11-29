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
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : ISyncProvider
{
    private const int MAX_SYNC_LOOKBACK_UNIX = 31449600; // 364 days
    private const long UNIX_MONTH = 2629743;
    private const long UNIX_WEEK = 604800;

    private static readonly JsonSerializerOptions s_readOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly IHttpClientFactory _clientFactory = clientFactory;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly ILogger<ISyncProvider> _logger = logger;
    private readonly INowProvider _nowProvider = nowProvider;
    private readonly IAccountService _accountService = accountService;
    private readonly IInstitutionService _institutionService = institutionService;
    private readonly ITransactionService _transactionService = transactionService;
    private readonly IBalanceService _balanceService = balanceService;
    private readonly IStringLocalizer<ResponseStrings> _responseLocalizer = responseLocalizer;
    private readonly IStringLocalizer<LogStrings> _logLocalizer = logLocalizer;

    /// <inheritdoc />
    public async Task<IList<string>> SyncDataAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        _logger.LogInformation("{LogMessage}", _logLocalizer["SimpleFinTokenConfiguredLog"]);

        // Deleted accounts do not get updated during sync.
        long earliestBalanceTimestamp = GetEarliestBalanceTimestamp(
            userData.Accounts.Where(a => !string.IsNullOrEmpty(a.SyncID) && !a.Deleted.HasValue)
        );

        long syncStartDate = GetSyncStartDate(
            userData.UserSettings?.ForceSyncLookbackMonths ?? 0,
            earliestBalanceTimestamp
        );

        _logger.LogInformation(
            "{LogMessage}",
            _logLocalizer[
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
                _logger.LogError("{LogMessage}", _logLocalizer["SimpleFinDataNotFoundLog"]);
                return [_responseLocalizer["SimpleFinDataNotFoundError"]];
            }

            List<string> errors = [.. simpleFinData.Errors];

            errors.AddRange(await SyncAccountsAsync(userData, simpleFinData.Accounts));

            _userDataContext.Update(userData);
            await _userDataContext.SaveChangesAsync();

            return errors;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "{LogMessage}",
                _logLocalizer["SimpleFinDataRetrievalErrorLog", ex.Message]
            );
            return [_responseLocalizer["SimpleFinDataRetrievalError"]];
        }
    }

    /// <inheritdoc />
    public async Task ConfigureAccessTokenAsync(Guid userGuid, string token)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var decodeAccessTokenResponse = await DecodeAccessToken(token);

        if (!decodeAccessTokenResponse.IsSuccessStatusCode)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["SimpleFinDecodeTokenErrorLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["SimpleFinDecodeTokenError"]);
        }

        var accessToken = await decodeAccessTokenResponse.Content.ReadAsStringAsync();

        if (!await IsAccessTokenValid(accessToken))
        {
            _logger.LogError("{LogMessage}", _logLocalizer["SimpleFinInvalidAccessTokenLog"]);
            throw new BudgetBoardServiceException(
                _responseLocalizer["SimpleFinInvalidAccessTokenError"]
            );
        }

        userData.AccessToken = accessToken;
        _userDataContext.Update(userData);
        await _userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await _userDataContext
                .ApplicationUsers.Include(u => u.Accounts)
                .ThenInclude(a => a.Transactions)
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Balances)
                .Include(u => u.Institutions)
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "{LogMessage}",
                _logLocalizer["UserDataRetrievalErrorLog", ex.Message]
            );
            throw new BudgetBoardServiceException(_responseLocalizer["UserDataRetrievalError"]);
        }

        if (foundUser == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["InvalidUserErrorLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["InvalidUserError"]);
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

    private long GetEarliestBalanceTimestamp(IEnumerable<Account> accounts)
    {
        long earliestBalanceTimestamp = ((DateTimeOffset)_nowProvider.UtcNow).ToUnixTimeSeconds();

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
                var firstBalanceTimestamp = balanceTimestamps.Min();
                if (
                    ((DateTimeOffset)firstBalanceTimestamp).ToUnixTimeSeconds()
                    < earliestBalanceTimestamp
                )
                {
                    earliestBalanceTimestamp = (
                        (DateTimeOffset)firstBalanceTimestamp
                    ).ToUnixTimeSeconds();
                }
            }
        }

        return earliestBalanceTimestamp;
    }

    private long GetSyncStartDate(int forceSyncLookbackMonths, long earliestBalanceTimestamp)
    {
        if (forceSyncLookbackMonths > 0)
        {
            return ((DateTimeOffset)_nowProvider.UtcNow).ToUnixTimeSeconds()
                - (UNIX_MONTH * forceSyncLookbackMonths);
        }

        // SimpleFIN can lookback a maximum of 365 days (not inclusive).
        if (
            earliestBalanceTimestamp
            > ((DateTimeOffset)_nowProvider.UtcNow).ToUnixTimeSeconds() - MAX_SYNC_LOOKBACK_UNIX
        )
        {
            return ((DateTimeOffset)_nowProvider.UtcNow).ToUnixTimeSeconds()
                - MAX_SYNC_LOOKBACK_UNIX;
        }

        var oneMonthAgo = ((DateTimeOffset)_nowProvider.UtcNow).ToUnixTimeSeconds() - UNIX_MONTH;
        var lastSyncWithBuffer = earliestBalanceTimestamp - UNIX_WEEK;

        // Start date is the earlier of one month ago or last sync minus one week.
        return Math.Min(oneMonthAgo, lastSyncWithBuffer);
    }

    private async Task<ISimpleFinAccountData?> GetAccountDataAsync(
        string accessToken,
        long startDate
    )
    {
        var response = await QuerySimpleFinAccountDataAsync(accessToken, startDate);
        var jsonString = await response.Content.ReadAsStringAsync();

        try
        {
            return JsonSerializer.Deserialize<SimpleFinAccountData>(jsonString, s_readOptions)
                ?? null;
        }
        catch (JsonException jex)
        {
            _logger.LogError(
                jex,
                "{LogMessage}",
                _logLocalizer["SimpleFinDeserializationErrorLog", jex.Message]
            );
            return null;
        }
    }

    private async Task<HttpResponseMessage> QuerySimpleFinAccountDataAsync(
        string accessToken,
        long? startDate
    )
    {
        SimpleFinData data = GetUrlCredentials(accessToken);

        var startArg = startDate.HasValue
            ? "?start-date=" + startDate.Value.ToString()
            : string.Empty;

        var request = new HttpRequestMessage(HttpMethod.Get, data.BaseUrl + "/accounts" + startArg);

        var client = _clientFactory.CreateClient();
        var byteArray = Encoding.ASCII.GetBytes(data.Auth);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(byteArray)
            );

        return await client.SendAsync(request);
    }

    private async Task<List<string>> SyncAccountsAsync(
        ApplicationUser userData,
        IEnumerable<ISimpleFinAccount> accountsData
    )
    {
        List<string> errors = [];
        foreach (var accountData in accountsData)
        {
            var existingAccount = userData.Accounts.SingleOrDefault(a =>
                a.SyncID == accountData.Id
            );
            if (existingAccount == null)
            {
                var institutionName = accountData.Org?.Name ?? accountData.Org?.Domain;

                if (string.IsNullOrEmpty(institutionName))
                {
                    _logger.LogError(
                        "{LogMessage}",
                        _logLocalizer["SimpleFinOrganizationMissingIdLog", accountData.Name]
                    );
                    errors.Add(
                        _logLocalizer["SimpleFinOrganizationMissingIdError", accountData.Name]
                    );

                    continue;
                }

                var institutionId = await GetOrCreateInstitutionIdAsync(userData, accountData);
                if (institutionId == Guid.Empty)
                {
                    _logger.LogError(
                        "{LogMessage}",
                        _logLocalizer["SyncInstitutionCreationErrorLog", accountData.Name]
                    );
                    errors.Add(
                        _responseLocalizer["SyncInstitutionCreationError", accountData.Name]
                    );

                    continue;
                }

                var newAccount = new AccountCreateRequest
                {
                    SyncID = accountData.Id,
                    Name = accountData.Name,
                    InstitutionID = institutionId,
                    Source = AccountSource.SimpleFIN,
                };

                await _accountService.CreateAccountAsync(userData.Id, newAccount);
            }
            else
            {
                // Deleted accounts do not get updated during sync.
                if (existingAccount.Deleted.HasValue)
                {
                    _logger.LogInformation(
                        "{LogMessage}",
                        _logLocalizer["SimpleFinAccountDeletedSkipLog", existingAccount.Name]
                    );
                    continue;
                }
            }

            errors.AddRange(
                await SyncTransactionsAsync(userData, accountData.Id, accountData.Transactions)
            );
            errors.AddRange(await SyncBalancesAsync(userData, accountData.Id, accountData));
        }

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
            _logger.LogError(
                "{LogMessage}",
                _logLocalizer["SimpleFinOrganizationMissingIdLog", accountData.Name]
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

        await _institutionService.CreateInstitutionAsync(userData.Id, newInstitution);

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
        if (!transactionsData.Any())
            return errors;

        var userAccount = userData.Accounts.FirstOrDefault(a =>
            (a.SyncID ?? string.Empty).Equals(syncId)
        );
        if (userAccount == null)
        {
            _logger.LogError(
                "{LogMessage}",
                _logLocalizer["SimpleFinAccountNotFoundForTransactionLog", syncId]
            );
            errors.Add(_responseLocalizer["SimpleFinAccountNotFoundForTransactionError", syncId]);
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

            await _transactionService.CreateTransactionAsync(userData.Id, newTransaction);
        }

        return errors;
    }

    private async Task<List<string>> SyncBalancesAsync(
        ApplicationUser userData,
        string syncId,
        ISimpleFinAccount accountData
    )
    {
        List<string> errors = [];
        var foundAccount = userData.Accounts.SingleOrDefault(a => a.SyncID == syncId);
        if (foundAccount == null)
        {
            _logger.LogError(
                "{LogMessage}",
                _logLocalizer["SimpleFinAccountNotFoundForBalanceLog", syncId]
            );
            errors.Add(_responseLocalizer["SimpleFinAccountNotFoundForBalanceError", syncId]);
            return errors;
        }

        // We only want to create a balance if it is newer than the latest balance we have.
        var latestBalance = foundAccount
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
                AccountID = foundAccount.ID,
            };

            await _balanceService.CreateBalancesAsync(userData.Id, newBalance);
        }

        return errors;
    }

    private async Task<HttpResponseMessage> DecodeAccessToken(string setupToken)
    {
        // SimpleFin tokens are Base64-encoded URLs on which a POST request will
        // return the access URL for getting bank data.

        byte[] data = Convert.FromBase64String(setupToken);
        string decodedString = Encoding.UTF8.GetString(data);

        using var request = new HttpRequestMessage(HttpMethod.Post, decodedString);
        var client = _clientFactory.CreateClient();
        var response = await client.SendAsync(request);

        return response;
    }

    private async Task<bool> IsAccessTokenValid(string accessToken) =>
        (await QuerySimpleFinAccountDataAsync(accessToken, null)).IsSuccessStatusCode;
}
