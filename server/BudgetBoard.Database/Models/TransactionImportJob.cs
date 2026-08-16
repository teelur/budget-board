namespace BudgetBoard.Database.Models;

public static class TransactionImportJobStatuses
{
    public const string Pending = "Pending";
    public const string Running = "Running";
    public const string Completed = "Completed";
    public const string CompletedWithErrors = "CompletedWithErrors";
    public const string Failed = "Failed";
}

public class TransactionImportJob
{
    public Guid ID { get; set; } = Guid.NewGuid();

    public required Guid UserID { get; set; }

    public ApplicationUser? User { get; set; }

    public required string Status { get; set; } = TransactionImportJobStatuses.Pending;

    public required string Payload { get; set; }

    public string? IdempotencyKey { get; set; }

    public int TotalCount { get; set; }

    public int ProcessedCount { get; set; }

    public int SucceededCount { get; set; }

    public int FailedCount { get; set; }

    public int AttemptCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? LastHeartbeatAt { get; set; }

    public DateTime? LeaseExpiresAt { get; set; }

    public string? ErrorMessage { get; set; }
}
