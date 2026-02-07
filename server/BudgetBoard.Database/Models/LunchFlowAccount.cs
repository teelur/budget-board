namespace BudgetBoard.Database.Models;

/// <summary>
/// Represents a LunchFlow synced account within the budgeting application.
/// </summary>
public class LunchFlowAccount
{
    /// <summary>
    /// Unique identifier for the account.
    /// </summary>
    public Guid ID { get; set; } = Guid.NewGuid();

    /// <summary>
    /// A name that uniquely describes an account among all the user's other accounts.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The Sync ID provided by LunchFlow to identify the account.
    /// </summary>
    public string SyncID { get; set; } = string.Empty;

    /// <summary>
    /// Name of the financial institution associated with the account.
    /// </summary>
    public string InstitutionName { get; set; } = string.Empty;

    /// <summary>
    /// URL or data string for the institution's logo.
    /// </summary>
    public string InstitutionLogo { get; set; } = string.Empty;

    /// <summary>
    /// String that uniquely identifies the account within LunchFlow.
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// If the currency is a standard currency, this is the currency code from the official ISO 4217.
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// The status of the account.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// The balance of the account.
    /// </summary>
    public decimal Balance { get; set; } = 0;

    /// <summary>
    /// The timestamp when the balance became what it is.
    /// </summary>
    public int BalanceDate { get; set; } = (int)DateTimeOffset.UnixEpoch.ToUnixTimeSeconds();

    /// <summary>
    /// The date and time when the account was last synchronized; null if never synchronized.
    /// </summary>
    public DateTime? LastSync { get; set; } = null;

    /// <summary>
    /// Identifier for the linked local Account entity within the budgeting application.
    /// </summary>
    public Guid? LinkedAccountId { get; set; } = null;

    /// <summary>
    /// Optional link to the local Account entity within the budgeting application.
    /// </summary>
    public Account? LinkedAccount { get; set; } = null;

    /// <summary>
    /// Identifier for the user who owns the account.
    /// </summary>
    public required Guid UserID { get; set; }

    /// <summary>
    /// Reference to the owning user.
    /// </summary>
    public ApplicationUser? User { get; set; } = null;
}
