using System.Text.Json;
using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
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
                request.Configuration != null
                    ? JsonSerializer.Serialize(request.Configuration)
                    : null,
            UserID = userData.Id,
        };

        userDataContext.WidgetSettings.Add(newWidget);
        await userDataContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<IWidgetResponse>> ReadWidgetSettingsAsync(Guid userID)
    {
        var userData = await GetCurrentUserAsync(userID.ToString());

        return userData.WidgetSettings.Select(ws => new WidgetResponse
        {
            ID = ws.ID,
            WidgetType = ws.WidgetType,
            IsVisible = ws.IsVisible,
            Configuration = ws.Configuration ?? string.Empty,
            UserID = ws.UserID,
        });
    }

    public async Task UpdateWidgetSettingsAsync(
        Guid widgetID,
        IWidgetSettingsUpdateRequest<NetWorthWidgetConfiguration> request
    )
    {
        var widget = await userDataContext.WidgetSettings.FindAsync(widgetID);
        if (widget == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["WidgetUpdateNotFoundError"]);
            throw new BudgetBoardServiceException(responseLocalizer["WidgetUpdateNotFoundError"]);
        }

        widget.WidgetType = request.WidgetType;
        widget.IsVisible = request.IsVisible;
        widget.Configuration =
            request.Configuration != null ? JsonSerializer.Serialize(request.Configuration) : null;

        await userDataContext.SaveChangesAsync();
    }

    public async Task DeleteWidgetSettingsAsync(Guid widgetID)
    {
        var widget = await userDataContext.WidgetSettings.FindAsync(widgetID);
        if (widget == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["WidgetDeleteNotFoundError"]);
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
            logger.LogError("{LogMessage}", logLocalizer["UserDataRetrievalError", ex.Message]);
            throw new BudgetBoardServiceException(responseLocalizer["UserDataRetrievalError"]);
        }

        if (foundUser == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["InvalidUserError"]);
            throw new BudgetBoardServiceException(responseLocalizer["InvalidUserError"]);
        }

        return foundUser;
    }
}
