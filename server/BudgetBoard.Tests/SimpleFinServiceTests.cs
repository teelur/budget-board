using BudgetBoard.Service;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Resources;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class SimpleFinServiceTests
{
    // This test is a quick and dirty check that values from the SimpleFIN demo get added to the database.
    // There's no validation that the data is correct, so more testing may be needed.
    [Fact]
    public async Task SyncTransactionHistoryAsync_WhenCalledWithValidData_ShouldUpdateWithSyncedData()
    {
        // Arrange
        var helper = new TestHelper();

        using var httpClient = new HttpClient();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var nowProviderMock = Mock.Of<INowProvider>();
        Mock.Get(nowProviderMock).Setup(_ => _.UtcNow).Returns(DateTime.UtcNow);

        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            nowProviderMock,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );
        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            nowProviderMock,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext,
            nowProviderMock,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var simpleFinService = new SimpleFinService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ISyncProvider>>(),
            nowProviderMock,
            accountService,
            transactionService,
            balanceService,
            Mock.Of<ISimpleFinOrganizationService>(),
            Mock.Of<ISimpleFinAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // This is a demo token provided by SimpleFIN for dev.
        helper.demoUser.SimpleFinAccessToken =
            "https://demo:demo@beta-bridge.simplefin.org/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.SyncTransactionHistoryAsync(helper.demoUser.Id);

        // Assert
        errors.Should().BeEmpty();
        helper.UserDataContext.Institutions.Should().NotBeEmpty();
        helper.UserDataContext.Accounts.Should().NotBeEmpty();
        helper.UserDataContext.Transactions.Should().NotBeEmpty();
        helper.UserDataContext.Balances.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateAccessToken_WhenCalledWithDemoSetupToken_ShouldUpdateAccessToken()
    {
        // Arrange
        var helper = new TestHelper();

        using var httpClient = new HttpClient();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var simpleFinService = new SimpleFinService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ISyncProvider>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ISimpleFinOrganizationService>(),
            Mock.Of<ISimpleFinAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // This is a demo token provided by SimpleFIN for dev.
        var accessToken =
            "aHR0cHM6Ly9iZXRhLWJyaWRnZS5zaW1wbGVmaW4ub3JnL3NpbXBsZWZpbi9jbGFpbS9ERU1P";

        // Act
        await simpleFinService.ConfigureAccessTokenAsync(helper.demoUser.Id, accessToken);

        // Assert
        helper
            .UserDataContext.Users.Single()
            .SimpleFinAccessToken.Should()
            .Be("https://demo:demo@beta-bridge.simplefin.org/simplefin");
    }
}
