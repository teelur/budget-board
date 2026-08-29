using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;

namespace BudgetBoard.Service.Models;

public static class RecurringAmountModeValues
{
    public const string Fixed = "Fixed";
    public const string Automatic = "Automatic";
}

public interface IRecurringRuleRequest
{
    Guid AccountID { get; }
    string? MerchantName { get; }
    string? Category { get; }
    string? Subcategory { get; }
    RecurringCadence Cadence { get; }
    DateOnly StartDate { get; }
    DateOnly? EndDate { get; }
    bool IsActive { get; }
    string AmountMode { get; }
    decimal Amount { get; }
}

public class RecurringRuleCreateRequest : IRecurringRuleRequest
{
    public Guid AccountID { get; set; }
    public string? MerchantName { get; set; }
    public string? Category { get; set; }
    public string? Subcategory { get; set; }
    public RecurringCadence Cadence { get; set; } =
        new()
        {
            Version = 1,
            Unit = RecurringCadenceUnitValues.Month,
            Interval = 1,
        };
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string AmountMode { get; set; } = RecurringAmountModeValues.Fixed;
    public decimal Amount { get; set; }
}

public interface IRecurringRuleUpdateRequest : IRecurringRuleRequest
{
    Guid ID { get; }
}

public class RecurringRuleUpdateRequest : RecurringRuleCreateRequest, IRecurringRuleUpdateRequest
{
    public Guid ID { get; set; }
}

public interface IRecurringRuleResponse
{
    Guid ID { get; }
    Guid AccountID { get; }
    string AccountName { get; }
    string? MerchantName { get; }
    string? Category { get; }
    string? Subcategory { get; }
    RecurringCadence Cadence { get; }
    DateOnly StartDate { get; }
    DateOnly? EndDate { get; }
    bool IsActive { get; }
    string AmountMode { get; }
    decimal Amount { get; }
    int MatchedTransactionCount { get; }
    DateOnly? NextOccurrenceDate { get; }
}

public class RecurringRuleResponse : IRecurringRuleResponse
{
    public Guid ID { get; set; }
    public Guid AccountID { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string? MerchantName { get; set; }
    public string? Category { get; set; }
    public string? Subcategory { get; set; }
    public RecurringCadence Cadence { get; set; } = new();
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; }
    public string AmountMode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int MatchedTransactionCount { get; set; }
    public DateOnly? NextOccurrenceDate { get; set; }

    public RecurringRuleResponse(RecurringRule rule, DateOnly today)
    {
        ID = rule.ID;
        AccountID = rule.AccountID;
        AccountName = rule.Account?.Name ?? string.Empty;
        MerchantName = rule.MerchantName;
        Category = rule.Category;
        Subcategory = rule.Subcategory;
        Cadence = RecurringCadenceSerializer.Deserialize(rule.Cadence);
        StartDate = rule.StartDate;
        EndDate = rule.EndDate;
        IsActive = rule.IsActive;
        AmountMode = rule.AmountMode.ToString();
        Amount = rule.Amount;
        MatchedTransactionCount = rule.Transactions.Count;
        NextOccurrenceDate = rule.IsActive
            ? RecurringRuleOccurrenceCalculator
                .GetOccurrences(rule, today, today.AddYears(1))
                .FirstOrDefault()
            : null;
    }
}

public class RecurringForecastOccurrenceResponse
{
    public Guid RuleID { get; set; }
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public string? MerchantName { get; set; }
    public Guid AccountID { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Subcategory { get; set; }
}
