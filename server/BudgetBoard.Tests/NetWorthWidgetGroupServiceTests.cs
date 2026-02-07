using System.Text.Json;
using BudgetBoard.Database.Models;
using BudgetBoard.Service;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

public class NetWorthWidgetGroupServiceTests
{
    [Fact]
    public async Task ReorderNetWorthWidgetGroupsAsync_WhenValidData_ShouldReorderGroups()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetGroupService = new NetWorthWidgetGroupService(
            Mock.Of<ILogger<INetWorthWidgetGroupService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var widgetSettings = new WidgetSettings
        {
            WidgetType = "NetWorth",
            Configuration = JsonSerializer.Serialize(
                WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration
            ),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widgetSettings);
        await helper.UserDataContext.SaveChangesAsync();

        var groupIdsInNewOrder = WidgetSettingsHelpers
            .DefaultNetWorthWidgetConfiguration.Groups.Select(g => g.ID)
            .Reverse()
            .ToList();

        var request = new NetWorthWidgetGroupReorderRequest
        {
            WidgetSettingsId = widgetSettings.ID,
            OrderedGroupIds = groupIdsInNewOrder,
        };

        // Act
        await netWorthWidgetGroupService.ReorderNetWorthWidgetGroupsAsync(
            helper.demoUser.Id,
            request
        );

        // Assert
        var updatedWidgetSettings = helper.UserDataContext.WidgetSettings.First(ws =>
            ws.ID == widgetSettings.ID
        );
        var updatedConfiguration = JsonSerializer.Deserialize<NetWorthWidgetConfiguration>(
            updatedWidgetSettings.Configuration!
        )!;
        updatedConfiguration.Should().NotBeNull();

        var updatedGroupIds = updatedConfiguration.Groups.Select(g => g.ID).ToList();
        updatedGroupIds.Should().NotBeNull();
        updatedGroupIds
            .Should()
            .BeEquivalentTo(groupIdsInNewOrder, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task ReorderNetWorthWidgetGroupsAsync_WhenInvalidGroupId_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetGroupService = new NetWorthWidgetGroupService(
            Mock.Of<ILogger<INetWorthWidgetGroupService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var widgetSettings = new WidgetSettings
        {
            WidgetType = "NetWorth",
            Configuration = JsonSerializer.Serialize(
                WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration
            ),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widgetSettings);
        await helper.UserDataContext.SaveChangesAsync();

        var invalidGroupId = Guid.NewGuid();
        var groupIdsInNewOrder = WidgetSettingsHelpers
            .DefaultNetWorthWidgetConfiguration.Groups.Select(g => g.ID)
            .Append(invalidGroupId)
            .ToList();

        var request = new NetWorthWidgetGroupReorderRequest
        {
            WidgetSettingsId = widgetSettings.ID,
            OrderedGroupIds = groupIdsInNewOrder,
        };

        // Act
        var act = async () =>
            await netWorthWidgetGroupService.ReorderNetWorthWidgetGroupsAsync(
                helper.demoUser.Id,
                request
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("NetWorthWidgetGroupNotFoundError");
    }
}
