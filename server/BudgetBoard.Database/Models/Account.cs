namespace BudgetBoard.Database.Models;

/// <summary>
/// Represents a financial account within the budgeting application.
/// </summary>
public class Account
{
    /// <summary>
    /// Unique identifier for the account.
    /// </summary>
    public Guid ID { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Name of the account.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Identifier for the associated financial institution, if any.
    /// </summary>
    public Guid? InstitutionID { get; set; } = null;

    /// <summary>
    /// Reference to the associated financial institution.
    /// </summary>
    public Institution? Institution { get; set; } = null;

    /// <summary>
    /// Type of the account (e.g., checking, savings).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Subtype of the account (e.g., brokerage, retirement).
    /// </summary>
    public string Subtype { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether transactions for this account are hidden from views.
    /// </summary>
    public bool HideTransactions { get; set; } = false;

    /// <summary>
    /// Indicates whether the account itself is hidden from views.
    /// </summary>
    public bool HideAccount { get; set; } = false;

    /// <summary>
    /// The date and time when the account was deleted; null if active.
    /// </summary>
    public DateTime? Deleted { get; set; } = null;

    /// <summary>
    /// Optional ordering index for the account.
    /// </summary>
    public int Index { get; set; } = 0;

    /// <summary>
    /// Interest rate for the account, if applicable.
    /// </summary>
    public decimal? InterestRate { get; set; } = null;

    /// <summary>
    /// Source of the account data (e.g., manual, simplefin).
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Collection of transactions associated with the account.
    /// </summary>
    public ICollection<Transaction> Transactions { get; set; } = [];

    /// <summary>
    /// Collection of financial goals linked to the account.
    /// </summary>
    public ICollection<Goal> Goals { get; set; } = [];

    /// <summary>
    /// Collection of balance records for the account.
    /// </summary>
    public ICollection<Balance> Balances { get; set; } = [];

    /// <summary>
    /// Reference to the associated SimpleFIN account, if any.
    /// </summary>
    public SimpleFinAccount? SimpleFinAccount { get; set; } = null;

    /// <summary>
    /// Identifier for the user who owns the account.
    /// </summary>
    public required Guid UserID { get; set; }

    /// <summary>
    /// Reference to the owning user.
    /// </summary>
    public ApplicationUser? User { get; set; } = null;
}
