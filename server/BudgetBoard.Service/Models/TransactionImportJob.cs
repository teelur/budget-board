namespace BudgetBoard.Service.Models;

public class TransactionImportJobResponse
{
    public Guid ID { get; init; }
    public string Status { get; init; } = string.Empty;
    public int TotalCount { get; init; }
    public int ProcessedCount { get; init; }
    public int SucceededCount { get; init; }
    public int FailedCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string? ErrorMessage { get; init; }
    public int ProgressPercentage =>
        TotalCount == 0 ? 100 : (int)Math.Round(ProcessedCount * 100d / TotalCount);
}
