namespace BudgetBoard.Database.Models;

/// <summary>
/// Records an authentication-management action without storing credentials or other secrets.
/// </summary>
public class AuthenticationAdminAuditEvent
{
    public long Id { get; set; }

    public Guid? ActorUserId { get; set; }

    public Guid? TargetUserId { get; set; }

    public string Action { get; set; } = string.Empty;

    public bool Succeeded { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string? RequestIpAddress { get; set; }

    public string? Details { get; set; }
}
