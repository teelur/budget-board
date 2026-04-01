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

public class NetWorthWidgetGroupService(
    ILogger<INetWorthWidgetGroupService> logger,
    UserDataContext userDataContext,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : INetWorthWidgetGroupService
{
    public async Task ReorderNetWorthWidgetGroupsAsync(
        Guid userGuid,
        INetWorthWidgetGroupReorderRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);

        var widgetSettings = GetWidgetSettings(userData, request.WidgetSettingsId);
        var configuration = GetNetWorthWidgetConfiguration(widgetSettings);

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
