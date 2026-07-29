using System.Text.Json;
using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Models.Widgets.NetWorthWidget;
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

        var widgetSettings = GetWidgetSettingsById(userData, request.WidgetSettingsId);
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

        var widgetSettings = GetWidgetSettingsById(userData, request.WidgetSettingsId);
        var configuration = GetNetWorthWidgetConfiguration(widgetSettings);
        var line = GetNetWorthWidgetLineById(request.LineId, configuration, out var group);

        line.Name = request.Name;

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

        var widgetSettings = GetWidgetSettingsById(userData, widgetSettingsId);
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

        var widgetSettings = GetWidgetSettingsById(userData, request.WidgetSettingsId);
        var configuration = GetNetWorthWidgetConfiguration(widgetSettings);
        var _ = GetNetWorthWidgetLineById(
            request.OrderedLineIds.First(),
            configuration,
            out var group
        );

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
        return await UserDataServiceHelper.GetCurrentUserAsync(
            userDataContext,
            logger,
            logLocalizer,
            responseLocalizer,
            guid,
            users => users.Include(u => u.WidgetSettings)
        );
    }

    private WidgetSettings GetWidgetSettingsById(ApplicationUser userData, Guid guid)
    {
        var widgetSettings = userData.WidgetSettings.FirstOrDefault(ws => ws.ID == guid);
        if (widgetSettings == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["WidgetSettingsNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["WidgetSettingsNotFoundError"]);
        }
        return widgetSettings;
    }

    private NetWorthWidgetLine GetNetWorthWidgetLineById(
        Guid netWorthWidgetLineId,
        NetWorthWidgetConfiguration configuration,
        out NetWorthWidgetGroup groupForLine
    )
    {
        NetWorthWidgetLine? line = null;
        NetWorthWidgetGroup? currentGroup = null;
        foreach (var group in configuration.Groups)
        {
            line = group.Lines.FirstOrDefault(l => l.ID == netWorthWidgetLineId);
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

        groupForLine = currentGroup!;
        return line;
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
}
