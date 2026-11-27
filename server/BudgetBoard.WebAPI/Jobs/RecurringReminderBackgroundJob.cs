using System.Globalization;
using BudgetBoard.Database.Data;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using BudgetBoard.WebAPI.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Quartz;

namespace BudgetBoard.WebAPI.Jobs;

[DisallowConcurrentExecution]
public sealed class RecurringReminderBackgroundJob(
    UserDataContext userDataContext,
    IRecurringRuleService recurringRuleService,
    INotificationDispatcher notificationDispatcher,
    INowProvider nowProvider,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    ILogger<RecurringReminderBackgroundJob> logger
) : IJob
{
    private const int DefaultLeadTimeDays = 3;
    private const int MaxReminderDetails = 10;

    public async ValueTask Execute(
        IJobExecutionContext context,
        CancellationToken cancellationToken
    )
    {
        var userIds = await userDataContext.ApplicationUsers
            .AsNoTracking()
            .Select(user => user.Id)
            .ToListAsync(cancellationToken);

        foreach (var userId in userIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var leadTimeDays = await userDataContext.UserSettings
                    .AsNoTracking()
                    .Where(settings => settings.UserID == userId)
                    .Select(settings => (int?)settings.RecurringReminderLeadTimeDays)
                    .FirstOrDefaultAsync(cancellationToken)
                    ?? DefaultLeadTimeDays;
                var reminderDate = nowProvider.Today.AddDays(leadTimeDays);
                var month = new DateOnly(reminderDate.Year, reminderDate.Month, 1);
                var occurrences = (await recurringRuleService.ReadForecastAsync(userId, month))
                    .Where(occurrence => occurrence.Date == reminderDate)
                    .ToList();
                if (occurrences.Count == 0)
                {
                    continue;
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
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
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
