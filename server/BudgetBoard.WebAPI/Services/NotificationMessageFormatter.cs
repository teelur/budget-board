using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Services;

internal static class NotificationMessageFormatter
{
    public static string GetTitle(IStringLocalizer<ResponseStrings> localizer) =>
        localizer["NotificationTitle"].Value;

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
