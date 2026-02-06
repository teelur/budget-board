using Bogus;
using BudgetBoard.Database.Models;
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
public class SimpleFinServiceTests
{
    [Fact]
    public async Task ConfigureAccessTokenAsync_WhenCalledWithValidSetupToken_ShouldUpdateAccessToken()
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
            Mock.Of<ILogger<ISimpleFinService>>(),
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

    [Fact]
    public async Task ConfigureAccessTokenAsync_WhenCalledWithInvalidBase64Token_ShouldThrowException()
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
            Mock.Of<ILogger<ISimpleFinService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ISimpleFinOrganizationService>(),
            Mock.Of<ISimpleFinAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var invalidToken = "not-a-valid-base64-token!!!";

        // Act & Assert
        await FluentActions
            .Invoking(async () =>
                await simpleFinService.ConfigureAccessTokenAsync(helper.demoUser.Id, invalidToken)
            )
            .Should()
            .ThrowAsync<BudgetBoardServiceException>();
    }

    [Fact]
    public async Task ConfigureAccessTokenAsync_WhenCalledWithInvalidUserId_ShouldThrowException()
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
            Mock.Of<ILogger<ISimpleFinService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ISimpleFinOrganizationService>(),
            Mock.Of<ISimpleFinAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var validToken = "aHR0cHM6Ly9iZXRhLWJyaWRnZS5zaW1wbGVmaW4ub3JnL3NpbXBsZWZpbi9jbGFpbS9ERU1P";
        var invalidUserId = Guid.NewGuid();

        // Act & Assert
        await FluentActions
            .Invoking(async () =>
                await simpleFinService.ConfigureAccessTokenAsync(invalidUserId, validToken)
            )
            .Should()
            .ThrowAsync<BudgetBoardServiceException>();
    }

    [Fact]
    public async Task ConfigureAccessTokenAsync_WhenAccessTokenFailsValidation_ShouldThrowException()
    {
        // Arrange
        var helper = new TestHelper();

        // Create a mock HTTP client that returns an error for validation
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
                    // Return success for decode, but failure for validation
                    return request.Method == HttpMethod.Post
                        ? new HttpResponseMessage
                        {
                            StatusCode = System.Net.HttpStatusCode.OK,
                            Content = new StringContent("https://invalid:invalid@invalid.com/test"),
                        }
                        : new HttpResponseMessage
                        {
                            StatusCode = System.Net.HttpStatusCode.Unauthorized,
                        };
                }
            );

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var simpleFinService = new SimpleFinService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ISimpleFinService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ISimpleFinOrganizationService>(),
            Mock.Of<ISimpleFinAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var validBase64Token = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes("https://invalid.com/claim")
        );

        // Act & Assert
        await FluentActions
            .Invoking(async () =>
                await simpleFinService.ConfigureAccessTokenAsync(
                    helper.demoUser.Id,
                    validBase64Token
                )
            )
            .Should()
            .ThrowAsync<BudgetBoardServiceException>();
    }

    [Fact]
    public async Task ConfigureAccessTokenAsync_WhenCalledWithEmptyToken_ShouldThrowException()
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
            Mock.Of<ILogger<ISimpleFinService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ISimpleFinOrganizationService>(),
            Mock.Of<ISimpleFinAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var emptyToken = string.Empty;

        // Act & Assert
        await FluentActions
            .Invoking(async () =>
                await simpleFinService.ConfigureAccessTokenAsync(helper.demoUser.Id, emptyToken)
            )
            .Should()
            .ThrowAsync<BudgetBoardServiceException>();
    }

    [Fact]
    public async Task RefreshAccountsAsync_WhenCalledWithValidData_ShouldRefreshAccounts()
    {
        // Arrange
        var helper = new TestHelper();

        var jsonResponse =
            @"{
            ""errors"": [],
            ""accounts"": [
                {
                    ""org"": {
                        ""domain"": ""example.com"",
                        ""sfin-url"": ""https://example.com/simplefin"",
                        ""name"": ""Example Bank"",
                        ""url"": ""https://example.com"",
                        ""id"": ""org-123""
                    },
                    ""id"": ""account-456"",
                    ""name"": ""Checking Account"",
                    ""currency"": ""USD"",
                    ""balance"": ""1000.50"",
                    ""balance-date"": 1609459200,
                    ""transactions"": []
                }
            ]
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
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse),
                }
            );

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var simpleFinOrganizationServiceMock = new Mock<ISimpleFinOrganizationService>();
        simpleFinOrganizationServiceMock
            .Setup(s =>
                s.CreateSimpleFinOrganizationAsync(
                    helper.demoUser.Id,
                    It.IsAny<ISimpleFinOrganizationCreateRequest>()
                )
            )
            .Callback<Guid, ISimpleFinOrganizationCreateRequest>(
                (userId, request) =>
                {
                    var org = new Database.Models.SimpleFinOrganization
                    {
                        Domain = request.Domain,
                        SimpleFinUrl = request.SimpleFinUrl,
                        Name = request.Name,
                        Url = request.Url,
                        SyncID = request.SyncID,
                        UserID = userId,
                    };
                    helper.UserDataContext.SimpleFinOrganizations.Add(org);
                    helper.UserDataContext.SaveChanges();
                }
            )
            .Returns(Task.CompletedTask);

        var simpleFinAccountServiceMock = new Mock<ISimpleFinAccountService>();

        var simpleFinService = new SimpleFinService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ISimpleFinService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            simpleFinOrganizationServiceMock.Object,
            simpleFinAccountServiceMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.RefreshAccountsAsync(helper.demoUser.Id);

        // Assert
        errors.Should().BeEmpty();
        simpleFinOrganizationServiceMock.Verify(
            s =>
                s.CreateSimpleFinOrganizationAsync(
                    helper.demoUser.Id,
                    It.IsAny<ISimpleFinOrganizationCreateRequest>()
                ),
            Times.Once
        );
        simpleFinAccountServiceMock.Verify(
            s =>
                s.CreateSimpleFinAccountAsync(
                    helper.demoUser.Id,
                    It.IsAny<ISimpleFinAccountCreateRequest>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task RefreshAccountsAsync_WhenCalledWithInvalidUserId_ShouldThrowException()
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
            Mock.Of<ILogger<ISimpleFinService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ISimpleFinOrganizationService>(),
            Mock.Of<ISimpleFinAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var invalidUserId = Guid.NewGuid();

        // Act & Assert
        await FluentActions
            .Invoking(async () => await simpleFinService.RefreshAccountsAsync(invalidUserId))
            .Should()
            .ThrowAsync<BudgetBoardServiceException>();
    }

    [Fact]
    public async Task RefreshAccountsAsync_WhenAccessTokenIsInvalid_ShouldReturnErrors()
    {
        // Arrange
        var helper = new TestHelper();

        // Create mock HTTP client that returns invalid/unparseable response
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
                    StatusCode = System.Net.HttpStatusCode.Unauthorized,
                    Content = new StringContent("Unauthorized"),
                }
            );

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var simpleFinService = new SimpleFinService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ISimpleFinService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ISimpleFinOrganizationService>(),
            Mock.Of<ISimpleFinAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // User has invalid access token
        helper.demoUser.SimpleFinAccessToken = "https://invalid:invalid@test.com/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.RefreshAccountsAsync(helper.demoUser.Id);

        // Assert
        errors.Should().NotBeEmpty();
        errors.Should().Contain("SimpleFinDataNotFoundError");
    }

    [Fact]
    public async Task RefreshAccountsAsync_WhenSimpleFinReturnsErrors_ShouldReturnErrors()
    {
        // Arrange
        var helper = new TestHelper();

        var errorMessage = "SimpleFIN API Error";
        var jsonResponse =
            @"{
            ""errors"": ["""
            + errorMessage
            + @"""],
            ""accounts"": []
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
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse),
                }
            );

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var simpleFinService = new SimpleFinService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ISimpleFinService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ISimpleFinOrganizationService>(),
            Mock.Of<ISimpleFinAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.RefreshAccountsAsync(helper.demoUser.Id);

        // Assert
        errors.Should().NotBeEmpty();
        errors.Should().Contain(errorMessage);
    }

    [Fact]
    public async Task RefreshAccountsAsync_WhenAccountExists_ShouldUpdateExistingAccount()
    {
        // Arrange
        var helper = new TestHelper();

        var jsonResponse =
            @"{
            ""errors"": [],
            ""accounts"": [
                {
                    ""org"": {
                        ""domain"": ""example.com"",
                        ""sfin-url"": ""https://example.com/simplefin"",
                        ""name"": ""Example Bank"",
                        ""url"": ""https://example.com"",
                        ""id"": ""org-123""
                    },
                    ""id"": ""account-456"",
                    ""name"": ""Checking Account"",
                    ""currency"": ""USD"",
                    ""balance"": ""1000.50"",
                    ""balance-date"": 1609459200,
                    ""transactions"": []
                }
            ]
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
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse),
                }
            );

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var simpleFinOrganizationServiceMock = new Mock<ISimpleFinOrganizationService>();
        var simpleFinAccountServiceMock = new Mock<ISimpleFinAccountService>();

        // Set up existing organization
        var existingOrg = new Database.Models.SimpleFinOrganization
        {
            ID = Guid.NewGuid(),
            Domain = "example.com",
            SimpleFinUrl = "https://example.com/simplefin",
            Name = "Example Bank",
            Url = "https://example.com",
            SyncID = "org-123",
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.SimpleFinOrganizations.Add(existingOrg);

        // Set up existing account
        var existingAccount = new Database.Models.SimpleFinAccount
        {
            ID = Guid.NewGuid(),
            SyncID = "account-456",
            Name = "Old Name",
            Currency = "USD",
            Balance = 500.00m,
            BalanceDate = (int)new DateTimeOffset(DateTime.UtcNow.AddDays(-1)).ToUnixTimeSeconds(),
            OrganizationId = existingOrg.ID,
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.SimpleFinAccounts.Add(existingAccount);
        helper.UserDataContext.SaveChanges();

        var simpleFinService = new SimpleFinService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ISimpleFinService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            simpleFinOrganizationServiceMock.Object,
            simpleFinAccountServiceMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.RefreshAccountsAsync(helper.demoUser.Id);

        // Assert
        errors.Should().BeEmpty();
        simpleFinAccountServiceMock.Verify(
            s =>
                s.UpdateSimpleFinAccountAsync(
                    helper.demoUser.Id,
                    It.Is<ISimpleFinAccountUpdateRequest>(r =>
                        r.ID == existingAccount.ID && r.Name == "Checking Account"
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task RefreshAccountsAsync_WhenAccessTokenIsMissing_ShouldReturnErrors()
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
            Mock.Of<ILogger<ISimpleFinService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ISimpleFinOrganizationService>(),
            Mock.Of<ISimpleFinAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // User has no access token configured
        helper.demoUser.SimpleFinAccessToken = null!;
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.RefreshAccountsAsync(helper.demoUser.Id);

        // Assert
        errors.Should().NotBeEmpty();
        errors.Should().Contain("SimpleFinMissingAccessTokenError");
    }

    [Fact]
    public async Task RefreshAccountsAsync_WhenHttpRequestFails_ShouldReturnErrors()
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
            .ThrowsAsync(new HttpRequestException("Network error"));

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var simpleFinService = new SimpleFinService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ISimpleFinService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ISimpleFinOrganizationService>(),
            Mock.Of<ISimpleFinAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.RefreshAccountsAsync(helper.demoUser.Id);

        // Assert
        errors.Should().NotBeEmpty();
        errors.Should().Contain("SimpleFinDataRequestError");
    }

    [Fact]
    public async Task RefreshAccountsAsync_WhenInvalidJsonResponse_ShouldReturnErrors()
    {
        // Arrange
        var helper = new TestHelper();

        var invalidJsonResponse = "{ invalid json }";

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

        var simpleFinService = new SimpleFinService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ISimpleFinService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ISimpleFinOrganizationService>(),
            Mock.Of<ISimpleFinAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.RefreshAccountsAsync(helper.demoUser.Id);

        // Assert
        errors.Should().NotBeEmpty();
        errors.Should().Contain("SimpleFinDataNotFoundError");
    }

    [Fact]
    public async Task RefreshAccountsAsync_WhenOrganizationExists_ShouldUpdateExisting()
    {
        // Arrange
        var helper = new TestHelper();

        var jsonResponse =
            @"{
            ""errors"": [],
            ""accounts"": [
                {
                    ""org"": {
                        ""domain"": ""example.com"",
                        ""sfin-url"": ""https://example.com/simplefin"",
                        ""name"": ""Updated Bank Name"",
                        ""url"": ""https://example.com"",
                        ""id"": ""org-123""
                    },
                    ""id"": ""account-456"",
                    ""name"": ""Checking Account"",
                    ""currency"": ""USD"",
                    ""balance"": ""1000.50"",
                    ""balance-date"": 1609459200,
                    ""transactions"": []
                }
            ]
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
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse),
                }
            );

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var simpleFinOrganizationServiceMock = new Mock<ISimpleFinOrganizationService>();
        var simpleFinAccountServiceMock = new Mock<ISimpleFinAccountService>();

        // Set up existing organization with old name
        var existingOrg = new Database.Models.SimpleFinOrganization
        {
            ID = Guid.NewGuid(),
            Domain = "example.com",
            SimpleFinUrl = "https://example.com/simplefin",
            Name = "Old Bank Name",
            Url = "https://example.com",
            SyncID = "org-123",
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.SimpleFinOrganizations.Add(existingOrg);
        helper.UserDataContext.SaveChanges();

        var simpleFinService = new SimpleFinService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ISimpleFinService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            simpleFinOrganizationServiceMock.Object,
            simpleFinAccountServiceMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.RefreshAccountsAsync(helper.demoUser.Id);

        // Assert
        errors.Should().BeEmpty();
        simpleFinOrganizationServiceMock.Verify(
            s =>
                s.UpdateSimpleFinOrganizationAsync(
                    helper.demoUser.Id,
                    It.Is<ISimpleFinOrganizationUpdateRequest>(r =>
                        r.ID == existingOrg.ID && r.Name == "Updated Bank Name"
                    )
                ),
            Times.Once
        );
        simpleFinAccountServiceMock.Verify(
            s =>
                s.CreateSimpleFinAccountAsync(
                    helper.demoUser.Id,
                    It.IsAny<ISimpleFinAccountCreateRequest>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task RemoveAccessTokenAsync_WhenCalledWithInvalidUserId_ShouldThrowException()
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
            Mock.Of<ILogger<ISimpleFinService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ISimpleFinOrganizationService>(),
            Mock.Of<ISimpleFinAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var invalidUserId = Guid.NewGuid();

        // Act & Assert
        await FluentActions
            .Invoking(async () => await simpleFinService.RemoveAccessTokenAsync(invalidUserId))
            .Should()
            .ThrowAsync<BudgetBoardServiceException>();
    }

    [Fact]
    public async Task RemoveAccessTokenAsync_WhenCalled_ShouldRemoveAccessTokenAndCleanupData()
    {
        // Arrange
        var helper = new TestHelper();

        using var httpClient = new HttpClient();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var accountServiceMock = new Mock<IAccountService>();
        var simpleFinOrganizationServiceMock = new Mock<ISimpleFinOrganizationService>();
        var simpleFinAccountServiceMock = new Mock<ISimpleFinAccountService>();

        var simpleFinService = new SimpleFinService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ISimpleFinService>>(),
            Mock.Of<INowProvider>(),
            accountServiceMock.Object,
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            simpleFinOrganizationServiceMock.Object,
            simpleFinAccountServiceMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Set up existing SimpleFIN data
        var org = new Database.Models.SimpleFinOrganization
        {
            Domain = "example.com",
            Name = "Example Bank",
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.SimpleFinOrganizations.Add(org);

        var account = new Database.Models.Account
        {
            Name = "My Account",
            Type = "checking",
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.Accounts.Add(account);

        var simpleFinAccount = new Database.Models.SimpleFinAccount
        {
            SyncID = "account-123",
            Name = "Checking",
            Currency = "USD",
            OrganizationId = org.ID,
            LinkedAccountId = account.ID,
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.SimpleFinAccounts.Add(simpleFinAccount);

        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        await simpleFinService.RemoveAccessTokenAsync(helper.demoUser.Id);

        // Assert
        helper.UserDataContext.Users.Single().SimpleFinAccessToken.Should().BeEmpty();
        simpleFinAccountServiceMock.Verify(
            s => s.DeleteSimpleFinAccountAsync(helper.demoUser.Id, It.IsAny<Guid>()),
            Times.Once
        );
    }

    [Fact]
    public async Task SyncTransactionHistoryAsync_WhenCalledWithValidData_ShouldSyncTransactionsAndBalances()
    {
        // Arrange
        var helper = new TestHelper();

        var jsonResponse =
            @"{
            ""errors"": [],
            ""accounts"": [
                {
                    ""org"": {
                        ""domain"": ""example.com"",
                        ""sfin-url"": ""https://example.com/simplefin"",
                        ""name"": ""Example Bank"",
                        ""url"": ""https://example.com"",
                        ""id"": ""org-123""
                    },
                    ""id"": ""account-456"",
                    ""name"": ""Checking Account"",
                    ""currency"": ""USD"",
                    ""balance"": ""1500.75"",
                    ""balance-date"": 1609459200,
                    ""transactions"": [
                        {
                            ""id"": ""txn-1"",
                            ""posted"": 1609372800,
                            ""amount"": ""-50.00"",
                            ""description"": ""Coffee Shop"",
                            ""transacted_at"": 1609372800,
                            ""pending"": false
                        }
                    ]
                }
            ]
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
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse),
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

        var simpleFinService = new SimpleFinService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ISimpleFinService>>(),
            nowProviderMock,
            Mock.Of<IAccountService>(),
            transactionServiceMock.Object,
            balanceServiceMock.Object,
            Mock.Of<ISimpleFinOrganizationService>(),
            Mock.Of<ISimpleFinAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Set up existing organization, SimpleFIN account, and linked account
        var org = new Database.Models.SimpleFinOrganization
        {
            Domain = "example.com",
            SimpleFinUrl = "https://example.com/simplefin",
            Name = "Example Bank",
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.SimpleFinOrganizations.Add(org);

        var account = new Database.Models.Account
        {
            Name = "My Checking",
            InstitutionID = null,
            Type = "checking",
            Subtype = "checking",
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.Accounts.Add(account);

        var simpleFinAccount = new Database.Models.SimpleFinAccount
        {
            SyncID = "account-456",
            Name = "Checking Account",
            Currency = "USD",
            Balance = 1000.00m,
            BalanceDate = (int)new DateTimeOffset(DateTime.UtcNow.AddDays(-2)).ToUnixTimeSeconds(),
            OrganizationId = org.ID,
            LinkedAccountId = account.ID,
            UserID = helper.demoUser.Id,
            LastSync = DateTime.UtcNow.AddDays(-1),
        };
        helper.UserDataContext.SimpleFinAccounts.Add(simpleFinAccount);

        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.SyncTransactionHistoryAsync(helper.demoUser.Id);

        // Assert
        errors.Should().BeEmpty();
        transactionServiceMock.Verify(
            s =>
                s.CreateTransactionAsync(
                    helper.demoUser,
                    It.IsAny<ITransactionCreateRequest>(),
                    It.IsAny<IEnumerable<ICategory>>(),
                    null
                ),
            Times.Once
        );
        balanceServiceMock.Verify(
            s => s.CreateBalancesAsync(helper.demoUser.Id, It.IsAny<IBalanceCreateRequest>()),
            Times.Once
        );
    }

    [Fact]
    public async Task SyncTransactionHistoryAsync_WhenCalledWithInvalidUserId_ShouldThrowException()
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
            Mock.Of<ILogger<ISimpleFinService>>(),
            Mock.Of<INowProvider>(),
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ISimpleFinOrganizationService>(),
            Mock.Of<ISimpleFinAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var invalidUserId = Guid.NewGuid();

        // Act & Assert
        await FluentActions
            .Invoking(async () => await simpleFinService.SyncTransactionHistoryAsync(invalidUserId))
            .Should()
            .ThrowAsync<BudgetBoardServiceException>();
    }

    [Fact]
    public async Task SyncTransactionHistoryAsync_WhenAccountIsDeleted_ShouldSkipSync()
    {
        // Arrange
        var helper = new TestHelper();

        var jsonResponse =
            @"{
            ""errors"": [],
            ""accounts"": [
                {
                    ""org"": {
                        ""domain"": ""example.com"",
                        ""sfin-url"": ""https://example.com/simplefin"",
                        ""name"": ""Example Bank"",
                        ""url"": ""https://example.com"",
                        ""id"": ""org-123""
                    },
                    ""id"": ""account-456"",
                    ""name"": ""Checking Account"",
                    ""currency"": ""USD"",
                    ""balance"": ""1500.75"",
                    ""balance-date"": 1609459200,
                    ""transactions"": [
                        {
                            ""id"": ""txn-1"",
                            ""posted"": 1609372800,
                            ""amount"": ""-50.00"",
                            ""description"": ""Coffee Shop"",
                            ""transacted_at"": 1609372800,
                            ""pending"": false
                        }
                    ]
                }
            ]
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
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse),
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

        var simpleFinService = new SimpleFinService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ISimpleFinService>>(),
            nowProviderMock,
            Mock.Of<IAccountService>(),
            transactionServiceMock.Object,
            balanceServiceMock.Object,
            Mock.Of<ISimpleFinOrganizationService>(),
            Mock.Of<ISimpleFinAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Set up existing organization, SimpleFIN account, and deleted linked account
        var org = new Database.Models.SimpleFinOrganization
        {
            Domain = "example.com",
            SimpleFinUrl = "https://example.com/simplefin",
            Name = "Example Bank",
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.SimpleFinOrganizations.Add(org);

        var deletedAccount = new Database.Models.Account
        {
            Name = "Deleted Checking",
            InstitutionID = null,
            Type = "checking",
            Subtype = "checking",
            UserID = helper.demoUser.Id,
            Deleted = DateTime.UtcNow.AddDays(-1),
        };
        helper.UserDataContext.Accounts.Add(deletedAccount);

        var simpleFinAccount = new Database.Models.SimpleFinAccount
        {
            SyncID = "account-456",
            Name = "Checking Account",
            Currency = "USD",
            Balance = 1000.00m,
            BalanceDate = (int)new DateTimeOffset(DateTime.UtcNow.AddDays(-2)).ToUnixTimeSeconds(),
            OrganizationId = org.ID,
            LinkedAccountId = deletedAccount.ID,
            UserID = helper.demoUser.Id,
            LastSync = DateTime.UtcNow.AddDays(-1),
        };
        helper.UserDataContext.SimpleFinAccounts.Add(simpleFinAccount);

        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.SyncTransactionHistoryAsync(helper.demoUser.Id);

        // Assert
        errors.Should().BeEmpty();
        transactionServiceMock.Verify(
            s =>
                s.CreateTransactionAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<ITransactionCreateRequest>(),
                    null,
                    null
                ),
            Times.Never
        );
        balanceServiceMock.Verify(
            s => s.CreateBalancesAsync(It.IsAny<Guid>(), It.IsAny<IBalanceCreateRequest>()),
            Times.Never
        );
    }

    [Fact]
    public async Task SyncTransactionHistoryAsync_WhenAccountNotLinked_ShouldSkipSync()
    {
        // Arrange
        var helper = new TestHelper();

        var jsonResponse =
            @"{
            ""errors"": [],
            ""accounts"": [
                {
                    ""org"": {
                        ""domain"": ""example.com"",
                        ""sfin-url"": ""https://example.com/simplefin"",
                        ""name"": ""Example Bank"",
                        ""url"": ""https://example.com"",
                        ""id"": ""org-123""
                    },
                    ""id"": ""account-456"",
                    ""name"": ""Checking Account"",
                    ""currency"": ""USD"",
                    ""balance"": ""1500.75"",
                    ""balance-date"": 1609459200,
                    ""transactions"": []
                }
            ]
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
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse),
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

        var simpleFinService = new SimpleFinService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ISimpleFinService>>(),
            nowProviderMock,
            Mock.Of<IAccountService>(),
            transactionServiceMock.Object,
            balanceServiceMock.Object,
            Mock.Of<ISimpleFinOrganizationService>(),
            Mock.Of<ISimpleFinAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Set up existing organization and SimpleFIN account without linked account
        var org = new Database.Models.SimpleFinOrganization
        {
            Domain = "example.com",
            SimpleFinUrl = "https://example.com/simplefin",
            Name = "Example Bank",
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.SimpleFinOrganizations.Add(org);

        var simpleFinAccount = new Database.Models.SimpleFinAccount
        {
            SyncID = "account-456",
            Name = "Checking Account",
            Currency = "USD",
            Balance = 1000.00m,
            BalanceDate = (int)new DateTimeOffset(DateTime.UtcNow.AddDays(-2)).ToUnixTimeSeconds(),
            OrganizationId = org.ID,
            LinkedAccountId = null, // Not linked
            UserID = helper.demoUser.Id,
            LastSync = DateTime.UtcNow.AddDays(-1),
        };
        helper.UserDataContext.SimpleFinAccounts.Add(simpleFinAccount);

        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.SyncTransactionHistoryAsync(helper.demoUser.Id);

        // Assert
        errors.Should().BeEmpty();
        transactionServiceMock.Verify(
            s =>
                s.CreateTransactionAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<ITransactionCreateRequest>(),
                    null,
                    null
                ),
            Times.Never
        );
        balanceServiceMock.Verify(
            s => s.CreateBalancesAsync(It.IsAny<Guid>(), It.IsAny<IBalanceCreateRequest>()),
            Times.Never
        );
    }

    [Fact]
    public async Task SyncTransactionHistoryAsync_WhenAccessTokenIsMissing_ShouldReturnErrors()
    {
        // Arrange
        var helper = new TestHelper();

        using var httpClient = new HttpClient();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(_ => _.CreateClient(string.Empty))
            .Returns(httpClient)
            .Verifiable();

        var fakeDate = new Faker().Date.Past().ToUniversalTime();
        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var simpleFinService = new SimpleFinService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ISimpleFinService>>(),
            nowProviderMock.Object,
            Mock.Of<IAccountService>(),
            Mock.Of<ITransactionService>(),
            Mock.Of<IBalanceService>(),
            Mock.Of<ISimpleFinOrganizationService>(),
            Mock.Of<ISimpleFinAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // User has no access token configured
        helper.demoUser.SimpleFinAccessToken = string.Empty;
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.SyncTransactionHistoryAsync(helper.demoUser.Id);

        // Assert
        errors.Should().NotBeEmpty();
        errors.Should().Contain("SimpleFinMissingAccessTokenError");
    }

    [Fact]
    public async Task SyncTransactionHistoryAsync_WhenMultipleAccountsHaveTransactions_ShouldSyncAll()
    {
        // Arrange
        var helper = new TestHelper();

        var jsonResponse =
            @"{
            ""errors"": [],
            ""accounts"": [
                {
                    ""org"": {
                        ""domain"": ""example.com"",
                        ""sfin-url"": ""https://example.com/simplefin"",
                        ""name"": ""Example Bank"",
                        ""url"": ""https://example.com"",
                        ""id"": ""org-123""
                    },
                    ""id"": ""account-456"",
                    ""name"": ""Checking Account"",
                    ""currency"": ""USD"",
                    ""balance"": ""1500.75"",
                    ""balance-date"": 1609459200,
                    ""transactions"": [
                        {
                            ""id"": ""txn-1"",
                            ""posted"": 1609372800,
                            ""amount"": ""-50.00"",
                            ""description"": ""Coffee Shop"",
                            ""transacted_at"": 1609372800,
                            ""pending"": false
                        }
                    ]
                },
                {
                    ""org"": {
                        ""domain"": ""example.com"",
                        ""sfin-url"": ""https://example.com/simplefin"",
                        ""name"": ""Example Bank"",
                        ""url"": ""https://example.com"",
                        ""id"": ""org-123""
                    },
                    ""id"": ""account-789"",
                    ""name"": ""Savings Account"",
                    ""currency"": ""USD"",
                    ""balance"": ""5000.00"",
                    ""balance-date"": 1609459200,
                    ""transactions"": [
                        {
                            ""id"": ""txn-2"",
                            ""posted"": 1609372800,
                            ""amount"": ""100.00"",
                            ""description"": ""Transfer In"",
                            ""transacted_at"": 1609372800,
                            ""pending"": false
                        }
                    ]
                }
            ]
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
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse),
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

        var simpleFinService = new SimpleFinService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ISimpleFinService>>(),
            nowProviderMock,
            Mock.Of<IAccountService>(),
            transactionServiceMock.Object,
            balanceServiceMock.Object,
            Mock.Of<ISimpleFinOrganizationService>(),
            Mock.Of<ISimpleFinAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Set up organization and two accounts
        var org = new Database.Models.SimpleFinOrganization
        {
            Domain = "example.com",
            SimpleFinUrl = "https://example.com/simplefin",
            Name = "Example Bank",
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.SimpleFinOrganizations.Add(org);

        var account1 = new Database.Models.Account
        {
            Name = "My Checking",
            Type = "checking",
            Subtype = "checking",
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.Accounts.Add(account1);

        var account2 = new Database.Models.Account
        {
            Name = "My Savings",
            Type = "savings",
            Subtype = "savings",
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.Accounts.Add(account2);

        var simpleFinAccount1 = new Database.Models.SimpleFinAccount
        {
            SyncID = "account-456",
            Name = "Checking Account",
            Currency = "USD",
            OrganizationId = org.ID,
            LinkedAccountId = account1.ID,
            UserID = helper.demoUser.Id,
            LastSync = DateTime.UtcNow.AddDays(-1),
        };
        helper.UserDataContext.SimpleFinAccounts.Add(simpleFinAccount1);

        var simpleFinAccount2 = new Database.Models.SimpleFinAccount
        {
            SyncID = "account-789",
            Name = "Savings Account",
            Currency = "USD",
            OrganizationId = org.ID,
            LinkedAccountId = account2.ID,
            UserID = helper.demoUser.Id,
            LastSync = DateTime.UtcNow.AddDays(-1),
        };
        helper.UserDataContext.SimpleFinAccounts.Add(simpleFinAccount2);

        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.SyncTransactionHistoryAsync(helper.demoUser.Id);

        // Assert
        errors.Should().BeEmpty();
        transactionServiceMock.Verify(
            s =>
                s.CreateTransactionAsync(helper.demoUser.Id, It.IsAny<ITransactionCreateRequest>()),
            Times.Exactly(2)
        );
        balanceServiceMock.Verify(
            s => s.CreateBalancesAsync(helper.demoUser.Id, It.IsAny<IBalanceCreateRequest>()),
            Times.Exactly(2)
        );
    }
}
