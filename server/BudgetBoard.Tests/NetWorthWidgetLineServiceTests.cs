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

[Collection("IntegrationTests")]
public class NetWorthWidgetLineServiceTests
{
    [Fact]
    public async Task CreateNetWorthWidgetLineAsync_WhenValidData_ShouldCreateNewLine()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetLineService = new NetWorthWidgetLineService(
            Mock.Of<ILogger<INetWorthWidgetLineService>>(),
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

        var request = new NetWorthWidgetLineCreateRequest
        {
            Name = "Test",
            Group = 0,
            Index = 12,
            WidgetSettingsId = widgetSettings.ID,
        };

        // Act
        await netWorthWidgetLineService.CreateNetWorthWidgetLineAsync(helper.demoUser.Id, request);

        // Assert
        var updatedWidgetSettings = helper.UserDataContext.WidgetSettings.First(ws =>
            ws.ID == widgetSettings.ID
        );
        var updatedConfiguration = JsonSerializer.Deserialize<NetWorthWidgetConfiguration>(
            updatedWidgetSettings.Configuration!
        )!;
        updatedConfiguration.Should().NotBeNull();

        var newLine = updatedConfiguration
            .Groups.ElementAt(0)
            .Lines.SingleOrDefault(l => l.Name == request.Name);
        newLine.Should().NotBeNull();
        newLine.Index.Should().Be(request.Index);
    }

    [Fact]
    public async Task CreateNetWorthWidgetLineAsync_WhenGroupDoesNotExist_ShouldAlsoCreateGroup()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetLineService = new NetWorthWidgetLineService(
            Mock.Of<ILogger<INetWorthWidgetLineService>>(),
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

        var request = new NetWorthWidgetLineCreateRequest
        {
            Name = "Test",
            Group = 5,
            Index = 12,
            WidgetSettingsId = widgetSettings.ID,
        };

        // Act
        await netWorthWidgetLineService.CreateNetWorthWidgetLineAsync(helper.demoUser.Id, request);

        // Assert
        var updatedWidgetSettings = helper.UserDataContext.WidgetSettings.First(ws =>
            ws.ID == widgetSettings.ID
        );
        var updatedConfiguration = JsonSerializer.Deserialize<NetWorthWidgetConfiguration>(
            updatedWidgetSettings.Configuration!
        )!;
        updatedConfiguration.Should().NotBeNull();

        var newGroup = updatedConfiguration.Groups.SingleOrDefault(g => g.Index == request.Group);
        newGroup.Should().NotBeNull();

        var newLine = newGroup.Lines.SingleOrDefault(l => l.Name == request.Name);
        newLine.Should().NotBeNull();
        newLine.Index.Should().Be(request.Index);
    }

