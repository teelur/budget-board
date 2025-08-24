using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class GoalService(
    ILogger<IGoalService> logger,
    UserDataContext userDataContext,
    INowProvider nowProvider
) : IGoalService
{
    private readonly ILogger<IGoalService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly INowProvider _nowProvider = nowProvider;

    public async Task CreateGoalAsync(Guid userGuid, IGoalCreateRequest createdGoal)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        if (
            (createdGoal.MonthlyContribution == 0 || !createdGoal.MonthlyContribution.HasValue)
            && !createdGoal.CompleteDate.HasValue
        )
        {
            _logger.LogError(
                "Attempt to create goal without a monthly contribution and target date."
            );
            throw new BudgetBoardServiceException(
                "A goal must have a monthly contribution or target date."
            );
        }

        if (
            createdGoal.CompleteDate.HasValue
            && createdGoal.CompleteDate.Value < _nowProvider.UtcNow
        )
        {
            _logger.LogError("Attempt to create goal with a target date in the past.");
            throw new BudgetBoardServiceException("A goal cannot have a target date in the past.");
        }

        if (!createdGoal.AccountIds.Any())
        {
            _logger.LogError("Attempt to create goal without any accounts.");
            throw new BudgetBoardServiceException(
                "A goal must be associated with at least one account."
            );
        }

        decimal runningBalance = 0.0M;
        var accounts = new List<Account>();
        foreach (var accountId in createdGoal.AccountIds)
        {
            var account = userData.Accounts.FirstOrDefault((a) => a.ID == accountId);
            if (account == null)
            {
                _logger.LogError("Attempt to create goal with invalid account.");
                throw new BudgetBoardServiceException(
                    "The account you are trying to use does not exist."
                );
            }

            runningBalance +=
                account.Balances.OrderByDescending(b => b.DateTime).FirstOrDefault()?.Amount ?? 0;
            accounts.Add(account);
        }

        var newGoal = new Goal
        {
            Name = createdGoal.Name,
            CompleteDate = createdGoal.CompleteDate,
            Amount = createdGoal.Amount,
            MonthlyContribution = createdGoal.MonthlyContribution,
            Accounts = accounts,
            UserID = userData.Id,
        };

        if (!createdGoal.InitialAmount.HasValue)
        {
            // The frontend will set the initial balance if we don't want to include existing balances
            // in the goal.
            newGoal.InitialAmount = runningBalance;
        }
        else
        {
            newGoal.InitialAmount = createdGoal.InitialAmount.Value;
        }

        userData.Goals.Add(newGoal);
        await _userDataContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<IGoalResponse>> ReadGoalsAsync(
        Guid userGuid,
        bool includeInterest
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var goalsResponse = new List<IGoalResponse>();
        var goals = userData.Goals.ToList();
        foreach (var goal in goals)
        {
            goalsResponse.Add(
                new GoalResponse(goal)
                {
                    CompleteDate = EstimateGoalCompleteDate(goal, includeInterest),
                    // Have to manually set this, since we override the CompleteDate in the constructor.
                    IsCompleteDateEditable = goal.CompleteDate != null,
                    MonthlyContribution = EstimateGoalMonthlyContribution(goal, includeInterest),
                    // Have to manually set this, since we override the MonthlyContribution in the constructor.
                    IsMonthlyContributionEditable = goal.MonthlyContribution != null,
                    MonthlyContributionProgress = GetGoalMonthlyContributionProgress(
                        goal.Accounts.SelectMany(a => a.Transactions)
                    ),
                    InterestRate = CalculateAverageInterestRate(goal),
                    PercentComplete = CalculatePercentComplete(goal),
                }
            );
        }

        return goalsResponse;
    }

    public async Task UpdateGoalAsync(Guid userGuid, IGoalUpdateRequest updatedGoal)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var goal = userData.Goals.FirstOrDefault(g => g.ID == updatedGoal.ID);
        if (goal == null)
        {
            _logger.LogError("Attempt to update goal that does not exist.");
            throw new BudgetBoardServiceException(
                "The goal you are trying to update does not exist."
            );
        }

        if (
            updatedGoal.CompleteDate.HasValue
            && updatedGoal.IsCompleteDateEditable
            && updatedGoal.CompleteDate.Value < _nowProvider.UtcNow
        )
        {
            _logger.LogError("Attempt to update goal with a target date in the past.");
            throw new BudgetBoardServiceException("A goal cannot have a target date in the past.");
        }

        if (
            ((updatedGoal.MonthlyContribution ?? -1) <= 0)
            && updatedGoal.IsMonthlyContributionEditable
        )
        {
            _logger.LogError("Attempt to update goal without a monthly contribution.");
            throw new BudgetBoardServiceException(
                "A goal must have a monthly contribution greater than 0."
            );
        }

        goal.Name = updatedGoal.Name;
        goal.Amount = updatedGoal.Amount;
        goal.CompleteDate = updatedGoal.IsCompleteDateEditable
            ? updatedGoal.CompleteDate
            : goal.CompleteDate;
        goal.MonthlyContribution = updatedGoal.IsMonthlyContributionEditable
            ? updatedGoal.MonthlyContribution
            : goal.MonthlyContribution;

        await _userDataContext.SaveChangesAsync();
    }

    public async Task DeleteGoalAsync(Guid userGuid, Guid guid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var goal = userData.Goals.FirstOrDefault(g => g.ID == guid);
        if (goal == null)
        {
            _logger.LogError("Attempt to delete goal that does not exist.");
            throw new BudgetBoardServiceException(
                "The goal you are trying to delete does not exist."
            );
        }

        _userDataContext.Goals.Remove(goal);
        await _userDataContext.SaveChangesAsync();
    }

    public async Task CompleteGoalAsync(Guid userGuid, Guid goalID, DateTime completedDate)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var goal = userData.Goals.FirstOrDefault(g => g.ID == goalID);
        if (goal == null)
        {
            _logger.LogError("Attempt to complete goal that does not exist.");
            throw new BudgetBoardServiceException(
                "The goal you are trying to complete does not exist."
            );
        }

        if (goal.Completed.HasValue)
        {
            _logger.LogError("Attempt to complete goal that has already been completed.");
            throw new BudgetBoardServiceException(
                "The goal you are trying to complete has already been completed."
            );
        }

        goal.Completed = completedDate;
        await _userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        List<ApplicationUser> users;
        ApplicationUser? foundUser;
        try
        {
            users = await _userDataContext
                .ApplicationUsers.Include(u => u.Goals)
                .ThenInclude((g) => g.Accounts)
                .ThenInclude((a) => a.Transactions)
                .Include(u => u.Accounts)
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

    private DateTime EstimateGoalCompleteDate(Goal goal, bool includeInterest = false)
    {
        if (goal.CompleteDate.HasValue)
            return goal.CompleteDate.Value;

        if (goal.MonthlyContribution == null || goal.MonthlyContribution == 0)
        {
            // If a complete date has not been set, then a monthly contribution is required.
            _logger.LogError("A target date cannot be estimated without a monthly contribution.");
            return DateTime.UnixEpoch;
        }

        decimal totalBalance = goal.Accounts.Sum(a =>
            a.Balances.OrderByDescending(b => b.DateTime).FirstOrDefault()?.Amount ?? 0
        );
        decimal amountLeft;
        if (goal.InitialAmount < 0)
        {
            // The amount for a debt is just the value of the debt.
            amountLeft = Math.Abs(totalBalance);
        }
        else
        {
            // The initial amount is the account balance at the time the goal was created.
            // If a user wishes to include the starting balance in the goal,
            // the initial amount will be set to zero.
            amountLeft = goal.Amount - goal.InitialAmount - totalBalance;
        }

        var numberOfMonthsLeftWithoutInterest = Math.Ceiling(
            amountLeft / (goal.MonthlyContribution ?? 0)
        );

        if (includeInterest)
        {
            var monthlyInterestRate = CalculateAverageInterestRate(goal) / 12;
            double numberOfMonthsLeftWithInterest = (double)numberOfMonthsLeftWithoutInterest;
            if (monthlyInterestRate > 0)
            {
                if (goal.Amount == 0)
                {
                    numberOfMonthsLeftWithInterest = Math.Ceiling(
                        Math.Log(
                            (double)(
                                ((goal.MonthlyContribution ?? 0) / monthlyInterestRate)
                                / (
                                    ((goal.MonthlyContribution ?? 0) / monthlyInterestRate)
                                    - amountLeft
                                )
                            )
                        ) / Math.Log(1 + (double)monthlyInterestRate)
                    );
                }
                else
                {
                    numberOfMonthsLeftWithInterest = Math.Ceiling(
                        Math.Log(
                            (double)(
                                (
                                    (goal.MonthlyContribution ?? 0)
                                    + monthlyInterestRate * goal.Amount
                                )
                                / (
                                    (goal.MonthlyContribution ?? 0)
                                    + monthlyInterestRate * totalBalance
                                )
                            )
                        ) / Math.Log(1 + (double)monthlyInterestRate)
                    );
                }
            }

            return new DateTime(_nowProvider.UtcNow.Year, _nowProvider.UtcNow.Month, 1).AddMonths(
                (int)numberOfMonthsLeftWithInterest
            );
        }

        return new DateTime(_nowProvider.UtcNow.Year, _nowProvider.UtcNow.Month, 1).AddMonths(
            (int)numberOfMonthsLeftWithoutInterest
        );
    }

    private decimal EstimateGoalMonthlyContribution(Goal goal, bool includeInterest = false)
    {
        if (goal.MonthlyContribution.HasValue)
            return goal.MonthlyContribution.Value;

        // If a monthly contribution has not been set, then a complete date is required.
        if (!goal.CompleteDate.HasValue)
        {
            _logger.LogError("A monthly contribution cannot be estimated without a target date.");
            return 0;
        }

        decimal totalBalance = goal.Accounts.Sum(a =>
            a.Balances.OrderByDescending(b => b.DateTime).FirstOrDefault()?.Amount ?? 0
        );
        decimal amountLeft;
        if (goal.Amount == 0)
        {
            // The amount for a debt is just the value of the debt.
            amountLeft = Math.Abs(totalBalance);
        }
        else
        {
            // The initial amount is the account balance at the time the goal was created.
            // If a user wishes to include the starting balance in the goal,
            // the initial amount will be set to zero.
            amountLeft = goal.Amount - (totalBalance - goal.InitialAmount);
        }

        var numberOfMonthsLeft =
            (goal.CompleteDate.Value.Year - _nowProvider.UtcNow.Year) * 12
            + (goal.CompleteDate.Value.Month - _nowProvider.UtcNow.Month);

        if (numberOfMonthsLeft <= 0)
        {
            // The goal is already past due, so you need to contribute the rest of the goal.
            return amountLeft;
        }

        var monthlyPaymentsWithoutInterest = amountLeft / numberOfMonthsLeft;

        // For now, we will just apply this to loans.
        if (includeInterest)
        {
            var monthlyInterestRate = CalculateAverageInterestRate(goal) / 12;
            decimal monthlyPaymentsWithInterest = monthlyPaymentsWithoutInterest;

            if (monthlyInterestRate > 0)
            {
                if (goal.Amount == 0)
                {
                    monthlyPaymentsWithInterest =
                        amountLeft
                        * (
                            monthlyInterestRate
                            * (decimal)Math.Pow(1 + (double)monthlyInterestRate, numberOfMonthsLeft)
                            / (
                                (
                                    (decimal)
                                        Math.Pow(
                                            1 + (double)monthlyInterestRate,
                                            numberOfMonthsLeft
                                        )
                                ) - 1
                            )
                        );
                }
                else
                {
                    monthlyPaymentsWithInterest =
                        (
                            monthlyInterestRate
                            * (
                                goal.Amount
                                - totalBalance
                                    * (decimal)
                                        Math.Pow(
                                            (double)(1 + monthlyInterestRate),
                                            numberOfMonthsLeft
                                        )
                            )
                        )
                        / (
                            (decimal)Math.Pow((double)(1 + monthlyInterestRate), numberOfMonthsLeft)
                            - 1
                        );
                }
            }

            return monthlyPaymentsWithInterest;
        }

        return monthlyPaymentsWithoutInterest;
    }

    private decimal GetGoalMonthlyContributionProgress(IEnumerable<Transaction> transactions)
    {
        if (transactions == null || !transactions.Any())
            return 0;

        var monthlyContribution = transactions
            .Where(t =>
                t.Date.Year == _nowProvider.UtcNow.Year && t.Date.Month == _nowProvider.UtcNow.Month
            )
            .Sum(t => t.Amount);

        return monthlyContribution;
    }

    private static decimal CalculatePercentComplete(Goal goal)
    {
        var accountsTotalBalance = goal.Accounts.Sum(a =>
            a.Balances.OrderByDescending(b => b.DateTime).FirstOrDefault()?.Amount ?? 0
        );

        decimal totalProgress = accountsTotalBalance - goal.InitialAmount;

        decimal adjustedAmount;

        // An initial amount less than zero indicates a debt.
        if (goal.InitialAmount < 0)
        {
            adjustedAmount = Math.Abs(goal.InitialAmount);
        }
        else
        {
            adjustedAmount = goal.Amount;
        }

        decimal percentComplete = (totalProgress / adjustedAmount) * 100.0M;

        return percentComplete > 100.0M ? 100.0M : percentComplete;
    }

    private static decimal CalculateAverageInterestRate(Goal goal)
    {
        var weightedInterestRates = new List<decimal>();
        var totalBalance = 0.0M;

        foreach (var account in goal.Accounts)
        {
            var balance = account.Balances.OrderByDescending(b => b.DateTime).FirstOrDefault();

            if (balance == null || balance.Amount == 0)
                continue;

            var interestRate = account.InterestRate ?? 0;
            var weightedInterestRate = interestRate * balance.Amount;

            if (weightedInterestRate == 0)
                continue;

            weightedInterestRates.Add(weightedInterestRate);
            totalBalance += balance.Amount;
        }

        return weightedInterestRates.Count > 0 ? weightedInterestRates.Sum() / totalBalance : 0;
    }
}
