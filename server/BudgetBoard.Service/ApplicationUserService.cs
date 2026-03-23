using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class ApplicationUserService(
    ILogger<IApplicationUserService> logger,
    UserDataContext userDataContext,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : IApplicationUserService
{
    private readonly ILogger<IApplicationUserService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly IStringLocalizer<ResponseStrings> _responseLocalizer = responseLocalizer;
    private readonly IStringLocalizer<LogStrings> _logLocalizer = logLocalizer;

    /// <inheritdoc />
    public async Task<IApplicationUserResponse> ReadApplicationUserAsync(
        Guid userGuid,
        UserManager<ApplicationUser> userManager
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var logins = await userManager.GetLoginsAsync(userData);
        var hasOidcLogin = logins.Any(l => l.LoginProvider == "oidc");
        var hasLocalLogin = logins.Any(l => l.LoginProvider == "local");

        return new ApplicationUserResponse(userData, hasOidcLogin, hasLocalLogin);
    }

    /// <inheritdoc />
    public async Task UpdateApplicationUserAsync(
        Guid userGuid,
        IApplicationUserUpdateRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        _userDataContext.Entry(userData).CurrentValues.SetValues(request);
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task WipeUserDataAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        await using var transaction = await _userDataContext.Database.BeginTransactionAsync();

        try
        {
            await _userDataContext
                .Values.Where(v => v.Asset != null && v.Asset.UserID == userGuid)
                .ExecuteDeleteAsync();

            await _userDataContext
                .Transactions.Where(t => t.Account != null && t.Account.UserID == userGuid)
                .ExecuteDeleteAsync();

            await _userDataContext
                .Balances.Where(b => b.Account != null && b.Account.UserID == userGuid)
                .ExecuteDeleteAsync();

            await _userDataContext
                .SimpleFinAccounts.Where(a => a.UserID == userGuid)
                .ExecuteDeleteAsync();

            await _userDataContext
                .LunchFlowAccounts.Where(a => a.UserID == userGuid)
                .ExecuteDeleteAsync();

            await _userDataContext
                .Accounts.Where(a => a.UserID == userGuid)
                .ExecuteDeleteAsync();

            await _userDataContext
                .SimpleFinOrganizations.Where(o => o.UserID == userGuid)
                .ExecuteDeleteAsync();

            await _userDataContext.Budgets.Where(b => b.UserID == userGuid).ExecuteDeleteAsync();

            await _userDataContext.Goals.Where(g => g.UserID == userGuid).ExecuteDeleteAsync();

            await _userDataContext.Assets.Where(a => a.UserID == userGuid).ExecuteDeleteAsync();

            await _userDataContext
                .AutomaticRules.Where(r => r.UserID == userGuid)
                .ExecuteDeleteAsync();

            await _userDataContext
                .TransactionCategories.Where(c => c.UserID == userGuid)
                .ExecuteDeleteAsync();

            await _userDataContext
                .Institutions.Where(i => i.UserID == userGuid)
                .ExecuteDeleteAsync();

            if (userData.UserSettings != null)
            {
                userData.UserSettings.EnableAutoCategorizer = false;
                userData.UserSettings.AutoCategorizerModelOID = null;
                userData.UserSettings.AutoCategorizerLastTrained = null;
                userData.UserSettings.AutoCategorizerModelStartDate = null;
                userData.UserSettings.AutoCategorizerModelEndDate = null;
            }

            userData.LastSync = DateTime.MinValue;

            await _userDataContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to wipe user data for user {UserId}", userGuid);
            throw new BudgetBoardServiceException("Failed to wipe user data.");
        }
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await _userDataContext
                .ApplicationUsers.Include(u => u.UserSettings)
                .FirstOrDefaultAsync(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["UserRetrievalErrorLog", ex.Message]);
            throw new BudgetBoardServiceException(_responseLocalizer["UserRetrievalError"]);
        }

        if (foundUser == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["InvalidUserErrorLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["InvalidUserError"]);
        }

        return foundUser;
    }
}
