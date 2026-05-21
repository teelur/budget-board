using System.Text.Json;
using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Models.Widgets.AccountsWidget;
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
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var defaultLayout = WidgetSettingsHelpers.GetDefaultWidgetLayout(request.WidgetType);

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
    }

    public async Task<IEnumerable<IWidgetResponse>> ReadWidgetSettingsAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        // Seed all default widgets when a user has none
        if (userData.WidgetSettings.Count == 0)
        {
            foreach (var layout in WidgetSettingsHelpers.DefaultLayouts)
            {
                await CreateWidgetSettingsAsync(
                    userGuid,
                    new WidgetSettingsCreateRequest
                    {
                        WidgetType = layout.WidgetType,
                        LgX = layout.LgX,
                        LgY = layout.LgY,
                        LgW = layout.LgW,
                        LgH = layout.LgH,
                        SmY = layout.SmY,
                        SmH = layout.SmH,
                    }
                );
            }
        }

        var widgetSettings = userData.WidgetSettings.Select(ws => new WidgetResponse
        {
            ID = ws.ID,
            WidgetType = ws.WidgetType,
            LgX = ws.LgX,
            LgY = ws.LgY,
            LgW = ws.LgW,
            LgH = ws.LgH,
            SmY = ws.SmY,
            SmH = ws.SmH,
            Configuration =
                ws.Configuration ?? GetDefaultConfiguration(ws.WidgetType) ?? string.Empty,
            UserID = ws.UserID,
        });

        return widgetSettings;
    }

    public async Task UpdateWidgetSettingsAsync(Guid userGuid, IWidgetSettingsUpdateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var widget = userData.WidgetSettings.FirstOrDefault(ws => ws.ID == request.ID);
        if (widget == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["WidgetUpdateNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["WidgetUpdateNotFoundError"]);
        }

        widget.LgX = request.LgX ?? widget.LgX;
        widget.LgY = request.LgY ?? widget.LgY;
        widget.LgW = request.LgW ?? widget.LgW;
        widget.LgH = request.LgH ?? widget.LgH;
        widget.SmY = request.SmY ?? widget.SmY;
        widget.SmH = request.SmH ?? widget.SmH;
        widget.Configuration = request.Configuration.HasValue
            ? ValidateAndSerializeConfiguration(widget.WidgetType, request.Configuration.Value)
            : widget.Configuration;

        await userDataContext.SaveChangesAsync();
    }

    public async Task BatchUpdateWidgetSettingsAsync(
        Guid userGuid,
        IEnumerable<IWidgetSettingsBatchUpdateRequest> requests
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        foreach (var req in requests)
        {
            var widget = userData.WidgetSettings.FirstOrDefault(ws => ws.ID == req.ID);
            if (widget == null)
            {
                logger.LogError("{LogMessage}", logLocalizer["WidgetUpdateNotFoundLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["WidgetUpdateNotFoundError"]
                );
            }

            widget.LgX = req.LgX ?? widget.LgX;
            widget.LgY = req.LgY ?? widget.LgY;
            widget.LgW = req.LgW ?? widget.LgW;
            widget.LgH = req.LgH ?? widget.LgH;
            widget.SmY = req.SmY ?? widget.SmY;
            widget.SmH = req.SmH ?? widget.SmH;
        }

        await userDataContext.SaveChangesAsync();
    }

    public async Task DeleteWidgetSettingsAsync(Guid userGuid, Guid widgetGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var widget = userData.WidgetSettings.FirstOrDefault(ws => ws.ID == widgetGuid);
        if (widget == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["WidgetDeleteNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["WidgetDeleteNotFoundError"]);
        }

        userDataContext.WidgetSettings.Remove(widget);
        await userDataContext.SaveChangesAsync();
    }

    public async Task ResetWidgetSettingsConfiguration(Guid userGuid, Guid widgetGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var widget = userData.WidgetSettings.FirstOrDefault(ws => ws.ID == widgetGuid);
        if (widget == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["WidgetDeleteNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["WidgetDeleteNotFoundError"]);
        }

        widget.Configuration = GetDefaultConfiguration(widget.WidgetType);
        await userDataContext.SaveChangesAsync();
    }

    public async Task ResetSmallScreenToLargeScreenLayout(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var widgetsGroupedByY = userData.WidgetSettings.GroupBy(ws => ws.LgY).OrderBy(g => g.Key);

        var iterator = 0;
        foreach (var widgetGroup in widgetsGroupedByY)
        {
            foreach (var widget in widgetGroup)
            {
                widget.SmY = iterator++;
                widget.SmH = widget.LgH;
            }
        }

        await userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await userDataContext
                .ApplicationUsers.Include(u => u.WidgetSettings)
                .FirstOrDefaultAsync(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            logger.LogError("{LogMessage}", logLocalizer["UserDataRetrievalLog", ex.Message]);
            throw new BudgetBoardServiceException(responseLocalizer["UserDataRetrievalError"]);
        }

        if (foundUser == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["InvalidUserLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["InvalidUserError"]);
        }

        return foundUser;
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
            WidgetTypes.NetWorth => JsonSerializer.Serialize(
                WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration
            ),
            _ => null,
        };
    }
}
