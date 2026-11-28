using System.Globalization;
using BudgetBoard.Database.Data;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using BudgetBoard.WebAPI.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Services;

public sealed class RecurringReminderNotificationService(
    UserDataContext userDataContext,
    IRecurringRuleService recurringRuleService,
    INotificationDispatcher notificationDispatcher,
    INowProvider nowProvider,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    ILogger<RecurringReminderNotificationService> logger
) : INotificationReminderService
{
    private const int DefaultLeadTimeDays = 3;
    private const int MaxReminderDetails = 10;

    public async Task CheckAfterSyncAsync(Guid userId)
    {
        try
        {
            var today = nowProvider.Today;
            var leadTimeDays = await userDataContext.UserSettings
                .AsNoTracking()
                .Where(settings => settings.UserID == userId)
                .Select(settings => (int?)settings.RecurringReminderLeadTimeDays)
                .FirstOrDefaultAsync()
                ?? DefaultLeadTimeDays;
            var reminderDate = today.AddDays(leadTimeDays);
            var month = new DateOnly(reminderDate.Year, reminderDate.Month, 1);
            var occurrences = (await recurringRuleService.ReadForecastAsync(userId, month))
                .Where(occurrence => occurrence.Date == reminderDate)
                .ToList();
            if (occurrences.Count == 0)
            {
                return;
            }

            if (
                !await notificationDispatcher.HasEnabledDestinationAsync(
                    userId,
                    NotificationEventTypes.RecurringReminder
                )
            )
            {
                return;
            }

            var claimed = await userDataContext.ApplicationUsers
                .Where(user =>
                    user.Id == userId
                    && (
                        user.LastRecurringReminderDate == null
                        || user.LastRecurringReminderDate.Value < today
                    )
                )
                .ExecuteUpdateAsync(update =>
                    update.SetProperty(user => user.LastRecurringReminderDate, (DateOnly?)today)
                );
            if (claimed == 0)
            {
                return;
            }

            await notificationDispatcher.DispatchAsync(
                new NotificationEvent(
                    userId,
                    NotificationEventTypes.RecurringReminder,
                    responseLocalizer["NotificationRecurringReminder", occurrences.Count],
                    BuildDetails(occurrences)
                )
            );
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

    private string BuildDetails(
        IReadOnlyCollection<RecurringForecastOccurrenceResponse> occurrences
    )
    {
        var details = occurrences
            .Take(MaxReminderDetails)
            .Select(occurrence =>
                responseLocalizer[
                    "NotificationRecurringReminderDetail",
                    occurrence.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    occurrence.MerchantName ?? occurrence.Category ?? string.Empty,
                    occurrence.Amount.ToString("0.##", CultureInfo.InvariantCulture)
                ].Value
            )
            .ToList();

        if (occurrences.Count > MaxReminderDetails)
        {
            details.Add(
                responseLocalizer[
                    "NotificationMoreTransactions",
                    occurrences.Count - MaxReminderDetails
                ].Value
            );
        }

        return string.Join(Environment.NewLine, details);
    }
}
