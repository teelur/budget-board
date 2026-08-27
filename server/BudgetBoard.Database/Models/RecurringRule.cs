namespace BudgetBoard.Database.Models;

/// <summary>
/// Defines how often a recurring rule repeats.
/// </summary>
public enum RecurringCadence
{
    /// <summary>
    /// Repeats every week.
    /// </summary>
    Weekly,

    /// <summary>
    /// Repeats every two weeks.
    /// </summary>
    Biweekly,

    /// <summary>
    /// Repeats every month.
    /// </summary>
    Monthly,

    /// <summary>
    /// Repeats every year.
    /// </summary>
    Yearly,
}

/// <summary>
/// Defines how the amount for a recurring rule is determined.
/// </summary>
public enum RecurringAmountMode
{
    /// <summary>
    /// Uses the configured fixed amount.
    /// </summary>
    Fixed,

    /// <summary>
    /// Determines the amount automatically from matching transactions.
    /// </summary>
    Automatic,
}

/// <summary>
/// Represents a rule for identifying recurring transactions.
/// </summary>
public class RecurringRule
{
    /// <summary>
    /// Unique identifier for the recurring rule.
    /// </summary>
    public Guid ID { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identifier for the user who owns the rule.
    /// </summary>
    public required Guid UserID { get; set; }

    /// <summary>
    /// Reference to the user who owns the rule.
    /// </summary>
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// Identifier for the account associated with the rule.
    /// </summary>
    public required Guid AccountID { get; set; }

    /// <summary>
    /// Reference to the account associated with the rule.
    /// </summary>
    public Account? Account { get; set; }

    /// <summary>
    /// Merchant name used to match transactions, if specified.
    /// </summary>
    public string? MerchantName { get; set; }

    /// <summary>
    /// Category used to match transactions, if specified.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Subcategory used to match transactions, if specified.
    /// </summary>
    public string? Subcategory { get; set; }

    /// <summary>
    /// Frequency at which the rule repeats.
    /// </summary>
    public RecurringCadence Cadence { get; set; } = RecurringCadence.Monthly;

    /// <summary>
    /// Date on which the recurring rule begins.
    /// </summary>
    public required DateOnly StartDate { get; set; }

    /// <summary>
    /// Date on which the recurring rule ends, if specified.
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Indicates whether the recurring rule is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Describes how the recurring amount is determined.
    /// </summary>
    public RecurringAmountMode AmountMode { get; set; } = RecurringAmountMode.Fixed;

    /// <summary>
    /// Fixed amount associated with the recurring rule.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Transactions identified by the recurring rule.
    /// </summary>
    public ICollection<Transaction> Transactions { get; set; } = [];
}
