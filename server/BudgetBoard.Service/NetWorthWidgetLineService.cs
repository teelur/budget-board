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
            Index = request.Index,
            Categories = [],
        };

        var group = configuration.Groups.FirstOrDefault(g => g.Index == request.Group);
        if (group == null)
        {
            group = new NetWorthWidgetGroup { Index = request.Group, Lines = [newLine] };
            configuration.Groups = [.. configuration.Groups, group];
        }
        else
        {
            group.Lines = [.. group.Lines, newLine];
        }

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

        NetWorthWidgetLine? line = null;
        NetWorthWidgetGroup? currentGroup = null;

        foreach (var group in configuration.Groups)
        {
            line = group.Lines.FirstOrDefault(l => l.ID == request.LineId);
            if (line != null)
            {
                currentGroup = group;
                break;
            }
        }

        if (line == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["NetWorthWidgetLineNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["NetWorthWidgetLineNotFoundError"]
            );
        }

        line.Name = request.Name;

        if (currentGroup!.Index != request.Group)
        {
            currentGroup.Lines = [.. currentGroup.Lines.Where(l => l.ID != request.LineId)];

            var targetGroup = configuration.Groups.FirstOrDefault(g => g.Index == request.Group);
            if (targetGroup == null)
            {
                targetGroup = new NetWorthWidgetGroup { Index = request.Group, Lines = [line] };
                configuration.Groups = [.. configuration.Groups, targetGroup];
            }
            else
            {
                targetGroup.Lines = [.. targetGroup.Lines, line];
            }

            configuration.Groups = [.. configuration.Groups.Where(g => g.Lines.Any())];
        }

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

        if (!configuration.Groups.SelectMany(g => g.Lines).Any(l => l.ID == lineId))
        {
            logger.LogError("{LogMessage}", logLocalizer["NetWorthWidgetLineNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["NetWorthWidgetLineNotFoundError"]
            );
        }

        foreach (var group in configuration.Groups)
        {
            group.Lines = [.. group.Lines.Where(l => l.ID != lineId)];
        }

        configuration.Groups = [.. configuration.Groups.Where(g => g.Lines.Any())];

        widgetSettings.Configuration = JsonSerializer.Serialize(configuration);
        await userDataContext.SaveChangesAsync();
    }

    public async Task ReorderNetWorthWidgetLinesAsync(
        Guid userGuid,
        INetWorthWidgetLineReorderRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);

        var widgetSettings = GetWidgetSettings(userData, request.WidgetSettingsId);
        var configuration = GetNetWorthWidgetConfiguration(widgetSettings);
        var group = configuration.Groups.FirstOrDefault(g => g.ID == request.GroupId);
        if (group == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["NetWorthWidgetGroupNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["NetWorthWidgetGroupNotFoundError"]
            );
        }

        var lineDict = group.Lines.ToDictionary(l => l.ID, l => l);
        var reorderedLines = new List<NetWorthWidgetLine>();
        int index = 0;
        foreach (var lineId in request.OrderedLineIds)
        {
            if (lineDict.TryGetValue(lineId, out var foundLine))
            {
                foundLine.Index = index++;
                reorderedLines.Add(foundLine);
            }
            else
            {
                logger.LogError("{LogMessage}", logLocalizer["NetWorthWidgetLineNotFoundLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["NetWorthWidgetLineNotFoundError"]
                );
            }
        }
        group.Lines = reorderedLines;
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
