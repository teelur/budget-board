using System.Net;
using System.Text;
using BudgetBoard.WebAPI.Controllers;
using BudgetBoard.WebAPI.Models;
using BudgetBoard.WebAPI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace BudgetBoard.IntegrationTests;

public class AppUpdateServiceTests
{
    [Fact]
    public async Task GetLatestAsync_StableRelease_ReturnsReleaseMetadata()
    {
        var (service, handler) = CreateService();
        SetupResponse(
            handler,
            _ =>
                CreateJsonResponse(
                    "{\"tag_name\":\"v3.0.0\",\"html_url\":\"https://github.com/teelur/budget-board/releases/tag/v3.0.0\",\"draft\":false,\"prerelease\":false}"
                )
        );

        var update = await service.GetLatestAsync("stable", CancellationToken.None);

        update
            .Should()
            .BeEquivalentTo(
                new AppUpdateResponse(
                    "stable",
                    "v3.0.0",
                    "https://github.com/teelur/budget-board/releases/tag/v3.0.0"
                )
            );
        VerifyRequestCount(handler, 1);
    }

    [Fact]
    public async Task GetLatestAsync_StablePrerelease_ReturnsNoUpdate()
    {
        var (service, handler) = CreateService();
        SetupResponse(
            handler,
            _ =>
                CreateJsonResponse(
                    "{\"tag_name\":\"v3.0.0-beta.1\",\"html_url\":\"https://github.com/teelur/budget-board/releases/tag/v3.0.0-beta.1\",\"draft\":false,\"prerelease\":true}"
                )
        );

        var update = await service.GetLatestAsync("stable", CancellationToken.None);

        update.Should().BeNull();
    }

    [Fact]
    public async Task GetLatestAsync_DevWorkflowRun_ReturnsShortCommitMetadata()
    {
        var (service, handler) = CreateService();
        SetupResponse(
            handler,
            _ =>
                CreateJsonResponse(
                    "{\"workflow_runs\":[{\"head_sha\":\"abcdef1234567890abcdef1234567890abcdef12\",\"html_url\":\"https://github.com/teelur/budget-board/actions/runs/123\",\"conclusion\":\"success\"}]}"
                )
        );

        var update = await service.GetLatestAsync("dev", CancellationToken.None);

        update
            .Should()
            .BeEquivalentTo(
                new AppUpdateResponse(
                    "dev",
                    "sha-abcdef1",
                    "https://github.com/teelur/budget-board/actions/runs/123"
                )
            );
    }

    [Fact]
    public async Task GetLatestAsync_CachesChannelsIndependently()
    {
        var (service, handler) = CreateService();
        SetupResponse(
            handler,
            request =>
                request.RequestUri?.AbsolutePath.Contains("releases/latest") == true
                    ? CreateJsonResponse(
                        "{\"tag_name\":\"v3.0.0\",\"html_url\":\"https://github.com/teelur/budget-board/releases/tag/v3.0.0\",\"draft\":false,\"prerelease\":false}"
                    )
                    : CreateJsonResponse(
                        "{\"workflow_runs\":[{\"head_sha\":\"abcdef1234567890abcdef1234567890abcdef12\",\"html_url\":\"https://github.com/teelur/budget-board/actions/runs/123\",\"conclusion\":\"success\"}]}"
                    )
        );

        await service.GetLatestAsync("stable", CancellationToken.None);
        await service.GetLatestAsync("stable", CancellationToken.None);
        await service.GetLatestAsync("dev", CancellationToken.None);
        await service.GetLatestAsync("dev", CancellationToken.None);

        VerifyRequestCount(handler, 2);
    }

    [Fact]
    public async Task GetLatestAsync_FailedGitHubResponse_IsNegativeCached()
    {
        var (service, handler) = CreateService();
        SetupResponse(handler, _ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

        var firstUpdate = await service.GetLatestAsync("stable", CancellationToken.None);
        var secondUpdate = await service.GetLatestAsync("stable", CancellationToken.None);

        firstUpdate.Should().BeNull();
        secondUpdate.Should().BeNull();
        VerifyRequestCount(handler, 1);
    }

    [Fact]
    public async Task GetLatestAsync_ConcurrentColdRequests_SharesGitHubRequest()
    {
        var (service, handler) = CreateService();
        var requestStarted = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var responseReady = new TaskCompletionSource<HttpResponseMessage>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Returns(() =>
            {
                requestStarted.TrySetResult(true);
                return responseReady.Task;
            });

        var firstRequest = service.GetLatestAsync("stable", CancellationToken.None);
        await requestStarted.Task.WaitAsync(TimeSpan.FromSeconds(1));
        var secondRequest = service.GetLatestAsync("stable", CancellationToken.None);

        responseReady.SetResult(
            CreateJsonResponse(
                "{\"tag_name\":\"v3.0.0\",\"html_url\":\"https://github.com/teelur/budget-board/releases/tag/v3.0.0\",\"draft\":false,\"prerelease\":false}"
            )
        );

        var results = await Task.WhenAll(firstRequest, secondRequest);

        results[0].Should().NotBeNull();
        results[1].Should().NotBeNull();
        VerifyRequestCount(handler, 1);
    }

    [Fact]
    public async Task Get_WhenUpdateChecksDisabled_ReturnsNoContentWithoutCallingService()
    {
        var service = new Mock<IAppUpdateService>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?> { ["DISABLE_UPDATE_CHECK"] = "true" }
            )
            .Build();
        var controller = new AppUpdateController(service.Object, configuration);

        var result = await controller.Get("stable", CancellationToken.None);

        result.Result.Should().BeOfType<NoContentResult>();
        service.Verify(
            currentService =>
                currentService.GetLatestAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Get_InvalidChannel_ReturnsBadRequestWithoutCallingService()
    {
        var service = new Mock<IAppUpdateService>();
        var configuration = new ConfigurationBuilder().Build();
        var controller = new AppUpdateController(service.Object, configuration);

        var result = await controller.Get("preview", CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
        service.VerifyNoOtherCalls();
    }

    private static (AppUpdateService Service, Mock<HttpMessageHandler> Handler) CreateService()
    {
        var handler = new Mock<HttpMessageHandler>();
        var client = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://api.github.com/"),
        };
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory
            .Setup(factory => factory.CreateClient(AppUpdateService.GitHubHttpClientName))
            .Returns(client);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new AppUpdateService(
            httpClientFactory.Object,
            cache,
            Mock.Of<ILogger<AppUpdateService>>()
        );

        return (service, handler);
    }

    private static void SetupResponse(
        Mock<HttpMessageHandler> handler,
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory
    )
    {
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Returns(
                (HttpRequestMessage request, CancellationToken _) =>
                    Task.FromResult(responseFactory(request))
            );
    }

    private static HttpResponseMessage CreateJsonResponse(string content) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json"),
        };

    private static void VerifyRequestCount(Mock<HttpMessageHandler> handler, int count)
    {
        handler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Exactly(count),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
    }
}
