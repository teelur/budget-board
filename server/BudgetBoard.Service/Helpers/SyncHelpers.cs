using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Helpers;

public static class SyncHelpers
{
    public readonly struct SyncBalanceError()
    {
        public string ErrorKey { get; init; } = string.Empty;
        public IList<string> ErrorParams { get; init; } = [];
    }

    public static async Task<SyncBalanceError?> SyncBalance(
        ApplicationUser userData,
        IBalanceCreateRequest newBalance,
        IBalanceService balanceService
    )
    {
        var userAccount = userData.Accounts.FirstOrDefault(a => a.ID == newBalance.AccountID);
        if (userAccount == null)
        {
            return new SyncBalanceError
            {
                ErrorKey = "AccountNotFoundError",
                ErrorParams = [newBalance.AccountID.ToString()],
            };
        }

        // We only want to create a balance if it is newer than the latest balance we have.
        var latestBalance = userAccount
            .Balances.OrderByDescending(b => b.DateTime)
            .FirstOrDefault();
        if (newBalance.DateTime > (latestBalance?.DateTime ?? DateTime.MinValue))
        {
            // If the account already has a balance for this date, update it instead of creating a new one.
            var existingBalance = userAccount.Balances.FirstOrDefault(b =>
                b.DateTime.Date == newBalance.DateTime.Date
            );
            if (existingBalance != null)
            {
                existingBalance.Amount = newBalance.Amount;
                await balanceService.UpdateBalanceAsync(
                    userData.Id,
                    new BalanceUpdateRequest
                    {
                        ID = existingBalance.ID,
                        Amount = newBalance.Amount,
                        DateTime = newBalance.DateTime,
                        AccountID = existingBalance.AccountID,
                    }
                );
                return null;
            }

            // Otherwise, create a new balance.
            await balanceService.CreateBalancesAsync(userData.Id, newBalance);
        }
        return null;
    }
}

public interface ILogStringsLocalizer { }

public interface IResponseStringsLocalizer { }
