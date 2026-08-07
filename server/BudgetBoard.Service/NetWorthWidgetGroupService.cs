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

public class NetWorthWidgetGroupService(
    ILogger<INetWorthWidgetGroupService> logger,
    UserDataContext userDataContext,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : INetWorthWidgetGroupService
{
    public async Task CreateNetWorthWidgetGroupAsync(
        Guid userGuid,
        INetWorthWidgetGroupCreateRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var widgetSettings = NetWorthWidgetSettingsHelpers.GetWidgetSettingsById(
            userData,
            request.WidgetSettingsId,
            logger,
            logLocalizer,
            responseLocalizer
        );
        using var configurationLock = await NetWorthWidgetConfigurationLock.AcquireLockAsync(
            request.WidgetSettingsId
        );
        var configuration = NetWorthWidgetSettingsHelpers.GetNetWorthWidgetConfiguration(
            widgetSettings,
            logger,
            logLocalizer,
            responseLocalizer
        );

        var newGroupIndex = configuration.Groups.Any() ? configuration.Groups.Max(g => g.Index) + 1 : 0;
        var newGroup = new NetWorthWidgetGroup { Index = newGroupIndex, Lines = [] };

        configuration.Groups = [.. configuration.Groups, newGroup];
        widgetSettings.Configuration = JsonSerializer.Serialize(configuration);
        await userDataContext.SaveChangesAsync();
    }

    public async Task ReorderNetWorthWidgetGroupsAsync(
        Guid userGuid,
        INetWorthWidgetGroupReorderRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var widgetSettings = NetWorthWidgetSettingsHelpers.GetWidgetSettingsById(
            userData,
            request.WidgetSettingsId,
            logger,
            logLocalizer,
            responseLocalizer
        );
        using var configurationLock = await NetWorthWidgetConfigurationLock.AcquireLockAsync(
            request.WidgetSettingsId
        );
        var configuration = NetWorthWidgetSettingsHelpers.GetNetWorthWidgetConfiguration(
            widgetSettings,
            logger,
            logLocalizer,
            responseLocalizer
        );

        var groupDict = configuration.Groups.ToDictionary(g => g.ID, g => g);
        var reorderedGroups = new List<NetWorthWidgetGroup>();
        int index = 0;
        foreach (var groupId in request.OrderedGroupIds)
        {
            if (groupDict.TryGetValue(groupId, out var foundGroup))
            {
                foundGroup.Index = index++;
                reorderedGroups.Add(foundGroup);
            }
            else
            {
                logger.LogError("{LogMessage}", logLocalizer["NetWorthWidgetGroupNotFoundLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["NetWorthWidgetGroupNotFoundError"]
                );
            }
        }
        configuration.Groups = reorderedGroups;
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
}
