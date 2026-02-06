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
public class SyncServiceTests
{
    [Fact]
    public async Task SyncAsync_WhenCalled_ShouldUpdateLastSync()
    {
        // Arrange
        var helper = new TestHelper();

        var nowProviderMock = Mock.Of<INowProvider>();
        var fixedNow = DateTime.UtcNow;
        Mock.Get(nowProviderMock).Setup(_ => _.UtcNow).Returns(fixedNow);

        var applicationUserServiceMock = new Mock<IApplicationUserService>();
        applicationUserServiceMock
            .Setup(_ =>
                _.UpdateApplicationUserAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IApplicationUserUpdateRequest>()
                )
            )
            .Returns<Guid, IApplicationUserUpdateRequest>((id, req) => Task.CompletedTask)
            .Verifiable();

        using var httpClient = new HttpClient();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(_ => _.CreateClient(string.Empty)).Returns(httpClient);

        var simpleFinServiceMock = new Mock<ISimpleFinService>();
        simpleFinServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync([])
            .Verifiable();
        simpleFinServiceMock
            .Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>()))
            .ReturnsAsync([])
            .Verifiable();

        var syncService = new SyncService(
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            simpleFinServiceMock.Object,
            Mock.Of<ILunchFlowService>(),
            Mock.Of<IGoalService>(),
            applicationUserServiceMock.Object,
            Mock.Of<IAutomaticRuleService>(),
            nowProviderMock,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        await syncService.SyncAsync(helper.demoUser.Id);

        // Assert
        applicationUserServiceMock.Verify(_ =>
            _.UpdateApplicationUserAsync(
                helper.demoUser.Id,
                It.Is<IApplicationUserUpdateRequest>(req => req.LastSync == fixedNow)
            )
        );
    }

    [Fact]
    public async Task SyncAsync_WhenSimpleFinTokenIsNull_ShouldSkipSimpleFinCalls()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.SimpleFinAccessToken = null!;
        helper.UserDataContext.SaveChanges();

        var simpleFinServiceMock = new Mock<ISimpleFinService>();

        var syncService = new SyncService(
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            simpleFinServiceMock.Object,
            Mock.Of<ILunchFlowService>(),
            Mock.Of<IGoalService>(),
            Mock.Of<IApplicationUserService>(),
            Mock.Of<IAutomaticRuleService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        await syncService.SyncAsync(helper.demoUser.Id);

        // Assert
        simpleFinServiceMock.Verify(_ => _.RefreshAccountsAsync(It.IsAny<Guid>()), Times.Never);
        simpleFinServiceMock.Verify(
            _ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()),
            Times.Never
        );
    }

    [Fact]
    public async Task SyncAsync_WhenSimpleFinTokenIsEmpty_ShouldSkipSimpleFinCalls()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.SimpleFinAccessToken = string.Empty;
        helper.UserDataContext.SaveChanges();

        var simpleFinServiceMock = new Mock<ISimpleFinService>();

        var syncService = new SyncService(
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            simpleFinServiceMock.Object,
            Mock.Of<ILunchFlowService>(),
            Mock.Of<IGoalService>(),
            Mock.Of<IApplicationUserService>(),
            Mock.Of<IAutomaticRuleService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        await syncService.SyncAsync(helper.demoUser.Id);

        // Assert
        simpleFinServiceMock.Verify(_ => _.RefreshAccountsAsync(It.IsAny<Guid>()), Times.Never);
        simpleFinServiceMock.Verify(
            _ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()),
            Times.Never
        );
    }

    [Fact]
    public async Task SyncAsync_WhenSimpleFinTokenIsPresent_ShouldCallSimpleFinServices()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.SimpleFinAccessToken = "test-token";
        helper.UserDataContext.SaveChanges();

        var simpleFinServiceMock = new Mock<ISimpleFinService>();
        simpleFinServiceMock
            .Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>()))
            .ReturnsAsync([])
            .Verifiable();
        simpleFinServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync([])
            .Verifiable();

        var syncService = new SyncService(
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            simpleFinServiceMock.Object,
            Mock.Of<ILunchFlowService>(),
            Mock.Of<IGoalService>(),
            Mock.Of<IApplicationUserService>(),
            Mock.Of<IAutomaticRuleService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        await syncService.SyncAsync(helper.demoUser.Id);

        // Assert
        simpleFinServiceMock.Verify(_ => _.RefreshAccountsAsync(helper.demoUser.Id), Times.Once);
        simpleFinServiceMock.Verify(
            _ => _.SyncTransactionHistoryAsync(helper.demoUser.Id),
            Times.Once
        );
    }

    [Fact]
    public async Task SyncAsync_WhenRefreshAccountsReturnsErrors_ShouldReturnErrors()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.SimpleFinAccessToken = "test-token";
        helper.UserDataContext.SaveChanges();

        var expectedErrors = new List<string> { "Error 1", "Error 2" };
        var simpleFinServiceMock = new Mock<ISimpleFinService>();
        simpleFinServiceMock
            .Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(expectedErrors);
        simpleFinServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync([]);

        var syncService = new SyncService(
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            simpleFinServiceMock.Object,
            Mock.Of<ILunchFlowService>(),
            Mock.Of<IGoalService>(),
            Mock.Of<IApplicationUserService>(),
            Mock.Of<IAutomaticRuleService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        var errors = await syncService.SyncAsync(helper.demoUser.Id);

        // Assert
        errors.Should().BeEquivalentTo(expectedErrors);
    }

    [Fact]
    public async Task SyncAsync_WhenSyncTransactionHistoryReturnsErrors_ShouldReturnErrors()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.SimpleFinAccessToken = "test-token";
        helper.UserDataContext.SaveChanges();

        var expectedErrors = new List<string> { "Transaction Error 1", "Transaction Error 2" };
        var simpleFinServiceMock = new Mock<ISimpleFinService>();
        simpleFinServiceMock.Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>())).ReturnsAsync([]);
        simpleFinServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync(expectedErrors);

        var syncService = new SyncService(
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            simpleFinServiceMock.Object,
            Mock.Of<ILunchFlowService>(),
            Mock.Of<IGoalService>(),
            Mock.Of<IApplicationUserService>(),
            Mock.Of<IAutomaticRuleService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        var errors = await syncService.SyncAsync(helper.demoUser.Id);

        // Assert
        errors.Should().BeEquivalentTo(expectedErrors);
    }

    [Fact]
    public async Task SyncAsync_WhenBothSimpleFinMethodsReturnErrors_ShouldCombineAllErrors()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.SimpleFinAccessToken = "test-token";
        helper.UserDataContext.SaveChanges();

        var accountErrors = new List<string> { "Account Error 1" };
        var transactionErrors = new List<string> { "Transaction Error 1", "Transaction Error 2" };
        var simpleFinServiceMock = new Mock<ISimpleFinService>();
        simpleFinServiceMock
            .Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(accountErrors);
        simpleFinServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync(transactionErrors);

        var syncService = new SyncService(
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            simpleFinServiceMock.Object,
            Mock.Of<ILunchFlowService>(),
            Mock.Of<IGoalService>(),
            Mock.Of<IApplicationUserService>(),
            Mock.Of<IAutomaticRuleService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        var errors = await syncService.SyncAsync(helper.demoUser.Id);

        // Assert
        errors.Should().HaveCount(3);
        errors.Should().Contain(accountErrors);
        errors.Should().Contain(transactionErrors);
    }

    [Fact]
    public async Task SyncAsync_ShouldCallCompleteGoalsAsync()
    {
        // Arrange
        var helper = new TestHelper();

        var goalServiceMock = new Mock<IGoalService>();
        goalServiceMock
            .Setup(_ => _.CompleteGoalsAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var simpleFinServiceMock = new Mock<ISimpleFinService>();
        simpleFinServiceMock.Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>())).ReturnsAsync([]);
        simpleFinServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync([]);

        var syncService = new SyncService(
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            simpleFinServiceMock.Object,
            Mock.Of<ILunchFlowService>(),
            goalServiceMock.Object,
            Mock.Of<IApplicationUserService>(),
            Mock.Of<IAutomaticRuleService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        await syncService.SyncAsync(helper.demoUser.Id);

        // Assert
        goalServiceMock.Verify(_ => _.CompleteGoalsAsync(helper.demoUser.Id), Times.Once);
    }

    [Fact]
    public async Task SyncAsync_ShouldCallRunAutomaticRulesAsync()
    {
        // Arrange
        var helper = new TestHelper();

        var automaticRuleServiceMock = new Mock<IAutomaticRuleService>();
        automaticRuleServiceMock
            .Setup(_ => _.RunAutomaticRulesAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var simpleFinServiceMock = new Mock<ISimpleFinService>();
        simpleFinServiceMock.Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>())).ReturnsAsync([]);
        simpleFinServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync([]);

        var syncService = new SyncService(
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            simpleFinServiceMock.Object,
            Mock.Of<ILunchFlowService>(),
            Mock.Of<IGoalService>(),
            Mock.Of<IApplicationUserService>(),
            automaticRuleServiceMock.Object,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        await syncService.SyncAsync(helper.demoUser.Id);

        // Assert
        automaticRuleServiceMock.Verify(
            _ => _.RunAutomaticRulesAsync(helper.demoUser.Id),
            Times.Once
        );
    }

    [Fact]
    public async Task SyncAsync_WithoutSimpleFinToken_ShouldStillCallGoalsAndRules()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.SimpleFinAccessToken = null!;
        helper.UserDataContext.SaveChanges();

        var goalServiceMock = new Mock<IGoalService>();
        goalServiceMock
            .Setup(_ => _.CompleteGoalsAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var automaticRuleServiceMock = new Mock<IAutomaticRuleService>();
        automaticRuleServiceMock
            .Setup(_ => _.RunAutomaticRulesAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var syncService = new SyncService(
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            Mock.Of<ISimpleFinService>(),
            Mock.Of<ILunchFlowService>(),
            goalServiceMock.Object,
            Mock.Of<IApplicationUserService>(),
            automaticRuleServiceMock.Object,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        await syncService.SyncAsync(helper.demoUser.Id);

        // Assert
        goalServiceMock.Verify(_ => _.CompleteGoalsAsync(helper.demoUser.Id), Times.Once);
        automaticRuleServiceMock.Verify(
            _ => _.RunAutomaticRulesAsync(helper.demoUser.Id),
            Times.Once
        );
    }

    [Fact]
    public async Task SyncAsync_InvalidUserId_ThrowsException()
    {
        // Arrange
        var helper = new TestHelper();

        var simpleFinServiceMock = new Mock<ISimpleFinService>();
        simpleFinServiceMock.Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>())).ReturnsAsync([]);
        simpleFinServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync([]);

        var syncService = new SyncService(
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            simpleFinServiceMock.Object,
            Mock.Of<ILunchFlowService>(),
            Mock.Of<IGoalService>(),
            Mock.Of<IApplicationUserService>(),
            Mock.Of<IAutomaticRuleService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        Func<Task> act = async () => await syncService.SyncAsync(Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidUserError");
    }

    [Fact]
    public async Task SyncAsync_WhenSimpleFinErrorsOccur_ShouldStillUpdateLastSync()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.SimpleFinAccessToken = "test-token";
        helper.UserDataContext.SaveChanges();

        var nowProviderMock = Mock.Of<INowProvider>();
        var fixedNow = DateTime.UtcNow;
        Mock.Get(nowProviderMock).Setup(_ => _.UtcNow).Returns(fixedNow);

        var simpleFinServiceMock = new Mock<ISimpleFinService>();
        simpleFinServiceMock
            .Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<string> { "Some error" });
        simpleFinServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync([]);

        var applicationUserServiceMock = new Mock<IApplicationUserService>();
        applicationUserServiceMock
            .Setup(_ =>
                _.UpdateApplicationUserAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IApplicationUserUpdateRequest>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable();

        var syncService = new SyncService(
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            simpleFinServiceMock.Object,
            Mock.Of<ILunchFlowService>(),
            Mock.Of<IGoalService>(),
            applicationUserServiceMock.Object,
            Mock.Of<IAutomaticRuleService>(),
            nowProviderMock,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        await syncService.SyncAsync(helper.demoUser.Id);

        // Assert
        applicationUserServiceMock.Verify(
            _ =>
                _.UpdateApplicationUserAsync(
                    helper.demoUser.Id,
                    It.Is<IApplicationUserUpdateRequest>(req => req.LastSync == fixedNow)
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task SyncAsync_WhenLunchFlowApiKeyIsNull_ShouldSkipLunchFlowCalls()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.LunchFlowApiKey = null!;
        helper.UserDataContext.SaveChanges();

        var simpleFinServiceMock = new Mock<ISimpleFinService>();
        simpleFinServiceMock.Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>())).ReturnsAsync([]);
        simpleFinServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync([]);

        var lunchFlowServiceMock = new Mock<ILunchFlowService>();

        var syncService = new SyncService(
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            simpleFinServiceMock.Object,
            lunchFlowServiceMock.Object,
            Mock.Of<IGoalService>(),
            Mock.Of<IApplicationUserService>(),
            Mock.Of<IAutomaticRuleService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        await syncService.SyncAsync(helper.demoUser.Id);

        // Assert
        lunchFlowServiceMock.Verify(_ => _.RefreshAccountsAsync(It.IsAny<Guid>()), Times.Never);
        lunchFlowServiceMock.Verify(
            _ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()),
            Times.Never
        );
    }

    [Fact]
    public async Task SyncAsync_WhenLunchFlowApiKeyIsEmpty_ShouldSkipLunchFlowCalls()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.LunchFlowApiKey = string.Empty;
        helper.UserDataContext.SaveChanges();

        var simpleFinServiceMock = new Mock<ISimpleFinService>();
        simpleFinServiceMock.Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>())).ReturnsAsync([]);
        simpleFinServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync([]);

        var lunchFlowServiceMock = new Mock<ILunchFlowService>();

        var syncService = new SyncService(
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            simpleFinServiceMock.Object,
            lunchFlowServiceMock.Object,
            Mock.Of<IGoalService>(),
            Mock.Of<IApplicationUserService>(),
            Mock.Of<IAutomaticRuleService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        await syncService.SyncAsync(helper.demoUser.Id);

        // Assert
        lunchFlowServiceMock.Verify(_ => _.RefreshAccountsAsync(It.IsAny<Guid>()), Times.Never);
        lunchFlowServiceMock.Verify(
            _ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()),
            Times.Never
        );
    }

    [Fact]
    public async Task SyncAsync_WhenLunchFlowApiKeyIsPresent_ShouldCallLunchFlowServices()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.LunchFlowApiKey = "valid-api-key";
        helper.UserDataContext.SaveChanges();

        var simpleFinServiceMock = new Mock<ISimpleFinService>();
        simpleFinServiceMock.Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>())).ReturnsAsync([]);
        simpleFinServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync([]);

        var lunchFlowServiceMock = new Mock<ILunchFlowService>();
        lunchFlowServiceMock
            .Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>()))
            .ReturnsAsync([])
            .Verifiable();
        lunchFlowServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync([])
            .Verifiable();

        var syncService = new SyncService(
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            simpleFinServiceMock.Object,
            lunchFlowServiceMock.Object,
            Mock.Of<IGoalService>(),
            Mock.Of<IApplicationUserService>(),
            Mock.Of<IAutomaticRuleService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        await syncService.SyncAsync(helper.demoUser.Id);

        // Assert
        lunchFlowServiceMock.Verify(_ => _.RefreshAccountsAsync(helper.demoUser.Id), Times.Once);
        lunchFlowServiceMock.Verify(
            _ => _.SyncTransactionHistoryAsync(helper.demoUser.Id),
            Times.Once
        );
    }

    [Fact]
    public async Task SyncAsync_WhenLunchFlowRefreshAccountsReturnsErrors_ShouldReturnErrors()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.LunchFlowApiKey = "valid-api-key";
        helper.UserDataContext.SaveChanges();

        var simpleFinServiceMock = new Mock<ISimpleFinService>();
        simpleFinServiceMock.Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>())).ReturnsAsync([]);
        simpleFinServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync([]);

        var lunchFlowServiceMock = new Mock<ILunchFlowService>();
        lunchFlowServiceMock
            .Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<string> { "LunchFlow Error 1", "LunchFlow Error 2" });
        lunchFlowServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync([]);

        var syncService = new SyncService(
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            simpleFinServiceMock.Object,
            lunchFlowServiceMock.Object,
            Mock.Of<IGoalService>(),
            Mock.Of<IApplicationUserService>(),
            Mock.Of<IAutomaticRuleService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        var errors = await syncService.SyncAsync(helper.demoUser.Id);

        // Assert
        errors.Should().HaveCount(2);
        errors.Should().Contain("LunchFlow Error 1");
        errors.Should().Contain("LunchFlow Error 2");
    }

    [Fact]
    public async Task SyncAsync_WhenLunchFlowSyncTransactionHistoryReturnsErrors_ShouldReturnErrors()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.LunchFlowApiKey = "valid-api-key";
        helper.UserDataContext.SaveChanges();

        var simpleFinServiceMock = new Mock<ISimpleFinService>();
        simpleFinServiceMock.Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>())).ReturnsAsync([]);
        simpleFinServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync([]);

        var lunchFlowServiceMock = new Mock<ILunchFlowService>();
        lunchFlowServiceMock.Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>())).ReturnsAsync([]);
        lunchFlowServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync(
                new List<string>
                {
                    "Transaction Error 1",
                    "Transaction Error 2",
                    "Transaction Error 3",
                }
            );

        var syncService = new SyncService(
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            simpleFinServiceMock.Object,
            lunchFlowServiceMock.Object,
            Mock.Of<IGoalService>(),
            Mock.Of<IApplicationUserService>(),
            Mock.Of<IAutomaticRuleService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        var errors = await syncService.SyncAsync(helper.demoUser.Id);

        // Assert
        errors.Should().HaveCount(3);
        errors.Should().Contain("Transaction Error 1");
        errors.Should().Contain("Transaction Error 2");
        errors.Should().Contain("Transaction Error 3");
    }

    [Fact]
    public async Task SyncAsync_WhenBothLunchFlowMethodsReturnErrors_ShouldCombineAllErrors()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.LunchFlowApiKey = "valid-api-key";
        helper.UserDataContext.SaveChanges();

        var simpleFinServiceMock = new Mock<ISimpleFinService>();
        simpleFinServiceMock.Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>())).ReturnsAsync([]);
        simpleFinServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync([]);

        var lunchFlowServiceMock = new Mock<ILunchFlowService>();
        lunchFlowServiceMock
            .Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<string> { "Refresh Error" });
        lunchFlowServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<string> { "Sync Error" });

        var syncService = new SyncService(
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            simpleFinServiceMock.Object,
            lunchFlowServiceMock.Object,
            Mock.Of<IGoalService>(),
            Mock.Of<IApplicationUserService>(),
            Mock.Of<IAutomaticRuleService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        var errors = await syncService.SyncAsync(helper.demoUser.Id);

        // Assert
        errors.Should().HaveCount(2);
        errors.Should().Contain("Refresh Error");
        errors.Should().Contain("Sync Error");
    }

    [Fact]
    public async Task SyncAsync_WhenBothSimpleFinAndLunchFlowAreConfigured_ShouldCallBoth()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.SimpleFinAccessToken = "simplefin-token";
        helper.demoUser.LunchFlowApiKey = "lunchflow-key";
        helper.UserDataContext.SaveChanges();

        var simpleFinServiceMock = new Mock<ISimpleFinService>();
        simpleFinServiceMock
            .Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>()))
            .ReturnsAsync([])
            .Verifiable();
        simpleFinServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync([])
            .Verifiable();

        var lunchFlowServiceMock = new Mock<ILunchFlowService>();
        lunchFlowServiceMock
            .Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>()))
            .ReturnsAsync([])
            .Verifiable();
        lunchFlowServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync([])
            .Verifiable();

        var syncService = new SyncService(
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            simpleFinServiceMock.Object,
            lunchFlowServiceMock.Object,
            Mock.Of<IGoalService>(),
            Mock.Of<IApplicationUserService>(),
            Mock.Of<IAutomaticRuleService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        await syncService.SyncAsync(helper.demoUser.Id);

        // Assert
        simpleFinServiceMock.Verify(_ => _.RefreshAccountsAsync(helper.demoUser.Id), Times.Once);
        simpleFinServiceMock.Verify(
            _ => _.SyncTransactionHistoryAsync(helper.demoUser.Id),
            Times.Once
        );
        lunchFlowServiceMock.Verify(_ => _.RefreshAccountsAsync(helper.demoUser.Id), Times.Once);
        lunchFlowServiceMock.Verify(
            _ => _.SyncTransactionHistoryAsync(helper.demoUser.Id),
            Times.Once
        );
    }

    [Fact]
    public async Task SyncAsync_WhenBothProvidersReturnErrors_ShouldCombineAllErrors()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.SimpleFinAccessToken = "simplefin-token";
        helper.demoUser.LunchFlowApiKey = "lunchflow-key";
        helper.UserDataContext.SaveChanges();

        var simpleFinServiceMock = new Mock<ISimpleFinService>();
        simpleFinServiceMock
            .Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<string> { "SimpleFin Error 1" });
        simpleFinServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<string> { "SimpleFin Error 2" });

        var lunchFlowServiceMock = new Mock<ILunchFlowService>();
        lunchFlowServiceMock
            .Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<string> { "LunchFlow Error 1" });
        lunchFlowServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<string> { "LunchFlow Error 2" });

        var syncService = new SyncService(
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            simpleFinServiceMock.Object,
            lunchFlowServiceMock.Object,
            Mock.Of<IGoalService>(),
            Mock.Of<IApplicationUserService>(),
            Mock.Of<IAutomaticRuleService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        var errors = await syncService.SyncAsync(helper.demoUser.Id);

        // Assert
        errors.Should().HaveCount(4);
        errors.Should().Contain("SimpleFin Error 1");
        errors.Should().Contain("SimpleFin Error 2");
        errors.Should().Contain("LunchFlow Error 1");
        errors.Should().Contain("LunchFlow Error 2");
    }

    [Fact]
    public async Task SyncAsync_WithoutBothTokens_ShouldStillCallGoalsAndRules()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.SimpleFinAccessToken = string.Empty;
        helper.demoUser.LunchFlowApiKey = string.Empty;
        helper.UserDataContext.SaveChanges();

        var goalServiceMock = new Mock<IGoalService>();
        goalServiceMock
            .Setup(_ => _.CompleteGoalsAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var automaticRuleServiceMock = new Mock<IAutomaticRuleService>();
        automaticRuleServiceMock
            .Setup(_ => _.RunAutomaticRulesAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var syncService = new SyncService(
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            Mock.Of<ISimpleFinService>(),
            Mock.Of<ILunchFlowService>(),
            goalServiceMock.Object,
            Mock.Of<IApplicationUserService>(),
            automaticRuleServiceMock.Object,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        await syncService.SyncAsync(helper.demoUser.Id);

        // Assert
        goalServiceMock.Verify(_ => _.CompleteGoalsAsync(helper.demoUser.Id), Times.Once);
        automaticRuleServiceMock.Verify(
            _ => _.RunAutomaticRulesAsync(helper.demoUser.Id),
            Times.Once
        );
    }

    [Fact]
    public async Task SyncAsync_WhenLunchFlowErrorsOccur_ShouldStillUpdateLastSync()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.LunchFlowApiKey = "lunchflow-key";
        helper.UserDataContext.SaveChanges();

        var nowProviderMock = Mock.Of<INowProvider>();
        var fixedNow = DateTime.UtcNow;
        Mock.Get(nowProviderMock).Setup(_ => _.UtcNow).Returns(fixedNow);

        var simpleFinServiceMock = new Mock<ISimpleFinService>();
        simpleFinServiceMock.Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>())).ReturnsAsync([]);
        simpleFinServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync([]);

        var lunchFlowServiceMock = new Mock<ILunchFlowService>();
        lunchFlowServiceMock
            .Setup(_ => _.RefreshAccountsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<string> { "Some error" });
        lunchFlowServiceMock
            .Setup(_ => _.SyncTransactionHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync([]);

        var applicationUserServiceMock = new Mock<IApplicationUserService>();
        applicationUserServiceMock
            .Setup(_ =>
                _.UpdateApplicationUserAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IApplicationUserUpdateRequest>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable();

        var syncService = new SyncService(
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            simpleFinServiceMock.Object,
            lunchFlowServiceMock.Object,
            Mock.Of<IGoalService>(),
            applicationUserServiceMock.Object,
            Mock.Of<IAutomaticRuleService>(),
            nowProviderMock,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        await syncService.SyncAsync(helper.demoUser.Id);

        // Assert
        applicationUserServiceMock.Verify(
            _ =>
                _.UpdateApplicationUserAsync(
                    helper.demoUser.Id,
                    It.Is<IApplicationUserUpdateRequest>(req => req.LastSync == fixedNow)
                ),
            Times.Once
        );
    }
}
