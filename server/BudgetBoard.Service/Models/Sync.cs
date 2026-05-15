namespace BudgetBoard.Service.Models;

public record SyncError(string Source, string Message)
{
    public SyncError()
        : this(string.Empty, string.Empty) { }
}
