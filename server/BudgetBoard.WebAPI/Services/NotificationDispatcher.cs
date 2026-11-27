using System.Text.Json;
using BudgetBoard.Database.Data;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.WebAPI.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Services;

public sealed class NotificationDispatcher(
    UserDataContext context,
    IEnumerable<INotificationDestinationProvider> providers,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer,
    ILogger<NotificationDispatcher> logger
) : INotificationDispatcher
{
    private const int MaxConcurrentSends = 4;
    private const int BatchTimeoutSeconds = 2;
    private readonly IReadOnlyDictionary<string, INotificationDestinationProvider> providerMap =
        providers.ToDictionary(provider => provider.Provider, StringComparer.Ordinal);

    public async Task DispatchAsync(NotificationEvent notification)
    {
        try
        {
            var destinations = await context.NotificationDestinations.AsNoTracking()
                .Where(d => d.UserID == notification.UserId && d.Enabled)
                .ToListAsync();
            foreach (var batch in destinations.Chunk(MaxConcurrentSends))
            {
                using var timeout = new CancellationTokenSource(
                    TimeSpan.FromSeconds(BatchTimeoutSeconds)
                );
                await Task.WhenAll(batch.Select(async destination =>
                {
                    try
                    {
                        var events = JsonSerializer.Deserialize<string[]>(destination.EventTypes)
                            ?? throw new InvalidOperationException(
                                responseLocalizer["NotificationEventConfigurationInvalid"].Value
                            );
                        if (events.Contains(notification.Type))
                        {
                            await GetProvider(destination.Provider)
                                .SendAsync(destination, notification, timeout.Token);
                        }
                    }
                    catch (Exception exception)
                    {
                        logger.LogWarning(
                            "{LogMessage}",
                            logLocalizer[
                                "NotificationDestinationDispatchFailedLog",
                                destination.ID,
                                exception.GetType().Name
                            ]
                        );
                    }
                }));
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                "{LogMessage}",
                logLocalizer["NotificationDispatchFailedLog", exception.GetType().Name]
            );
        }
    }

    private INotificationDestinationProvider GetProvider(string provider)
    {
        if (providerMap.TryGetValue(provider, out var destinationProvider))
        {
            return destinationProvider;
        }

        throw new InvalidOperationException(
            responseLocalizer["NotificationProviderInvalid"].Value
        );
    }
}
