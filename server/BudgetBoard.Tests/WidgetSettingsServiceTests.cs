using System.Text.Json;
using Bogus;
using BudgetBoard.Database.Models;
using BudgetBoard.IntegrationTests.Fakers;
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
public class WidgetSettingsServiceTests
{
    private static readonly string[] items = ["Checking", "Savings", "Credit Card", "Loan"];

    [Fact]
    public async Task CreateWidgetSettingsAsync_WhenValidData_ShouldCreateSettings()
    {
        // Arrange
        var helper = new TestHelper();

        var widgetSettingsService = new WidgetSettingsService(
            Mock.Of<ILogger<IWidgetSettingsService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var request = new WidgetSettingsCreateRequest<NetWorthWidgetConfiguration>
        {
            WidgetType = WidgetTypes.NetWorth,
            IsVisible = true,
            Configuration = new NetWorthWidgetConfiguration
            {
                Lines = new Faker<NetWorthWidgetLine>()
                    .RuleFor(l => l.Name, f => f.Finance.AccountName())
                    .RuleFor(
                        l => l.Categories,
                        f =>
                            [
                                new NetWorthWidgetCategory
                                {
                                    ID = Guid.NewGuid(),
                                    Value = f.PickRandom(items),
                                    Type = "Account",
                                    Subtype = "Category",
                                },
                            ]
                    )
                    .RuleFor(l => l.Group, f => f.Random.Int(0, 2))
                    .RuleFor(l => l.Index, f => f.Random.Int(0, 10))
                    .Generate(3),
            },
            UserID = helper.demoUser.Id,
        };

        // Act
        await widgetSettingsService.CreateWidgetSettingsAsync(helper.demoUser.Id, request);

        // Assert
        var settings = helper.UserDataContext.WidgetSettings.SingleOrDefault(ws =>
            ws.UserID == helper.demoUser.Id
        );
        settings.Should().NotBeNull();
        settings.WidgetType.Should().Be(WidgetTypes.NetWorth);
        settings.Configuration.Should().Be(JsonSerializer.Serialize(request.Configuration));
    }

    [Fact]
    public async Task ReadWidgetSettingsAsync_WhenSettingsExist_ShouldReturnSettings()
    {
        // Arrange
        var helper = new TestHelper();

        var widgetSettingsService = new WidgetSettingsService(
            Mock.Of<ILogger<IWidgetSettingsService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var existingSettings = new WidgetSettings
        {
            WidgetType = WidgetTypes.NetWorth,
            IsVisible = true,
            Configuration = JsonSerializer.Serialize(
                new NetWorthWidgetConfiguration
                {
                    Lines = new Faker<NetWorthWidgetLine>()
                        .RuleFor(l => l.Name, f => f.Finance.AccountName())
                        .RuleFor(
                            l => l.Categories,
                            f =>
                                [
                                    new NetWorthWidgetCategory
                                    {
                                        ID = Guid.NewGuid(),
                                        Value = f.PickRandom(items),
                                        Type = "Account",
                                        Subtype = "Category",
                                    },
                                ]
                        )
                        .RuleFor(l => l.Group, f => f.Random.Int(0, 2))
                        .RuleFor(l => l.Index, f => f.Random.Int(0, 10))
                        .Generate(3),
                }
            ),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(existingSettings);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        var settings = await widgetSettingsService.ReadWidgetSettingsAsync(helper.demoUser.Id);

        // Assert
        settings.Should().NotBeNull();
        settings.Count().Should().Be(1);
        var setting = settings.First();
        setting.ID.Should().Be(existingSettings.ID);
        setting.WidgetType.Should().Be(WidgetTypes.NetWorth);
        setting.IsVisible.Should().BeTrue();
        setting.Configuration.Should().Be(existingSettings.Configuration);
        setting.UserID.Should().Be(helper.demoUser.Id);
    }

    [Fact]
    public async Task ReadWidgetSettingsAsync_WhenNoSettingsExist_ShouldCreateDefaultSettings()
    {
        // Arrange
        var helper = new TestHelper();

        var widgetSettingsService = new WidgetSettingsService(
            Mock.Of<ILogger<IWidgetSettingsService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        var settings = await widgetSettingsService.ReadWidgetSettingsAsync(helper.demoUser.Id);

        // Assert
        settings.Should().NotBeNull();
        settings.Count().Should().Be(1);
        var setting = settings.First();
        setting.WidgetType.Should().Be(WidgetTypes.NetWorth);
        setting.IsVisible.Should().BeTrue();
        setting
            .Configuration.Should()
            .Be(JsonSerializer.Serialize(WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration));
        setting.UserID.Should().Be(helper.demoUser.Id);
    }

    [Fact]
    public async Task UpdateWidgetSettingsAsync_WhenValidData_ShouldUpdateWidgetSettings()
    {
        // Arrange
        var helper = new TestHelper();

        var widgetSettingsService = new WidgetSettingsService(
            Mock.Of<ILogger<IWidgetSettingsService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var existingSettings = new WidgetSettings
        {
            ID = Guid.NewGuid(),
            WidgetType = WidgetTypes.NetWorth,
            IsVisible = true,
            Configuration = JsonSerializer.Serialize(
                WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration
            ),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(existingSettings);
        await helper.UserDataContext.SaveChangesAsync();

        var updateRequest = new WidgetSettingsUpdateRequest<NetWorthWidgetConfiguration>
        {
            ID = existingSettings.ID,
            IsVisible = false,
            Configuration = new NetWorthWidgetConfiguration
            {
                Lines = new Faker<NetWorthWidgetLine>()
                    .RuleFor(l => l.Name, f => f.Finance.AccountName())
                    .RuleFor(
                        l => l.Categories,
                        f =>
                            [
                                new NetWorthWidgetCategory
                                {
                                    ID = Guid.NewGuid(),
                                    Value = f.PickRandom(items),
                                    Type = "Account",
                                    Subtype = "Category",
                                },
                            ]
                    )
                    .RuleFor(l => l.Group, f => f.Random.Int(0, 2))
                    .RuleFor(l => l.Index, f => f.Random.Int(0, 10))
                    .Generate(2),
            },
        };

        // Act
        await widgetSettingsService.UpdateWidgetSettingsAsync(helper.demoUser.Id, updateRequest);

        // Assert
        var settings = helper.UserDataContext.WidgetSettings.SingleOrDefault(ws =>
            ws.ID == existingSettings.ID && ws.UserID == helper.demoUser.Id
        );
        settings.Should().NotBeNull();
        settings.IsVisible.Should().BeFalse();
        settings.Configuration.Should().Be(JsonSerializer.Serialize(updateRequest.Configuration));
    }

    [Fact]
    public async Task UpdateWidgetSettingsAsync_WhenWidgetDoesNotExist_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var widgetSettingsService = new WidgetSettingsService(
            Mock.Of<ILogger<IWidgetSettingsService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var updateRequest = new WidgetSettingsUpdateRequest<NetWorthWidgetConfiguration>
        {
            ID = Guid.NewGuid(),
            IsVisible = false,
            Configuration = new NetWorthWidgetConfiguration
            {
                Lines = new Faker<NetWorthWidgetLine>()
                    .RuleFor(l => l.Name, f => f.Finance.AccountName())
                    .RuleFor(
                        l => l.Categories,
                        f =>
                            [
                                new NetWorthWidgetCategory
                                {
                                    ID = Guid.NewGuid(),
                                    Value = f.PickRandom(items),
                                    Type = "Account",
                                    Subtype = "Category",
                                },
                            ]
                    )
                    .RuleFor(l => l.Group, f => f.Random.Int(0, 2))
                    .RuleFor(l => l.Index, f => f.Random.Int(0, 10))
                    .Generate(2),
            },
        };

        // Act
        var action = async () =>
            await widgetSettingsService.UpdateWidgetSettingsAsync(
                helper.demoUser.Id,
                updateRequest
            );

        // Assert
        await action
            .Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("WidgetUpdateNotFoundError");
    }

    [Fact]
    public async Task DeleteWidgetSettingsAsync_WhenValidData_ShouldDeleteWidgetSettings()
    {
        // Arrange
        var helper = new TestHelper();

        var widgetSettingsService = new WidgetSettingsService(
            Mock.Of<ILogger<IWidgetSettingsService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var existingSettings = new WidgetSettings
        {
            ID = Guid.NewGuid(),
            WidgetType = WidgetTypes.NetWorth,
            IsVisible = true,
            Configuration = JsonSerializer.Serialize(
                WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration
            ),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(existingSettings);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await widgetSettingsService.DeleteWidgetSettingsAsync(
            helper.demoUser.Id,
            existingSettings.ID
        );

        // Assert
        var settings = helper.UserDataContext.WidgetSettings.SingleOrDefault(ws =>
            ws.ID == existingSettings.ID && ws.UserID == helper.demoUser.Id
        );
        settings.Should().BeNull();
    }

    [Fact]
    public async Task DeleteWidgetSettings_WhenWidgetSettingsDoesNotExist_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var widgetSettingsService = new WidgetSettingsService(
            Mock.Of<ILogger<IWidgetSettingsService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var nonExistentWidgetId = Guid.NewGuid();

        // Act
        var action = async () =>
            await widgetSettingsService.DeleteWidgetSettingsAsync(
                helper.demoUser.Id,
                nonExistentWidgetId
            );

        // Assert
        await action
            .Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("WidgetDeleteNotFoundError");
    }
}