    [Fact]
    public async Task UpdateNetWorthWidgetLineAsync_WhenValidData_ShouldUpdateLine()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetLineService = new NetWorthWidgetLineService(
            Mock.Of<ILogger<INetWorthWidgetLineService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var initialConfiguration = WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration;
        var lineToUpdate = initialConfiguration.Groups.ElementAt(0).Lines.ElementAt(0);
        var widgetSettings = new WidgetSettings
        {
            WidgetType = "NetWorth",
            Configuration = JsonSerializer.Serialize(initialConfiguration),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widgetSettings);
        await helper.UserDataContext.SaveChangesAsync();

        var request = new NetWorthWidgetLineUpdateRequest
        {
            LineId = lineToUpdate.ID,
            Name = "Updated Name",
            WidgetSettingsId = widgetSettings.ID,
        };

        // Act
        await netWorthWidgetLineService.UpdateNetWorthWidgetLineAsync(helper.demoUser.Id, request);

        // Assert
        var updatedWidgetSettings = helper.UserDataContext.WidgetSettings.First(ws =>
            ws.ID == widgetSettings.ID
        );
        var updatedConfiguration = JsonSerializer.Deserialize<NetWorthWidgetConfiguration>(
            updatedWidgetSettings.Configuration!
        )!;
        updatedConfiguration.Should().NotBeNull();

        var updatedLine = updatedConfiguration
            .Groups.SelectMany(g => g.Lines)
            .SingleOrDefault(l => l.ID == request.LineId);
        updatedLine.Should().NotBeNull();
        updatedLine.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task UpdateNetWorthWidgetLineAsync_WhenLineDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetLineService = new NetWorthWidgetLineService(
            Mock.Of<ILogger<INetWorthWidgetLineService>>(),
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

        var request = new NetWorthWidgetLineUpdateRequest
        {
            LineId = Guid.NewGuid(),
            Name = "Updated Name",
            WidgetSettingsId = widgetSettings.ID,
        };

        // Act
        Func<Task> act = async () =>
            await netWorthWidgetLineService.UpdateNetWorthWidgetLineAsync(
                helper.demoUser.Id,
                request
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("NetWorthWidgetLineNotFoundError");
    }

    [Fact]
    public async Task UpdateNetWorthWidgetLineAsync_WhenMovingToAnotherGroup_ShouldMoveLine()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetLineService = new NetWorthWidgetLineService(
            Mock.Of<ILogger<INetWorthWidgetLineService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var initialConfiguration = WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration;
        var lineToUpdate = initialConfiguration.Groups.ElementAt(0).Lines.ElementAt(0);
        var widgetSettings = new WidgetSettings
        {
            WidgetType = "NetWorth",
            Configuration = JsonSerializer.Serialize(initialConfiguration),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widgetSettings);
        await helper.UserDataContext.SaveChangesAsync();

        var request = new NetWorthWidgetLineUpdateRequest
        {
            LineId = lineToUpdate.ID,
            Name = "Updated Name",
            Group = 1,
            WidgetSettingsId = widgetSettings.ID,
        };

        // Act
        await netWorthWidgetLineService.UpdateNetWorthWidgetLineAsync(helper.demoUser.Id, request);

        // Assert
        var updatedWidgetSettings = helper.UserDataContext.WidgetSettings.First(ws =>
            ws.ID == widgetSettings.ID
        );
        var updatedConfiguration = JsonSerializer.Deserialize<NetWorthWidgetConfiguration>(
            updatedWidgetSettings.Configuration!
        )!;
        updatedConfiguration.Should().NotBeNull();

        var oldGroupLine = updatedConfiguration
            .Groups.ElementAt(0)
            .Lines.SingleOrDefault(l => l.ID == request.LineId);
        oldGroupLine.Should().BeNull();

        var newGroupLine = updatedConfiguration
            .Groups.ElementAt(1)
            .Lines.SingleOrDefault(l => l.ID == request.LineId);
        newGroupLine.Should().NotBeNull();
        newGroupLine.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task DeleteNetWorthWidgetLineAsync_WhenValidData_ShouldDeleteLine()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetLineService = new NetWorthWidgetLineService(
            Mock.Of<ILogger<INetWorthWidgetLineService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var initialConfiguration = WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration;
        var lineToDelete = initialConfiguration.Groups.ElementAt(0).Lines.ElementAt(0);
        var widgetSettings = new WidgetSettings
        {
            WidgetType = "NetWorth",
            Configuration = JsonSerializer.Serialize(initialConfiguration),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widgetSettings);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await netWorthWidgetLineService.DeleteNetWorthWidgetLineAsync(
            helper.demoUser.Id,
            widgetSettings.ID,
            lineToDelete.ID
        );

        // Assert
        var updatedWidgetSettings = helper.UserDataContext.WidgetSettings.First(ws =>
            ws.ID == widgetSettings.ID
        );
        var updatedConfiguration = JsonSerializer.Deserialize<NetWorthWidgetConfiguration>(
            updatedWidgetSettings.Configuration!
        )!;
        updatedConfiguration.Should().NotBeNull();

        var deletedLine = updatedConfiguration
            .Groups.SelectMany(g => g.Lines)
            .SingleOrDefault(l => l.ID == lineToDelete.ID);
        deletedLine.Should().BeNull();
    }

    [Fact]
    public async Task DeleteNetWorthWidgetLineAsync_WhenLineDoesNotExist_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetLineService = new NetWorthWidgetLineService(
            Mock.Of<ILogger<INetWorthWidgetLineService>>(),
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

        var nonExistentLineId = Guid.NewGuid();

        // Act
        Func<Task> act = async () =>
            await netWorthWidgetLineService.DeleteNetWorthWidgetLineAsync(
                helper.demoUser.Id,
                widgetSettings.ID,
                nonExistentLineId
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("NetWorthWidgetLineNotFoundError");
    }

    [Fact]
    public async Task ReorderNetWorthWidgetLinesAsync_WhenValidData_ShouldReorderLines()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetLineService = new NetWorthWidgetLineService(
            Mock.Of<ILogger<INetWorthWidgetLineService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var initialConfiguration = WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration;
        var widgetSettings = new WidgetSettings
        {
            WidgetType = "NetWorth",
            Configuration = JsonSerializer.Serialize(initialConfiguration),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widgetSettings);
        await helper.UserDataContext.SaveChangesAsync();

        var lineIdsInNewOrder = initialConfiguration
            .Groups.ElementAt(0)
            .Lines.Select(l => l.ID)
            .Reverse()
            .ToList();

        var request = new NetWorthWidgetLineReorderRequest
        {
            WidgetSettingsId = widgetSettings.ID,
            GroupId = initialConfiguration.Groups.ElementAt(0).ID,
            OrderedLineIds = lineIdsInNewOrder,
        };

        // Act
        await netWorthWidgetLineService.ReorderNetWorthWidgetLinesAsync(
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

        var reorderedLineIds = updatedConfiguration
            .Groups.ElementAt(0)
            .Lines.Select(l => l.ID)
            .ToList();
        reorderedLineIds
            .Should()
            .BeEquivalentTo(lineIdsInNewOrder, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task ReorderNetWorthWidgetLinesAsync_WhenGroupNotFound_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetLineService = new NetWorthWidgetLineService(
            Mock.Of<ILogger<INetWorthWidgetLineService>>(),
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

        var request = new NetWorthWidgetLineReorderRequest
        {
            WidgetSettingsId = widgetSettings.ID,
            GroupId = Guid.NewGuid(),
            OrderedLineIds = [],
        };

        // Act
        Func<Task> act = async () =>
            await netWorthWidgetLineService.ReorderNetWorthWidgetLinesAsync(
                helper.demoUser.Id,
                request
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("NetWorthWidgetGroupNotFoundError");
    }

    [Fact]
    public async Task ReorderNetWorthWidgetLinesAsync_WhenLineIdsMismatch_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var netWorthWidgetLineService = new NetWorthWidgetLineService(
            Mock.Of<ILogger<INetWorthWidgetLineService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var initialConfiguration = WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration;
        var widgetSettings = new WidgetSettings
        {
            WidgetType = "NetWorth",
            Configuration = JsonSerializer.Serialize(initialConfiguration),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widgetSettings);
        await helper.UserDataContext.SaveChangesAsync();

        var request = new NetWorthWidgetLineReorderRequest
        {
            WidgetSettingsId = widgetSettings.ID,
            GroupId = initialConfiguration.Groups.ElementAt(0).ID,
            OrderedLineIds = [Guid.NewGuid()],
        };

        // Act
        Func<Task> act = async () =>
            await netWorthWidgetLineService.ReorderNetWorthWidgetLinesAsync(
                helper.demoUser.Id,
                request
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("NetWorthWidgetLineNotFoundError");
    }
}
