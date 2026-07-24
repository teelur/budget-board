using System.Text.Json;
using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Models.Widgets.AccountsWidget;
using BudgetBoard.Service.Models.Widgets.MetricWidget;
using BudgetBoard.Service.Models.Widgets.NetWorthWidget;
using BudgetBoard.Service.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class WidgetSettingsService(
    ILogger<IWidgetSettingsService> logger,
    UserDataContext userDataContext,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : IWidgetSettingsService
{
    public async Task CreateWidgetSettingsAsync(Guid userGuid, IWidgetSettingsCreateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid);

        var defaultLayout = GetDefaultWidgetLayout(request.WidgetType);
        var newWidget = new WidgetSettings
        {
            WidgetType = request.WidgetType,
            LgX = request.LgX ?? defaultLayout.LgX,
            LgY = request.LgY ?? defaultLayout.LgY,
            LgW = request.LgW ?? defaultLayout.LgW,
            LgH = request.LgH ?? defaultLayout.LgH,
            SmY = request.SmY ?? defaultLayout.SmY,
            SmH = request.SmH ?? defaultLayout.SmH,
            Configuration = GetDefaultConfiguration(request.WidgetType),
            UserID = userData.Id,
        };

        userDataContext.WidgetSettings.Add(newWidget);
        await userDataContext.SaveChangesAsync();

        static DefaultWidgetLayout GetDefaultWidgetLayout(string widgetType) =>
            WidgetSettingsHelpers.DefaultLayouts.FirstOrDefault(dl => dl.WidgetType == widgetType)
            ?? WidgetSettingsHelpers.GenericDefaultLayout;
    }

    public async Task<IEnumerable<IWidgetResponse>> ReadWidgetSettingsAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid);

        return userData.WidgetSettings.Select(ws => new WidgetResponse
        {
            ID = ws.ID,
            WidgetType = ws.WidgetType,
            LgX = ws.LgX,
            LgY = ws.LgY,
            LgW = ws.LgW,
            LgH = ws.LgH,
            SmY = ws.SmY,
            SmH = ws.SmH,
            Configuration = string.IsNullOrEmpty(ws.Configuration)
                ? GetDefaultConfiguration(ws.WidgetType) ?? string.Empty
                : ws.Configuration,
            UserID = ws.UserID,
        });
    }

    public async Task UpdateWidgetSettingsAsync(
        Guid userGuid,
        IEnumerable<IWidgetSettingsUpdateRequest> requests
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);

        foreach (var request in requests)
        {
            var widget = GetWidgetSettingsById(userData, request.ID);

            if (request.LgX.HasValue)
            {
                widget.LgX = request.LgX.Value;
            }
            if (request.LgY.HasValue)
            {
                widget.LgY = request.LgY.Value;
            }
            if (request.LgW.HasValue)
            {
                widget.LgW = request.LgW.Value;
            }
            if (request.LgH.HasValue)
            {
                widget.LgH = request.LgH.Value;
            }
            if (request.SmY.HasValue)
            {
                widget.SmY = request.SmY.Value;
            }
            if (request.SmH.HasValue)
            {
                widget.SmH = request.SmH.Value;
            }
            if (request.Configuration.IsSpecified)
            {
                widget.Configuration = request.Configuration.Value.HasValue
                    ? ValidateAndSerializeConfiguration(
                        widget.WidgetType,
                        request.Configuration.Value.Value
                    )
                    : GetDefaultConfiguration(widget.WidgetType);
            }
        }

        await userDataContext.SaveChangesAsync();
    }

    public async Task DeleteWidgetSettingsAsync(Guid userGuid, Guid widgetGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var widget = GetWidgetSettingsById(userData, widgetGuid);

        userDataContext.WidgetSettings.Remove(widget);
        await userDataContext.SaveChangesAsync();
    }

    public async Task ResetSmallScreenToLargeScreenLayoutAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid);

        var widgetsInSmallScreenOrder = userData
            .WidgetSettings.OrderBy(ws => ws.LgY)
            .ThenBy(ws => ws.LgX);

        var currentY = 0;
        foreach (var widget in widgetsInSmallScreenOrder)
        {
            widget.SmY = currentY;
            widget.SmH = widget.LgH;
            currentY += widget.SmH;
        }

        await userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(Guid id)
    {
        return await UserDataServiceHelper.GetCurrentUserAsync(
            userDataContext,
            logger,
            logLocalizer,
            responseLocalizer,
            id,
            users => users.Include(u => u.WidgetSettings)
        );
    }

    private WidgetSettings GetWidgetSettingsById(ApplicationUser userData, Guid widgetId)
    {
        var widget = userData.WidgetSettings.FirstOrDefault(ws => ws.ID == widgetId);
        if (widget == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["WidgetSettingsNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["WidgetSettingsNotFoundError"]);
        }

        return widget;
    }

    private string ValidateAndSerializeConfiguration(string widgetType, JsonElement configuration)
    {
        try
        {
            return widgetType switch
            {
                WidgetTypes.NetWorth => JsonSerializer.Serialize(
                    JsonSerializer.Deserialize<NetWorthWidgetConfiguration>(configuration)
                        ?? throw new JsonException()
                ),
                WidgetTypes.Accounts => JsonSerializer.Serialize(
                    JsonSerializer.Deserialize<AccountsWidgetConfiguration>(configuration)
                        ?? throw new JsonException()
                ),
                WidgetTypes.Metric => JsonSerializer.Serialize(
                    JsonSerializer.Deserialize<MetricWidgetConfiguration>(configuration)
                        ?? throw new JsonException()
                ),
                _ => configuration.GetRawText(),
            };
        }
        catch (JsonException)
        {
            logger.LogError("{LogMessage}", logLocalizer["WidgetConfigurationDeserializationLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["WidgetConfigurationDeserializationError"]
            );
        }
    }

    private static string? GetDefaultConfiguration(string widgetType)
    {
        return widgetType switch
        {
            WidgetTypes.Accounts => JsonSerializer.Serialize(new { accountIds = new List<Guid>() }),
            WidgetTypes.NetWorth => JsonSerializer.Serialize(
                WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration
            ),
            WidgetTypes.Metric => JsonSerializer.Serialize(
                WidgetSettingsHelpers.DefaultMetricWidgetConfiguration
            ),
            _ => null,
        };
    }
}
