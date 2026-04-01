namespace BudgetBoard.Database.Models;

/// <summary>
/// Represents a balance record for a financial account at a specific point in time.
/// </summary>
public class Balance
{
    /// <summary>
    /// Unique identifier for the balance record.
    /// </summary>
    public Guid ID { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The balance amount.
    /// </summary>
    public required decimal Amount { get; set; }

    /// <summary>
    /// The date and time the balance was recorded.
    /// </summary>
    public required DateTime DateTime { get; set; }

    /// <summary>
    /// The date and time when the balance record was deleted; null if active.
    /// </summary>
    public DateTime? Deleted { get; set; } = null;

    /// <summary>
    /// Identifier for the associated account.
    /// </summary>
    public required Guid AccountID { get; set; }

    /// <summary>
    /// Reference to the associated account.
    /// </summary>
    public Account? Account { get; set; } = null;
}
