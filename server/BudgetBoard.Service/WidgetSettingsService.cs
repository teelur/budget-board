using System.Text.Json;
using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
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
    public async Task CreateWidgetSettingsAsync(
        Guid userGuid,
        IWidgetSettingsCreateRequest<NetWorthWidgetConfiguration> request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var newWidget = new WidgetSettings
        {
            WidgetType = request.WidgetType,
            IsVisible = request.IsVisible,
            Configuration =
                (request.Configuration) != null
                    ? JsonSerializer.Serialize(request.Configuration)
                    : null,
            UserID = userData.Id,
        };

        userDataContext.WidgetSettings.Add(newWidget);
        await userDataContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<IWidgetResponse>> ReadWidgetSettingsAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var widgetSettings = userData.WidgetSettings.Select(ws => new WidgetResponse
        {
            ID = ws.ID,
            WidgetType = ws.WidgetType,
            IsVisible = ws.IsVisible,
            Configuration =
                ws.Configuration
                ?? JsonSerializer.Serialize(
                    WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration
                ),
            UserID = ws.UserID,
        });

        // Until we add customizable dashboards, we will need to automatically create the widget settings.
        if (!widgetSettings.Any())
        {
            await this.CreateWidgetSettingsAsync(
                userGuid,
                new WidgetSettingsCreateRequest<NetWorthWidgetConfiguration>
                {
                    WidgetType = "NetWorth",
                    IsVisible = true,
                    Configuration = WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration,
                    UserID = userGuid,
                }
            );

            widgetSettings = userData.WidgetSettings.Select(ws => new WidgetResponse
            {
                ID = ws.ID,
                WidgetType = ws.WidgetType,
                IsVisible = ws.IsVisible,
                Configuration =
                    ws.Configuration
                    ?? JsonSerializer.Serialize(
                        WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration
                    ),
                UserID = ws.UserID,
            });
        }

        return widgetSettings;
    }

    public async Task UpdateWidgetSettingsAsync(
        Guid userGuid,
        IWidgetSettingsUpdateRequest<NetWorthWidgetConfiguration> request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var widget = userData.WidgetSettings.FirstOrDefault(ws => ws.ID == request.ID);
        if (widget == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["WidgetUpdateNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["WidgetUpdateNotFoundError"]);
        }

        widget.IsVisible = request.IsVisible;
        widget.Configuration =
            request.Configuration != null ? JsonSerializer.Serialize(request.Configuration) : null;

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
}
