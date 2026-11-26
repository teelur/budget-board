namespace BudgetBoard.Service.Models;

public static class NotificationEventTypes
{
    public const string SyncFailed = "sync_failed";

    public const string ImportFailed = "import_failed";

    public const string GoalReached = "goal_reached";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        SyncFailed,
        ImportFailed,
        GoalReached,
    };
}

public sealed record NotificationEvent(
    Guid UserId,
    string Type,
    string Message,
    string? FinancialDetails = null
);
