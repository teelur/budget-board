using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class NetWorthWidgetLineService(
    ILogger<INetWorthWidgetLineService> logger,
    UserDataContext userDataContext,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : INetWorthWidgetLineService
{
    public async Task CreateNetWorthWidgetLineAsync(
        Guid userGuid,
        INetWorthWidgetLineCreateRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);

        var widgetSettings = GetWidgetSettings(userData, request.WidgetSettingsId);
        var configuration = GetNetWorthWidgetConfiguration(widgetSettings);

        var newLine = new NetWorthWidgetLine
        {
            Name = request.Name,
            Group = request.Group,
            Index = request.Index,
            Categories = [],
        };

        var lines = configuration.Lines.ToList();
        lines.Add(newLine);
        configuration.Lines = lines;

        widgetSettings.Configuration = JsonSerializer.Serialize(configuration);
        await userDataContext.SaveChangesAsync();
    }

    public async Task UpdateNetWorthWidgetLineAsync(
        Guid userGuid,
        INetWorthWidgetLineUpdateRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);

        var widgetSettings = GetWidgetSettings(userData, request.WidgetSettingsId);
        var configuration = GetNetWorthWidgetConfiguration(widgetSettings);

        var line = configuration.Lines.FirstOrDefault(l => l.ID == request.LineId);
        if (line == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["NetWorthWidgetLineNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["NetWorthWidgetLineNotFoundError"]
            );
        }

        line.Name = request.Name;
        line.Group = request.Group;
        line.Index = request.Index;

        widgetSettings.Configuration = JsonSerializer.Serialize(configuration);
        await userDataContext.SaveChangesAsync();
    }

    public async Task DeleteNetWorthWidgetLineAsync(
        Guid userGuid,
        Guid widgetSettingsId,
        Guid lineId
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);

        var widgetSettings = GetWidgetSettings(userData, widgetSettingsId);
        var configuration = GetNetWorthWidgetConfiguration(widgetSettings);

        configuration.Lines = [.. configuration.Lines.Where(l => l.ID != lineId)];

        widgetSettings.Configuration = JsonSerializer.Serialize(configuration);
        await userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(Guid guid)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await userDataContext
                .ApplicationUsers.Include(u => u.WidgetSettings)
                .FirstOrDefaultAsync(u => u.Id == guid);
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

    private WidgetSettings GetWidgetSettings(ApplicationUser userData, Guid guid)
    {
        var widgetSettings = userData.WidgetSettings.FirstOrDefault(ws => ws.ID == guid);
        if (widgetSettings == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["WidgetSettingsNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["WidgetSettingsNotFoundError"]);
        }
        return widgetSettings;
    }

    private NetWorthWidgetConfiguration GetNetWorthWidgetConfiguration(
        WidgetSettings widgetSettings
    )
    {
        if (string.IsNullOrEmpty(widgetSettings.Configuration))
        {
            logger.LogError("{LogMessage}", logLocalizer["WidgetConfigurationNullLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["WidgetConfigurationNullError"]
            );
        }

        var configuration = JsonSerializer.Deserialize<NetWorthWidgetConfiguration>(
            widgetSettings.Configuration
        );
        if (configuration == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["WidgetConfigurationDeserializationLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["WidgetConfigurationDeserializationError"]
            );
        }
        return configuration;
    }
}
