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
using Moq.Protected;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class LunchFlowServiceTests
{
    [Fact]
    public async Task ConfigureApiKeyAsync_WhenCalledWithValidApiKey_ShouldUpdateApiKey()
    {
        // Arrange
        var helper = new TestHelper();

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(@"{""accounts"": [], ""total"": 0}"),
                }
            );

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var lunchFlowAccountServiceMock = new Mock<ILunchFlowAccountService>();
        var lunchFlowService = new LunchFlowService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ILunchFlowService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            lunchFlowAccountServiceMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var validApiKey = "test-api-key-12345";

        // Act
        await lunchFlowService.ConfigureApiKeyAsync(helper.demoUser.Id, validApiKey);

        // Assert
        helper.UserDataContext.Users.Single().LunchFlowApiKey.Should().Be(validApiKey);
    }

    [Fact]
    public async Task ConfigureApiKeyAsync_WhenCalledWithEmptyApiKey_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();

        using var httpClient = new HttpClient();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var lunchFlowService = new LunchFlowService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ILunchFlowService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ILunchFlowAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var emptyApiKey = string.Empty;

        // Act & Assert
        await FluentActions
            .Invoking(async () =>
                await lunchFlowService.ConfigureApiKeyAsync(helper.demoUser.Id, emptyApiKey)
            )
            .Should()
            .ThrowAsync<BudgetBoardServiceException>();
    }

    [Fact]
    public async Task ConfigureApiKeyAsync_WhenCalledWithNullApiKey_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();

        using var httpClient = new HttpClient();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var lunchFlowService = new LunchFlowService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ILunchFlowService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ILunchFlowAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act & Assert
        await FluentActions
            .Invoking(async () =>
                await lunchFlowService.ConfigureApiKeyAsync(helper.demoUser.Id, null!)
            )
            .Should()
            .ThrowAsync<BudgetBoardServiceException>();
    }

    [Fact]
    public async Task ConfigureApiKeyAsync_WhenApiKeyFailsValidation_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.Unauthorized }
            );

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var lunchFlowService = new LunchFlowService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ILunchFlowService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ILunchFlowAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var invalidApiKey = "invalid-api-key";

        // Act & Assert
        await FluentActions
            .Invoking(async () =>
                await lunchFlowService.ConfigureApiKeyAsync(helper.demoUser.Id, invalidApiKey)
            )
            .Should()
            .ThrowAsync<BudgetBoardServiceException>();
    }

    [Fact]
    public async Task ConfigureApiKeyAsync_WhenCalledWithInvalidUserId_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();

        using var httpClient = new HttpClient();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var lunchFlowService = new LunchFlowService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ILunchFlowService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ILunchFlowAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var validApiKey = "test-api-key";
        var invalidUserId = Guid.NewGuid();

        // Act & Assert
        await FluentActions
            .Invoking(async () =>
                await lunchFlowService.ConfigureApiKeyAsync(invalidUserId, validApiKey)
            )
            .Should()
            .ThrowAsync<BudgetBoardServiceException>();
    }

    [Fact]
    public async Task RemoveApiKeyAsync_WhenCalled_ShouldRemoveApiKeyAndCleanupData()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.LunchFlowApiKey = "existing-api-key";

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Source = AccountSource.LunchFlow;
        helper.UserDataContext.Accounts.Add(account);

        var lunchFlowAccountFaker = new LunchFlowAccountFaker(helper.demoUser.Id);
        var lunchFlowAccount = lunchFlowAccountFaker.Generate();
        lunchFlowAccount.LinkedAccountId = account.ID;
        helper.UserDataContext.LunchFlowAccounts.Add(lunchFlowAccount);

        await helper.UserDataContext.SaveChangesAsync();

        using var httpClient = new HttpClient();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var accountServiceMock = new Mock<IAccountService>();
        var lunchFlowAccountServiceMock = new Mock<ILunchFlowAccountService>();

        var lunchFlowService = new LunchFlowService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ILunchFlowService>>(),
            Mock.Of<INowProvider>(),
            accountServiceMock.Object,
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            lunchFlowAccountServiceMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        await lunchFlowService.RemoveApiKeyAsync(helper.demoUser.Id);

        // Assert
        helper.UserDataContext.Users.Single().LunchFlowApiKey.Should().BeEmpty();
        accountServiceMock.Verify(
            _ => _.UpdateAccountSourceAsync(helper.demoUser.Id, account.ID, AccountSource.Manual),
            Times.Once
        );
        lunchFlowAccountServiceMock.Verify(
            _ => _.DeleteLunchFlowAccountAsync(helper.demoUser.Id, lunchFlowAccount.ID),
            Times.Once
        );
    }

    [Fact]
    public async Task RemoveApiKeyAsync_WhenCalledWithInvalidUserId_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();

        using var httpClient = new HttpClient();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var lunchFlowService = new LunchFlowService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ILunchFlowService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ILunchFlowAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var invalidUserId = Guid.NewGuid();

        // Act & Assert
        await FluentActions
            .Invoking(async () => await lunchFlowService.RemoveApiKeyAsync(invalidUserId))
            .Should()
            .ThrowAsync<BudgetBoardServiceException>();
    }

    [Fact]
    public async Task RefreshAccountsAsync_WhenCalledWithValidData_ShouldRefreshAccounts()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.LunchFlowApiKey = "valid-api-key";
        await helper.UserDataContext.SaveChangesAsync();

        var jsonResponse =
            @"{
                ""accounts"": [
                    {
                        ""id"": ""12345"",
                        ""name"": ""Test Checking"",
                        ""institution_name"": ""Test Bank"",
                        ""institution_logo"": ""https://example.com/logo.png"",
                        ""provider"": ""test_provider"",
                        ""currency"": ""USD"",
                        ""status"": ""active""
                    }
                ],
                ""total"": 1
            }";

        var balanceResponse =
            @"{
                ""balance"": {
                    ""amount"": 1500.50,
                    ""currency"": ""USD""
                }
            }";

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString().Contains("/accounts")
                    && !req.RequestUri.ToString().Contains("/balance")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse),
                }
            );

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString().Contains("/balance")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(balanceResponse),
                }
            );

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var nowProviderMock = Mock.Of<INowProvider>();
        Mock.Get(nowProviderMock).Setup(_ => _.UtcNow).Returns(DateTime.UtcNow);

        var lunchFlowAccountServiceMock = new Mock<ILunchFlowAccountService>();

        var lunchFlowService = new LunchFlowService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ILunchFlowService>>(),
            nowProviderMock,
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            lunchFlowAccountServiceMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        var errors = await lunchFlowService.RefreshAccountsAsync(helper.demoUser.Id);

        // Assert
        errors.Should().BeEmpty();
        lunchFlowAccountServiceMock.Verify(
            _ =>
                _.CreateLunchFlowAccountAsync(
                    helper.demoUser.Id,
                    It.Is<ILunchFlowAccountCreateRequest>(req =>
                        req.SyncID == "12345" && req.Name == "Test Checking"
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task RefreshAccountsAsync_WhenAccountExists_ShouldUpdateExistingAccount()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.LunchFlowApiKey = "valid-api-key";

        var lunchFlowAccountFaker = new LunchFlowAccountFaker(helper.demoUser.Id);
        var existingAccount = lunchFlowAccountFaker.Generate();
        existingAccount.SyncID = "12345";
        existingAccount.Name = "Old Name";
        helper.UserDataContext.LunchFlowAccounts.Add(existingAccount);
        await helper.UserDataContext.SaveChangesAsync();

        var jsonResponse =
            @"{
                ""accounts"": [
                    {
                        ""id"": ""12345"",
                        ""name"": ""Updated Checking"",
                        ""institution_name"": ""Updated Bank"",
                        ""institution_logo"": ""https://example.com/logo.png"",
                        ""provider"": ""test_provider"",
                        ""currency"": ""USD"",
                        ""status"": ""active""
                    }
                ],
                ""total"": 1
            }";

        var balanceResponse =
            @"{
                ""balance"": {
                    ""amount"": 2000.00,
                    ""currency"": ""USD""
                }
            }";

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString().Contains("/accounts")
                    && !req.RequestUri.ToString().Contains("/balance")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse),
                }
            );

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString().Contains("/balance")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(balanceResponse),
                }
            );

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var nowProviderMock = Mock.Of<INowProvider>();
        Mock.Get(nowProviderMock).Setup(_ => _.UtcNow).Returns(DateTime.UtcNow);

        var lunchFlowAccountServiceMock = new Mock<ILunchFlowAccountService>();

        var lunchFlowService = new LunchFlowService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ILunchFlowService>>(),
            nowProviderMock,
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            lunchFlowAccountServiceMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        var errors = await lunchFlowService.RefreshAccountsAsync(helper.demoUser.Id);

        // Assert
        errors.Should().BeEmpty();
        lunchFlowAccountServiceMock.Verify(
            _ =>
                _.UpdateLunchFlowAccountAsync(
                    helper.demoUser.Id,
                    It.Is<ILunchFlowAccountUpdateRequest>(req =>
                        req.ID == existingAccount.ID && req.Name == "Updated Checking"
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task RefreshAccountsAsync_WhenApiKeyIsMissing_ShouldReturnErrors()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.LunchFlowApiKey = string.Empty;
        await helper.UserDataContext.SaveChangesAsync();

        using var httpClient = new HttpClient();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var lunchFlowService = new LunchFlowService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ILunchFlowService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ILunchFlowAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        var errors = await lunchFlowService.RefreshAccountsAsync(helper.demoUser.Id);

        // Assert
        errors.Should().NotBeEmpty();
        errors.Should().Contain(e => e.Contains("LunchFlow"));
    }

    [Fact]
    public async Task RefreshAccountsAsync_WhenHttpRequestFails_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.LunchFlowApiKey = "valid-api-key";
        await helper.UserDataContext.SaveChangesAsync();

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var lunchFlowService = new LunchFlowService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ILunchFlowService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ILunchFlowAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act & Assert
        await FluentActions
            .Invoking(async () => await lunchFlowService.RefreshAccountsAsync(helper.demoUser.Id))
            .Should()
            .ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task RefreshAccountsAsync_WhenInvalidJsonResponse_ShouldReturnErrors()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.LunchFlowApiKey = "valid-api-key";
        await helper.UserDataContext.SaveChangesAsync();

        var invalidJsonResponse = "{ this is not valid json";

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(invalidJsonResponse),
                }
            );

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var lunchFlowService = new LunchFlowService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ILunchFlowService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ILunchFlowAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        var errors = await lunchFlowService.RefreshAccountsAsync(helper.demoUser.Id);

        // Assert
        errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RefreshAccountsAsync_WhenBalanceRetrievalFails_ShouldReturnErrors()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.LunchFlowApiKey = "valid-api-key";
        await helper.UserDataContext.SaveChangesAsync();

        var accountsResponse =
            @"{
                ""accounts"": [
                    {
                        ""id"": ""12345"",
                        ""name"": ""Test Checking"",
                        ""institution_name"": ""Test Bank"",
                        ""institution_logo"": ""https://example.com/logo.png"",
                        ""provider"": ""test_provider"",
                        ""currency"": ""USD"",
                        ""status"": ""active""
                    }
                ],
                ""total"": 1
            }";

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString().Contains("/accounts")
                    && !req.RequestUri.ToString().Contains("/balance")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(accountsResponse),
                }
            );

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString().Contains("/balance")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                }
            );

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var lunchFlowService = new LunchFlowService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ILunchFlowService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ILunchFlowAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        var errors = await lunchFlowService.RefreshAccountsAsync(helper.demoUser.Id);

        // Assert
        errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SyncTransactionHistoryAsync_WhenCalledWithValidData_ShouldSyncTransactionsAndBalances()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.LunchFlowApiKey = "valid-api-key";

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        helper.UserDataContext.Accounts.Add(account);

        var lunchFlowAccountFaker = new LunchFlowAccountFaker(helper.demoUser.Id);
        var lunchFlowAccount = lunchFlowAccountFaker.Generate();
        lunchFlowAccount.LinkedAccountId = account.ID;
        helper.UserDataContext.LunchFlowAccounts.Add(lunchFlowAccount);

        await helper.UserDataContext.SaveChangesAsync();

        var transactionsResponse =
            @"{
                ""transactions"": [
                    {
                        ""id"": ""txn123"",
                        ""account_id"": ""12345"",
                        ""amount"": -50.25,
                        ""currency"": ""USD"",
                        ""date"": ""2024-01-15"",
                        ""merchant"": ""Test Store"",
                        ""description"": ""Purchase"",
                        ""is_pending"": false
                    }
                ],
                ""total"": 1
            }";

        var balanceResponse =
            @"{
                ""balance"": {
                    ""amount"": 1500.50,
                    ""currency"": ""USD""
                }
            }";

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString().Contains("/transactions")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(transactionsResponse),
                }
            );

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString().Contains("/balance")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(balanceResponse),
                }
            );

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var nowProviderMock = Mock.Of<INowProvider>();
        Mock.Get(nowProviderMock).Setup(_ => _.UtcNow).Returns(DateTime.UtcNow);

        var transactionServiceMock = new Mock<ITransactionService>();
        var balanceServiceMock = new Mock<IBalanceService>();

        var lunchFlowService = new LunchFlowService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ILunchFlowService>>(),
            nowProviderMock,
            Mock.Of<IAccountService>(),
            transactionServiceMock.Object,
            balanceServiceMock.Object,
            Mock.Of<ILunchFlowAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        var errors = await lunchFlowService.SyncTransactionHistoryAsync(helper.demoUser.Id);

        // Assert
        errors.Should().BeEmpty();
        transactionServiceMock.Verify(
            _ =>
                _.CreateTransactionAsync(
                    helper.demoUser.Id,
                    It.Is<ITransactionCreateRequest>(req =>
                        req.SyncID == "txn123" && req.Amount == -50.25m
                    )
                ),
            Times.Once
        );
        balanceServiceMock.Verify(
            _ => _.CreateBalancesAsync(helper.demoUser.Id, It.IsAny<IBalanceCreateRequest>()),
            Times.AtMostOnce
        );
    }

    [Fact]
    public async Task SyncTransactionHistoryAsync_WhenApiKeyIsMissing_ShouldReturnErrors()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.LunchFlowApiKey = string.Empty;
        await helper.UserDataContext.SaveChangesAsync();

        using var httpClient = new HttpClient();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var lunchFlowService = new LunchFlowService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ILunchFlowService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ILunchFlowAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        var errors = await lunchFlowService.SyncTransactionHistoryAsync(helper.demoUser.Id);

        // Assert
        errors.Should().NotBeEmpty();
        errors.Should().Contain(e => e.Contains("LunchFlow"));
    }

    [Fact]
    public async Task SyncTransactionHistoryAsync_WhenAccountNotLinked_ShouldSkipSync()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.LunchFlowApiKey = "valid-api-key";

        var lunchFlowAccountFaker = new LunchFlowAccountFaker(helper.demoUser.Id);
        var lunchFlowAccount = lunchFlowAccountFaker.Generate();
        lunchFlowAccount.LinkedAccountId = null;
        helper.UserDataContext.LunchFlowAccounts.Add(lunchFlowAccount);

        await helper.UserDataContext.SaveChangesAsync();

        using var httpClient = new HttpClient();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var transactionServiceMock = new Mock<ITransactionService>();

        var lunchFlowService = new LunchFlowService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ILunchFlowService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            transactionServiceMock.Object,
            Mock.Of<IBalanceService>(),
            Mock.Of<ILunchFlowAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        var errors = await lunchFlowService.SyncTransactionHistoryAsync(helper.demoUser.Id);

        // Assert
        errors.Should().BeEmpty();
        transactionServiceMock.Verify(
            _ => _.CreateTransactionAsync(It.IsAny<Guid>(), It.IsAny<ITransactionCreateRequest>()),
            Times.Never
        );
    }

    [Fact]
    public async Task SyncTransactionHistoryAsync_WhenDuplicateTransaction_ShouldSkipDuplicate()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.LunchFlowApiKey = "valid-api-key";

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        helper.UserDataContext.Accounts.Add(account);

        var existingTransaction = new Transaction
        {
            ID = Guid.NewGuid(),
            SyncID = "txn123",
            AccountID = account.ID,
            Amount = -50.25m,
            Date = DateTime.Parse("2024-01-15"),
            MerchantName = "Test Store",
            Source = TransactionSource.LunchFlow.Value,
        };
        helper.UserDataContext.Transactions.Add(existingTransaction);

        var lunchFlowAccountFaker = new LunchFlowAccountFaker(helper.demoUser.Id);
        var lunchFlowAccount = lunchFlowAccountFaker.Generate();
        lunchFlowAccount.LinkedAccountId = account.ID;
        helper.UserDataContext.LunchFlowAccounts.Add(lunchFlowAccount);

        await helper.UserDataContext.SaveChangesAsync();

        var transactionsResponse =
            @"{
                ""transactions"": [
                    {
                        ""id"": ""txn123"",
                        ""account_id"": ""12345"",
                        ""amount"": -50.25,
                        ""currency"": ""USD"",
                        ""date"": ""2024-01-15"",
                        ""merchant"": ""Test Store"",
                        ""description"": ""Purchase"",
                        ""is_pending"": false
                    }
                ],
                ""total"": 1
            }";

        var balanceResponse =
            @"{
                ""balance"": {
                    ""amount"": 1500.50,
                    ""currency"": ""USD""
                }
            }";

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString().Contains("/transactions")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(transactionsResponse),
                }
            );

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString().Contains("/balance")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(balanceResponse),
                }
            );

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var nowProviderMock = Mock.Of<INowProvider>();
        Mock.Get(nowProviderMock).Setup(_ => _.UtcNow).Returns(DateTime.UtcNow);

        var transactionServiceMock = new Mock<ITransactionService>();

        var lunchFlowService = new LunchFlowService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ILunchFlowService>>(),
            nowProviderMock,
            Mock.Of<IAccountService>(),
            transactionServiceMock.Object,
            Mock.Of<IBalanceService>(),
            Mock.Of<ILunchFlowAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        var errors = await lunchFlowService.SyncTransactionHistoryAsync(helper.demoUser.Id);

        // Assert
        errors.Should().BeEmpty();
        transactionServiceMock.Verify(
            _ => _.CreateTransactionAsync(It.IsAny<Guid>(), It.IsAny<ITransactionCreateRequest>()),
            Times.Never
        );
    }

    [Fact]
    public async Task SyncTransactionHistoryAsync_WhenLinkedAccountNotFound_ShouldReturnErrors()
    {
        // Arrange
        var helper = new TestHelper();
        helper.demoUser.LunchFlowApiKey = "valid-api-key";

        var lunchFlowAccountFaker = new LunchFlowAccountFaker(helper.demoUser.Id);
        var lunchFlowAccount = lunchFlowAccountFaker.Generate();
        lunchFlowAccount.LinkedAccountId = Guid.NewGuid(); // Non-existent account
        helper.UserDataContext.LunchFlowAccounts.Add(lunchFlowAccount);

        await helper.UserDataContext.SaveChangesAsync();

        var transactionsResponse =
            @"{
                ""transactions"": [
                    {
                        ""id"": ""txn123"",
                        ""account_id"": ""12345"",
                        ""amount"": -50.25,
                        ""currency"": ""USD"",
                        ""date"": ""2024-01-15"",
                        ""merchant"": ""Test Store"",
                        ""description"": ""Purchase"",
                        ""is_pending"": false
                    }
                ],
                ""total"": 1
            }";

        var balanceResponse =
            @"{
                ""balance"": {
                    ""amount"": 1500.50,
                    ""currency"": ""USD""
                }
            }";

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                (HttpRequestMessage request, CancellationToken token) =>
                {
                    return request.RequestUri!.ToString().Contains("/transactions")
                        ? new HttpResponseMessage
                        {
                            StatusCode = System.Net.HttpStatusCode.OK,
                            Content = new StringContent(transactionsResponse),
                        }
                        : new HttpResponseMessage
                        {
                            StatusCode = System.Net.HttpStatusCode.OK,
                            Content = new StringContent(balanceResponse),
                        };
                }
            );

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var nowProviderMock = Mock.Of<INowProvider>();
        Mock.Get(nowProviderMock).Setup(_ => _.UtcNow).Returns(DateTime.UtcNow);

        var lunchFlowService = new LunchFlowService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ILunchFlowService>>(),
            nowProviderMock,
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ILunchFlowAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        var errors = await lunchFlowService.SyncTransactionHistoryAsync(helper.demoUser.Id);

        // Assert
        errors.Should().NotBeEmpty();
    }
}
