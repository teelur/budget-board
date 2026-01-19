using System.Text.Json;
using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service.Interfaces;

/// <inheritdoc />
public class LunchFlowService(
    IHttpClientFactory clientFactory,
    UserDataContext userDataContext,
    ILogger<ILunchFlowService> logger,
    INowProvider nowProvider,
    IAccountService accountService,
    ITransactionService transactionService,
    IBalanceService balanceService,
    ILunchFlowAccountService lunchFlowAccountService,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : ILunchFlowService
{
    const string LunchFlowBaseUrl = "https://api.lunchflow.com/v1";
    const string LunchFlowAccountsEndpoint = "/accounts";
    const string LunchFlowTransactionsEndpoint = "/transactions";
    const string LunchFlowBalancesEndpoint = "/balance";

    private static readonly JsonSerializerOptions s_readOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <inheritdoc />
    public async Task ConfigureApiKeyAsync(Guid userGuid, string apiKey)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var requestUrl = LunchFlowBaseUrl + LunchFlowAccountsEndpoint;
        var response = await SendQuery(requestUrl, apiKey);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError(
                "{LogMessage}",
                logLocalizer["LunchFlowApiKeyConfigurationErrorLog", response.StatusCode]
            );
            throw new BudgetBoardServiceException(
                responseLocalizer["LunchFlowApiKeyConfigurationError"]
            );
        }

        userData.LunchFlowApiKey = apiKey;

        userDataContext.ApplicationUsers.Update(userData);
        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RemoveApiKeyAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        userData.LunchFlowApiKey = string.Empty;

        userDataContext.Update(userData);
        await userDataContext.SaveChangesAsync();

        await RemoveLunchFlowDataAsync(userGuid);
    }

    /// <inheritdoc />
    public async Task<IList<string>> RefreshAccountsAsync(Guid userGuid)
    {
        var errors = new List<string>();
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        if (string.IsNullOrEmpty(userData.LunchFlowApiKey))
        {
            logger.LogError("{LogMessage}", logLocalizer["LunchFlowApiKeyMissingErrorLog"]);
            errors.Add(responseLocalizer["LunchFlowApiKeyMissingError"]);
            return errors;
        }

        var lunchFlowData = await GetLunchFlowAccountsDataAsync(userData.LunchFlowApiKey);
        if (lunchFlowData == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["LunchFlowAccountDataRetrievalErrorLog"]);
            errors.Add(responseLocalizer["LunchFlowAccountDataRetrievalError"]);
            return errors;
        }

        errors.AddRange(await RefreshLunchFlowAccountsAsync(userData, lunchFlowData.Accounts));
        return errors;
    }

    /// <inheritdoc />
    public async Task<IList<string>> SyncTransactionHistoryAsync(Guid userGuid)
    {
        var errors = new List<string>();

        errors.AddRange(await SyncTransactionsAsync(userGuid));
        errors.AddRange(await SyncBalancesAsync(userGuid));

        return errors;
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
                .Include(u => u.LunchFlowAccounts)
                .ThenInclude(a => a.LinkedAccount)
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

    private async Task<List<string>> RemoveLunchFlowDataAsync(Guid userGuid)
    {
        List<string> errors = [];
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        foreach (var lunchFlowAccount in userData.LunchFlowAccounts.ToList())
        {
            var linkedAccount = userData.Accounts.SingleOrDefault(a =>
                a.ID == lunchFlowAccount.LinkedAccountId
            );
            if (linkedAccount != null)
            {
                await accountService.UpdateAccountSourceAsync(
                    userData.Id,
                    linkedAccount.ID,
                    AccountSource.Manual
                );
            }

            await lunchFlowAccountService.DeleteLunchFlowAccountAsync(
                userData.Id,
                lunchFlowAccount.ID
            );
        }

        return errors;
    }

    private async Task<HttpResponseMessage> SendQuery(string uri, string apiKey)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        var client = clientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        return await client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> QueryLunchFlowAccountDataAsync(string apiKey)
    {
        var requestUrl = LunchFlowBaseUrl + LunchFlowAccountsEndpoint;
        return await SendQuery(requestUrl, apiKey);
    }

    private async Task<ILunchFlowAccountsData?> GetLunchFlowAccountsDataAsync(string apiKey)
    {
        var response = await QueryLunchFlowAccountDataAsync(apiKey);
        var jsonString = await response.Content.ReadAsStringAsync();

        try
        {
            return JsonSerializer.Deserialize<LunchFlowAccountsData>(jsonString, s_readOptions)
                ?? null;
        }
        catch (JsonException jex)
        {
            logger.LogError(
                jex,
                "{LogMessage}",
                logLocalizer["LunchFlowDeserializationErrorLog", jex.Message]
            );
            return null;
        }
    }

    private async Task<List<string>> RefreshLunchFlowAccountsAsync(
        ApplicationUser userData,
        IEnumerable<ILunchFlowAccountData> lunchFlowAccounts
    )
    {
        List<string> errors = [];

        foreach (var lunchFlowAccount in lunchFlowAccounts)
        {
            var existingAccount = userData.LunchFlowAccounts.FirstOrDefault(a =>
                a.SyncID == lunchFlowAccount.ID
            );
            if (existingAccount == null)
            {
                await lunchFlowAccountService.CreateLunchFlowAccountAsync(
                    userData.Id,
                    new LunchFlowAccountCreateRequest
                    {
                        SyncID = lunchFlowAccount.ID,
                        Name = lunchFlowAccount.Name,
                        InstitutionName = lunchFlowAccount.InstitutionName,
                        InstitutionLogo = lunchFlowAccount.InstitutionLogo,
                        Provider = lunchFlowAccount.Provider,
                        Currency = lunchFlowAccount.Currency,
                        Status = lunchFlowAccount.Status,
                    }
                );
            }
            else
            {
                await lunchFlowAccountService.UpdateLunchFlowAccountAsync(
                    userData.Id,
                    new LunchFlowAccountUpdateRequest
                    {
                        ID = existingAccount.ID,
                        Name = lunchFlowAccount.Name,
                        InstitutionName = lunchFlowAccount.InstitutionName,
                        InstitutionLogo = lunchFlowAccount.InstitutionLogo,
                        Provider = lunchFlowAccount.Provider,
                        Currency = lunchFlowAccount.Currency,
                        Status = lunchFlowAccount.Status,
                    }
                );
            }
        }

        return errors;
    }

    private async Task<HttpResponseMessage> QueryLunchFlowTransactionDataAsync(
        string apiKey,
        string accountId
    )
    {
        var requestUrl =
            LunchFlowBaseUrl
            + LunchFlowAccountsEndpoint
            + $"/{accountId}"
            + LunchFlowTransactionsEndpoint;
        return await SendQuery(requestUrl, apiKey);
    }

    private async Task<ILunchFlowTransactionsData?> GetLunchFlowTransactionsDataAsync(
        string apiKey,
        string accountId
    )
    {
        var response = await QueryLunchFlowTransactionDataAsync(apiKey, accountId);
        var jsonString = await response.Content.ReadAsStringAsync();

        try
        {
            return JsonSerializer.Deserialize<LunchFlowTransactionsData>(jsonString, s_readOptions)
                ?? null;
        }
        catch (JsonException jex)
        {
            logger.LogError(
                jex,
                "{LogMessage}",
                logLocalizer["LunchFlowDeserializationErrorLog", jex.Message]
            );
            return null;
        }
    }

    private async Task<IList<string>> SyncTransactionsAsync(Guid userGuid)
    {
        var errors = new List<string>();
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        if (string.IsNullOrEmpty(userData.LunchFlowApiKey))
        {
            logger.LogError("{LogMessage}", logLocalizer["LunchFlowApiKeyMissingErrorLog"]);
            errors.Add(responseLocalizer["LunchFlowApiKeyMissingError"]);
            return errors;
        }

        var lunchFlowAccounts = userData.LunchFlowAccounts.Where(a => a.LinkedAccountId != null);
        foreach (var lunchFlowAccount in lunchFlowAccounts)
        {
            var lunchFlowTransactionsData = await GetLunchFlowTransactionsDataAsync(
                userData.LunchFlowApiKey,
                lunchFlowAccount.SyncID
            );
            if (lunchFlowTransactionsData == null)
            {
                logger.LogError(
                    "{LogMessage}",
                    logLocalizer[
                        "LunchFlowTransactionDataRetrievalErrorLog",
                        lunchFlowAccount.SyncID
                    ]
                );
                errors.Add(
                    responseLocalizer[
                        "LunchFlowTransactionDataRetrievalError",
                        lunchFlowAccount.SyncID
                    ]
                );
                continue;
            }

            var userAccount = userData.Accounts.FirstOrDefault(a =>
                a.ID == lunchFlowAccount.LinkedAccountId
            );
            if (userAccount == null)
            {
                logger.LogError(
                    "{LogMessage}",
                    logLocalizer[
                        "LunchFlowLinkedAccountNotFoundForSyncErrorLog",
                        lunchFlowAccount.SyncID
                    ]
                );
                errors.Add(
                    responseLocalizer[
                        "LunchFlowLinkedAccountNotFoundForSyncError",
                        lunchFlowAccount.SyncID
                    ]
                );
                continue;
            }

            List<Transaction> userTransactions =
            [
                .. userAccount.Transactions.OrderByDescending(t => t.Date),
            ];
            foreach (var transaction in lunchFlowTransactionsData.Transactions)
            {
                if (userTransactions.Any(t => t.SyncID != null && t.SyncID == transaction.ID))
                {
                    continue;
                }

                await transactionService.CreateTransactionAsync(
                    userData.Id,
                    new TransactionCreateRequest
                    {
                        AccountID = userAccount.ID,
                        SyncID = transaction.ID,
                        Amount = transaction.Amount,
                        Date = DateTime.Parse(transaction.Date),
                        MerchantName = transaction.Merchant,
                        Source = TransactionSource.LunchFlow.ToString(),
                    }
                );
            }
        }

        return errors;
    }

    private async Task<HttpResponseMessage> QueryLunchFlowBalanceDataAsync(
        string apiKey,
        string accountId
    )
    {
        var requestUrl =
            LunchFlowBaseUrl
            + LunchFlowAccountsEndpoint
            + $"/{accountId}"
            + LunchFlowBalancesEndpoint;
        return await SendQuery(requestUrl, apiKey);
    }

    private async Task<ILunchFlowBalanceData?> GetLunchFlowBalancesDataAsync(
        string apiKey,
        string accountId
    )
    {
        var response = await QueryLunchFlowBalanceDataAsync(apiKey, accountId);
        var jsonString = await response.Content.ReadAsStringAsync();

        try
        {
            return JsonSerializer.Deserialize<LunchFlowBalanceData>(jsonString, s_readOptions)
                ?? null;
        }
        catch (JsonException jex)
        {
            logger.LogError(
                jex,
                "{LogMessage}",
                logLocalizer["LunchFlowDeserializationErrorLog", jex.Message]
            );
            return null;
        }
    }

    private async Task<IList<string>> SyncBalancesAsync(Guid userGuid)
    {
        var errors = new List<string>();
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        if (string.IsNullOrEmpty(userData.LunchFlowApiKey))
        {
            logger.LogError("{LogMessage}", logLocalizer["LunchFlowApiKeyMissingErrorLog"]);
            errors.Add(responseLocalizer["LunchFlowApiKeyMissingError"]);
            return errors;
        }

        var lunchFlowAccounts = userData.LunchFlowAccounts.Where(a => a.LinkedAccountId != null);
        foreach (var lunchFlowAccount in lunchFlowAccounts)
        {
            var lunchFlowBalanceData = await GetLunchFlowBalancesDataAsync(
                userData.LunchFlowApiKey,
                lunchFlowAccount.SyncID
            );
            if (lunchFlowBalanceData == null)
            {
                logger.LogError(
                    "{LogMessage}",
                    logLocalizer["LunchFlowBalanceDataRetrievalErrorLog", lunchFlowAccount.SyncID]
                );
                errors.Add(
                    responseLocalizer["LunchFlowBalanceDataRetrievalError", lunchFlowAccount.SyncID]
                );
                continue;
            }

            var error = await SyncHelpers.SyncBalance(
                userData,
                new BalanceCreateRequest
                {
                    AccountID = lunchFlowAccount.LinkedAccountId!.Value,
                    Amount = lunchFlowBalanceData.Balance,
                    DateTime = nowProvider.Now,
                },
                balanceService
            );
            if (error.HasValue)
            {
                errors.Add(responseLocalizer[error.Value.ErrorKey, [.. error.Value.ErrorParams]]);
            }
        }

        return errors;
    }
}
