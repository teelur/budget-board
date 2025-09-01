using Bogus;
using BudgetBoard.Database.Models;
using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class SimpleFinServiceTests
{
    // This test depends on the SimpleFIN service decoding the setup token.
    // If this test ends up being flakey, might be worth to Mock the SimpleFIN service part of the test to return the expected data.
    // For now, it is a nice integration test to ensure the SimpleFIN service is working as expected.
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
            Mock.Of<ILogger<ISimpleFinService>>(),
            helper.UserDataContext,
            Mock.Of<IAccountService>(),
            Mock.Of<IInstitutionService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<IGoalService>(),
            Mock.Of<IApplicationUserService>(),
            Mock.Of<IAutomaticRuleService>(),
            Mock.Of<INowProvider>()
        );

        // This is a demo token provided by SimpleFIN for dev.
        var accessToken =
            "aHR0cHM6Ly9iZXRhLWJyaWRnZS5zaW1wbGVmaW4ub3JnL3NpbXBsZWZpbi9jbGFpbS9ERU1P";

        // Act
        await simpleFinService.UpdateAccessTokenFromSetupToken(helper.demoUser.Id, accessToken);

        // Assert
        helper
            .UserDataContext.Users.Single()
            .AccessToken.Should()
            .Be("https://demo:demo@beta-bridge.simplefin.org/simplefin");
    }

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

        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );
        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext
        );
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );
        var applicationUserService = new ApplicationUserService(
            Mock.Of<ILogger<IApplicationUserService>>(),
            helper.UserDataContext
        );
        var automaticRuleService = new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext
        );

        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProvider = Mock.Of<INowProvider>();
        Mock.Get(nowProvider).Setup(_ => _.UtcNow).Returns(fakeDate);

        var simpleFinService = new SimpleFinService(
            httpClientFactoryMock.Object,
            Mock.Of<ILogger<ISimpleFinService>>(),
            helper.UserDataContext,
            accountService,
            institutionService,
            transactionService,
            balanceService,
            goalService,
            applicationUserService,
            automaticRuleService,
            nowProvider
        );

        // This is a demo token provided by SimpleFIN for dev.
        helper.demoUser.AccessToken = "https://demo:demo@beta-bridge.simplefin.org/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        await simpleFinService.SyncAsync(helper.demoUser.Id);

        // Assert
        helper.UserDataContext.Institutions.Should().NotBeEmpty();
        helper.UserDataContext.Accounts.Should().NotBeEmpty();
        helper.UserDataContext.Transactions.Should().NotBeEmpty();
        helper.UserDataContext.Balances.Should().NotBeEmpty();
        helper.UserDataContext.Users.Single().LastSync.Should().Be(fakeDate);
    }

    [Fact]
    public async Task SyncAsync_WhenAutomaticRules_ShouldApplyRules()
    {
        // Arrange
        var helper = new TestHelper();

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK))
            .Verifiable();

        var httpClient = new HttpClient(handlerMock.Object);

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );
        var transactionService = new TransactionService(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );
        var balanceService = new BalanceService(
            Mock.Of<ILogger<IBalanceService>>(),
            helper.UserDataContext
        );
        var goalService = new GoalService(
            Mock.Of<ILogger<IGoalService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );
        var applicationUserService = new ApplicationUserService(
            Mock.Of<ILogger<IApplicationUserService>>(),
            helper.UserDataContext
        );
        var automaticRuleService = new AutomaticRuleService(
            Mock.Of<ILogger<IAutomaticRuleService>>(),
            helper.UserDataContext
        );

        var fakeDate = new Faker().Date.Past().ToUniversalTime();
        var nowProvider = Mock.Of<INowProvider>();
        Mock.Get(nowProvider).Setup(_ => _.UtcNow).Returns(fakeDate);

        var simpleFinService = new SimpleFinService(
            httpClientFactoryMock.Object,
            Mock.Of<ILogger<ISimpleFinService>>(),
            helper.UserDataContext,
            accountService,
            institutionService,
            transactionService,
            balanceService,
            goalService,
            applicationUserService,
            automaticRuleService,
            nowProvider
        );

        // This is a demo token provided by SimpleFIN for dev.
        helper.demoUser.AccessToken = "https://demo:demo@beta-bridge.simplefin.org/simplefin";
        helper.UserDataContext.SaveChanges();

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

        helper.UserDataContext.Accounts.Add(account);

        var transactionFaker = new TransactionFaker([account.ID]);

        var transaction = transactionFaker.Generate();
        transaction.MerchantName = "Starbucks Coffee";
        transaction.Date = fakeDate.AddDays(-1);

        helper.UserDataContext.Transactions.Add(transaction);

        var automaticRule = new AutomaticRule()
        {
            UserID = helper.demoUser.Id,
            Conditions =
            [
                new RuleCondition()
                {
                    Field = "MerchantName",
                    Operator = "Contains",
                    Value = "Starbucks",
                },
            ],
            Actions =
            [
                new RuleAction()
                {
                    Field = "Category",
                    Operator = "Set",
                    Value = "Coffee Shops",
                },
            ],
        };

        helper.UserDataContext.AutomaticRules.Add(automaticRule);
        helper.UserDataContext.SaveChanges();

        // Act
        await simpleFinService.SyncAsync(helper.demoUser.Id);

        // Assert
        helper.UserDataContext.Transactions.Should().NotBeEmpty();
        helper.UserDataContext.Transactions.Single().Category.Should().Be("Food & Dining");
        helper.UserDataContext.Transactions.Single().Subcategory.Should().Be("Coffee Shops");
    }
}
