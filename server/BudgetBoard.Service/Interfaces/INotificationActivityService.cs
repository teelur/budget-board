using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Interfaces;

public interface INotificationActivityScope : IAsyncDisposable
{
    Task CompleteAsync();
}

public interface INotificationActivityService
{
    Task<INotificationActivityScope> BeginAsync(Guid userId);

    Task TrackBudgetMonthAsync(Guid userId, DateOnly transactionDate);

    Task TrackTransactionCreatedAsync(Guid userId, Transaction transaction);
}
