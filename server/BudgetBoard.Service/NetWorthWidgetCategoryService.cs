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

        var widgetSettings = NetWorthWidgetSettingsHelpers.GetWidgetSettingsById(
            userData,
            request.WidgetSettingsId,
            logger,
            logLocalizer,
            responseLocalizer
        );
        var configuration = NetWorthWidgetSettingsHelpers.GetNetWorthWidgetConfiguration(
            widgetSettings,
            logger,
            logLocalizer,
            responseLocalizer
        );

        var newCategory = new NetWorthWidgetCategory
        {
            ID = Guid.NewGuid(),
            Value = request.Value,
            Type = request.Type,
            Subtype = request.Subtype,
        };

        var line = NetWorthWidgetSettingsHelpers.GetNetWorthWidgetLineById(
            configuration,
            request.LineId,
            logger,
            logLocalizer,
            responseLocalizer
        );
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

        var widgetSettings = NetWorthWidgetSettingsHelpers.GetWidgetSettingsById(
            userData,
            request.WidgetSettingsId,
            logger,
            logLocalizer,
            responseLocalizer
        );
        var configuration = NetWorthWidgetSettingsHelpers.GetNetWorthWidgetConfiguration(
            widgetSettings,
            logger,
            logLocalizer,
            responseLocalizer
        );
        var line = NetWorthWidgetSettingsHelpers.GetNetWorthWidgetLineById(
            configuration,
            request.LineId,
            logger,
            logLocalizer,
            responseLocalizer
        );
        var category = NetWorthWidgetSettingsHelpers.GetNetWorthWidgetCategoryById(
            line,
            request.Id,
            logger,
            logLocalizer,
            responseLocalizer
        );

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

        var widgetSettings = NetWorthWidgetSettingsHelpers.GetWidgetSettingsById(
            userData,
            widgetSettingsId,
            logger,
            logLocalizer,
            responseLocalizer
        );
        var configuration = NetWorthWidgetSettingsHelpers.GetNetWorthWidgetConfiguration(
            widgetSettings,
            logger,
            logLocalizer,
            responseLocalizer
        );
        var line = NetWorthWidgetSettingsHelpers.GetNetWorthWidgetLineById(
            configuration,
            lineId,
            logger,
            logLocalizer,
            responseLocalizer
        );

        line.Categories = [.. line.Categories.Where(c => c.ID != categoryId)];

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
