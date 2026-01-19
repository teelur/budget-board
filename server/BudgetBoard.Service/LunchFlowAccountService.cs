using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

/// <inheritdoc />
public class LunchFlowAccountService(
    ILogger<ILunchFlowAccountService> logger,
    UserDataContext userDataContext,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : ILunchFlowAccountService
{
    /// <inheritdoc />
    public async Task CreateLunchFlowAccountAsync(
        Guid userGuid,
        ILunchFlowAccountCreateRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        if (userData.LunchFlowAccounts.Any(a => a.SyncID == request.SyncID))
        {
            logger.LogError("{LogMessage}", logLocalizer["DuplicateLunchFlowAccountLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["DuplicateLunchFlowAccountError"]
            );
        }

        var newLunchFlowAccount = new LunchFlowAccount
        {
            Name = request.Name,
            SyncID = request.SyncID,
            InstitutionName = request.InstitutionName,
            InstitutionLogo = request.InstitutionLogo,
            Provider = request.Provider,
            Currency = request.Currency ?? string.Empty,
            Status = request.Status ?? string.Empty,
            Balance = request.Balance,
            BalanceDate = request.BalanceDate,
            LastSync = request.LastSync,
            LinkedAccountId = request.LinkedAccountId,
            UserID = userData.Id,
        };

        userDataContext.LunchFlowAccounts.Add(newLunchFlowAccount);
        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ILunchFlowAccountResponse>> ReadLunchFlowAccountsAsync(
        Guid userGuid
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        return userData.LunchFlowAccounts.Select(a => new LunchFlowAccountResponse(a)).ToList();
    }

    /// <inheritdoc />
    public async Task UpdateLunchFlowAccountAsync(
        Guid userGuid,
        ILunchFlowAccountUpdateRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var lunchFlowAccount = userData.LunchFlowAccounts.FirstOrDefault(a => a.ID == request.ID);
        if (lunchFlowAccount == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["LunchFlowAccountNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["LunchFlowAccountNotFoundError"]
            );
        }

        lunchFlowAccount.Name = request.Name;
        lunchFlowAccount.Status = request.Status ?? string.Empty;
        lunchFlowAccount.InstitutionName = request.InstitutionName;
        lunchFlowAccount.InstitutionLogo = request.InstitutionLogo;
        lunchFlowAccount.Provider = request.Provider;
        lunchFlowAccount.Currency = request.Currency ?? string.Empty;
        lunchFlowAccount.Balance = request.Balance;
        lunchFlowAccount.BalanceDate = (int)
            new DateTimeOffset(request.BalanceDate).ToUnixTimeSeconds();
        lunchFlowAccount.LastSync = request.LastSync;

        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteLunchFlowAccountAsync(Guid userGuid, Guid accountGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var lunchFlowAccount = userData.LunchFlowAccounts.FirstOrDefault(a => a.ID == accountGuid);
        if (lunchFlowAccount == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["LunchFlowAccountNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["LunchFlowAccountNotFoundError"]
            );
        }

        userData.LunchFlowAccounts.Remove(lunchFlowAccount);
        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task UpdateLinkedAccountAsync(
        Guid userGuid,
        Guid lunchFlowAccountGuid,
        Guid? linkedAccountGuid
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var lunchFlowAccount = userData.LunchFlowAccounts.FirstOrDefault(a =>
            a.ID == lunchFlowAccountGuid
        );
        if (lunchFlowAccount == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["LunchFlowAccountNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["LunchFlowAccountNotFoundError"]
            );
        }

        if (linkedAccountGuid != null && !userData.Accounts.Any(a => a.ID == linkedAccountGuid))
        {
            logger.LogError("{LogMessage}", logLocalizer["InvalidLunchFlowLinkedAccountIDLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["InvalidLinkedAccountIDError"]);
        }

        var oldAccountLinkedId = lunchFlowAccount.LinkedAccountId;
        if (oldAccountLinkedId != null)
        {
            var oldLinkedAccount = userData.Accounts.FirstOrDefault(a =>
                a.ID == oldAccountLinkedId
            );
            oldLinkedAccount?.Source = AccountSource.Manual;
        }

        lunchFlowAccount.LinkedAccountId = linkedAccountGuid;
        lunchFlowAccount.LastSync = null;

        var linkedAccount = userData.Accounts.FirstOrDefault(a => a.ID == linkedAccountGuid);
        linkedAccount?.Source =
            linkedAccountGuid != null ? AccountSource.LunchFlow : AccountSource.Manual;

        await userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await userDataContext
                .ApplicationUsers.Include(u => u.LunchFlowAccounts)
                .Include(u => u.Accounts)
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            logger.LogError("{LogMessage}", logLocalizer["UserDataRetrievalErrorLog", ex.Message]);
            throw new BudgetBoardServiceException(responseLocalizer["UserDataRetrievalError"]);
        }

        if (foundUser == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["InvalidUserErrorLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["InvalidUserError"]);
        }

        return foundUser;
    }
}
