using System.Text.Json;
using Bogus;
using BudgetBoard.Database.Models;
using BudgetBoard.Service;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Models.Widgets.NetWorthWidget;
using BudgetBoard.Service.Resources;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class WidgetSettingsServiceTests
{
    private static readonly string[] items = ["Checking", "Savings", "Credit Card", "Loan"];

    private static WidgetSettingsService CreateService(TestHelper helper) =>
        new(
            Mock.Of<ILogger<IWidgetSettingsService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

    [Fact]
    public async Task CreateWidgetSettingsAsync_WhenValidData_ShouldCreateSettings()
    {
        // Arrange
        var helper = new TestHelper();
        var service = CreateService(helper);

        var request = new WidgetSettingsCreateRequest
        {
            WidgetType = WidgetTypes.NetWorth,
            X = 0,
            Y = 5,
            W = 4,
            H = 5,
        };

        // Act
        await service.CreateWidgetSettingsAsync(helper.demoUser.Id, request);

        // Assert
        var settings = helper.UserDataContext.WidgetSettings.SingleOrDefault(ws =>
            ws.UserID == helper.demoUser.Id
        );
        settings.Should().NotBeNull();
        settings!.WidgetType.Should().Be(WidgetTypes.NetWorth);
        settings.X.Should().Be(0);
        settings.Y.Should().Be(5);
        settings.W.Should().Be(4);
        settings.H.Should().Be(5);
    }

    [Fact]
    public async Task ReadWidgetSettingsAsync_WhenSettingsExist_ShouldReturnSettings()
    {
        // Arrange
        var helper = new TestHelper();
        var service = CreateService(helper);

        var existingSettings = new WidgetSettings
        {
            WidgetType = WidgetTypes.NetWorth,
            X = 0,
            Y = 5,
            W = 4,
            H = 5,
            Configuration = JsonSerializer.Serialize(
                new NetWorthWidgetConfiguration
                {
                    Groups =
                    [
                        new NetWorthWidgetGroup
                        {
                            Index = 0,
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
                                .RuleFor(l => l.Index, f => f.Random.Int(0, 10))
                                .Generate(3),
                        },
                    ],
                }
            ),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(existingSettings);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        var settings = await service.ReadWidgetSettingsAsync(helper.demoUser.Id);

        // Assert
        settings.Should().NotBeNull();
        settings.Count().Should().Be(1);
        var setting = settings.First();
        setting.ID.Should().Be(existingSettings.ID);
        setting.WidgetType.Should().Be(WidgetTypes.NetWorth);
        setting.X.Should().Be(0);
        setting.Y.Should().Be(5);
        setting.W.Should().Be(4);
        setting.H.Should().Be(5);
        setting.Configuration.Should().Be(existingSettings.Configuration);
        setting.UserID.Should().Be(helper.demoUser.Id);
    }

    [Fact]
    public async Task ReadWidgetSettingsAsync_WhenNoSettingsExist_ShouldCreateDefaultSettings()
    {
        // Arrange
        var helper = new TestHelper();
        var service = CreateService(helper);

        // Act
        var settings = await service.ReadWidgetSettingsAsync(helper.demoUser.Id);

        // Assert
        settings.Should().NotBeNull();
        settings.Count().Should().Be(WidgetSettingsHelpers.DefaultLayouts.Count);

        foreach (var layout in WidgetSettingsHelpers.DefaultLayouts)
        {
            var match = settings.FirstOrDefault(s => s.WidgetType == layout.WidgetType);
            match.Should().NotBeNull($"default widget '{layout.WidgetType}' should be seeded");
            match!.X.Should().Be(layout.X);
            match.Y.Should().Be(layout.Y);
            match.W.Should().Be(layout.W);
            match.H.Should().Be(layout.H);
            match.UserID.Should().Be(helper.demoUser.Id);
        }
    }

    [Fact]
    public async Task UpdateWidgetSettingsAsync_WhenValidData_ShouldUpdateWidgetSettings()
    {
        // Arrange
        var helper = new TestHelper();
        var service = CreateService(helper);

        var existingSettings = new WidgetSettings
        {
            ID = Guid.NewGuid(),
            WidgetType = WidgetTypes.NetWorth,
            X = 0,
            Y = 5,
            W = 4,
            H = 5,
            Configuration = JsonSerializer.Serialize(
                WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration
            ),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(existingSettings);
        await helper.UserDataContext.SaveChangesAsync();

        var updatedConfig = new NetWorthWidgetConfiguration
        {
            Groups =
            [
                new NetWorthWidgetGroup
                {
                    Index = 0,
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
                        .RuleFor(l => l.Index, f => f.Random.Int(0, 10))
                        .Generate(3),
                },
            ],
        };

        var updateRequest = new WidgetSettingsUpdateRequest<NetWorthWidgetConfiguration>
        {
            ID = existingSettings.ID,
            X = 4,
            Y = 0,
            W = 8,
            H = 5,
            Configuration = updatedConfig,
        };

        // Act
        await service.UpdateWidgetSettingsAsync(helper.demoUser.Id, updateRequest);

        // Assert
        var updated = helper.UserDataContext.WidgetSettings.SingleOrDefault(ws =>
            ws.ID == existingSettings.ID && ws.UserID == helper.demoUser.Id
        );
        updated.Should().NotBeNull();
        updated!.X.Should().Be(4);
        updated.Y.Should().Be(0);
        updated.W.Should().Be(8);
        updated.H.Should().Be(5);
        updated.Configuration.Should().Be(JsonSerializer.Serialize(updatedConfig));
    }

    [Fact]
    public async Task UpdateWidgetSettingsAsync_WhenWidgetDoesNotExist_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();
        var service = CreateService(helper);

        var updateRequest = new WidgetSettingsUpdateRequest<NetWorthWidgetConfiguration>
        {
            ID = Guid.NewGuid(),
            X = 0,
            Y = 0,
            W = 4,
            H = 5,
            Configuration = new NetWorthWidgetConfiguration { Groups = [] },
        };

        // Act
        var action = async () =>
            await service.UpdateWidgetSettingsAsync(helper.demoUser.Id, updateRequest);

        // Assert
        await action
            .Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("WidgetUpdateNotFoundError");
    }

    [Fact]
    public async Task BatchUpdateWidgetSettingsAsync_WhenValidData_ShouldUpdatePositions()
    {
        // Arrange
        var helper = new TestHelper();
        var service = CreateService(helper);

        var widget1 = new WidgetSettings
        {
            ID = Guid.NewGuid(),
            WidgetType = WidgetTypes.Accounts,
            X = 0,
            Y = 0,
            W = 4,
            H = 5,
            UserID = helper.demoUser.Id,
        };
        var widget2 = new WidgetSettings
        {
            ID = Guid.NewGuid(),
            WidgetType = WidgetTypes.NetWorth,
            X = 0,
            Y = 5,
            W = 4,
            H = 5,
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.AddRange(widget1, widget2);
        await helper.UserDataContext.SaveChangesAsync();

        var batchRequests = new List<WidgetSettingsBatchUpdateRequest>
        {
            new()
            {
                ID = widget1.ID,
                X = 8,
                Y = 0,
                W = 4,
                H = 3,
            },
            new()
            {
                ID = widget2.ID,
                X = 0,
                Y = 3,
                W = 8,
                H = 6,
            },
        };

        // Act
        await service.BatchUpdateWidgetSettingsAsync(helper.demoUser.Id, batchRequests);

        // Assert
        var updated1 = helper.UserDataContext.WidgetSettings.Single(ws => ws.ID == widget1.ID);
        updated1.X.Should().Be(8);
        updated1.Y.Should().Be(0);
        updated1.W.Should().Be(4);
        updated1.H.Should().Be(3);

        var updated2 = helper.UserDataContext.WidgetSettings.Single(ws => ws.ID == widget2.ID);
        updated2.X.Should().Be(0);
        updated2.Y.Should().Be(3);
        updated2.W.Should().Be(8);
        updated2.H.Should().Be(6);
    }

    [Fact]
    public async Task DeleteWidgetSettingsAsync_WhenValidData_ShouldDeleteWidgetSettings()
    {
        // Arrange
        var helper = new TestHelper();
        var service = CreateService(helper);

        var existingSettings = new WidgetSettings
        {
            ID = Guid.NewGuid(),
            WidgetType = WidgetTypes.NetWorth,
            X = 0,
            Y = 5,
            W = 4,
            H = 5,
            Configuration = JsonSerializer.Serialize(
                WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration
            ),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(existingSettings);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await service.DeleteWidgetSettingsAsync(helper.demoUser.Id, existingSettings.ID);

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
        var service = CreateService(helper);

        var nonExistentWidgetId = Guid.NewGuid();

        // Act
        var action = async () =>
            await service.DeleteWidgetSettingsAsync(helper.demoUser.Id, nonExistentWidgetId);

        // Assert
        await action
            .Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("WidgetDeleteNotFoundError");
    }
}
