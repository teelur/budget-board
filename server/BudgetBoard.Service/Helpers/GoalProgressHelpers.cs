using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Helpers;

public static class GoalProgressHelpers
{
    public static decimal GetEffectiveTargetAmount(Goal goal) =>
        goal.InitialAmount < 0 ? Math.Abs(goal.InitialAmount) : goal.Amount;

    public static decimal CalculatePercentComplete(Goal goal)
    {
        var targetAmount = GetEffectiveTargetAmount(goal);
        if (targetAmount == 0)
        {
            return 0;
        }

        var accountsTotalBalance = goal.Accounts.Sum(account =>
            account.Balances.OrderByDescending(balance => balance.Date).FirstOrDefault()?.Amount ?? 0
        );
        var currentProgress = accountsTotalBalance - goal.InitialAmount;
        var percentComplete = currentProgress / targetAmount * 100;

        return percentComplete > 100 ? 100 : percentComplete;
    }
}
