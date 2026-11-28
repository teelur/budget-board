using BudgetBoard.WebAPI.Models;

namespace BudgetBoard.WebAPI.Services;

public interface INotificationDestinationService
{
    Task<IReadOnlyList<NotificationDestinationResponse>> ReadAsync(Guid userId, CancellationToken cancellationToken);
    Task<NotificationDestinationDetailsResponse?> ReadAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<NotificationDestinationResponse> CreateAsync(Guid userId, NotificationDestinationRequest request, CancellationToken cancellationToken);
    Task<NotificationDestinationResponse?> UpdateAsync(Guid userId, Guid id, NotificationDestinationRequest request, CancellationToken cancellationToken);
    Task<NotificationDestinationResponse?> SetEnabledAsync(Guid userId, Guid id, bool enabled, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<bool?> TestAsync(Guid userId, Guid id, CancellationToken cancellationToken);
}
