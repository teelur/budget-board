namespace BudgetBoard.Database.Models;

/// <summary>
/// Records an authentication-management action without storing credentials or other secrets.
/// </summary>
public class AuthenticationAdminAuditEvent
{
    /// <summary>
    /// Unique identifier for the audit event.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Identifier for the user who initiated the action, if available.
    /// </summary>
    public Guid? ActorUserId { get; set; }

    /// <summary>
    /// Identifier for the user affected by the action, if applicable.
    /// </summary>
    public Guid? TargetUserId { get; set; }

    /// <summary>
    /// The authentication-management action.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Whether the action succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// UTC date and time when the audit event was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// IP address from which the action was requested, if available.
    /// </summary>
    public string? RequestIpAddress { get; set; }

    /// <summary>
    /// Additional non-sensitive details about the action, if available.
    /// </summary>
    public string? Details { get; set; }
}
