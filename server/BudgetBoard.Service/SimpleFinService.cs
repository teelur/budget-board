using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class SimpleFinService(
    IHttpClientFactory clientFactory,
    ILogger<ISimpleFinService> logger,
    UserDataContext userDataContext,
    IAccountService accountService,
    IInstitutionService institutionService,
    ITransactionService transactionService,
    IBalanceService balanceService,
    IGoalService goalService,
    IApplicationUserService applicationUserService,
    IAutomaticRuleService automaticRuleService,
    INowProvider nowProvider
) : ISimpleFinService
{
    public const long UNIX_MONTH = 2629743;
    public const long UNIX_WEEK = 604800;

    private static readonly JsonSerializerOptions s_readOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers =
            {
                static typeInfo =>
                {
                    if (typeInfo.Type == typeof(ISimpleFinAccountData))
                    {
                        typeInfo.CreateObject = () => new SimpleFinAccountData();
                    }
                    else if (typeInfo.Type == typeof(ISimpleFinAccount))
                    {
                        typeInfo.CreateObject = () => new SimpleFinAccount();
                    }
                    else if (typeInfo.Type == typeof(ISimpleFinTransaction))
                    {
                        typeInfo.CreateObject = () => new SimpleFinTransaction();
                    }
                    else if (typeInfo.Type == typeof(ISimpleFinOrganization))
                    {
                        typeInfo.CreateObject = () => new SimpleFinOrganization();
                    }
                },
            },
        },
    };

    private readonly IHttpClientFactory _clientFactory = clientFactory;
    private readonly ILogger<ISimpleFinService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly INowProvider _nowProvider = nowProvider;

    private readonly IAccountService _accountService = accountService;
    private readonly IInstitutionService _institutionService = institutionService;
    private readonly ITransactionService _transactionService = transactionService;
    private readonly IBalanceService _balanceService = balanceService;
    private readonly IGoalService _goalService = goalService;
    private readonly IApplicationUserService _applicationUserService = applicationUserService;
    private readonly IAutomaticRuleService _automaticRuleService = automaticRuleService;

    public async Task<IEnumerable<string>> SyncAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        if (string.IsNullOrEmpty(userData.AccessToken))
        {
            throw new BudgetBoardServiceException("SimpleFIN key is not configured for this user.");
        }

        long startDate;
        if (userData.LastSync == DateTime.MinValue)
        {
            // If we haven't synced before, sync the full 90 days of history
            startDate =
                ((DateTimeOffset)_nowProvider.UtcNow).ToUnixTimeSeconds() - (UNIX_MONTH * 3);
        }
        else
        {
            var oneMonthAgo =
                ((DateTimeOffset)_nowProvider.UtcNow).ToUnixTimeSeconds() - UNIX_MONTH;
            var lastSyncWithBuffer =
                ((DateTimeOffset)userData.LastSync).ToUnixTimeSeconds() - UNIX_WEEK;

            startDate = Math.Min(oneMonthAgo, lastSyncWithBuffer);
        }

        var simpleFinData = await GetAccountData(userData.AccessToken, startDate);
        if (simpleFinData == null)
        {
            _logger.LogError("SimpleFin data not found.");
            throw new BudgetBoardServiceException("SimpleFin data not found.");
        }

        await SyncInstitutionsAsync(userData, simpleFinData.Accounts);
        await SyncAccountsAsync(userData, simpleFinData.Accounts);

        await SyncGoalsAsync(userData);

        await ApplyAutomaticRules(userData);

        await _applicationUserService.UpdateApplicationUserAsync(
            userData.Id,
            new ApplicationUserUpdateRequest { LastSync = _nowProvider.UtcNow }
        );

        return simpleFinData.Errors;
    }

    public async Task UpdateAccessTokenFromSetupToken(Guid userGuid, string setupToken)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var decodeAccessTokenResponse = await DecodeAccessToken(setupToken);

        if (!decodeAccessTokenResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Attempt to decode setup token was unsuccessful.");
            throw new BudgetBoardServiceException("There was an issue decoding the setup token.");
        }

        var accessToken = await decodeAccessTokenResponse.Content.ReadAsStringAsync();

        if (!await IsAccessTokenValid(accessToken))
        {
            _logger.LogError("Attempt to update user with invalid access token.");
            throw new BudgetBoardServiceException("Invalid access token.");
        }

        userData.AccessToken = accessToken;
        await _userDataContext.SaveChangesAsync();
    }

    private async Task<HttpResponseMessage> DecodeAccessToken(string setupToken)
    {
        // SimpleFin tokens are Base64-encoded URLs on which a POST request will
        // return the access URL for getting bank data.

        byte[] data = Convert.FromBase64String(setupToken);
        string decodedString = Encoding.UTF8.GetString(data);

        var request = new HttpRequestMessage(HttpMethod.Post, decodedString);
        var client = _clientFactory.CreateClient();
        var response = await client.SendAsync(request);

        return response;
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        List<ApplicationUser> users;
        ApplicationUser? foundUser;
        try
        {
            users = await _userDataContext
                .ApplicationUsers.Include(u => u.Accounts)
                .ThenInclude(a => a.Transactions)
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Balances)
                .Include(u => u.Institutions)
                .Include(u => u.Goals)
                .ThenInclude(g => g.Accounts)
                .AsSplitQuery()
                .ToListAsync();
            foundUser = users.FirstOrDefault(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "An error occurred while retrieving the user data: {ExceptionMessage}",
                ex.Message
            );
            throw new BudgetBoardServiceException(
                "An error occurred while retrieving the user data."
            );
        }

        if (foundUser == null)
        {
            _logger.LogError("Attempt to create an account for an invalid user.");
            throw new BudgetBoardServiceException("Provided user not found.");
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

    private async Task<HttpResponseMessage> GetSimpleFinAccountData(
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

    private async Task<ISimpleFinAccountData> GetAccountData(string accessToken, long startDate)
    {
        var response = await GetSimpleFinAccountData(accessToken, startDate);
        var jsonString = await response.Content.ReadAsStringAsync();

        try
        {
            return JsonSerializer.Deserialize<ISimpleFinAccountData>(jsonString, s_readOptions)
                ?? new SimpleFinAccountData();
        }
        catch (JsonException jex)
        {
            _logger.LogError(jex, "Error deserializing SimpleFin data: {Message}", jex.Message);
            return new SimpleFinAccountData();
        }
    }

    private async Task<bool> IsAccessTokenValid(string accessToken) =>
        (await GetSimpleFinAccountData(accessToken, null)).IsSuccessStatusCode;

    private async Task SyncInstitutionsAsync(
        ApplicationUser userData,
        IEnumerable<ISimpleFinAccount> accountsData
    )
    {
        var institutions = accountsData.Select(a => a.Org).Distinct();
        foreach (var institution in institutions)
        {
            if (institution == null)
                continue;
            if (userData.Institutions.Any(i => i.Name == institution.Name))
                continue;

            var newInstitution = new InstitutionCreateRequest
            {
                Name = institution.Name ?? string.Empty,
            };

            await _institutionService.CreateInstitutionAsync(userData.Id, newInstitution);
        }
    }

    private async Task SyncAccountsAsync(
        ApplicationUser userData,
        IEnumerable<ISimpleFinAccount> accountsData
    )
    {
        // This is a temporary fix for a bug that didn't assign account source for manual accounts.
        // I will remove this later once we can be sure this won't affect anyone.
        foreach (var account in userData.Accounts)
        {
            if (string.IsNullOrEmpty(account.Source))
            {
                account.Source =
                    account.SyncID != null ? AccountSource.SimpleFIN : AccountSource.Manual;
            }
        }
        foreach (var accountData in accountsData)
        {
            var institutionId = userData
                .Institutions.FirstOrDefault(institution =>
                    institution.Name == accountData.Org.Name
                )
                ?.ID;

            var foundAccount = userData.Accounts.SingleOrDefault(a => a.SyncID == accountData.Id);
            if (foundAccount != null)
            {
                foundAccount.InstitutionID = institutionId;
                foundAccount.Source = AccountSource.SimpleFIN;

                _userDataContext.SaveChanges();
            }
            else
            {
                var newAccount = new AccountCreateRequest
                {
                    SyncID = accountData.Id,
                    Name = accountData.Name,
                    InstitutionID = institutionId,
                    Source = AccountSource.SimpleFIN,
                };

                await _accountService.CreateAccountAsync(userData.Id, newAccount);
            }

            await SyncTransactionsAsync(userData, accountData.Id, accountData.Transactions);
            await SyncBalancesAsync(userData, accountData.Id, accountData);
        }
    }

    private async Task SyncTransactionsAsync(
        ApplicationUser userData,
        string syncId,
        IEnumerable<ISimpleFinTransaction> transactionsData
    )
    {
        if (!transactionsData.Any())
            return;

        var userAccount = userData.Accounts.FirstOrDefault(a =>
            (a.SyncID ?? string.Empty).Equals(syncId)
        );
        var userTransactions = userAccount?.Transactions.OrderByDescending(t => t.Date).ToList();

        // User account should never be null here, but let's not make a bad problem worse.
        if (userAccount != null)
        {
            foreach (var transactionData in transactionsData)
            {
                if (
                    (userTransactions ?? []).Any(t =>
                        (t.SyncID ?? string.Empty).Equals(transactionData.Id)
                    )
                )
                {
                    // Transaction already exists.
                    continue;
                }
                else
                {
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
            }
        }
    }

    private async Task SyncBalancesAsync(
        ApplicationUser userData,
        string syncId,
        ISimpleFinAccount accountData
    )
    {
        var foundAccount = userData.Accounts.SingleOrDefault(a => a.SyncID == syncId);

        // User account should never be null here, but let's not make a bad problem worse.
        if (foundAccount != null)
        {
            var balanceDates = foundAccount.Balances.Select(b => b.DateTime);
            /**
             * We should only update the balance if account has no balances or the
             * last balance is older than the last balance in the SimpleFin data.
             */
            if (
                !balanceDates.Any()
                || balanceDates.Max() < DateTime.UnixEpoch.AddSeconds(accountData.BalanceDate)
            )
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
        }
    }

    private async Task SyncGoalsAsync(ApplicationUser userData)
    {
        var userGoals = userData.Goals.ToList();

        foreach (var goal in userGoals)
        {
            // Skip goals that are already completed
            if (goal.Completed.HasValue)
            {
                continue;
            }

            var accountsTotalBalance = goal.Accounts.Sum(a =>
                a.Balances.OrderByDescending(b => b.DateTime).FirstOrDefault()?.Amount ?? 0
            );

            var completeDate = goal
                .Accounts.SelectMany(a => a.Balances)
                .OrderByDescending(b => b.DateTime)
                .FirstOrDefault()
                ?.DateTime;

            if (!completeDate.HasValue)
            {
                _logger.LogError("No balance found for goal {GoalName}", goal.Name);
                continue;
            }

            if (goal.Amount == 0)
            {
                // Debt payoff goal: complete when balance is zero or negative
                if (accountsTotalBalance >= 0)
                {
                    await _goalService.CompleteGoalAsync(userData.Id, goal.ID, completeDate.Value);
                }
            }
            else
            {
                // Savings goal: complete when balance reaches or exceeds target
                if ((accountsTotalBalance - goal.InitialAmount) >= goal.Amount)
                {
                    await _goalService.CompleteGoalAsync(userData.Id, goal.ID, completeDate.Value);
                }
            }
        }
    }

    private async Task ApplyAutomaticRules(ApplicationUser userData)
    {
        var customCategories = userData.TransactionCategories.Select(tc => new CategoryBase()
        {
            Value = tc.Value,
            Parent = tc.Parent,
        });

        var allCategories = TransactionCategoriesConstants.DefaultTransactionCategories.Concat(
            customCategories
        );

        var rules = await _automaticRuleService.ReadAutomaticRulesAsync(userData.Id);

        foreach (var rule in rules)
        {
            bool invalidCondition = false;

            var matchedTransactions = userData
                .Accounts.SelectMany(a => a.Transactions)
                .Where(t => t.Deleted == null && !(t.Account?.HideTransactions ?? false));
            foreach (var condition in rule.Conditions)
            {
                try
                {
                    matchedTransactions = AutomaticRuleHelpers.FilterOnCondition(
                        condition,
                        matchedTransactions,
                        allCategories
                    );
                }
                catch (BudgetBoardServiceException bbex)
                {
                    _logger.LogError(
                        bbex,
                        "Error applying condition {ConditionId} of rule {RuleId}: {Message}",
                        condition.ID,
                        rule.ID,
                        bbex.Message
                    );

                    invalidCondition = true;
                    break;
                }
            }

            if (invalidCondition)
            {
                _logger.LogInformation(
                    "An error occurred in one of the conditions. Rule actions will not be applied."
                );
                continue;
            }

            _logger.LogInformation(
                "Rule {RuleId} matched {matchedTransactionsCount} transactions.",
                rule.ID,
                matchedTransactions.Count()
            );

            int updatedCount = 0;

            foreach (var action in rule.Actions)
            {
                try
                {
                    updatedCount += await AutomaticRuleHelpers.ApplyActionToTransactions(
                        action,
                        matchedTransactions,
                        allCategories,
                        _transactionService,
                        userData.Id
                    );
                }
                catch (BudgetBoardServiceException bbex)
                {
                    _logger.LogError(
                        bbex,
                        "Error applying action {ActionId} of rule {RuleId}: {Message}",
                        action.ID,
                        rule.ID,
                        bbex.Message
                    );
                    continue;
                }
            }

            _logger.LogInformation(
                "Applied automatic rule {RuleId}, updated {UpdatedCount} transactions.",
                rule.ID,
                updatedCount
            );
        }
    }
}
