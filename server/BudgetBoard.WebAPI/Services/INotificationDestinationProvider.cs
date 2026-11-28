using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;
using BudgetBoard.WebAPI.Models;

namespace BudgetBoard.WebAPI.Services;

public interface INotificationDestinationProvider
{
    string Provider { get; }

    string BuildConfiguration(
        NotificationDestinationRequest request,
        string? existingConfiguration
    );

    NotificationProviderConfigurationDetails ReadConfiguration(string configuration);

    Task<bool> SendAsync(
        NotificationDestination destination,
        NotificationEvent notification,
        CancellationToken cancellationToken
    );
}
