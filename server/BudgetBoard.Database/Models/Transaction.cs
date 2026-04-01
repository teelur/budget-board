namespace BudgetBoard.Database.Models;

/// <summary>
/// Represents a financial transaction within an account.
/// </summary>
public class Transaction
{
    /// <summary>
    /// Unique identifier for the transaction.
    /// </summary>
    public Guid ID { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Optional external sync identifier for the transaction.
    /// </summary>
    public string? SyncID { get; set; } = null;

    /// <summary>
    /// The transaction amount.
    /// </summary>
    public required decimal Amount { get; set; }

    /// <summary>
    /// The date the transaction occurred.
    /// </summary>
    public required DateTime Date { get; set; }

    /// <summary>
    /// The category assigned to the transaction.
    /// </summary>
    public string? Category { get; set; } = null;

    /// <summary>
    /// The subcategory assigned to the transaction.
    /// </summary>
    public string? Subcategory { get; set; } = null;

    /// <summary>
    /// The merchant name associated with the transaction.
    /// </summary>
    public string? MerchantName { get; set; } = null;

    /// <summary>
    /// The date and time when the transaction was deleted; null if active.
    /// </summary>
    public DateTime? Deleted { get; set; } = null;

    /// <summary>
    /// Source of the transaction data (e.g., manual, imported).
    /// </summary>
    public required string Source { get; set; }

    /// <summary>
    /// Identifier for the account associated with the transaction.
    /// </summary>
    public required Guid AccountID { get; set; }

    /// <summary>
    /// Reference to the associated account.
    /// </summary>
    public Account? Account { get; set; } = null;
}
