namespace BudgetBoard.Service.Models;

public static class NotificationEventTypes
{
    public const string SyncFailed = "sync_failed";

    public const string ImportFailed = "import_failed";

    public const string GoalReached = "goal_reached";

    public const string BudgetWarning = "budget_warning";

    public const string TransactionsAdded = "transactions_added";

    public const string LargeTransactionsAdded = "large_transactions_added";

    public const string RecurringReminder = "recurring_reminder";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        SyncFailed,
        ImportFailed,
        GoalReached,
        BudgetWarning,
        TransactionsAdded,
        LargeTransactionsAdded,
        RecurringReminder,
    };
}

public sealed record NotificationEvent(
    Guid UserId,
    string Type,
    string Message,
    string? FinancialDetails = null
);
