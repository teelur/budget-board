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

public class BalanceService(
    ILogger<IBalanceService> logger,
    UserDataContext userDataContext,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : IBalanceService
{
    /// <inheritdoc />
    public async Task CreateBalancesAsync(Guid userGuid, IBalanceCreateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var account = GetAccountById(userData, request.AccountID);

        // We only want to create a balance if a balance doesn't already exist for the same date.
        var existingBalance = account.Balances.FirstOrDefault(b => b.Date == request.Date);
        if (existingBalance != null)
        {
            existingBalance.Amount = request.Amount;
        }
        else
        {
            var newBalance = new Balance
            {
                Date = request.Date,
                Amount = request.Amount,
                AccountID = request.AccountID,
            };

            userDataContext.Balances.Add(newBalance);
        }

        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IBalanceResponse>> ReadBalancesAsync(
        Guid userGuid,
        Guid accountId
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var account = GetAccountById(userData, accountId);

        return account.Balances.Select(b => new BalanceResponse(b)).ToList();
    }

    /// <inheritdoc />
    public async Task UpdateBalanceAsync(Guid userGuid, IBalanceUpdateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var balance = GetBalanceById(userData, request.ID);
        var account = GetAccountById(userData, balance.AccountID);

        var duplicateBalance = account.Balances.FirstOrDefault(b =>
            b.Date == request.Date && b.ID != request.ID
        );
        if (duplicateBalance != null)
        {
            logger.LogError("{LogMessage}", logLocalizer["BalanceDuplicateDateLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["BalanceDuplicateDateError"]);
        }

        if (request.Amount.HasValue)
        {
            balance.Amount = request.Amount.Value;
        }
        if (request.Date.HasValue)
        {
            balance.Date = request.Date.Value;
        }

        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteBalanceAsync(Guid userGuid, Guid guid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var balance = GetBalanceById(userData, guid);

        userDataContext.Balances.Remove(balance);
        await userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        return await UserDataServiceHelper.GetCurrentUserAsync(
            userDataContext,
            logger,
            logLocalizer,
            responseLocalizer,
            id,
            users => users.Include(u => u.Accounts).ThenInclude(a => a.Balances)
        );
    }

    private Account GetAccountById(ApplicationUser userData, Guid accountId)
    {
        var account = userData.Accounts.FirstOrDefault(a => a.ID == accountId);
        if (account == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["AccountNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["AccountNotFoundError"]);
        }

        return account;
    }

    private Balance GetBalanceById(ApplicationUser userData, Guid balanceId)
    {
        var balance = userData
            .Accounts.SelectMany(a => a.Balances)
            .FirstOrDefault(b => b.ID == balanceId);
        if (balance == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["BalanceNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["BalanceNotFoundError"]);
        }

        return balance;
    }
}
