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
        var userData = await GetCurrentUserAsync(userGuid);

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
        var userData = await GetCurrentUserAsync(userGuid);
        return userData.LunchFlowAccounts.Select(a => new LunchFlowAccountResponse(a)).ToList();
    }

    /// <inheritdoc />
    public async Task UpdateLunchFlowAccountAsync(
        Guid userGuid,
        ILunchFlowAccountUpdateRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var lunchFlowAccount = GetLunchFlowAccountById(userData, request.ID);

        lunchFlowAccount.Name = request.Name;
        lunchFlowAccount.InstitutionName = request.InstitutionName;
        lunchFlowAccount.InstitutionLogo = request.InstitutionLogo;
        lunchFlowAccount.Provider = request.Provider;
        lunchFlowAccount.Currency = request.Currency ?? string.Empty;
        lunchFlowAccount.Status = request.Status ?? string.Empty;
        lunchFlowAccount.Balance = request.Balance;
        lunchFlowAccount.BalanceDate = (int)
            new DateTimeOffset(request.BalanceDate).ToUnixTimeSeconds();
        lunchFlowAccount.LastSync = request.LastSync;

        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteLunchFlowAccountAsync(Guid userGuid, Guid accountGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var lunchFlowAccount = GetLunchFlowAccountById(userData, accountGuid);

        if (lunchFlowAccount.LinkedAccountId.HasValue)
        {
            var linkedAccount = userData.Accounts.FirstOrDefault(a =>
                a.ID == lunchFlowAccount.LinkedAccountId.Value
            );
            linkedAccount?.Source = AccountSource.Manual;
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
        var userData = await GetCurrentUserAsync(userGuid);
        var lunchFlowAccount = GetLunchFlowAccountById(userData, lunchFlowAccountGuid);

        if (
            linkedAccountGuid is Guid targetAccountGuid
            && !userData.Accounts.Any(a => a.ID == targetAccountGuid)
        )
        {
            logger.LogError("{LogMessage}", logLocalizer["InvalidLunchFlowLinkedAccountIDLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["InvalidLinkedAccountIDError"]);
        }

        if (lunchFlowAccount.LinkedAccountId.HasValue)
        {
            var oldLinkedAccount = userData.Accounts.FirstOrDefault(a =>
                a.ID == lunchFlowAccount.LinkedAccountId.Value
            );
            oldLinkedAccount?.Source = AccountSource.Manual;
        }

        lunchFlowAccount.LinkedAccountId = linkedAccountGuid;
        lunchFlowAccount.LastSync = null;

        if (linkedAccountGuid.HasValue)
        {
            var linkedAccount = userData.Accounts.FirstOrDefault(a =>
                a.ID == linkedAccountGuid.Value
            );
            linkedAccount?.Source = AccountSource.LunchFlow;
        }

        await userDataContext.SaveChangesAsync();
    }

    public async Task UpdateLunchFlowAccountSyncStartDateAsync(
        Guid userGuid,
        Guid lunchFlowAccountGuid,
        DateOnly? syncStartDate
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var lunchFlowAccount = GetLunchFlowAccountById(userData, lunchFlowAccountGuid);

        lunchFlowAccount.SyncStartDate = syncStartDate;

        await userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(Guid id)
    {
        return await UserDataServiceHelper.GetCurrentUserAsync(
            userDataContext,
            logger,
            logLocalizer,
            responseLocalizer,
            id,
            users => users.Include(u => u.LunchFlowAccounts).Include(u => u.Accounts)
        );
    }

    private LunchFlowAccount GetLunchFlowAccountById(ApplicationUser user, Guid accountGuid)
    {
        var lunchFlowAccount = user.LunchFlowAccounts.FirstOrDefault(a => a.ID == accountGuid);
        if (lunchFlowAccount == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["LunchFlowAccountNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["LunchFlowAccountNotFoundError"]
            );
        }
        return lunchFlowAccount;
    }
}
