using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

public interface INotificationDispatcher
{
    Task DispatchAsync(NotificationEvent notification);
}
