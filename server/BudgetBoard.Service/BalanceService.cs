using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class BalanceService(
    ILogger<IBalanceService> logger,
    UserDataContext userDataContext,
    INowProvider nowProvider
) : IBalanceService
{
    private readonly ILogger<IBalanceService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly INowProvider _nowProvider = nowProvider;

    /// <inheritdoc />
    public async Task CreateBalancesAsync(Guid userGuid, IBalanceCreateRequest balance)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var account = userData.Accounts.FirstOrDefault(a => a.ID == balance.AccountID);
        if (account == null)
        {
            _logger.LogError("Attempt to add balance to account that does not exist.");
            throw new BudgetBoardServiceException(
                "The account you are trying to add a balance to does not exist."
            );
        }

        Balance newBalance = new()
        {
            DateTime = balance.DateTime,
            Amount = balance.Amount,
            AccountID = balance.AccountID,
        };

        account.Balances.Add(newBalance);
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IBalanceResponse>> ReadBalancesAsync(
        Guid userGuid,
        Guid accountId
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var account = userData.Accounts.FirstOrDefault(a => a.ID == accountId);
        if (account == null)
        {
            _logger.LogError("Attempt to read balance from account that does not exist.");
            throw new BudgetBoardServiceException(
                "The account you are trying to read a balance from does not exist."
            );
        }

        return account.Balances.Select(b => new BalanceResponse(b)).ToList();
    }

    /// <inheritdoc />
    public async Task UpdateBalanceAsync(Guid userGuid, IBalanceUpdateRequest updatedBalance)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var balance = userData
            .Accounts.SelectMany(a => a.Balances)
            .FirstOrDefault(b => b.ID == updatedBalance.ID);
        if (balance == null)
        {
            _logger.LogError("Attempt to update balance that does not exist.");
            throw new BudgetBoardServiceException(
                "The balance you are trying to update does not exist."
            );
        }

        balance.DateTime = updatedBalance.DateTime;
        balance.Amount = updatedBalance.Amount;

        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteBalanceAsync(Guid userGuid, Guid guid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var balance = userData
            .Accounts.SelectMany(a => a.Balances)
            .FirstOrDefault(b => b.ID == guid);
        if (balance == null)
        {
            _logger.LogError("Attempt to delete balance that does not exist.");
            throw new BudgetBoardServiceException(
                "The balance you are trying to delete does not exist."
            );
        }

        balance.Deleted = _nowProvider.UtcNow;
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RestoreBalanceAsync(Guid userGuid, Guid guid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var balance = userData
            .Accounts.SelectMany(a => a.Balances)
            .FirstOrDefault(b => b.ID == guid);
        if (balance == null)
        {
            _logger.LogError("Attempt to restore balance that does not exist.");
            throw new BudgetBoardServiceException(
                "The balance you are trying to restore does not exist."
            );
        }

        balance.Deleted = null;
        await _userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        List<ApplicationUser> users;
        ApplicationUser? foundUser;
        try
        {
            users = await _userDataContext
                .ApplicationUsers.Include(u => u.Accounts)
                .ThenInclude(a => a.Balances)
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
}
