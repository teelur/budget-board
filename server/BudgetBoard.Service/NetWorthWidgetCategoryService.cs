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

public class NetWorthWidgetCategoryService(
    ILogger<INetWorthWidgetCategoryService> logger,
    UserDataContext userDataContext,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : INetWorthWidgetCategoryService
{
    public async Task CreateNetWorthWidgetCategoryAsync(
        Guid userGuid,
        INetWorthWidgetCategoryCreateRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);

        var widgetSettings = GetWidgetSettings(userData, request.WidgetSettingsId);
        var configuration = GetNetWorthWidgetConfiguration(widgetSettings);

        var newCategory = new NetWorthWidgetCategory
        {
            ID = Guid.NewGuid(),
            Value = request.Value,
            Type = request.Type,
            Subtype = request.Subtype,
        };

        var line = GetNetWorthWidgetLine(configuration, request.LineId);
        line.Categories.Add(newCategory);

        widgetSettings.Configuration = JsonSerializer.Serialize(configuration);
        await userDataContext.SaveChangesAsync();
    }

    public async Task UpdateNetWorthWidgetCategoryAsync(
        Guid userGuid,
        INetWorthWidgetCategoryUpdateRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);

        var widgetSettings = GetWidgetSettings(userData, request.WidgetSettingsId);
        var configuration = GetNetWorthWidgetConfiguration(widgetSettings);
        var line = GetNetWorthWidgetLine(configuration, request.LineId);
        var category = line.Categories.FirstOrDefault(c => c.ID == request.Id);
        if (category == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["NetWorthWidgetCategoryNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["NetWorthWidgetCategoryNotFoundError"]
            );
        }

        if (request.Type == "Line" && request.Subtype == "Name")
        {
            var targetLine = configuration
                .Groups.SelectMany(g => g.Lines)
                .FirstOrDefault(l =>
                    l.Name.Equals(request.Value, StringComparison.CurrentCultureIgnoreCase)
                );

            if (targetLine == null)
            {
                logger.LogError("{LogMessage}", logLocalizer["NetWorthWidgetLineNotFoundLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["NetWorthWidgetLineNotFoundError"]
                );
            }

            if (
                targetLine.Categories.Any(c =>
                    c.Type == "Line"
                    && c.Subtype == "Name"
                    && c.Value.Equals(line.Name, StringComparison.CurrentCultureIgnoreCase)
                )
            )
            {
                logger.LogError(
                    "{LogMessage}",
                    logLocalizer["NetWorthWidgetCategoryTargetLineDependsOnThisLineLog"]
                );
                throw new BudgetBoardServiceException(
                    responseLocalizer["NetWorthWidgetCategoryTargetLineDependsOnThisLineError"]
                );
            }
        }

        category.Value = request.Value;
        category.Type = request.Type;
        category.Subtype = request.Subtype;

        widgetSettings.Configuration = JsonSerializer.Serialize(configuration);
        await userDataContext.SaveChangesAsync();
    }

    public async Task DeleteNetWorthWidgetCategoryAsync(
        Guid userGuid,
        Guid widgetSettingsId,
        Guid lineId,
        Guid categoryId
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);

        var widgetSettings = GetWidgetSettings(userData, widgetSettingsId);
        var configuration = GetNetWorthWidgetConfiguration(widgetSettings);
        var line = GetNetWorthWidgetLine(configuration, lineId);

        line.Categories = [.. line.Categories.Where(c => c.ID != categoryId)];

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

    private NetWorthWidgetLine GetNetWorthWidgetLine(
        NetWorthWidgetConfiguration configuration,
        Guid lineId
    )
    {
        var line = configuration
            .Groups.SelectMany(g => g.Lines)
            .FirstOrDefault(l => l.ID == lineId);
        if (line == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["NetWorthWidgetLineNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["NetWorthWidgetLineNotFoundError"]
            );
        }
        return line;
    }
}
