using System.Text.Json;
using System.Text.Json.Nodes;
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
    public static IEnumerable<object[]> DefaultWidgetConfigurations =>
        [
            [WidgetTypes.Accounts, JsonSerializer.Serialize(new { accountIds = new List<Guid>() })],
            [
                WidgetTypes.NetWorth,
                JsonSerializer.Serialize(WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration),
            ],
            [
                WidgetTypes.Metric,
                JsonSerializer.Serialize(WidgetSettingsHelpers.DefaultMetricWidgetConfiguration),
            ],
        ];

    private static WidgetSettingsService CreateService(TestHelper helper) =>
        new(
            Mock.Of<ILogger<IWidgetSettingsService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

    private static string NormalizeJsonWithoutIds(string configuration)
    {
        var root = JsonNode.Parse(configuration)!;
        RemoveIds(root);
        return root.ToJsonString();
    }

    private static void RemoveIds(JsonNode node)
    {
        if (node is JsonObject jsonObject)
        {
            foreach (var property in jsonObject.ToList())
            {
                if (string.Equals(property.Key, "id", StringComparison.OrdinalIgnoreCase))
                {
                    jsonObject.Remove(property.Key);
                }
                else if (property.Value is not null)
                {
                    RemoveIds(property.Value);
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                if (item is not null)
                {
                    RemoveIds(item);
                }
            }
        }
    }

    #region CreateWidgetSettingsAsync
    [Fact]
    public async Task CreateWidgetSettingsAsync_WhenValidData_ShouldCreateSettings()
    {
        // Arrange
        var helper = new TestHelper();
        var service = CreateService(helper);

        var request = new WidgetSettingsCreateRequest
        {
            WidgetType = WidgetTypes.NetWorth,
            LgX = 0,
            LgY = 5,
            LgW = 4,
            LgH = 5,
            SmY = 5,
            SmH = 5,
        };

        // Act
        await service.CreateWidgetSettingsAsync(helper.demoUser.Id, request);

        // Assert
        var settings = helper.UserDataContext.WidgetSettings.SingleOrDefault(ws =>
            ws.UserID == helper.demoUser.Id
        );
        settings.Should().NotBeNull();
        settings.WidgetType.Should().Be(WidgetTypes.NetWorth);
        settings.LgX.Should().Be(0);
        settings.LgY.Should().Be(5);
        settings.LgW.Should().Be(4);
        settings.LgH.Should().Be(5);
        settings.SmY.Should().Be(5);
        settings.SmH.Should().Be(5);
    }

    [Fact]
    public async Task CreateWidgetSettingsAsync_WhenMetricTypeAndNoLayoutProvided_ShouldUseMetricDefaults()
    {
        // Arrange
        var helper = new TestHelper();
        var service = CreateService(helper);

        var request = new WidgetSettingsCreateRequest { WidgetType = WidgetTypes.Metric };

        // Act
        await service.CreateWidgetSettingsAsync(helper.demoUser.Id, request);

        // Assert
        var settings = helper.UserDataContext.WidgetSettings.SingleOrDefault(ws =>
            ws.UserID == helper.demoUser.Id
        );

        settings.Should().NotBeNull();
        settings!.WidgetType.Should().Be(WidgetTypes.Metric);
        settings.LgX.Should().Be(4);
        settings.LgY.Should().Be(0);
        settings.LgW.Should().Be(2);
        settings.LgH.Should().Be(6);
        settings.SmY.Should().Be(30);
        settings.SmH.Should().Be(6);
        settings
            .Configuration.Should()
            .Be(JsonSerializer.Serialize(WidgetSettingsHelpers.DefaultMetricWidgetConfiguration));
    }

    [Fact]
    public async Task CreateWidgetSettingsAsync_WhenDefaultLayoutNotFound_ShouldReturnGenericDefaultLayout()
    {
        // Arrange
        var helper = new TestHelper();
        var service = CreateService(helper);

        var request = new WidgetSettingsCreateRequest { WidgetType = "unknown" };

        // Act
        await service.CreateWidgetSettingsAsync(helper.demoUser.Id, request);

        // Assert
        var settings = helper.UserDataContext.WidgetSettings.SingleOrDefault(ws =>
            ws.UserID == helper.demoUser.Id
        );

        settings.Should().NotBeNull();
        settings!.WidgetType.Should().Be("unknown");
        settings.LgX.Should().Be(0); // Generic default layout values
        settings.LgY.Should().Be(0);
        settings.LgW.Should().Be(4);
        settings.LgH.Should().Be(5);
        settings.SmY.Should().Be(0);
        settings.SmH.Should().Be(5);
    }
    #endregion

    #region ReadWidgetSettingsAsync
    [Fact]
    public async Task ReadWidgetSettingsAsync_WhenSettingsExist_ShouldReturnSettings()
    {
        // Arrange
        var helper = new TestHelper();
        var service = CreateService(helper);

        var existingSettings = new WidgetSettings
        {
            WidgetType = WidgetTypes.NetWorth,
            LgX = 0,
            LgY = 5,
            LgW = 4,
            LgH = 5,
            SmY = 5,
            SmH = 5,
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
        settings.Single().Should().BeEquivalentTo(new WidgetResponse(existingSettings));
    }

    [Fact]
    public async Task ReadWidgetSettingsAsync_WhenNoSettingsExist_ShouldReturnEmptyCollection()
    {
        // Arrange
        var helper = new TestHelper();
        var service = CreateService(helper);

        // Act
        var settings = await service.ReadWidgetSettingsAsync(helper.demoUser.Id);

        // Assert
        settings.Should().NotBeNull();
        settings.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(DefaultWidgetConfigurations))]
    public async Task ReadWidgetSettingsAsync_WhenConfigurationIsEmpty_ShouldReturnDefaultConfiguration(
        string widgetType,
        string expectedConfiguration
    )
    {
        // Arrange
        var helper = new TestHelper();
        var service = CreateService(helper);

        var existingWidget = new WidgetSettings
        {
            ID = Guid.NewGuid(),
            WidgetType = widgetType,
            LgX = 0,
            LgY = 5,
            LgW = 4,
            LgH = 5,
            SmY = 5,
            SmH = 5,
            Configuration = string.Empty,
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(existingWidget);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        var settings = await service.ReadWidgetSettingsAsync(helper.demoUser.Id);

        // Assert
        settings.Should().NotBeNull();
        settings.Should().ContainSingle();
        var setting = settings.Single();
        setting.ID.Should().Be(existingWidget.ID);
        setting.WidgetType.Should().Be(widgetType);
        NormalizeJsonWithoutIds(setting.Configuration)
            .Should()
            .Be(NormalizeJsonWithoutIds(expectedConfiguration));
        setting.UserID.Should().Be(helper.demoUser.Id);
    }

    [Fact]
    public async Task ReadWidgetSettingsAsync_WhenConfigurationIsNull_ShouldReturnDefaultConfiguration()
    {
        // Arrange
        var helper = new TestHelper();
        var service = CreateService(helper);
        var existingWidget = new WidgetSettings
        {
            ID = Guid.NewGuid(),
            WidgetType = WidgetTypes.Metric,
            LgX = 0,
            LgY = 5,
            LgW = 4,
            LgH = 5,
            SmY = 5,
            SmH = 5,
            Configuration = null,
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(existingWidget);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        var settings = await service.ReadWidgetSettingsAsync(helper.demoUser.Id);

        // Assert
        settings.Should().ContainSingle();
        NormalizeJsonWithoutIds(settings.Single().Configuration)
            .Should()
            .Be(
                NormalizeJsonWithoutIds(
                    JsonSerializer.Serialize(WidgetSettingsHelpers.DefaultMetricWidgetConfiguration)
                )
            );
    }

    [Fact]
    public async Task ReadWidgetSettingsAsync_WhenConfigurationIsEmptyAndNoDefaultExists_ShouldReturnEmptyConfiguration()
    {
        // Arrange
        var helper = new TestHelper();
        var service = CreateService(helper);
        var existingWidget = new WidgetSettings
        {
            ID = Guid.NewGuid(),
            WidgetType = "unknown",
            LgX = 0,
            LgY = 5,
            LgW = 4,
            LgH = 5,
            SmY = 5,
            SmH = 5,
            Configuration = string.Empty,
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(existingWidget);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        var settings = await service.ReadWidgetSettingsAsync(helper.demoUser.Id);

        // Assert
        settings.Should().ContainSingle();
        settings.Single().Configuration.Should().BeEmpty();
    }
    #endregion

    #region UpdateWidgetSettingsAsync
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
            LgX = 0,
            LgY = 5,
            LgW = 4,
            LgH = 5,
            SmY = 5,
            SmH = 5,
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

        var updateRequest = new WidgetSettingsUpdateRequest
        {
            ID = existingSettings.ID,
            LgX = 4,
            LgY = 0,
            LgW = 8,
            LgH = 5,
            SmY = 0,
            SmH = 5,
            Configuration = JsonSerializer.SerializeToElement(updatedConfig),
        };

        // Act
        await service.UpdateWidgetSettingsAsync(helper.demoUser.Id, [updateRequest]);

        // Assert
        var updated = helper.UserDataContext.WidgetSettings.SingleOrDefault(ws =>
            ws.ID == existingSettings.ID && ws.UserID == helper.demoUser.Id
        );
        updated.Should().NotBeNull();
        updated.LgX.Should().Be(4);
        updated.LgY.Should().Be(0);
        updated.LgW.Should().Be(8);
        updated.LgH.Should().Be(5);
        updated.SmY.Should().Be(0);
        updated.SmH.Should().Be(5);
        updated.Configuration.Should().Be(JsonSerializer.Serialize(updatedConfig));
    }

    [Fact]
    public async Task UpdateWidgetSettingsAsync_WhenWidgetDoesNotExist_ShouldThrowWidgetSettingsNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();
        var service = CreateService(helper);

        var updateRequest = new WidgetSettingsUpdateRequest
        {
            ID = Guid.NewGuid(),
            LgX = 0,
            LgY = 0,
            LgW = 4,
            LgH = 5,
            SmY = 0,
            SmH = 5,
            Configuration = JsonSerializer.SerializeToElement(
                new NetWorthWidgetConfiguration { Groups = [] }
            ),
        };

        // Act
        var action = async () =>
            await service.UpdateWidgetSettingsAsync(helper.demoUser.Id, [updateRequest]);

        // Assert
        await action
            .Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("WidgetSettingsNotFoundError");
    }

    [Fact]
    public async Task UpdateWidgetSettingsAsync_WhenMultipleValidData_ShouldUpdateValues()
    {
        // Arrange
        var helper = new TestHelper();
        var service = CreateService(helper);

        var widget1 = new WidgetSettings
        {
            ID = Guid.NewGuid(),
            WidgetType = WidgetTypes.Accounts,
            LgX = 0,
            LgY = 0,
            LgW = 4,
            LgH = 5,
            SmY = 0,
            SmH = 5,
            UserID = helper.demoUser.Id,
        };
        var widget2 = new WidgetSettings
        {
            ID = Guid.NewGuid(),
            WidgetType = WidgetTypes.NetWorth,
            LgX = 0,
            LgY = 5,
            LgW = 4,
            LgH = 5,
            SmY = 5,
            SmH = 5,
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.AddRange(widget1, widget2);
        await helper.UserDataContext.SaveChangesAsync();

        var batchRequests = new List<WidgetSettingsUpdateRequest>
        {
            new()
            {
                ID = widget1.ID,
                LgX = 8,
                LgY = 0,
                LgW = 4,
                LgH = 3,
                SmY = 0,
                SmH = 3,
            },
            new()
            {
                ID = widget2.ID,
                LgX = 0,
                LgY = 3,
                LgW = 8,
                LgH = 6,
                SmY = 3,
                SmH = 6,
            },
        };

        // Act
        await service.UpdateWidgetSettingsAsync(helper.demoUser.Id, batchRequests);

        // Assert
        var updated1 = helper.UserDataContext.WidgetSettings.Single(ws => ws.ID == widget1.ID);
        updated1.LgX.Should().Be(8);
        updated1.LgY.Should().Be(0);
        updated1.LgW.Should().Be(4);
        updated1.LgH.Should().Be(3);
        updated1.SmY.Should().Be(0);
        updated1.SmH.Should().Be(3);

        var updated2 = helper.UserDataContext.WidgetSettings.Single(ws => ws.ID == widget2.ID);
        updated2.LgX.Should().Be(0);
        updated2.LgY.Should().Be(3);
        updated2.LgW.Should().Be(8);
        updated2.LgH.Should().Be(6);
        updated2.SmY.Should().Be(3);
        updated2.SmH.Should().Be(6);
    }

    [Fact]
    public async Task UpdateWidgetSettingsAsync_WhenMetricWidgetConfigurationNull_ShouldRestoreMetricDefaultMarkup()
    {
        // Arrange
        var helper = new TestHelper();
        var service = CreateService(helper);

        var metricWidget = new WidgetSettings
        {
            ID = Guid.NewGuid(),
            WidgetType = WidgetTypes.Metric,
            LgX = 9,
            LgY = 0,
            LgW = 3,
            LgH = 8,
            SmY = 0,
            SmH = 6,
            Configuration = JsonSerializer.Serialize(new { markup = "custom metric markup" }),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(metricWidget);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await service.UpdateWidgetSettingsAsync(
            helper.demoUser.Id,
            new List<WidgetSettingsUpdateRequest>
            {
                new() { ID = metricWidget.ID, Configuration = null },
            }
        );

        // Assert
        var updated = helper.UserDataContext.WidgetSettings.Single(ws => ws.ID == metricWidget.ID);
        updated
            .Configuration.Should()
            .Be(JsonSerializer.Serialize(WidgetSettingsHelpers.DefaultMetricWidgetConfiguration));
    }

    [Fact]
    public async Task UpdateWidgetSettingsAsync_WhenNetWorthConfigurationIsInvalid_ShouldThrowWidgetConfigurationDeserializationError()
    {
        // Arrange
        var helper = new TestHelper();
        var service = CreateService(helper);

        var existingSettings = new WidgetSettings
        {
            ID = Guid.NewGuid(),
            WidgetType = WidgetTypes.NetWorth,
            LgX = 0,
            LgY = 5,
            LgW = 4,
            LgH = 5,
            SmY = 5,
            SmH = 5,
            Configuration = JsonSerializer.Serialize(
                WidgetSettingsHelpers.DefaultNetWorthWidgetConfiguration
            ),
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.Add(existingSettings);
        await helper.UserDataContext.SaveChangesAsync();

        var invalidConfig = "{ invalid json }";

        var updateRequest = new WidgetSettingsUpdateRequest
        {
            ID = existingSettings.ID,
            Configuration = JsonSerializer.SerializeToElement(invalidConfig),
        };

        // Act
        var action = async () =>
            await service.UpdateWidgetSettingsAsync(helper.demoUser.Id, [updateRequest]);

        // Assert
        await action
            .Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("WidgetConfigurationDeserializationError");
    }
    #endregion

    #region DeleteWidgetSettingsAsync
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
            LgX = 0,
            LgY = 5,
            LgW = 4,
            LgH = 5,
            SmY = 5,
            SmH = 5,
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
    #endregion

    #region ResetSmallScreenToLargeScreenLayoutAsync
    [Fact]
    public async Task ResetSmallScreenToLargeScreenLayoutAsync_WhenWidgetsExist_ShouldAssignSmPositionsInLgYOrder()
    {
        // Arrange
        var helper = new TestHelper();
        var service = CreateService(helper);

        // Three widgets at different LgY rows
        var widget1 = new WidgetSettings
        {
            ID = Guid.NewGuid(),
            WidgetType = WidgetTypes.Accounts,
            LgX = 0,
            LgY = 0,
            LgW = 4,
            LgH = 3,
            SmY = 99,
            SmH = 99,
            UserID = helper.demoUser.Id,
        };
        var widget2 = new WidgetSettings
        {
            ID = Guid.NewGuid(),
            WidgetType = WidgetTypes.NetWorth,
            LgX = 4,
            LgY = 5,
            LgW = 4,
            LgH = 6,
            SmY = 99,
            SmH = 99,
            UserID = helper.demoUser.Id,
        };
        var widget3 = new WidgetSettings
        {
            ID = Guid.NewGuid(),
            WidgetType = WidgetTypes.UncategorizedTransactions,
            LgX = 0,
            LgY = 11,
            LgW = 4,
            LgH = 4,
            SmY = 99,
            SmH = 99,
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.AddRange(widget1, widget2, widget3);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await service.ResetSmallScreenToLargeScreenLayoutAsync(helper.demoUser.Id);

        // Assert — SmY should stack by cumulative SmH in ascending (LgY, LgX) order; SmH mirrors LgH
        var result1 = helper.UserDataContext.WidgetSettings.Single(ws => ws.ID == widget1.ID);
        var result2 = helper.UserDataContext.WidgetSettings.Single(ws => ws.ID == widget2.ID);
        var result3 = helper.UserDataContext.WidgetSettings.Single(ws => ws.ID == widget3.ID);

        result1.SmY.Should().Be(0);
        result1.SmH.Should().Be(widget1.LgH);

        result2.SmY.Should().Be(result1.SmY + result1.SmH);
        result2.SmH.Should().Be(widget2.LgH);

        result3.SmY.Should().Be(result2.SmY + result2.SmH);
        result3.SmH.Should().Be(widget3.LgH);
    }

    [Fact]
    public async Task ResetSmallScreenToLargeScreenLayoutAsync_WhenMultipleWidgetsShareSameLgY_ShouldOrderByLgXAndStackByHeight()
    {
        // Arrange
        var helper = new TestHelper();
        var service = CreateService(helper);

        // Two widgets sharing LgY = 0, one widget at LgY = 5
        var widgetA = new WidgetSettings
        {
            ID = Guid.NewGuid(),
            WidgetType = WidgetTypes.Accounts,
            LgX = 0,
            LgY = 0,
            LgW = 4,
            LgH = 5,
            SmY = 99,
            SmH = 99,
            UserID = helper.demoUser.Id,
        };
        var widgetB = new WidgetSettings
        {
            ID = Guid.NewGuid(),
            WidgetType = WidgetTypes.NetWorth,
            LgX = 4,
            LgY = 0,
            LgW = 4,
            LgH = 4,
            SmY = 99,
            SmH = 99,
            UserID = helper.demoUser.Id,
        };
        var widgetC = new WidgetSettings
        {
            ID = Guid.NewGuid(),
            WidgetType = WidgetTypes.SpendingTrends,
            LgX = 0,
            LgY = 5,
            LgW = 8,
            LgH = 3,
            SmY = 99,
            SmH = 99,
            UserID = helper.demoUser.Id,
        };

        helper.UserDataContext.WidgetSettings.AddRange(widgetA, widgetB, widgetC);
        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await service.ResetSmallScreenToLargeScreenLayoutAsync(helper.demoUser.Id);

        // Assert — widgets are ordered by (LgY, LgX) and stacked by cumulative SmH
        var resultA = helper.UserDataContext.WidgetSettings.Single(ws => ws.ID == widgetA.ID);
        var resultB = helper.UserDataContext.WidgetSettings.Single(ws => ws.ID == widgetB.ID);
        var resultC = helper.UserDataContext.WidgetSettings.Single(ws => ws.ID == widgetC.ID);

        resultA.SmY.Should().Be(0);
        resultA.SmH.Should().Be(widgetA.LgH);

        resultB.SmY.Should().Be(resultA.SmY + resultA.SmH);
        resultB.SmH.Should().Be(widgetB.LgH);

        resultC.SmY.Should().Be(resultB.SmY + resultB.SmH);
        resultC.SmH.Should().Be(widgetC.LgH);
    }
    #endregion
}
