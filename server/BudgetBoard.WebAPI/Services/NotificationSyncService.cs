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
    INotificationActivityService notificationActivityService,
    INotificationReminderService notificationReminderService
) : ISyncService
{
    private const int MaxSyncErrorDetailsLength = 2000;

    public async Task<IReadOnlyList<SyncError>> SyncAsync(Guid userGuid)
    {
        await using var notificationScope = await notificationActivityService.BeginAsync(userGuid);
        IReadOnlyList<SyncError> errors;
        try
        {
            errors = await syncService.SyncAsync(userGuid);
        }
        catch (Exception exception)
        {
            await notificationScope.CompleteAsync();
            var details = exception is BudgetBoardServiceException
                ? exception.Message
                : localizer["NotificationUnexpectedSyncError", exception.GetType().Name].Value;
            await NotifyFailureAsync(userGuid, details: details);
            await CheckRecurringRemindersAsync(userGuid);
            throw;
        }

        await notificationScope.CompleteAsync();
        if (errors.Count > 0)
        {
            await NotifyFailureAsync(
                userGuid,
                localizer["NotificationSyncFailureWithErrorCount", errors.Count].Value,
                FormatSyncErrors(errors)
            );
        }
        await CheckRecurringRemindersAsync(userGuid);
        return errors;
    }

    private async Task CheckRecurringRemindersAsync(Guid userId)
    {
        try
        {
            await notificationReminderService.CheckAfterSyncAsync(userId);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                "{LogMessage}",
                logLocalizer[
                    "NotificationRecurringReminderFailedLog",
                    exception.GetType().Name
                ]
            );
        }
    }

    private static string? FormatSyncErrors(IReadOnlyList<SyncError> errors)
    {
        var details = string.Join(
            Environment.NewLine,
            errors
                .Where(error => !string.IsNullOrWhiteSpace(error.Message))
                .Select(error =>
                    string.IsNullOrWhiteSpace(error.Source)
                        ? error.Message
                        : $"{error.Source}: {error.Message}"
                )
        );
        return string.IsNullOrWhiteSpace(details)
            ? null
            : NotificationMessageFormatter.Truncate(details, MaxSyncErrorDetailsLength);
    }

    private async Task NotifyFailureAsync(Guid userId, string? message = null, string? details = null)
    {
        try
        {
            await dispatcher.DispatchAsync(
                new NotificationEvent(
                    userId,
                    NotificationEventTypes.SyncFailed,
                    message ?? localizer["NotificationSyncFailure"].Value,
                    string.IsNullOrWhiteSpace(details)
                        ? null
                        : NotificationMessageFormatter.Truncate(
                            details,
                            MaxSyncErrorDetailsLength
                        )
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
