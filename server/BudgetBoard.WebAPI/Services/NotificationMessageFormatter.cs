using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Services;

internal static class NotificationMessageFormatter
{
    public static string GetTitle(
        NotificationEvent notification,
        IStringLocalizer<ResponseStrings> localizer
    ) =>
        notification.Type switch
        {
            NotificationEventTypes.SyncFailed => localizer["NotificationTitleSyncFailed"].Value,
            NotificationEventTypes.ImportFailed => localizer["NotificationTitleImportFailed"].Value,
            NotificationEventTypes.GoalReached => localizer["NotificationTitleGoalReached"].Value,
            NotificationEventTypes.BudgetWarning => localizer["NotificationTitleBudgetWarning"].Value,
            NotificationEventTypes.TransactionsAdded => localizer[
                "NotificationTitleTransactionsAdded"
            ].Value,
            NotificationEventTypes.LargeTransactionsAdded => localizer[
                "NotificationTitleLargeTransactionsAdded"
            ].Value,
            NotificationEventTypes.RecurringReminder => localizer[
                "NotificationTitleRecurringReminder"
            ].Value,
            _ => localizer["NotificationTitleTest"].Value,
        };

    public static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..(maxLength - 1)] + "…";

    public static string? GetFinancialDetails(
        NotificationEvent notification,
        bool includeFinancialDetails,
        int maxLength
    ) =>
        includeFinancialDetails && !string.IsNullOrWhiteSpace(notification.FinancialDetails)
            ? Truncate(notification.FinancialDetails, maxLength)
            : null;
}
