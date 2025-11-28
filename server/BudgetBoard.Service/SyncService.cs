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

public class SyncService(
    IHttpClientFactory clientFactory,
    ILogger<ISyncService> logger,
    UserDataContext userDataContext,
    ISyncProvider simpleFinService,
    ITransactionService transactionService,
    IGoalService goalService,
    IApplicationUserService applicationUserService,
    IAutomaticRuleService automaticRuleService,
    INowProvider nowProvider,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : ISyncService
{
    private readonly IHttpClientFactory _clientFactory = clientFactory;
    private readonly ILogger<ISyncService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly INowProvider _nowProvider = nowProvider;
    private readonly ISyncProvider _simpleFinService = simpleFinService;
    private readonly ITransactionService _transactionService = transactionService;
    private readonly IGoalService _goalService = goalService;
    private readonly IApplicationUserService _applicationUserService = applicationUserService;
    private readonly IAutomaticRuleService _automaticRuleService = automaticRuleService;
    private readonly IStringLocalizer<ResponseStrings> _responseLocalizer = responseLocalizer;
    private readonly IStringLocalizer<LogStrings> _logLocalizer = logLocalizer;

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> SyncAsync(Guid userGuid)
    {
        var errors = new List<string>();
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        // A previous bug caused some accounts to be created without a source. This will
        // backfill those accounts with the correct source.
        BackfillMissingAccountSources(userData.Accounts);

        if (!string.IsNullOrEmpty(userData.AccessToken))
        {
            errors.AddRange(await _simpleFinService.SyncDataAsync(userGuid));
        }
        else
        {
            _logger.LogInformation("{LogMessage}", _logLocalizer["SimpleFinTokenNotConfiguredLog"]);
        }

        await _goalService.CompleteGoalsAsync(userData.Id);
        await _automaticRuleService.RunAutomaticRulesAsync(userData.Id);

        await _applicationUserService.UpdateApplicationUserAsync(
            userData.Id,
            new ApplicationUserUpdateRequest { LastSync = _nowProvider.UtcNow }
        );

        return errors;
    }

    /// <inheritdoc />
    public async Task UpdateAccessTokenFromSetupToken(Guid userGuid, string setupToken) =>
        await _simpleFinService.ConfigureAccessTokenAsync(userGuid, setupToken);

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
                .Include(u => u.Goals)
                .ThenInclude(g => g.Accounts)
                .Include(u => u.UserSettings)
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

    private static void BackfillMissingAccountSources(IEnumerable<Account> accounts)
    {
        foreach (var account in accounts)
        {
            if (string.IsNullOrEmpty(account.Source))
            {
                account.Source =
                    account.SyncID != null ? AccountSource.SimpleFIN : AccountSource.Manual;
            }
        }
    }
}
