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
    NotificationWebhookSender sender,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer,
    ILogger<NotificationDispatcher> logger
) : INotificationDispatcher
{
    public async Task DispatchAsync(NotificationEvent notification)
    {
        try
        {
            var destinations = await context.NotificationDestinations.AsNoTracking()
                .Where(d => d.UserID == notification.UserId)
                .ToListAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await Task.WhenAll(destinations.Select(async destination =>
            {
                try
                {
                    var events = JsonSerializer.Deserialize<string[]>(destination.EventTypes)
                        ?? throw new InvalidOperationException(
                            responseLocalizer["NotificationEventConfigurationInvalid"].Value
                        );
                    if (events.Contains(notification.Type))
                    {
                        await sender.SendAsync(destination, notification, timeout.Token);
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
        catch (Exception exception)
        {
            logger.LogWarning(
                "{LogMessage}",
                logLocalizer["NotificationDispatchFailedLog", exception.GetType().Name]
            );
        }
    }
}
