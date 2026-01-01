namespace BudgetBoard.Database.Models;

/// <summary>
/// Represents a SimpleFIN synced account within the budgeting application.
/// </summary>
public class SimpleFinAccount
{
    /// <summary>
    /// Unique identifier for the account.
    /// </summary>
    public Guid ID { get; set; } = Guid.NewGuid();

    /// <summary>
    /// String that uniquely identifies the account within the organization.
    /// It is recommended that this id be chosen such that it does not reveal any sensitive data
    /// (login information for the bank’s web banking portal, for instance).
    /// </summary>
    public string SyncID { get; set; } = string.Empty;

    /// <summary>
    /// A name that uniquely describes an account among all the users other accounts.
    /// This name should be chosen so that a person can easily associate it with only one of their accounts within an organization.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// If the currency is a standard currency, this is the currency code from the official ISO 4217.
    /// For example "ZMW" or "USD".
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// The balance of the account as of balance-date.
    /// </summary>
    public int Balance { get; set; } = 0;

    /// <summary>
    /// The timestamp when the balance and available-balance became what they are.
    /// </summary>
    public int BalanceDate { get; set; } = (int)DateTimeOffset.UnixEpoch.ToUnixTimeSeconds();

    /// <summary>
    /// The date and time when the account was last synchronized; null if never synchronized.
    /// </summary>
    public DateTime? LastSync { get; set; } = null;

    /// <summary>
    /// Identifier for the associated SimpleFIN Organization, if any.
    /// </summary>
    public Guid? OrganizationId { get; set; } = null;

    /// <summary>
    /// Reference to the associated SimpleFIN Organization.
    /// </summary>
    public SimpleFinOrganization? Organization { get; set; } = null;

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
