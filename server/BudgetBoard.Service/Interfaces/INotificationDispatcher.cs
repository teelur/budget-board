using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

public interface INotificationDispatcher
{
    Task<bool> HasEnabledDestinationAsync(Guid userId, string eventType);

    Task DispatchAsync(NotificationEvent notification);
}
