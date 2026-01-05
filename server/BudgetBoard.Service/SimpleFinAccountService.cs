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
public class SimpleFinAccountService(
    ILogger<ISimpleFinAccountService> logger,
    UserDataContext userDataContext,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : ISimpleFinAccountService
{
    /// <inheritdoc />
    public async Task CreateSimpleFinAccountAsync(
        Guid userGuid,
        ISimpleFinAccountCreateRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var organization = userData.SimpleFinOrganizations.SingleOrDefault(i =>
            i.ID == request.OrganizationId
        );
        if (organization == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["InvalidOrganizationIDLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["InvalidOrganizationIDError"]);
        }

        var newSimpleFinAccount = new SimpleFinAccount
        {
            SyncID = request.SyncID,
            Name = request.Name,
            Currency = request.Currency,
            Balance = request.Balance,
            BalanceDate = request.BalanceDate,
            OrganizationId = request.OrganizationId,
            UserID = userData.Id,
        };

        userDataContext.SimpleFinAccounts.Add(newSimpleFinAccount);
        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ISimpleFinAccountResponse>> ReadSimpleFinAccountsAsync(
        Guid userGuid
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        return userData
            .SimpleFinOrganizations.SelectMany(o => o.Accounts)
            .Select(a => new SimpleFinAccountResponse(a))
            .ToList();
    }

    /// <inheritdoc />
    public async Task UpdateAccountAsync(Guid userGuid, ISimpleFinAccountUpdateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var accountToUpdate = userData
            .SimpleFinOrganizations.SelectMany(o => o.Accounts)
            .SingleOrDefault(a => a.ID == request.ID);
        if (accountToUpdate == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["SimpleFinAccountIDUpdateNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["SimpleFinAccountIDUpdateNotFoundError"]
            );
        }

        accountToUpdate.Name = request.Name;
        accountToUpdate.Currency = request.Currency;
        accountToUpdate.Balance = request.Balance;
        accountToUpdate.BalanceDate = (int)
            new DateTimeOffset(request.BalanceDate).ToUnixTimeSeconds();
        accountToUpdate.LastSync = request.LastSync;

        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteAccountAsync(Guid userGuid, Guid accountGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var accountToDelete = userData
            .SimpleFinOrganizations.SelectMany(o => o.Accounts)
            .SingleOrDefault(a => a.ID == accountGuid);
        if (accountToDelete == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["SimpleFinAccountIDDeleteNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["SimpleFinAccountIDDeleteNotFoundError"]
            );
        }

        userDataContext.SimpleFinAccounts.Remove(accountToDelete);
        await userDataContext.SaveChangesAsync();
    }

    public async Task UpdateLinkedAccountAsync(
        Guid userGuid,
        Guid simpleFinAccountGuid,
        Guid? linkedAccountGuid
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var accountToUpdate = userData.SimpleFinAccounts.SingleOrDefault(o =>
            o.ID == simpleFinAccountGuid
        );
        if (accountToUpdate == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["SimpleFinAccountUpdateNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["SimpleFinAccountUpdateNotFoundError"]
            );
        }

        if (linkedAccountGuid != null && !userData.Accounts.Any(a => a.ID == linkedAccountGuid))
        {
            logger.LogError("{LogMessage}", logLocalizer["InvalidLinkedAccountIDLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["InvalidLinkedAccountIDError"]);
        }

        var oldAccountLinkedId = accountToUpdate.LinkedAccountId;
        if (oldAccountLinkedId != null)
        {
            var oldLinkedAccount = userData.Accounts.FirstOrDefault(a =>
                a.ID == oldAccountLinkedId
            );
            oldLinkedAccount?.Source = AccountSource.Manual;
        }

        accountToUpdate.LinkedAccountId = linkedAccountGuid;
        accountToUpdate.LastSync = null;

        var linkedAccount = userData.Accounts.FirstOrDefault(a => a.ID == linkedAccountGuid);
        linkedAccount?.Source =
            linkedAccountGuid != null ? AccountSource.SimpleFIN : AccountSource.Manual;

        await userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await userDataContext
                .ApplicationUsers.Include(u => u.SimpleFinOrganizations)
                .ThenInclude(i => i.Accounts)
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
