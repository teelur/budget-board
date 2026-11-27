using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.WebAPI.Resources;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Services;

public sealed class NotificationSyncService(
    ISyncService syncService,
    INotificationDispatcher dispatcher,
    IStringLocalizer<ApiResponseStrings> localizer,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    ILogger<NotificationSyncService> logger,
    INotificationActivityService notificationActivityService
) : ISyncService
{
    public async Task<IReadOnlyList<SyncError>> SyncAsync(Guid userGuid)
    {
        await using var notificationScope = await notificationActivityService.BeginAsync(userGuid);
        IReadOnlyList<SyncError> errors;
        try
        {
            errors = await syncService.SyncAsync(userGuid);
        }
        catch (Exception)
        {
            await notificationScope.CompleteAsync();
            await NotifyFailureAsync(userGuid);
            throw;
        }

        await notificationScope.CompleteAsync();
        if (errors.Count > 0)
        {
            await NotifyFailureAsync(userGuid);
        }
        return errors;
    }

    private async Task NotifyFailureAsync(Guid userId)
    {
        try
        {
            await dispatcher.DispatchAsync(
                new NotificationEvent(
                    userId,
                    NotificationEventTypes.SyncFailed,
                    localizer["NotificationSyncFailure"]
                )
            );
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                "{LogMessage}",
                logLocalizer[
                    "NotificationSyncFailureDispatchFailedLog",
                    exception.GetType().Name
                ]
            );
        }
    }
}
