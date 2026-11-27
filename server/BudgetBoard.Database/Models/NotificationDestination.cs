namespace BudgetBoard.Database.Models;

public static class NotificationDestinationProviders
{
    public const string Webhook = "webhook";
    public const string Ntfy = "ntfy";
    public const string Discord = "discord";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Webhook,
        Ntfy,
        Discord,
    };
}

/// <summary>
/// Represents a user's configured notification destination.
/// </summary>
public class NotificationDestination
{
    /// <summary>
    /// Unique identifier for the notification destination.
    /// </summary>
    public Guid ID { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identifier for the user who owns the destination.
    /// </summary>
    public required Guid UserID { get; set; }

    /// <summary>
    /// Reference to the owning user.
    /// </summary>
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// User-provided name for the destination.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Provider used to deliver notifications to this destination.
    /// </summary>
    public string Provider { get; set; } = NotificationDestinationProviders.Webhook;

    /// <summary>
    /// JSON-serialized provider configuration, including any destination credentials.
    /// </summary>
    public required string Configuration { get; set; }

    /// <summary>
    /// JSON-serialized event types subscribed to by this destination.
    /// </summary>
    public required string EventTypes { get; set; }

    /// <summary>
    /// Indicates whether financial details may be included in notifications.
    /// </summary>
    public bool IncludeFinancialDetails { get; set; }
}
