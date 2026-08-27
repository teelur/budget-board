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

public class RecurringRuleService(
    ILogger<IRecurringRuleService> logger,
    UserDataContext userDataContext,
    INowProvider nowProvider,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : IRecurringRuleService
{
    private const int MatchDateWindowDays = 5;
    private const decimal MatchAmountTolerance = 0.2M;
    private const string TransferCategory = "Transfer";

    public async Task<IReadOnlyList<IRecurringRuleResponse>> ReadRecurringRulesAsync(Guid userGuid)
    {
        var rules = await ReadUserRulesQuery(userGuid)
            .OrderBy(rule => rule.StartDate)
            .ThenBy(rule => rule.MerchantName)
            .ToListAsync();

        return
        [
            .. rules.Select(rule =>
                (IRecurringRuleResponse)new RecurringRuleResponse(rule, nowProvider.Today)
            ),
        ];
    }

    public async Task<IRecurringRuleResponse> CreateRecurringRuleAsync(
        Guid userGuid,
        IRecurringRuleRequest request,
        Guid? transactionID = null
    )
    {
        var account =
            await userDataContext.Accounts.FirstOrDefaultAsync(account =>
                account.ID == request.AccountID && account.UserID == userGuid
            ) ?? throw CreateServiceException("RecurringRuleAccountNotFoundError");
        ValidateRequest(request);

        Transaction? transaction = null;
        if (transactionID.HasValue)
        {
            transaction = await FindTransactionAsync(userGuid, transactionID.Value);
            if (transaction.AccountID != request.AccountID)
            {
                throw CreateServiceException("RecurringRuleAccountMismatchError");
            }
            if (transaction.RecurringRuleID.HasValue)
            {
                throw CreateServiceException("TransactionAlreadyRecurringError");
            }
        }

        var rule = new RecurringRule
        {
            UserID = userGuid,
            AccountID = request.AccountID,
            Account = account,
            MerchantName = request.MerchantName,
            Category = request.Category,
            Subcategory = request.Subcategory,
            Cadence = ParseCadence(request.Cadence),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            AmountMode = ParseAmountMode(request.AmountMode),
            Amount = request.Amount,
        };

        if (transaction is not null)
        {
            rule.Transactions.Add(transaction);
            transaction.RecurringRule = rule;
        }

        userDataContext.RecurringRules.Add(rule);
        await userDataContext.SaveChangesAsync();

        return new RecurringRuleResponse(rule, nowProvider.Today);
    }

    public async Task<IRecurringRuleResponse> UpdateRecurringRuleAsync(
        Guid userGuid,
        IRecurringRuleUpdateRequest request
    )
    {
        var rule = await ReadUserRulesQuery(userGuid).FirstOrDefaultAsync(r => r.ID == request.ID);
        if (rule is null)
        {
            throw CreateServiceException("RecurringRuleNotFoundError");
        }

        var account = await userDataContext.Accounts.FirstOrDefaultAsync(a =>
            a.ID == request.AccountID && a.UserID == userGuid
        );
        if (account is null)
        {
            throw CreateServiceException("RecurringRuleAccountNotFoundError");
        }

        ValidateRequest(request);

        rule.AccountID = request.AccountID;
        rule.Account = account;
        rule.MerchantName = request.MerchantName;
        rule.Category = request.Category;
        rule.Subcategory = request.Subcategory;
        rule.Cadence = ParseCadence(request.Cadence);
        rule.StartDate = request.StartDate;
        rule.EndDate = request.EndDate;
        rule.IsActive = request.IsActive;
        rule.AmountMode = ParseAmountMode(request.AmountMode);
        rule.Amount = request.Amount;

        await userDataContext.SaveChangesAsync();
        return new RecurringRuleResponse(rule, nowProvider.Today);
    }

    public async Task DeleteRecurringRuleAsync(Guid userGuid, Guid recurringRuleID)
    {
        var rule =
            await ReadUserRulesQuery(userGuid).FirstOrDefaultAsync(r => r.ID == recurringRuleID)
            ?? throw CreateServiceException("RecurringRuleNotFoundError");
        userDataContext.RecurringRules.Remove(rule);
        await userDataContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<RecurringForecastOccurrenceResponse>> ReadForecastAsync(
        Guid userGuid,
        DateOnly month
    )
    {
        var monthStart = new DateOnly(month.Year, month.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var today = nowProvider.Today;

        if (monthEnd < today)
        {
            return [];
        }

        var rangeStart =
            monthStart > today || monthStart.Month != today.Month || monthStart.Year != today.Year
                ? monthStart
                : today;

        var rules = await ReadUserRulesQuery(userGuid)
            .Where(rule =>
                rule.IsActive
                && rule.StartDate <= monthEnd
                && (rule.EndDate == null || rule.EndDate >= rangeStart)
                && rule.Account!.HideTransactions == false
                && !IsExcludedBudgetCategory(rule.Category)
            )
            .ToListAsync();

        var forecast = new List<RecurringForecastOccurrenceResponse>();
        foreach (var rule in rules)
        {
            var dates = RecurringRuleOccurrenceCalculator.GetOccurrences(
                rule,
                rangeStart,
                monthEnd
            );
            var amount = GetForecastAmount(rule);

            foreach (var date in dates)
            {
                if (HasMatchedOccurrence(rule, date))
                {
                    continue;
                }

                forecast.Add(
                    new RecurringForecastOccurrenceResponse
                    {
                        RuleID = rule.ID,
                        Date = date,
                        Amount = amount,
                        MerchantName = rule.MerchantName,
                        AccountID = rule.AccountID,
                        AccountName = rule.Account?.Name ?? string.Empty,
                        Category = rule.Category,
                        Subcategory = rule.Subcategory,
                    }
                );
            }
        }

        return
        [
            .. forecast
                .OrderBy(occurrence => occurrence.Date)
                .ThenBy(occurrence => occurrence.MerchantName),
        ];
    }

    public async Task MatchTransactionAsync(Guid userGuid, Transaction transaction)
    {
        if (
            transaction.RecurringRuleID.HasValue
            || transaction.Deleted.HasValue
            || IsExcludedBudgetCategory(transaction.Category)
            || transaction.SourceTransactionLink is not null
            || transaction.TargetTransactionLink is not null
        )
        {
            return;
        }

        var rules = await ReadUserRulesQuery(userGuid)
            .Where(rule =>
                rule.IsActive
                && rule.AccountID == transaction.AccountID
                && !IsExcludedBudgetCategory(rule.Category)
            )
            .ToListAsync();

        var matchingRules = rules.Where(rule => IsTransactionMatch(rule, transaction)).ToList();

        if (matchingRules.Count != 1)
        {
            if (matchingRules.Count > 1)
            {
                logger.LogWarning(
                    "{LogMessage}",
                    logLocalizer["RecurringRuleAmbiguousMatchLog", transaction.ID]
                );
            }
            return;
        }

        var matchingRule = matchingRules[0];
        transaction.RecurringRuleID = matchingRule.ID;
        transaction.RecurringRule = matchingRule;
    }

    public async Task AssignTransactionAsync(
        Guid userGuid,
        Guid recurringRuleID,
        Guid transactionID
    )
    {
        var rule = await ReadUserRulesQuery(userGuid)
            .FirstOrDefaultAsync(r => r.ID == recurringRuleID);
        if (rule is null)
        {
            throw CreateServiceException("RecurringRuleNotFoundError");
        }

        var transaction = await FindTransactionAsync(userGuid, transactionID);
        if (transaction.AccountID != rule.AccountID)
        {
            throw CreateServiceException("RecurringRuleAccountMismatchError");
        }
        if (transaction.RecurringRuleID.HasValue && transaction.RecurringRuleID != rule.ID)
        {
            throw CreateServiceException("TransactionAlreadyRecurringError");
        }

        transaction.RecurringRuleID = rule.ID;
        transaction.RecurringRule = rule;
        await userDataContext.SaveChangesAsync();
    }

    public async Task UnassignTransactionAsync(Guid userGuid, Guid transactionID)
    {
        var transaction = await FindTransactionAsync(userGuid, transactionID);
        transaction.RecurringRuleID = null;
        transaction.RecurringRule = null;
        await userDataContext.SaveChangesAsync();
    }

    private IQueryable<RecurringRule> ReadUserRulesQuery(Guid userGuid)
    {
        return userDataContext
            .RecurringRules.Where(rule => rule.UserID == userGuid)
            .Include(rule => rule.Account)
            .Include(rule => rule.Transactions);
    }

    private async Task<Transaction> FindTransactionAsync(Guid userGuid, Guid transactionID)
    {
        var transaction = await userDataContext
            .Transactions.Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.ID == transactionID && t.Account!.UserID == userGuid);

        return transaction ?? throw CreateServiceException("TransactionNotFoundError");
    }

    private static bool IsTransactionMatch(RecurringRule rule, Transaction transaction)
    {
        if (
            !AreEqual(rule.MerchantName, transaction.MerchantName)
            || !AreEqual(rule.Category, transaction.Category)
            || !AreEqual(rule.Subcategory, transaction.Subcategory)
            || !IsAmountWithinTolerance(GetForecastAmount(rule), transaction.Amount)
        )
        {
            return false;
        }

        var nearbyOccurrences = RecurringRuleOccurrenceCalculator.GetOccurrences(
            rule,
            transaction.Date.AddDays(-MatchDateWindowDays),
            transaction.Date.AddDays(MatchDateWindowDays)
        );

        return nearbyOccurrences.Any(occurrence =>
            Math.Abs(occurrence.DayNumber - transaction.Date.DayNumber) <= MatchDateWindowDays
            && !HasMatchedOccurrence(rule, occurrence, transaction.ID)
        );
    }

    private static bool HasMatchedOccurrence(
        RecurringRule rule,
        DateOnly occurrence,
        Guid? ignoredTransactionID = null
    )
    {
        return rule.Transactions.Any(transaction =>
            transaction.ID != ignoredTransactionID
            && transaction.Deleted is null
            && Math.Abs(transaction.Date.DayNumber - occurrence.DayNumber) <= MatchDateWindowDays
        );
    }

    private static decimal GetForecastAmount(RecurringRule rule)
    {
        if (rule.AmountMode == RecurringAmountMode.Fixed || rule.Transactions.Count < 2)
        {
            return rule.Amount;
        }

        var amounts = rule
            .Transactions.Where(transaction => transaction.Deleted is null)
            .Select(transaction => transaction.Amount)
            .OrderBy(amount => amount)
            .ToList();
        if (amounts.Count < 2)
        {
            return rule.Amount;
        }

        var middle = amounts.Count / 2;
        return amounts.Count % 2 == 0
            ? (amounts[middle - 1] + amounts[middle]) / 2
            : amounts[middle];
    }

    private static bool IsAmountWithinTolerance(decimal expected, decimal actual)
    {
        if (expected == 0)
        {
            return actual == 0;
        }

        return Math.Abs(actual - expected) <= Math.Abs(expected) * MatchAmountTolerance;
    }

    private static bool AreEqual(string? first, string? second) =>
        string.Equals(Normalize(first), Normalize(second), StringComparison.OrdinalIgnoreCase);

    private static string Normalize(string? value) =>
        new string((value ?? string.Empty).Where(char.IsLetterOrDigit).ToArray());

    private static bool IsExcludedBudgetCategory(string? category) =>
        string.Equals(category, TransferCategory, StringComparison.OrdinalIgnoreCase)
        || string.Equals(
            category,
            TransactionCategoriesConstants.HideFromBudgetsCategory,
            StringComparison.OrdinalIgnoreCase
        );

    private static RecurringCadence ParseCadence(string cadence)
    {
        if (Enum.TryParse<RecurringCadence>(cadence, true, out var parsed))
        {
            return parsed;
        }

        throw new BudgetBoardServiceException("Invalid recurring cadence.");
    }

    private static RecurringAmountMode ParseAmountMode(string amountMode)
    {
        if (Enum.TryParse<RecurringAmountMode>(amountMode, true, out var parsed))
        {
            return parsed;
        }

        throw new BudgetBoardServiceException("Invalid recurring amount mode.");
    }

    private static void ValidateRequest(IRecurringRuleRequest request)
    {
        ParseCadence(request.Cadence);
        ParseAmountMode(request.AmountMode);
        if (request.AccountID == Guid.Empty || request.StartDate == DateOnly.MinValue)
        {
            throw new BudgetBoardServiceException(
                "Recurring rule account and start date are required."
            );
        }
        if (request.EndDate.HasValue && request.EndDate.Value < request.StartDate)
        {
            throw new BudgetBoardServiceException(
                "Recurring rule end date must not precede its start date."
            );
        }
        if (request.Amount == 0)
        {
            throw new BudgetBoardServiceException("Recurring rule amount must not be zero.");
        }
    }

    private BudgetBoardServiceException CreateServiceException(string resourceKey) =>
        new(responseLocalizer[resourceKey]);
}
