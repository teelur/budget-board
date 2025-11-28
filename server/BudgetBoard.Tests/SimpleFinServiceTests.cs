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
    public async Task SyncAsync_WhenCalledWithValidData_ShouldUpdateWithSyncedData()
    {
        // Arrange
        var helper = new TestHelper();

        var httpClient = new HttpClient();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var nowProviderMock = Mock.Of<INowProvider>();
        Mock.Get(nowProviderMock).Setup(_ => _.UtcNow).Returns(DateTime.UtcNow);

        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            nowProviderMock,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );
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
            institutionService,
            transactionService,
            balanceService,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // This is a demo token provided by SimpleFIN for dev.
        helper.demoUser.AccessToken = "https://demo:demo@beta-bridge.simplefin.org/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.SyncDataAsync(helper.demoUser.Id);

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
        var httpClient = new HttpClient();
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
            Mock.Of<IInstitutionService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
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
            .AccessToken.Should()
            .Be("https://demo:demo@beta-bridge.simplefin.org/simplefin");
    }
}
