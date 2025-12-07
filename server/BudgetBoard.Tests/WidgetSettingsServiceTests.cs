using System.Text.Json;
using BudgetBoard.Database.Models;
using BudgetBoard.Service;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class WidgetSettingsServiceTests
{
    private static NetWorthWidgetConfiguration CreateConfiguration(string lineName)
    {
        return new NetWorthWidgetConfiguration
        {
            Lines =
            [
                new NetWorthWidgetLine
                {
                    Name = lineName,
                    Categories =
                    [
                        new NetWorthWidgetCategory
                        {
                            Value = "Cash",
                            Type = "Account",
                            Subtype = "Category",
                        },
                    ],
                    Group = 1,
                    Index = 0,
                },
            ],
        };
    }

    [Fact]
    public async Task CreateWidgetSettingsAsync_WhenValidRequest_ShouldPersistWidget()
    {
        // Arrange
        var helper = new TestHelper();
        var service = new WidgetSettingsService(
            Mock.Of<ILogger<IWidgetSettingsService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var request = new WidgetSettingsCreateRequest<NetWorthWidgetConfiguration>
        {
            WidgetType = "NetWorth",
            IsVisible = false,
            Configuration = CreateConfiguration("Primary"),
            UserID = helper.demoUser.Id,
        };

        // Act
        await service.CreateWidgetSettingsAsync(helper.demoUser.Id, request);

        // Assert
        var saved = helper.UserDataContext.WidgetSettings.Single();
        saved.WidgetType.Should().Be(request.WidgetType);
        saved.IsVisible.Should().Be(request.IsVisible);
        saved.UserID.Should().Be(helper.demoUser.Id);
        saved.Configuration.Should().Be(JsonSerializer.Serialize(request.Configuration));
    }

    [Fact]
    public async Task ReadWidgetSettingsAsync_WhenWidgetExists_ShouldReturnConfiguration()
    {
        // Arrange
        var helper = new TestHelper();
        var service = new WidgetSettingsService(
            Mock.Of<ILogger<IWidgetSettingsService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var configuration = CreateConfiguration("Balances");
        var widget = new WidgetSettings
        {
            WidgetType = "NetWorth",
            IsVisible = true,
            Configuration = JsonSerializer.Serialize(configuration),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widget);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await service.ReadWidgetSettingsAsync(helper.demoUser.Id);

        // Assert
        result.Should().ContainSingle();
        result
            .Single()
            .Should()
            .BeEquivalentTo(
                new WidgetResponse
                {
                    ID = widget.ID,
                    WidgetType = widget.WidgetType,
                    IsVisible = widget.IsVisible,
                    Configuration = widget.Configuration ?? string.Empty,
                    UserID = widget.UserID,
                }
            );
    }

    [Fact]
    public async Task UpdateWidgetSettingsAsync_WhenWidgetExists_ShouldUpdateValues()
    {
        // Arrange
        var helper = new TestHelper();
        var service = new WidgetSettingsService(
            Mock.Of<ILogger<IWidgetSettingsService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var widget = new WidgetSettings
        {
            WidgetType = "Old",
            IsVisible = true,
            Configuration = JsonSerializer.Serialize(CreateConfiguration("Old")),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widget);
        helper.UserDataContext.SaveChanges();

        var updateRequest = new WidgetSettingsUpdateRequest<NetWorthWidgetConfiguration>
        {
            ID = widget.ID,
            WidgetType = "New",
            IsVisible = false,
            Configuration = CreateConfiguration("New"),
            UserID = helper.demoUser.Id,
        };

        // Act
        await service.UpdateWidgetSettingsAsync(widget.ID, updateRequest);

        // Assert
        var updated = helper.UserDataContext.WidgetSettings.Single();
        updated.WidgetType.Should().Be(updateRequest.WidgetType);
        updated.IsVisible.Should().BeFalse();
        updated.Configuration.Should().Be(JsonSerializer.Serialize(updateRequest.Configuration));
    }

    [Fact]
    public async Task UpdateWidgetSettingsAsync_WhenWidgetMissing_ShouldThrow()
    {
        // Arrange
        var helper = new TestHelper();
        var service = new WidgetSettingsService(
            Mock.Of<ILogger<IWidgetSettingsService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var updateRequest = new WidgetSettingsUpdateRequest<NetWorthWidgetConfiguration>
        {
            ID = Guid.NewGuid(),
            WidgetType = "Missing",
            IsVisible = true,
            Configuration = CreateConfiguration("Missing"),
            UserID = helper.demoUser.Id,
        };

        // Act
        Func<Task> act = async () =>
            await service.UpdateWidgetSettingsAsync(updateRequest.ID, updateRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("WidgetUpdateNotFoundError");
    }

    [Fact]
    public async Task DeleteWidgetSettingsAsync_WhenWidgetExists_ShouldRemoveWidget()
    {
        // Arrange
        var helper = new TestHelper();
        var service = new WidgetSettingsService(
            Mock.Of<ILogger<IWidgetSettingsService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var widget = new WidgetSettings
        {
            WidgetType = "Delete",
            IsVisible = true,
            Configuration = JsonSerializer.Serialize(CreateConfiguration("Delete")),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(widget);
        helper.UserDataContext.SaveChanges();

        // Act
        await service.DeleteWidgetSettingsAsync(widget.ID);

        // Assert
        helper.UserDataContext.WidgetSettings.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteWidgetSettingsAsync_WhenWidgetMissing_ShouldThrow()
    {
        // Arrange
        var helper = new TestHelper();
        var service = new WidgetSettingsService(
            Mock.Of<ILogger<IWidgetSettingsService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        Func<Task> act = async () => await service.DeleteWidgetSettingsAsync(Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("WidgetDeleteNotFoundError");
    }
}
