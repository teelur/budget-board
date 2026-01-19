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
    ILogger<ISyncService> logger,
    UserDataContext userDataContext,
    ISimpleFinService simpleFinService,
    ILunchFlowService lunchFlowService,
    IGoalService goalService,
    IApplicationUserService applicationUserService,
    IAutomaticRuleService automaticRuleService,
    INowProvider nowProvider,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : ISyncService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> SyncAsync(Guid userGuid)
    {
        var errors = new List<string>();
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        // The external sync provider update changed how the Source is determined. Need to reconcile existing accounts.
        SetManualSourceForUnlinkedAccounts(userData.Accounts);

        if (!string.IsNullOrEmpty(userData.SimpleFinAccessToken))
        {
            errors.AddRange(await simpleFinService.RefreshAccountsAsync(userGuid));
            errors.AddRange(await simpleFinService.SyncTransactionHistoryAsync(userGuid));
        }
        else
        {
            logger.LogInformation("{LogMessage}", logLocalizer["SimpleFinTokenNotConfiguredLog"]);
        }

        if (!string.IsNullOrEmpty(userData.LunchFlowApiKey))
        {
            errors.AddRange(await lunchFlowService.RefreshAccountsAsync(userGuid));
            errors.AddRange(await lunchFlowService.SyncTransactionHistoryAsync(userGuid));
        }
        else
        {
            logger.LogInformation("{LogMessage}", logLocalizer["LunchFlowApiKeyNotConfiguredLog"]);
        }

        await goalService.CompleteGoalsAsync(userData.Id);
        await automaticRuleService.RunAutomaticRulesAsync(userData.Id);

        await applicationUserService.UpdateApplicationUserAsync(
            userData.Id,
            new ApplicationUserUpdateRequest { LastSync = nowProvider.UtcNow }
        );

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
                .Include(u => u.Goals)
                .ThenInclude(g => g.Accounts)
                .Include(u => u.UserSettings)
                .Include(u => u.Accounts)
                .ThenInclude(a => a.SimpleFinAccount)
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

    private static void SetManualSourceForUnlinkedAccounts(IEnumerable<Account> accounts)
    {
        foreach (var account in accounts.Where(a => a.SimpleFinAccount == null))
        {
            account.Source = AccountSource.Manual;
        }
    }
}
