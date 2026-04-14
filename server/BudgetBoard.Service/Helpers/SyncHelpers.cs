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

        await balanceService.CreateBalancesAsync(userData.Id, newBalance);
        return null;
    }
}

public interface ILogStringsLocalizer { }

public interface IResponseStringsLocalizer { }
