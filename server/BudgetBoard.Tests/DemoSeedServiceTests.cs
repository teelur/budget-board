using BudgetBoard.Database.Models;
using BudgetBoard.Service;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class DemoSeedServiceTests
{
    private static readonly Guid DemoUserId = new("00000000-0000-0000-0000-000000000001");
    private const string DemoEmail = "demo@example.com";

    private static Mock<UserManager<ApplicationUser>> CreateMockUserManager(TestHelper helper)
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!
        );

        mockUserManager.Setup(um => um.Users).Returns(() => helper.UserDataContext.Users);

        mockUserManager
            .Setup(um => um.DeleteAsync(It.IsAny<ApplicationUser>()))
            .Callback<ApplicationUser>(user =>
            {
                helper.UserDataContext.Users.Remove(user);
                helper.UserDataContext.SaveChanges();
            })
            .ReturnsAsync(IdentityResult.Success);

        mockUserManager
            .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Callback<ApplicationUser, string>(
                (user, _) =>
                {
                    helper.UserDataContext.Users.Add(user);
                    helper.UserDataContext.SaveChanges();
                }
            )
            .ReturnsAsync(IdentityResult.Success);

        return mockUserManager;
    }

    private static DemoSeedService CreateService(
        TestHelper helper,
        Mock<UserManager<ApplicationUser>> mockUserManager
    )
    {
        var mockTransactionService = new Mock<ITransactionService>();
        mockTransactionService
            .Setup(ts =>
                ts.CreateTransactionAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<ITransactionCreateRequest>(),
                    It.IsAny<IEnumerable<ITransactionCategory>>(),
                    It.IsAny<AutomaticTransactionCategorizerHelper>()
                )
            )
            .Returns(Task.CompletedTask);

        var widgetSettingsService = new WidgetSettingsService(
            Mock.Of<ILogger<IWidgetSettingsService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        return new DemoSeedService(
            Mock.Of<ILogger<DemoSeedService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            mockUserManager.Object,
            TestHelper.CreateMockLocalizer<LogStrings>(),
            mockTransactionService.Object,
            widgetSettingsService
        );
    }

    [Fact]
    public async Task ResetAndSeedAsync_WhenCalled_DeletesAllExistingUsers()
    {
        // Arrange
        var helper = new TestHelper();
        var mockUserManager = CreateMockUserManager(helper);
        var service = CreateService(helper, mockUserManager);

        // Act
        await service.ResetAndSeedAsync();

        // Assert — the seeded demoUser from TestHelper should be gone; only the new demo user remains
        helper.UserDataContext.Users.Should().HaveCount(1);
        helper.UserDataContext.Users.Single().Email.Should().Be(DemoEmail);
    }

    [Fact]
    public async Task ResetAndSeedAsync_WhenCalled_CreatesDemoUserWithFixedId()
    {
        // Arrange
        var helper = new TestHelper();
        var mockUserManager = CreateMockUserManager(helper);
        var service = CreateService(helper, mockUserManager);

        // Act
        await service.ResetAndSeedAsync();

        // Assert
        var demoUser = helper.UserDataContext.Users.Single();
        demoUser.Id.Should().Be(DemoUserId);
        demoUser.Email.Should().Be(DemoEmail);
        demoUser.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task ResetAndSeedAsync_WhenCalled_SeedsExpectedInstitutions()
    {
        // Arrange
        var helper = new TestHelper();
        var mockUserManager = CreateMockUserManager(helper);
        var service = CreateService(helper, mockUserManager);

        // Act
        await service.ResetAndSeedAsync();

        // Assert
        var institutions = helper
            .UserDataContext.Institutions.Where(i => i.UserID == DemoUserId)
            .ToList();
        institutions.Should().HaveCount(2);
        institutions
            .Select(i => i.Name)
            .Should()
            .BeEquivalentTo(["Greenfield Bank", "Summit Credit Union"]);
    }

    [Fact]
    public async Task ResetAndSeedAsync_WhenCalled_SeedsExpectedAccounts()
    {
        // Arrange
        var helper = new TestHelper();
        var mockUserManager = CreateMockUserManager(helper);
        var service = CreateService(helper, mockUserManager);

        // Act
        await service.ResetAndSeedAsync();

        // Assert
        var accounts = helper.UserDataContext.Accounts.Where(a => a.UserID == DemoUserId).ToList();
        accounts.Should().HaveCount(4);
        accounts
            .Select(a => a.Name)
            .Should()
            .BeEquivalentTo(["Checking", "Savings", "Visa Rewards", "Brokerage"]);
    }

    [Fact]
    public async Task ResetAndSeedAsync_WhenCalled_SeedsExpectedBudgets()
    {
        // Arrange
        var helper = new TestHelper();
        var mockUserManager = CreateMockUserManager(helper);
        var service = CreateService(helper, mockUserManager);

        // Act
        await service.ResetAndSeedAsync();

        // Assert
        var budgets = helper.UserDataContext.Budgets.Where(b => b.UserID == DemoUserId).ToList();
        budgets.Should().NotBeEmpty();
        budgets
            .Select(b => b.Category)
            .Should()
            .Contain(
                [
                    "Food & Dining",
                    "Shopping",
                    "Auto & Transport",
                    "Entertainment",
                    "Bills & Utilities",
                    "Health & Fitness",
                    "Income",
                ]
            );
    }

    [Fact]
    public async Task ResetAndSeedAsync_WhenCalled_SeedsAsset()
    {
        // Arrange
        var helper = new TestHelper();
        var mockUserManager = CreateMockUserManager(helper);
        var service = CreateService(helper, mockUserManager);

        // Act
        await service.ResetAndSeedAsync();

        // Assert
        var assets = helper.UserDataContext.Assets.Where(a => a.UserID == DemoUserId).ToList();
        assets.Should().HaveCount(1);
        assets.Single().Name.Should().Be("Primary Residence");
    }

    [Fact]
    public async Task ResetAndSeedAsync_WhenCalled_SeedsGoal()
    {
        // Arrange
        var helper = new TestHelper();
        var mockUserManager = CreateMockUserManager(helper);
        var service = CreateService(helper, mockUserManager);

        // Act
        await service.ResetAndSeedAsync();

        // Assert
        var goals = helper.UserDataContext.Goals.Where(g => g.UserID == DemoUserId).ToList();
        goals.Should().HaveCount(1);
        goals.Single().Name.Should().Be("Emergency Fund");
    }

    [Fact]
    public async Task ResetAndSeedAsync_WhenCalled_SeedsWidgetSettings()
    {
        // Arrange
        var helper = new TestHelper();
        var mockUserManager = CreateMockUserManager(helper);
        var service = CreateService(helper, mockUserManager);

        // Act
        await service.ResetAndSeedAsync();

        // Assert
        var widgetSettings = helper
            .UserDataContext.WidgetSettings.Where(ws => ws.UserID == DemoUserId)
            .ToList();
        widgetSettings.Should().HaveCount(WidgetSettingsHelpers.DefaultLayouts.Count);
        widgetSettings
            .Select(ws => ws.WidgetType)
            .Should()
            .BeEquivalentTo(WidgetSettingsHelpers.DefaultLayouts.Select(dl => dl.WidgetType));

        // Widgets with configurations: Accounts, NetWorth, Metric
        widgetSettings
            .Where(ws =>
                ws.WidgetType is WidgetTypes.Accounts or WidgetTypes.NetWorth or WidgetTypes.Metric
            )
            .Should()
            .OnlyContain(ws => !string.IsNullOrWhiteSpace(ws.Configuration));

        // Widgets without configurations: UncategorizedTransactions, SpendingTrends
        widgetSettings
            .Where(ws =>
                ws.WidgetType is WidgetTypes.UncategorizedTransactions or WidgetTypes.SpendingTrends
            )
            .Should()
            .OnlyContain(ws => ws.Configuration == null);

        var accountsWidget = widgetSettings.First(ws => ws.WidgetType == WidgetTypes.Accounts);
        accountsWidget.Configuration.Should().Be("{\"accountIds\":[]}");
    }

    [Fact]
    public async Task ResetAndSeedAsync_WhenCalled_SeedsUserSettings()
    {
        // Arrange
        var helper = new TestHelper();
        var mockUserManager = CreateMockUserManager(helper);
        var service = CreateService(helper, mockUserManager);

        // Act
        await service.ResetAndSeedAsync();

        // Assert
        helper
            .UserDataContext.UserSettings.Where(us => us.UserID == DemoUserId)
            .Should()
            .HaveCount(1);
    }

    [Fact]
    public async Task ResetAndSeedAsync_WhenCreateUserFails_DoesNotSeedData()
    {
        // Arrange
        var helper = new TestHelper();
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!
        );

        mockUserManager.Setup(um => um.Users).Returns(() => helper.UserDataContext.Users);

        mockUserManager
            .Setup(um => um.DeleteAsync(It.IsAny<ApplicationUser>()))
            .Callback<ApplicationUser>(user =>
            {
                helper.UserDataContext.Users.Remove(user);
                helper.UserDataContext.SaveChanges();
            })
            .ReturnsAsync(IdentityResult.Success);

        mockUserManager
            .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Test error" }));

        var service = CreateService(helper, mockUserManager);

        // Act
        await service.ResetAndSeedAsync();

        // Assert — no accounts or institutions should have been seeded
        helper.UserDataContext.Accounts.Should().BeEmpty();
        helper.UserDataContext.Institutions.Should().BeEmpty();
    }
}
