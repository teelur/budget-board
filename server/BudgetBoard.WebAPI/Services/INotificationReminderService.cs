namespace BudgetBoard.WebAPI.Services;

public interface INotificationReminderService
{
    Task CheckAfterSyncAsync(Guid userId);
}
