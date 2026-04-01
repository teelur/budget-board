namespace BudgetBoard.Database.Models;

/// <summary>
/// Represents a SimpleFIN synced organization within the budgeting application.
/// </summary>
public class SimpleFinOrganization
{
    /// <summary>
    /// Unique identifier for the organization.
    /// </summary>
    public Guid ID { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Domain name of the financial institution.
    /// </summary>
    public string? Domain { get; set; } = null;

    /// <summary>
    /// Root URL of organization’s SimpleFIN Server.
    /// </summary>
    public string SimpleFinUrl { get; set; } = string.Empty;

    /// <summary>
    /// Human-friendly name of the financial institution.
    /// </summary>
    public string? Name { get; set; } = null;

    /// <summary>
    /// Optional URL of financial institution
    /// </summary>
    public string? Url { get; set; } = null;

    /// <summary>
    /// Optional ID for the financial institution.
    /// The ID must be unique per SimpleFIN server, but it is not guaranteed that IDs are globally unique.
    /// Prefer domain as a globally unique ID for each institution
    /// </summary>
    public string? SyncID { get; set; } = null;

    /// <summary>
    /// Collection of SimpleFIN accounts associated with the organization.
    /// </summary>
    public ICollection<SimpleFinAccount> Accounts { get; set; } = [];

    /// <summary>
    /// Identifier for the user who owns the account.
    /// </summary>
    public required Guid UserID { get; set; }

    /// <summary>
    /// Reference to the owning user.
    /// </summary>
    public ApplicationUser? User { get; set; } = null;
}
