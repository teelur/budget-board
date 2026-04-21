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
            X = request.X ?? defaultLayout.X,
            Y = request.Y ?? defaultLayout.Y,
            W = request.W ?? defaultLayout.W,
            H = request.H ?? defaultLayout.H,
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
                        X = layout.X,
                        Y = layout.Y,
                        W = layout.W,
                        H = layout.H,
                    }
                );
            }
        }

        var widgetSettings = userData.WidgetSettings.Select(ws => new WidgetResponse
        {
            ID = ws.ID,
            WidgetType = ws.WidgetType,
            X = ws.X,
            Y = ws.Y,
            W = ws.W,
            H = ws.H,
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

        widget.X = request.X;
        widget.Y = request.Y;
        widget.W = request.W;
        widget.H = request.H;
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

            widget.X = req.X;
            widget.Y = req.Y;
            widget.W = req.W;
            widget.H = req.H;
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
