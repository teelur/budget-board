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

public class GoalService(
    ILogger<IGoalService> logger,
    UserDataContext userDataContext,
    INowProvider nowProvider,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : IGoalService
{
    private readonly ILogger<IGoalService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly INowProvider _nowProvider = nowProvider;
    private readonly IStringLocalizer<ResponseStrings> _responseLocalizer = responseLocalizer;
    private readonly IStringLocalizer<LogStrings> _logLocalizer = logLocalizer;

    /// <inheritdoc />
    public async Task CreateGoalAsync(Guid userGuid, IGoalCreateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        if (
            (request.MonthlyContribution == 0 || !request.MonthlyContribution.HasValue)
            && !request.CompleteDate.HasValue
        )
        {
            _logger.LogError(
                "{LogMessage}",
                _logLocalizer["GoalCreateMissingContributionOrDateLog"]
            );
            throw new BudgetBoardServiceException(
                _responseLocalizer["GoalCreateMissingContributionOrDateError"]
            );
        }

        if (request.CompleteDate.HasValue && request.CompleteDate.Value < _nowProvider.UtcNow)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["GoalCreatePastDateLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["GoalCreatePastDateError"]);
        }

        if (!request.AccountIds.Any())
        {
            _logger.LogError("{LogMessage}", _logLocalizer["GoalCreateNoAccountsLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["GoalCreateNoAccountsError"]);
        }

        decimal runningBalance = 0.0M;
        var accounts = new List<Account>();
        foreach (var accountId in request.AccountIds)
        {
            var account = userData.Accounts.FirstOrDefault((a) => a.ID == accountId);
            if (account == null)
            {
                _logger.LogError("{LogMessage}", _logLocalizer["GoalCreateInvalidAccountLog"]);
                throw new BudgetBoardServiceException(
                    _responseLocalizer["GoalCreateInvalidAccountError"]
                );
            }

            runningBalance +=
                account.Balances.OrderByDescending(b => b.DateTime).FirstOrDefault()?.Amount ?? 0;
            accounts.Add(account);
        }

        var newGoal = new Goal
        {
            Name = request.Name,
            CompleteDate = request.CompleteDate,
            Amount = request.Amount,
            InitialAmount = request.InitialAmount ?? runningBalance,
            MonthlyContribution = request.MonthlyContribution,
            Accounts = accounts,
            UserID = userData.Id,
        };

        _userDataContext.Goals.Add(newGoal);
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IGoalResponse>> ReadGoalsAsync(
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

    /// <inheritdoc />
    public async Task UpdateGoalAsync(Guid userGuid, IGoalUpdateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var goal = userData.Goals.FirstOrDefault(g => g.ID == request.ID);
        if (goal == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["GoalUpdateNotFoundLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["GoalUpdateNotFoundError"]);
        }

        if (
            request.CompleteDate.HasValue
            && request.IsCompleteDateEditable
            && request.CompleteDate.Value < _nowProvider.UtcNow
        )
        {
            _logger.LogError("{LogMessage}", _logLocalizer["GoalUpdatePastDateLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["GoalUpdatePastDateError"]);
        }

        if (((request.MonthlyContribution ?? -1) <= 0) && request.IsMonthlyContributionEditable)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["GoalUpdateNoMonthlyContributionLog"]);
            throw new BudgetBoardServiceException(
                _responseLocalizer["GoalUpdateNoMonthlyContributionError"]
            );
        }

        goal.Name = request.Name;
        goal.Amount = request.Amount;
        goal.CompleteDate = request.IsCompleteDateEditable
            ? request.CompleteDate
            : goal.CompleteDate;
        goal.MonthlyContribution = request.IsMonthlyContributionEditable
            ? request.MonthlyContribution
            : goal.MonthlyContribution;

        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteGoalAsync(Guid userGuid, Guid guid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var goal = userData.Goals.FirstOrDefault(g => g.ID == guid);
        if (goal == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["GoalDeleteNotFoundLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["GoalDeleteNotFoundError"]);
        }

        _userDataContext.Goals.Remove(goal);
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task CompleteGoalAsync(Guid userGuid, Guid goalID, DateTime completedDate)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var goal = userData.Goals.FirstOrDefault(g => g.ID == goalID);
        if (goal == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["GoalCompleteNotFoundLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["GoalCompleteNotFoundError"]);
        }

        if (goal.Completed.HasValue)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["GoalCompleteAlreadyCompletedLog"]);
            throw new BudgetBoardServiceException(
                _responseLocalizer["GoalCompleteAlreadyCompletedError"]
            );
        }

        goal.Completed = completedDate;
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task CompleteGoalsAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var goals = userData.Goals.ToList();
        foreach (var goal in goals)
        {
            // Skip goals that are already completed
            if (goal.Completed.HasValue)
                continue;

            var percentComplete = CalculatePercentComplete(goal);

            // Complete the goal if it's 100% complete
            if (percentComplete >= 100.0M)
            {
                goal.Completed = _nowProvider.UtcNow;
            }
        }

        await _userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await _userDataContext
                .ApplicationUsers.Include(u => u.Goals)
                .ThenInclude((g) => g.Accounts)
                .ThenInclude((a) => a.Transactions)
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Balances)
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "{LogMessage}",
                _logLocalizer["UserDataRetrievalErrorLog", ex.Message]
            );
            throw new BudgetBoardServiceException(_responseLocalizer["UserDataRetrievalError"]);
        }

        if (foundUser == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["InvalidUserErrorLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["InvalidUserError"]);
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
                                (goal.MonthlyContribution ?? 0)
                                / monthlyInterestRate
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

        if (adjustedAmount == 0)
        {
            return 0.0M;
        }

        decimal percentComplete = totalProgress / adjustedAmount * 100.0M;

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
