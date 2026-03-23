namespace BudgetBoard.WebAPI.Services;

public interface IToshlFullSyncQueue
{
    Task QueueAsync(Guid userGuid);
}
