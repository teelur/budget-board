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
    public async Task SyncAsync_WhenCalled_ShouldBackfillMissingAccountSource()
    {
        // Arrange
        var helper = new TestHelper();

        var nowProviderMock = Mock.Of<INowProvider>();
        Mock.Get(nowProviderMock).Setup(_ => _.UtcNow).Returns(DateTime.UtcNow);

        var simpleFinServiceMock = new Mock<ISyncProvider>();
        simpleFinServiceMock
            .Setup(_ => _.SyncDataAsync(It.IsAny<Guid>()))
            .ReturnsAsync([])
            .Verifiable();

        var syncService = new SyncService(
            Mock.Of<IHttpClientFactory>(),
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            simpleFinServiceMock.Object,
            Mock.Of<ITransactionService>(),
            Mock.Of<IGoalService>(),
            Mock.Of<IApplicationUserService>(),
            Mock.Of<IAutomaticRuleService>(),
            nowProviderMock,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var accountWithoutSource = accountFaker.Generate();
        accountWithoutSource.SyncID = null;
        accountWithoutSource.Source = string.Empty;
        helper.UserDataContext.Accounts.Add(accountWithoutSource);

        var accountWithSyncID = accountFaker.Generate();
        accountWithSyncID.SyncID = "existing-sync-id";
        accountWithSyncID.Source = string.Empty;
        helper.UserDataContext.Accounts.Add(accountWithSyncID);

        await helper.UserDataContext.SaveChangesAsync();

        // Act
        await syncService.SyncAsync(helper.demoUser.Id);

        // Assert
        helper
            .UserDataContext.Accounts.Single(a => a.ID == accountWithoutSource.ID)
            .Source.Should()
            .Be(AccountSource.Manual);
        helper
            .UserDataContext.Accounts.Single(a => a.ID == accountWithSyncID.ID)
            .Source.Should()
            .Be(AccountSource.SimpleFIN);
    }

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

        var syncProviderMock = new Mock<ISyncProvider>();
        syncProviderMock
            .Setup(_ => _.SyncDataAsync(It.IsAny<Guid>()))
            .ReturnsAsync([])
            .Verifiable();

        var syncService = new SyncService(
            httpClientFactoryMock.Object,
            Mock.Of<ILogger<ISyncService>>(),
            helper.UserDataContext,
            syncProviderMock.Object,
            Mock.Of<ITransactionService>(),
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
}
