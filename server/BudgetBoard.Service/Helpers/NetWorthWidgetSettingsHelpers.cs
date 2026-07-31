using System.Text.Json;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Models.Widgets.NetWorthWidget;
using BudgetBoard.Service.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service.Helpers;

public static class NetWorthWidgetSettingsHelpers
{
    public static NetWorthWidgetConfiguration GetNetWorthWidgetConfiguration(
        WidgetSettings widgetSettings,
        ILogger logger,
        IStringLocalizer<LogStrings> logLocalizer,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        if (string.IsNullOrEmpty(widgetSettings.Configuration))
        {
            logger.LogError("{LogMessage}", logLocalizer["WidgetConfigurationNullLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["WidgetConfigurationNullError"]
            );
        }

        try
        {
            return JsonSerializer.Deserialize<NetWorthWidgetConfiguration>(
                    widgetSettings.Configuration
                ) ?? throw new JsonException();
        }
        catch (JsonException)
        {
            logger.LogError("{LogMessage}", logLocalizer["WidgetConfigurationDeserializationLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["WidgetConfigurationDeserializationError"]
            );
        }
    }

    public static WidgetSettings GetWidgetSettingsById(
        ApplicationUser userData,
        Guid guid,
        ILogger logger,
        IStringLocalizer<LogStrings> logLocalizer,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        var widgetSettings = userData.WidgetSettings.FirstOrDefault(ws => ws.ID == guid);
        if (widgetSettings == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["WidgetSettingsNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["WidgetSettingsNotFoundError"]);
        }
        return widgetSettings;
    }

    public static NetWorthWidgetGroup GetNetWorthWidgetGroupById(
        NetWorthWidgetConfiguration configuration,
        Guid groupId,
        ILogger logger,
        IStringLocalizer<LogStrings> logLocalizer,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        var group = configuration.Groups.FirstOrDefault(g => g.ID == groupId);
        if (group == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["NetWorthWidgetGroupNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["NetWorthWidgetGroupNotFoundError"]
            );
        }
        return group;
    }

    public static NetWorthWidgetLine GetNetWorthWidgetLineById(
        NetWorthWidgetConfiguration configuration,
        Guid lineId,
        ILogger logger,
        IStringLocalizer<LogStrings> logLocalizer,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        var line = configuration
            .Groups.SelectMany(g => g.Lines)
            .FirstOrDefault(l => l.ID == lineId);
        if (line == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["NetWorthWidgetLineNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["NetWorthWidgetLineNotFoundError"]
            );
        }
        return line;
    }

    public static NetWorthWidgetCategory GetNetWorthWidgetCategoryById(
        NetWorthWidgetLine line,
        Guid categoryId,
        ILogger logger,
        IStringLocalizer<LogStrings> logLocalizer,
        IStringLocalizer<ResponseStrings> responseLocalizer
    )
    {
        var category = line.Categories.FirstOrDefault(c => c.ID == categoryId);
        if (category == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["NetWorthWidgetCategoryNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["NetWorthWidgetCategoryNotFoundError"]
            );
        }
        return category;
    }
}
