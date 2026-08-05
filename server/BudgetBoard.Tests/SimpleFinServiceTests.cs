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
    #region ConfigureAccessTokenAsync
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
    public async Task ConfigureAccessTokenAsync_WhenCalledWithInvalidUserId_ShouldThrowInvalidUserError()
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

        // Act
        Func<Task> act = async () =>
            await simpleFinService.ConfigureAccessTokenAsync(invalidUserId, validToken);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidUserError");
    }

    [Fact]
    public async Task ConfigureAccessTokenAsync_WhenCalledWithInvalidBase64Token_ShouldThrowSimpleFinDecodeTokenInvalidError()
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

        // Act
        Func<Task> act = async () =>
            await simpleFinService.ConfigureAccessTokenAsync(helper.demoUser.Id, invalidToken);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("SimpleFinDecodeTokenInvalidError");
    }

    [Fact]
    public async Task ConfigureAccessTokenAsync_WhenAccessTokenFailsValidation_ShouldThrowSimpleFinInvalidAccessTokenError()
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

        // Act
        Func<Task> act = async () =>
            await simpleFinService.ConfigureAccessTokenAsync(helper.demoUser.Id, validBase64Token);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("SimpleFinInvalidAccessTokenError");
    }

    [Fact]
    public async Task ConfigureAccessTokenAsync_WhenCalledWithEmptyToken_ShouldThrowSimpleFinDecodeTokenRequestError()
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

        // Act
        Func<Task> act = async () =>
            await simpleFinService.ConfigureAccessTokenAsync(helper.demoUser.Id, emptyToken);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("SimpleFinDecodeTokenRequestError");
    }

    [Fact]
    public async Task ConfigureAccessTokenAsync_WhenDecodeRequestIsNotSuccess_ShouldThrowSimpleFinDecodeTokenError()
    {
        // Arrange
        var helper = new TestHelper();

        // Create a mock HTTP client that returns an error for decode
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.BadRequest }
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

        // Act
        Func<Task> act = async () =>
            await simpleFinService.ConfigureAccessTokenAsync(helper.demoUser.Id, validBase64Token);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("SimpleFinDecodeTokenError");
    }
    #endregion

    #region RefreshAccountsAsync
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
    public async Task RefreshAccountsAsync_WhenCalledWithInvalidUserId_ShouldThrowInvalidUserError()
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

        // Act
        Func<Task> act = async () => await simpleFinService.RefreshAccountsAsync(invalidUserId);

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidUserError");
    }

    [Fact]
    public async Task RefreshAccountsAsync_WhenAccessTokenIsInvalid_ShouldReturnSimpleFinDataNotFoundError()
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
    public async Task RefreshAccountsAsync_WhenAccessTokenIsMissing_ShouldReturnSimpleFinMissingAccessTokenError()
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
    public async Task RefreshAccountsAsync_WhenAccessTokenCannotBeParsed_ShouldReturnParseError()
    {
        // Arrange
        var helper = new TestHelper();
        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(DateTime.UtcNow);
        var simpleFinService = CreateSimpleFinService(
            helper,
            CreateResponseHandler("{\"errors\":[],\"accounts\":[]}"),
            nowProviderMock.Object
        );

        helper.demoUser.SimpleFinAccessToken = "malformed-access-token";
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.RefreshAccountsAsync(helper.demoUser.Id);

        // Assert
        errors.Should().ContainSingle().Which.Should().Be("SimpleFinAccessTokenParseError");
    }

    [Fact]
    public async Task RefreshAccountsAsync_WhenHttpRequestFails_ShouldReturnSimpleFinDataRequestError()
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
    public async Task RefreshAccountsAsync_WhenInvalidJsonResponse_ShouldReturnSimpleFinDataNotFoundError()
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
    public async Task RefreshAccountsAsync_WhenOneAccountHasBadBalance_ShouldRefreshRemainingAccounts()
    {
        // Arrange
        var helper = new TestHelper();

        // First account has an empty balance string — decimal.Parse will throw.
        // Second account is valid and must still be refreshed.
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
                    ""id"": ""account-bad"",
                    ""name"": ""Bad Balance Account"",
                    ""currency"": ""USD"",
                    ""balance"": """",
                    ""balance-date"": 1609459200,
                    ""transactions"": []
                },
                {
                    ""org"": {
                        ""domain"": ""example.com"",
                        ""sfin-url"": ""https://example.com/simplefin"",
                        ""name"": ""Example Bank"",
                        ""url"": ""https://example.com"",
                        ""id"": ""org-123""
                    },
                    ""id"": ""account-valid"",
                    ""name"": ""Valid Account"",
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

        // Assert: the valid account was still created despite the bad-balance account throwing
        simpleFinAccountServiceMock.Verify(
            s =>
                s.CreateSimpleFinAccountAsync(
                    helper.demoUser.Id,
                    It.Is<ISimpleFinAccountCreateRequest>(r => r.SyncID == "account-valid")
                ),
            Times.Once
        );
        simpleFinAccountServiceMock.Verify(
            s =>
                s.CreateSimpleFinAccountAsync(
                    helper.demoUser.Id,
                    It.Is<ISimpleFinAccountCreateRequest>(r => r.SyncID == "account-bad")
                ),
            Times.Never
        );

        // Assert: an error is reported for the bad-balance account
        errors.Should().NotBeEmpty();
        errors.Should().Contain(e => e.Contains("SimpleFinAccountSyncException"));
    }

    [Fact]
    public async Task RefreshAccountsAsync_WhenOrganizationCreationDoesNotPersist_ShouldReturnOrganizationError()
    {
        // Arrange
        var helper = new TestHelper();
        var organizationServiceMock = new Mock<ISimpleFinOrganizationService>();
        var accountServiceMock = new Mock<ISimpleFinAccountService>();
        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(DateTime.UtcNow);
        var simpleFinService = CreateSimpleFinService(
            helper,
            CreateResponseHandler(
                """
                {
                    "errors": [],
                    "accounts": [
                        {
                            "org": { "domain": "example.com", "sfin-url": "https://example.com/simplefin", "name": "Example Bank", "url": "https://example.com" },
                            "id": "account-456",
                            "name": "Checking Account",
                            "currency": "USD",
                            "balance": "1000.50",
                            "balance-date": 1609459200,
                            "transactions": []
                        }
                    ]
                }
                """
            ),
            nowProviderMock.Object,
            simpleFinOrganizationService: organizationServiceMock.Object,
            simpleFinAccountService: accountServiceMock.Object
        );

        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.RefreshAccountsAsync(helper.demoUser.Id);

        // Assert
        errors
            .Should()
            .ContainSingle()
            .Which.Should()
            .Contain("SyncSimpleFinOrganizationCreationError");
        organizationServiceMock.Verify(
            service =>
                service.CreateSimpleFinOrganizationAsync(
                    helper.demoUser.Id,
                    It.IsAny<ISimpleFinOrganizationCreateRequest>()
                ),
            Times.Once
        );
        accountServiceMock.Verify(
            service =>
                service.CreateSimpleFinAccountAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<ISimpleFinAccountCreateRequest>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RefreshAccountsAsync_WhenMatchingSyncIdBelongsToAnotherOrganization_ShouldCreateAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var organizationServiceMock = new Mock<ISimpleFinOrganizationService>();
        var accountServiceMock = new Mock<ISimpleFinAccountService>();
        var organization = new SimpleFinOrganization
        {
            Domain = "example.com",
            SimpleFinUrl = "https://example.com/simplefin",
            Name = "Example Bank",
            UserID = helper.demoUser.Id,
        };
        var existingAccount = new SimpleFinAccount
        {
            SyncID = "account-456",
            Name = "Other Account",
            Currency = "USD",
            Balance = 500.00m,
            OrganizationId = null,
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.SimpleFinOrganizations.Add(organization);
        helper.UserDataContext.SimpleFinAccounts.Add(existingAccount);

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(DateTime.UtcNow);
        var simpleFinService = CreateSimpleFinService(
            helper,
            CreateResponseHandler(
                """
                {
                    "errors": [],
                    "accounts": [
                        {
                            "org": { "domain": "example.com", "sfin-url": "https://example.com/simplefin", "name": "Example Bank", "url": "https://example.com" },
                            "id": "account-456",
                            "name": "Checking Account",
                            "currency": "USD",
                            "balance": "1000.50",
                            "balance-date": 1609459200,
                            "transactions": []
                        }
                    ]
                }
                """
            ),
            nowProviderMock.Object,
            simpleFinOrganizationService: organizationServiceMock.Object,
            simpleFinAccountService: accountServiceMock.Object
        );

        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.RefreshAccountsAsync(helper.demoUser.Id);

        // Assert
        errors.Should().BeEmpty();
        accountServiceMock.Verify(
            service =>
                service.CreateSimpleFinAccountAsync(
                    helper.demoUser.Id,
                    It.Is<ISimpleFinAccountCreateRequest>(request =>
                        request.SyncID == "account-456"
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task RefreshAccountsAsync_WhenSyncIdDoesNotMatchOrBelongsToAnotherOrganization_ShouldCreateAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var targetOrganization = new SimpleFinOrganization
        {
            Domain = "example.com",
            SimpleFinUrl = "https://example.com/simplefin",
            Name = "Example Bank",
            UserID = helper.demoUser.Id,
        };
        var otherOrganization = new SimpleFinOrganization
        {
            Domain = "other.example.com",
            SimpleFinUrl = "https://other.example.com/simplefin",
            Name = "Other Bank",
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.SimpleFinOrganizations.AddRange(
            targetOrganization,
            otherOrganization
        );
        helper.UserDataContext.SimpleFinAccounts.AddRange(
            new SimpleFinAccount
            {
                SyncID = "different-sync-id",
                Name = "Different Account",
                Currency = "USD",
                OrganizationId = targetOrganization.ID,
                UserID = helper.demoUser.Id,
            },
            new SimpleFinAccount
            {
                SyncID = "account-456",
                Name = "Other Organization Account",
                Currency = "USD",
                OrganizationId = otherOrganization.ID,
                UserID = helper.demoUser.Id,
            }
        );

        var accountServiceMock = new Mock<ISimpleFinAccountService>();
        var organizationServiceMock = new Mock<ISimpleFinOrganizationService>();
        var simpleFinService = CreateSimpleFinService(
            helper,
            CreateResponseHandler(
                """
                {
                    "errors": [],
                    "accounts": [
                        {
                            "org": { "domain": "example.com", "sfin-url": "https://example.com/simplefin", "name": "Example Bank", "url": "https://example.com" },
                            "id": "account-456",
                            "name": "Checking Account",
                            "currency": "USD",
                            "balance": "1000.50",
                            "balance-date": 1609459200,
                            "transactions": []
                        }
                    ]
                }
                """
            ),
            Mock.Of<INowProvider>(),
            simpleFinOrganizationService: organizationServiceMock.Object,
            simpleFinAccountService: accountServiceMock.Object
        );
        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.RefreshAccountsAsync(helper.demoUser.Id);

        // Assert
        errors.Should().BeEmpty();
        accountServiceMock.Verify(
            service =>
                service.CreateSimpleFinAccountAsync(
                    helper.demoUser.Id,
                    It.Is<ISimpleFinAccountCreateRequest>(request =>
                        request.SyncID == "account-456"
                    )
                ),
            Times.Once
        );
    }
    #endregion

    #region RemoveAccessTokenAsync
    [Fact]
    public async Task RemoveAccessTokenAsync_WhenCalledWithInvalidUserId_ShouldThrowInvalidUserError()
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

        // Act
        Func<Task> act = async () => await simpleFinService.RemoveAccessTokenAsync(invalidUserId);

        // Act & Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidUserError");
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
            InstitutionID = Guid.NewGuid(),
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
    public async Task RemoveAccessTokenAsync_WhenSimpleFinAccountIsUnlinked_ShouldNotUpdateAnAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var accountServiceMock = new Mock<IAccountService>();
        var simpleFinAccountServiceMock = new Mock<ISimpleFinAccountService>();
        var simpleFinOrganizationServiceMock = new Mock<ISimpleFinOrganizationService>();
        var simpleFinService = new SimpleFinService(
            new Mock<IHttpClientFactory>().Object,
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

        var organization = new SimpleFinOrganization
        {
            Domain = "example.com",
            Name = "Example Bank",
            UserID = helper.demoUser.Id,
        };
        var simpleFinAccount = new SimpleFinAccount
        {
            SyncID = "unlinked-account",
            Name = "Unlinked",
            Currency = "USD",
            OrganizationId = organization.ID,
            LinkedAccountId = null,
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.Accounts.Add(
            new Account
            {
                Name = "Unrelated Account",
                Type = "checking",
                InstitutionID = Guid.NewGuid(),
                UserID = helper.demoUser.Id,
            }
        );
        helper.UserDataContext.SimpleFinOrganizations.Add(organization);
        helper.UserDataContext.SimpleFinAccounts.Add(simpleFinAccount);
        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        await simpleFinService.RemoveAccessTokenAsync(helper.demoUser.Id);

        // Assert
        accountServiceMock.Verify(
            service =>
                service.UpdateAccountAsync(It.IsAny<Guid>(), It.IsAny<IAccountUpdateRequest>()),
            Times.Never
        );
        simpleFinAccountServiceMock.Verify(
            service => service.DeleteSimpleFinAccountAsync(helper.demoUser.Id, simpleFinAccount.ID),
            Times.Once
        );
        simpleFinOrganizationServiceMock.Verify(
            service =>
                service.DeleteSimpleFinOrganizationAsync(helper.demoUser.Id, organization.ID),
            Times.Once
        );
    }
    #endregion

    #region SyncTransactionHistoryAsync
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
            InstitutionID = Guid.NewGuid(),
            Type = "checking",
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
            s => s.CreateTransactionAsync(helper.demoUser, It.IsAny<ITransactionCreateRequest>()),
            Times.Once
        );
        balanceServiceMock.Verify(
            s => s.CreateBalancesAsync(helper.demoUser.Id, It.IsAny<IBalanceCreateRequest>()),
            Times.Once
        );
    }

    [Fact]
    public async Task SyncTransactionHistoryAsync_WhenCalledWithInvalidUserId_ShouldThrowInvalidUserError()
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

        // Act
        Func<Task> act = async () =>
            await simpleFinService.SyncTransactionHistoryAsync(invalidUserId);

        // Act & Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidUserError");
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
            InstitutionID = Guid.NewGuid(),
            Type = "checking",
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
                    It.IsAny<ITransactionCreateRequest>()
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

        helper.UserDataContext.Accounts.Add(
            new Database.Models.Account
            {
                Name = "Unrelated Account",
                InstitutionID = Guid.NewGuid(),
                Type = "checking",
                UserID = helper.demoUser.Id,
            }
        );

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
                    It.IsAny<ITransactionCreateRequest>()
                ),
            Times.Never
        );
        balanceServiceMock.Verify(
            s => s.CreateBalancesAsync(It.IsAny<Guid>(), It.IsAny<IBalanceCreateRequest>()),
            Times.Never
        );
    }

    [Fact]
    public async Task SyncTransactionHistoryAsync_WhenLinkedAccountDoesNotExist_ShouldSkipSync()
    {
        // Arrange
        var helper = new TestHelper();
        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(DateTime.UtcNow);
        var transactionServiceMock = new Mock<ITransactionService>();
        var balanceServiceMock = new Mock<IBalanceService>();
        var simpleFinService = CreateSimpleFinService(
            helper,
            CreateResponseHandler(
                """
                {
                    "errors": [],
                    "accounts": [
                        {
                            "org": { "domain": "example.com", "sfin-url": "https://example.com/simplefin", "name": "Example Bank", "url": "https://example.com" },
                            "id": "account-456",
                            "name": "Checking Account",
                            "currency": "USD",
                            "balance": "1000.00",
                            "balance-date": 1609459200,
                            "transactions": []
                        }
                    ]
                }
                """
            ),
            nowProviderMock.Object,
            transactionService: transactionServiceMock.Object,
            balanceService: balanceServiceMock.Object
        );

        var unrelatedAccount = new Account
        {
            Name = "Manual Account",
            Type = "checking",
            InstitutionID = Guid.NewGuid(),
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.Accounts.Add(unrelatedAccount);
        var missingLinkedAccountId = Guid.NewGuid();
        helper.UserDataContext.SimpleFinAccounts.Add(
            new SimpleFinAccount
            {
                SyncID = "account-456",
                Name = "Checking Account",
                Currency = "USD",
                LinkedAccountId = missingLinkedAccountId,
                UserID = helper.demoUser.Id,
            }
        );
        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();
        helper.UserDataContext.SimpleFinAccounts.Single().LinkedAccountId = missingLinkedAccountId;
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.SyncTransactionHistoryAsync(helper.demoUser.Id);

        // Assert
        errors.Should().BeEmpty();
        transactionServiceMock.Verify(
            service =>
                service.CreateTransactionAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<ITransactionCreateRequest>()
                ),
            Times.Never
        );
        balanceServiceMock.Verify(
            service =>
                service.CreateBalancesAsync(It.IsAny<Guid>(), It.IsAny<IBalanceCreateRequest>()),
            Times.Never
        );
    }

    [Fact]
    public async Task SyncTransactionHistoryAsync_WhenLinkedAccountIsDeleted_ShouldExcludeItFromSyncWindow()
    {
        // Arrange
        var helper = new TestHelper();
        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(provider => provider.UtcNow).Returns(DateTime.UtcNow);
        var simpleFinService = CreateSimpleFinService(
            helper,
            CreateResponseHandler("{\"errors\":[],\"accounts\":[]}"),
            nowProviderMock.Object
        );
        var deletedAccount = new Account
        {
            Name = "Deleted Account",
            Type = "checking",
            InstitutionID = Guid.NewGuid(),
            UserID = helper.demoUser.Id,
            Deleted = DateTime.UtcNow,
        };
        helper.UserDataContext.Accounts.Add(deletedAccount);
        helper.UserDataContext.SimpleFinAccounts.Add(
            new SimpleFinAccount
            {
                SyncID = "account-456",
                Name = "Checking Account",
                LinkedAccountId = deletedAccount.ID,
                LinkedAccount = deletedAccount,
                UserID = helper.demoUser.Id,
            }
        );
        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();
        helper.UserDataContext.SimpleFinAccounts.Single().LinkedAccountId = deletedAccount.ID;
        helper.UserDataContext.SaveChanges();
        helper
            .UserDataContext.Accounts.Single(account => account.ID == deletedAccount.ID)
            .Deleted.Should()
            .NotBeNull();
        helper
            .UserDataContext.SimpleFinAccounts.Single()
            .LinkedAccountId.Should()
            .Be(deletedAccount.ID);

        // Act
        var errors = await simpleFinService.SyncTransactionHistoryAsync(helper.demoUser.Id);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task SyncTransactionsAsync_WhenAccountNavigationIsMissing_ShouldReturnTransactionError()
    {
        // Arrange
        var helper = new TestHelper();
        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(DateTime.UtcNow);
        var simpleFinService = CreateSimpleFinService(
            helper,
            CreateResponseHandler("{\"errors\":[],\"accounts\":[]}"),
            nowProviderMock.Object
        );

        var account = new Account
        {
            Name = "Checking",
            Type = "checking",
            InstitutionID = Guid.NewGuid(),
            UserID = helper.demoUser.Id,
        };
        var simpleFinAccount = new SimpleFinAccount
        {
            ID = Guid.NewGuid(),
            SyncID = "account-456",
            Name = "Checking Account",
            Currency = "USD",
            LinkedAccountId = account.ID,
            UserID = helper.demoUser.Id,
        };
        var transactionData = new SimpleFinTransactionData
        {
            Id = "txn-1",
            Posted = 1609372800,
            Amount = "-50.00",
            Description = "Coffee Shop",
            TransactedAt = 1609372800,
            Pending = false,
        };
        var userData = new ApplicationUser { Id = helper.demoUser.Id, Accounts = [account] };

        // Act
        var method = typeof(SimpleFinService).GetMethod(
            "SyncTransactionsAsync",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
        );
        var task =
            (Task<List<string>>)
                method!.Invoke(
                    simpleFinService,
                    [userData, simpleFinAccount, new[] { transactionData }]
                )!;
        var errors = await task;

        // Assert
        errors
            .Should()
            .ContainSingle()
            .Which.Should()
            .Contain("SimpleFinAccountNotFoundForTransactionError");
    }

    [Fact]
    public async Task SyncBalancesAsync_WhenAccountCannotBeFound_ShouldReturnAccountError()
    {
        // Arrange
        var helper = new TestHelper();
        var simpleFinService = CreateSimpleFinService(
            helper,
            CreateResponseHandler("{\"errors\":[],\"accounts\":[]}"),
            Mock.Of<INowProvider>()
        );
        var accountData = new SimpleFinAccountData { Balance = "100.00", BalanceDate = 1609459200 };
        var simpleFinAccountId = Guid.NewGuid();
        var account = new Account
        {
            Name = "Checking",
            Type = "checking",
            InstitutionID = Guid.NewGuid(),
            UserID = helper.demoUser.Id,
            SimpleFinAccount = new SimpleFinAccount
            {
                ID = simpleFinAccountId,
                UserID = helper.demoUser.Id,
            },
        };
        var userData = new ApplicationUser
        {
            Id = helper.demoUser.Id,
            Accounts = new FirstEnumerationOnlyAccountCollection(account),
        };

        // Act
        var method = typeof(SimpleFinService).GetMethod(
            "SyncBalancesAsync",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
        );
        var task =
            (Task<List<string>>)
                method!.Invoke(simpleFinService, [userData, simpleFinAccountId, accountData])!;
        var errors = await task;

        // Assert
        errors.Should().ContainSingle().Which.Should().Contain("AccountNotFoundError");
    }

    [Fact]
    public async Task SyncBalancesAsync_WhenUnrelatedAccountPrecedesLinkedAccount_ShouldFindLinkedAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceServiceMock = new Mock<IBalanceService>();
        var simpleFinService = CreateSimpleFinService(
            helper,
            CreateResponseHandler("{\"errors\":[],\"accounts\":[]}"),
            Mock.Of<INowProvider>(),
            balanceService: balanceServiceMock.Object
        );
        var simpleFinAccountId = Guid.NewGuid();
        var unrelatedAccount = new Account
        {
            Name = "Manual Account",
            Type = "checking",
            InstitutionID = Guid.NewGuid(),
            UserID = helper.demoUser.Id,
        };
        var linkedAccount = new Account
        {
            Name = "Checking",
            Type = "checking",
            InstitutionID = Guid.NewGuid(),
            UserID = helper.demoUser.Id,
            SimpleFinAccount = new SimpleFinAccount
            {
                ID = simpleFinAccountId,
                UserID = helper.demoUser.Id,
            },
        };
        var userData = new ApplicationUser
        {
            Id = helper.demoUser.Id,
            Accounts = [unrelatedAccount, linkedAccount],
        };
        var accountData = new SimpleFinAccountData { Balance = "100.00", BalanceDate = 1609459200 };

        // Act
        var method = typeof(SimpleFinService).GetMethod(
            "SyncBalancesAsync",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
        );
        var task =
            (Task<List<string>>)
                method!.Invoke(simpleFinService, [userData, simpleFinAccountId, accountData])!;
        var errors = await task;

        // Assert
        errors.Should().BeEmpty();
        balanceServiceMock.Verify(
            service =>
                service.CreateBalancesAsync(
                    helper.demoUser.Id,
                    It.Is<IBalanceCreateRequest>(request => request.AccountID == linkedAccount.ID)
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task SyncAccountsAsync_WhenUnrelatedAccountPrecedesLinkedAccount_ShouldFindLinkedAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceServiceMock = new Mock<IBalanceService>();
        var simpleFinService = CreateSimpleFinService(
            helper,
            CreateResponseHandler("{\"errors\":[],\"accounts\":[]}"),
            Mock.Of<INowProvider>(),
            balanceService: balanceServiceMock.Object
        );
        var simpleFinAccountId = Guid.NewGuid();
        var linkedAccountId = Guid.NewGuid();
        var unrelatedAccount = new Account
        {
            Name = "Unrelated Account",
            Type = "checking",
            InstitutionID = Guid.NewGuid(),
            UserID = helper.demoUser.Id,
        };
        var linkedAccount = new Account
        {
            ID = linkedAccountId,
            Name = "Linked Account",
            Type = "checking",
            InstitutionID = Guid.NewGuid(),
            UserID = helper.demoUser.Id,
            SimpleFinAccount = new SimpleFinAccount
            {
                ID = simpleFinAccountId,
                UserID = helper.demoUser.Id,
            },
        };
        var simpleFinAccount = new SimpleFinAccount
        {
            ID = simpleFinAccountId,
            SyncID = "account-456",
            Name = "Checking Account",
            LinkedAccountId = linkedAccountId,
            UserID = helper.demoUser.Id,
        };
        var userData = new ApplicationUser
        {
            Id = helper.demoUser.Id,
            SimpleFinAccounts = [simpleFinAccount],
            Accounts = [unrelatedAccount, linkedAccount],
        };
        var accountData = new SimpleFinAccountData
        {
            Id = "account-456",
            Balance = "100.00",
            BalanceDate = 1609459200,
            Transactions = [],
        };

        // Act
        var method = typeof(SimpleFinService).GetMethod(
            "SyncAccountsAsync",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
        );
        var task =
            (Task<List<string>>)
                method!.Invoke(simpleFinService, [userData, new[] { accountData }])!;
        var errors = await task;

        // Assert
        errors.Should().BeEmpty();
        balanceServiceMock.Verify(
            service =>
                service.CreateBalancesAsync(
                    helper.demoUser.Id,
                    It.Is<IBalanceCreateRequest>(request => request.AccountID == linkedAccountId)
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task SyncAccountsAsync_WhenTransactionSyncReturnsErrors_ShouldStillSyncBalance()
    {
        // Arrange
        var helper = new TestHelper();
        var balanceServiceMock = new Mock<IBalanceService>();
        var simpleFinService = CreateSimpleFinService(
            helper,
            CreateResponseHandler("{\"errors\":[],\"accounts\":[]}"),
            Mock.Of<INowProvider>(),
            balanceService: balanceServiceMock.Object
        );
        var simpleFinAccountId = Guid.NewGuid();
        var linkedAccountId = Guid.NewGuid();
        var unrelatedAccount = new Account
        {
            Name = "Transaction Account",
            Type = "checking",
            InstitutionID = Guid.NewGuid(),
            UserID = helper.demoUser.Id,
        };
        var linkedAccount = new Account
        {
            ID = linkedAccountId,
            Name = "Linked Account",
            Type = "checking",
            InstitutionID = Guid.NewGuid(),
            UserID = helper.demoUser.Id,
            SimpleFinAccount = new SimpleFinAccount
            {
                ID = Guid.NewGuid(),
                UserID = helper.demoUser.Id,
            },
        };
        var balanceAccount = new Account
        {
            Name = "Balance Account",
            Type = "checking",
            InstitutionID = Guid.NewGuid(),
            UserID = helper.demoUser.Id,
            SimpleFinAccount = new SimpleFinAccount
            {
                ID = simpleFinAccountId,
                UserID = helper.demoUser.Id,
            },
        };
        var simpleFinAccount = new SimpleFinAccount
        {
            ID = simpleFinAccountId,
            SyncID = "account-456",
            Name = "Checking Account",
            Currency = "USD",
            LinkedAccountId = linkedAccountId,
            LastSync = new DateTime(2025, 1, 1),
            UserID = helper.demoUser.Id,
        };
        var userData = new ApplicationUser
        {
            Id = helper.demoUser.Id,
            SimpleFinAccounts = [simpleFinAccount],
            Accounts = new SequencedAccountCollection(
                [unrelatedAccount, linkedAccount],
                [linkedAccount],
                [balanceAccount],
                [balanceAccount]
            ),
        };
        var accountData = new SimpleFinAccountData
        {
            Id = "account-456",
            Balance = "100.00",
            BalanceDate = 1609459200,
            Transactions =
            [
                new SimpleFinTransactionData
                {
                    Id = "transaction-1",
                    Posted = 1609372800,
                    Amount = "-50.00",
                    Description = "Coffee Shop",
                    TransactedAt = 1609372800,
                    Pending = false,
                },
            ],
        };

        // Act
        var method = typeof(SimpleFinService).GetMethod(
            "SyncAccountsAsync",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
        );
        var task =
            (Task<List<string>>)
                method!.Invoke(simpleFinService, [userData, new[] { accountData }])!;
        var errors = await task;

        // Assert
        errors
            .Should()
            .ContainSingle()
            .Which.Should()
            .Contain("SimpleFinAccountNotFoundForTransactionError");
        balanceServiceMock.Verify(
            service =>
                service.CreateBalancesAsync(It.IsAny<Guid>(), It.IsAny<IBalanceCreateRequest>()),
            Times.Once
        );
        simpleFinAccount.LastSync.Should().Be(new DateTime(2025, 1, 1));
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
    public async Task SyncTransactionHistoryAsync_WhenInvalidJsonResponse_ShouldReturnSimpleFinDataNotFoundError()
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
                    Content = new StringContent("{ invalid json }"),
                }
            );

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(_ => _.CreateClient(string.Empty)).Returns(httpClient);

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(DateTime.UtcNow);

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

        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.SyncTransactionHistoryAsync(helper.demoUser.Id);

        // Assert
        errors.Should().ContainSingle().Which.Should().Be("SimpleFinDataNotFoundError");
    }

    [Fact]
    public async Task SyncTransactionHistoryAsync_WhenAccountCollectionIsNull_ShouldReturnRetrievalError()
    {
        // Arrange
        var helper = new TestHelper();
        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(DateTime.UtcNow);
        var simpleFinService = CreateSimpleFinService(
            helper,
            CreateResponseHandler("{\"errors\":[],\"accounts\":null}"),
            nowProviderMock.Object
        );

        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.SyncTransactionHistoryAsync(helper.demoUser.Id);

        // Assert
        errors.Should().ContainSingle().Which.Should().Be("SimpleFinDataRetrievalError");
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(4, true)]
    public async Task SyncTransactionHistoryAsync_WhenForceLookbackIsConfigured_ShouldUseConfiguredLookback(
        int lookbackMonths,
        bool shouldCapLookback
    )
    {
        // Arrange
        var helper = new TestHelper();
        var fixedNow = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fixedNow);

        Uri? requestUri = null;
        var httpMessageHandler = CreateResponseHandler(
            "{\"errors\":[],\"accounts\":[]}",
            request => requestUri = request.RequestUri
        );
        var simpleFinService = CreateSimpleFinService(
            helper,
            httpMessageHandler,
            nowProviderMock.Object
        );

        helper.UserDataContext.UserSettings.Add(
            new UserSettings
            {
                UserID = helper.demoUser.Id,
                ForceSyncLookbackMonths = lookbackMonths,
            }
        );
        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();

        var nowUnix = new DateTimeOffset(fixedNow).ToUnixTimeSeconds();
        var lookbackUnix = shouldCapLookback ? 7689600L : 2629743L * lookbackMonths;

        // Act
        var errors = await simpleFinService.SyncTransactionHistoryAsync(helper.demoUser.Id);

        // Assert
        errors.Should().BeEmpty();
        requestUri.Should().NotBeNull();
        requestUri!.Query.Should().Contain($"start-date={nowUnix - lookbackUnix}");
    }

    [Fact]
    public async Task SyncTransactionHistoryAsync_WhenLinkedAccountHasNeverSynced_ShouldUseMaximumLookback()
    {
        // Arrange
        var helper = new TestHelper();
        var fixedNow = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fixedNow);

        Uri? requestUri = null;
        var httpMessageHandler = CreateResponseHandler(
            "{\"errors\":[],\"accounts\":[]}",
            request => requestUri = request.RequestUri
        );
        var simpleFinService = CreateSimpleFinService(
            helper,
            httpMessageHandler,
            nowProviderMock.Object
        );

        var account = new Account
        {
            Name = "Checking",
            Type = "checking",
            InstitutionID = Guid.NewGuid(),
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SimpleFinAccounts.Add(
            new SimpleFinAccount
            {
                SyncID = "account-never-synced",
                Name = "Checking",
                Currency = "USD",
                LinkedAccountId = account.ID,
                UserID = helper.demoUser.Id,
            }
        );
        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();

        var nowUnix = new DateTimeOffset(fixedNow).ToUnixTimeSeconds();

        // Act
        var errors = await simpleFinService.SyncTransactionHistoryAsync(helper.demoUser.Id);

        // Assert
        errors.Should().BeEmpty();
        requestUri.Should().NotBeNull();
        requestUri!.Query.Should().Contain($"start-date={nowUnix - 7689600L}");
    }

    [Fact]
    public async Task SyncTransactionHistoryAsync_WhenProviderAccountIsNotKnown_ShouldReturnAccountError()
    {
        // Arrange
        var helper = new TestHelper();
        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(DateTime.UtcNow);
        var httpMessageHandler = CreateResponseHandler(
            """
            {
                "errors": [],
                "accounts": [
                    {
                        "org": { "domain": "example.com", "sfin-url": "https://example.com/simplefin", "name": "Example Bank", "url": "https://example.com" },
                        "id": "unknown-account",
                        "name": "Unknown Account",
                        "currency": "USD",
                        "balance": "100.00",
                        "balance-date": 1609459200,
                        "transactions": []
                    }
                ]
            }
            """
        );
        var simpleFinService = CreateSimpleFinService(
            helper,
            httpMessageHandler,
            nowProviderMock.Object
        );
        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.SyncTransactionHistoryAsync(helper.demoUser.Id);

        // Assert
        errors
            .Should()
            .ContainSingle()
            .Which.Should()
            .Contain("SimpleFinAccountNotFoundForSyncError");
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
            InstitutionID = Guid.NewGuid(),
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.Accounts.Add(account1);

        var account2 = new Database.Models.Account
        {
            Name = "My Savings",
            Type = "savings",
            InstitutionID = Guid.NewGuid(),
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
                s.CreateTransactionAsync(
                    It.Is<ApplicationUser>(u => u.Id == helper.demoUser.Id),
                    It.IsAny<ITransactionCreateRequest>()
                ),
            Times.Exactly(2)
        );
        balanceServiceMock.Verify(
            s => s.CreateBalancesAsync(helper.demoUser.Id, It.IsAny<IBalanceCreateRequest>()),
            Times.Exactly(2)
        );
    }

    [Fact]
    public async Task SyncTransactionHistoryAsync_WhenTransactionsAreFilteredOrAlreadyExist_ShouldOnlyCreateNewTransactions()
    {
        // Arrange
        var helper = new TestHelper();
        var fixedNow = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fixedNow);
        var transactionServiceMock = new Mock<ITransactionService>();
        var simpleFinService = CreateSimpleFinService(
            helper,
            CreateResponseHandler(
                """
                {
                    "errors": [],
                    "accounts": [
                        {
                            "org": { "domain": "example.com", "sfin-url": "https://example.com/simplefin", "name": "Example Bank", "url": "https://example.com" },
                            "id": "account-456",
                            "name": "Checking Account",
                            "currency": "USD",
                            "balance": "1000.50",
                            "balance-date": 1609459200,
                            "transactions": [
                                { "id": "existing", "posted": 1767225600, "amount": "-10.00", "description": "Existing", "transacted_at": 1767225600, "pending": false },
                                { "id": "too-old", "posted": 1735689600, "amount": "-20.00", "description": "Old", "transacted_at": 1735689600, "pending": false },
                                { "id": "pending-new", "posted": 1735689600, "amount": "-30.00", "description": "Pending", "transacted_at": 1767225600, "pending": true }
                            ]
                        }
                    ]
                }
                """
            ),
            nowProviderMock.Object,
            transactionService: transactionServiceMock.Object
        );

        var organization = new SimpleFinOrganization
        {
            Domain = "example.com",
            Name = "Example Bank",
            UserID = helper.demoUser.Id,
        };
        var account = new Account
        {
            Name = "Checking",
            Type = "checking",
            InstitutionID = Guid.NewGuid(),
            UserID = helper.demoUser.Id,
        };
        var simpleFinAccount = new SimpleFinAccount
        {
            SyncID = "account-456",
            Name = "Checking Account",
            Currency = "USD",
            OrganizationId = organization.ID,
            LinkedAccountId = account.ID,
            SyncStartDate = new DateOnly(2025, 1, 1),
            UserID = helper.demoUser.Id,
        };
        account.SimpleFinAccount = simpleFinAccount;
        account.Transactions.Add(
            new Transaction
            {
                SyncID = "existing",
                Amount = -10.00m,
                Date = new DateOnly(2026, 1, 1),
                Source = TransactionSource.SimpleFIN,
                AccountID = account.ID,
            }
        );
        account.Transactions.Add(
            new Transaction
            {
                SyncID = null,
                Amount = -5.00m,
                Date = new DateOnly(2026, 1, 1),
                Source = TransactionSource.SimpleFIN,
                AccountID = account.ID,
            }
        );
        helper.UserDataContext.SimpleFinOrganizations.Add(organization);
        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SimpleFinAccounts.Add(simpleFinAccount);
        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.SyncTransactionHistoryAsync(helper.demoUser.Id);

        // Assert
        errors.Should().BeEmpty();
        transactionServiceMock.Verify(
            service =>
                service.CreateTransactionAsync(
                    helper.demoUser,
                    It.Is<ITransactionCreateRequest>(request =>
                        request.SyncID == "pending-new"
                        && request.Date == new DateOnly(2026, 1, 1)
                        && request.Amount == -30.00m
                        && request.MerchantName == "Pending"
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task SyncTransactionHistoryAsync_WhenOneAccountThrowsDuringSync_ShouldSyncRemainingAccounts()
    {
        // Arrange
        var helper = new TestHelper();

        // First account has an empty balance (causes decimal.Parse to throw).
        // Second account is valid and must still be synced.
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
                    ""id"": ""account-bad-balance"",
                    ""name"": ""Bad Balance Account"",
                    ""currency"": ""USD"",
                    ""balance"": """",
                    ""balance-date"": 1609459200,
                    ""transactions"": []
                },
                {
                    ""org"": {
                        ""domain"": ""example.com"",
                        ""sfin-url"": ""https://example.com/simplefin"",
                        ""name"": ""Example Bank"",
                        ""url"": ""https://example.com"",
                        ""id"": ""org-123""
                    },
                    ""id"": ""account-valid"",
                    ""name"": ""Valid Account"",
                    ""currency"": ""USD"",
                    ""balance"": ""2500.00"",
                    ""balance-date"": 1609459200,
                    ""transactions"": [
                        {
                            ""id"": ""txn-1"",
                            ""posted"": 1609372800,
                            ""amount"": ""-25.00"",
                            ""description"": ""Grocery Store"",
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

        var org = new SimpleFinOrganization
        {
            Domain = "example.com",
            SimpleFinUrl = "https://example.com/simplefin",
            Name = "Example Bank",
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.SimpleFinOrganizations.Add(org);

        // Bad-balance account: linked so sync is attempted and the parse throws.
        var accountBad = new Account
        {
            Name = "Bad Balance",
            Type = "checking",
            InstitutionID = Guid.NewGuid(),
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.Accounts.Add(accountBad);

        var simpleFinAccountBad = new SimpleFinAccount
        {
            SyncID = "account-bad-balance",
            Name = "Bad Balance Account",
            Currency = "USD",
            Balance = 500.00m,
            BalanceDate = (int)new DateTimeOffset(DateTime.UtcNow.AddDays(-2)).ToUnixTimeSeconds(),
            OrganizationId = org.ID,
            LinkedAccountId = accountBad.ID,
            UserID = helper.demoUser.Id,
            LastSync = DateTime.UtcNow.AddDays(-1),
        };
        helper.UserDataContext.SimpleFinAccounts.Add(simpleFinAccountBad);

        // Valid account: should sync even though the previous account threw.
        var accountValid = new Account
        {
            Name = "Valid Checking",
            Type = "checking",
            InstitutionID = Guid.NewGuid(),
            UserID = helper.demoUser.Id,
        };
        helper.UserDataContext.Accounts.Add(accountValid);

        var simpleFinAccountValid = new SimpleFinAccount
        {
            SyncID = "account-valid",
            Name = "Valid Account",
            Currency = "USD",
            Balance = 2000.00m,
            BalanceDate = (int)new DateTimeOffset(DateTime.UtcNow.AddDays(-2)).ToUnixTimeSeconds(),
            OrganizationId = org.ID,
            LinkedAccountId = accountValid.ID,
            UserID = helper.demoUser.Id,
            LastSync = DateTime.UtcNow.AddDays(-1),
        };
        helper.UserDataContext.SimpleFinAccounts.Add(simpleFinAccountValid);

        helper.demoUser.SimpleFinAccessToken = "https://demo:demo@test.com/simplefin";
        helper.UserDataContext.SaveChanges();

        // Act
        var errors = await simpleFinService.SyncTransactionHistoryAsync(helper.demoUser.Id);

        // Assert: the valid account still synced despite the bad-balance account throwing
        transactionServiceMock.Verify(
            s =>
                s.CreateTransactionAsync(
                    It.Is<ApplicationUser>(u => u.Id == helper.demoUser.Id),
                    It.IsAny<ITransactionCreateRequest>()
                ),
            Times.Once
        );
        balanceServiceMock.Verify(
            s => s.CreateBalancesAsync(helper.demoUser.Id, It.IsAny<IBalanceCreateRequest>()),
            Times.Once
        );

        // Assert: an error is reported for the bad-balance account
        errors.Should().NotBeEmpty();
        errors.Should().Contain(e => e.Contains("SimpleFinAccountSyncException"));
    }
    #endregion

    [Theory]
    [InlineData(LinkedAccountState.Missing, true)]
    [InlineData(LinkedAccountState.Deleted, false)]
    [InlineData(LinkedAccountState.Active, true)]
    [InlineData(LinkedAccountState.Unrelated, true)]
    public void IsActiveLinkedAccount_WhenEvaluatingLinkedAccountStates_ShouldReturnExpectedResult(
        LinkedAccountState accountState,
        bool expectedResult
    )
    {
        // Arrange
        var linkedAccountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var simpleFinAccount = new SimpleFinAccount
        {
            LinkedAccountId = linkedAccountId,
            UserID = userId,
        };
        Account? linkedAccount = accountState switch
        {
            LinkedAccountState.Missing => null,
            LinkedAccountState.Deleted => new Account
            {
                ID = linkedAccountId,
                InstitutionID = Guid.NewGuid(),
                UserID = userId,
                Deleted = DateTime.UtcNow,
            },
            LinkedAccountState.Active => new Account
            {
                ID = linkedAccountId,
                InstitutionID = Guid.NewGuid(),
                UserID = userId,
            },
            LinkedAccountState.Unrelated => new Account
            {
                ID = Guid.NewGuid(),
                InstitutionID = Guid.NewGuid(),
                UserID = userId,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(accountState), accountState, null),
        };
        var userData = new ApplicationUser { Accounts = [] };
        if (linkedAccount is not null)
        {
            userData.Accounts.Add(linkedAccount);
        }

        // Act
        var result = SimpleFinService.IsActiveLinkedAccount(userData, simpleFinAccount);

        // Assert
        result.Should().Be(expectedResult);
    }

    public enum LinkedAccountState
    {
        Missing,
        Deleted,
        Active,
        Unrelated,
    }

    private static SimpleFinService CreateSimpleFinService(
        TestHelper helper,
        HttpMessageHandler httpMessageHandler,
        INowProvider nowProvider,
        IAccountService? accountService = null,
        ITransactionService? transactionService = null,
        IBalanceService? balanceService = null,
        ISimpleFinOrganizationService? simpleFinOrganizationService = null,
        ISimpleFinAccountService? simpleFinAccountService = null
    )
    {
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(factory => factory.CreateClient(string.Empty))
            .Returns(new HttpClient(httpMessageHandler));

        return new SimpleFinService(
            httpClientFactoryMock.Object,
            helper.UserDataContext,
            Mock.Of<ILogger<ISimpleFinService>>(),
            nowProvider,
            accountService ?? Mock.Of<IAccountService>(),
            transactionService ?? Mock.Of<ITransactionService>(),
            balanceService ?? Mock.Of<IBalanceService>(),
            simpleFinOrganizationService ?? Mock.Of<ISimpleFinOrganizationService>(),
            simpleFinAccountService ?? Mock.Of<ISimpleFinAccountService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );
    }

    private static HttpMessageHandler CreateResponseHandler(
        string responseContent,
        Action<HttpRequestMessage>? onRequest = null
    )
    {
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                (HttpRequestMessage request, CancellationToken _) =>
                {
                    onRequest?.Invoke(request);
                    return new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Content = new StringContent(responseContent),
                    };
                }
            );

        return httpMessageHandlerMock.Object;
    }

    private sealed class FirstEnumerationOnlyAccountCollection(Account account)
        : ICollection<Account>
    {
        private readonly List<Account> accounts = [account];
        private bool hasBeenEnumerated;

        public int Count => accounts.Count;
        public bool IsReadOnly => false;

        public void Add(Account item) => accounts.Add(item);

        public void Clear() => accounts.Clear();

        public bool Contains(Account item) => accounts.Contains(item);

        public void CopyTo(Account[] array, int arrayIndex) => accounts.CopyTo(array, arrayIndex);

        public bool Remove(Account item) => accounts.Remove(item);

        public IEnumerator<Account> GetEnumerator()
        {
            if (hasBeenEnumerated)
            {
                return Enumerable.Empty<Account>().GetEnumerator();
            }

            hasBeenEnumerated = true;
            return accounts.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }

    private sealed class SequencedAccountCollection(params Account[][] accountEnumerations)
        : ICollection<Account>
    {
        private int enumerationNumber;

        public int Count => accountEnumerations.SelectMany(accounts => accounts).Count();
        public bool IsReadOnly => true;

        public void Add(Account item) => throw new NotSupportedException();

        public void Clear() => throw new NotSupportedException();

        public bool Contains(Account item) =>
            accountEnumerations.Any(accounts => accounts.Contains(item));

        public void CopyTo(Account[] array, int arrayIndex) =>
            accountEnumerations
                .SelectMany(accounts => accounts)
                .ToArray()
                .CopyTo(array, arrayIndex);

        public bool Remove(Account item) => throw new NotSupportedException();

        public IEnumerator<Account> GetEnumerator()
        {
            var selectedEnumeration = accountEnumerations[
                Math.Min(enumerationNumber++, accountEnumerations.Length - 1)
            ];
            return selectedEnumeration.AsEnumerable().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}
