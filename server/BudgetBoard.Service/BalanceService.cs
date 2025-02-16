﻿using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class BalanceService(ILogger<IBalanceService> logger, UserDataContext userDataContext) : IBalanceService
{
    private readonly ILogger<IBalanceService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;

    public async Task CreateBalancesAsync(Guid userGuid, IBalanceCreateRequest balance)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var account = userData.Accounts.FirstOrDefault(a => a.ID == balance.AccountID);
        if (account == null)
        {
            _logger.LogError("Attempt to add balance to account that does not exist.");
            throw new Exception("The account you are trying to add a balance to does not exist.");
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

    public async Task<IEnumerable<IBalanceResponse>> ReadBalancesAsync(Guid userGuid, Guid accountId)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var account = userData.Accounts.FirstOrDefault(a => a.ID == accountId);
        if (account == null)
        {
            _logger.LogError("Attempt to read balance from account that does not exist.");
            throw new Exception("The account you are trying to read a balance from does not exist.");
        }

        return account.Balances.Select(b => new BalanceResponse(b));
    }

    public async Task UpdateBalanceAsync(Guid userGuid, IBalanceUpdateRequest updatedBalance)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var balance = userData.Accounts.SelectMany(a => a.Balances).FirstOrDefault(b => b.ID == updatedBalance.ID);
        if (balance == null)
        {
            _logger.LogError("Attempt to update balance that does not exist.");
            throw new Exception("The balance you are trying to update does not exist.");
        }

        balance.DateTime = updatedBalance.DateTime;
        balance.Amount = updatedBalance.Amount;

        await _userDataContext.SaveChangesAsync();
    }

    public async Task DeleteBalanceAsync(Guid userGuid, Guid balanceId)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var balance = userData.Accounts.SelectMany(a => a.Balances).FirstOrDefault(b => b.ID == balanceId);
        if (balance == null)
        {
            _logger.LogError("Attempt to delete balance that does not exist.");
            throw new Exception("The balance you are trying to delete does not exist.");
        }

        _userDataContext.Balances.Remove(balance);
        await _userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        List<ApplicationUser> users;
        ApplicationUser? foundUser;
        try
        {
            users = await _userDataContext.ApplicationUsers
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Balances)
                .AsSplitQuery()
                .ToListAsync();
            foundUser = users.FirstOrDefault(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while retrieving the user data: {ExceptionMessage}", ex.Message);
            throw new Exception("An error occurred while retrieving the user data.");
        }

        if (foundUser == null)
        {
            _logger.LogError("Attempt to create an account for an invalid user.");
            throw new Exception("Provided user not found.");
        }

        return foundUser;
    }
}
