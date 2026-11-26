using System.Net;
using System.Text.Json;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;
using BudgetBoard.WebAPI.Services;
using FluentAssertions;
using BudgetBoard.WebAPI.Resources;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

public class NotificationDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_AttemptsOnlySubscribedDestinationsAndIsolatesFailures()
    {
        var helper = new TestHelper();
        var userId = helper.demoUser.Id;
        var handler = new CountingHandler();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(NotificationWebhookSender.ClientName))
            .Returns(new HttpClient(handler));
        var sender = new NotificationWebhookSender(
            factory.Object,
            Mock.Of<ILogger<NotificationWebhookSender>>(),
            TestHelper.CreateMockLocalizer<ApiLogStrings>(),
            TestHelper.CreateMockLocalizer<ApiResponseStrings>()
        );
        var dispatcher = new NotificationDispatcher(
            helper.UserDataContext,
            sender,
            TestHelper.CreateMockLocalizer<ApiLogStrings>(),
            TestHelper.CreateMockLocalizer<ApiResponseStrings>(),
            Mock.Of<ILogger<NotificationDispatcher>>()
        );
        var config = JsonSerializer.Serialize(new
        {
            Url = "https://example.test/",
            Headers = new Dictionary<string, string>(),
            ContentType = "application/json",
            BodyTemplate = "{\"content\":\"{{message}}\"}",
        });
        helper.UserDataContext.NotificationDestinations.AddRange(
            new NotificationDestination
            {
                UserID = userId,
                Name = "bad",
                Configuration = "invalid",
                EventTypes = "[\"sync_failed\"]",
            },
            new NotificationDestination
            {
                UserID = userId,
                Name = "good",
                Configuration = config,
                EventTypes = "[\"sync_failed\"]",
            },
            new NotificationDestination
            {
                UserID = userId,
                Name = "unsubscribed",
                Configuration = config,
                EventTypes = "[]",
            }
        );
        await helper.UserDataContext.SaveChangesAsync();

        await dispatcher.DispatchAsync(new NotificationEvent(userId, "sync_failed", "test"));

        handler.Attempts.Should().Be(1);
    }

    [Fact]
    public async Task DispatchAsync_LimitsConcurrentSendsAndProcessesRemainingDestinations()
    {
        var helper = new TestHelper();
        var userId = helper.demoUser.Id;
        var handler = new ConcurrentCountingHandler();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(NotificationWebhookSender.ClientName))
            .Returns(new HttpClient(handler));
        var sender = new NotificationWebhookSender(
            factory.Object,
            Mock.Of<ILogger<NotificationWebhookSender>>(),
            TestHelper.CreateMockLocalizer<ApiLogStrings>(),
            TestHelper.CreateMockLocalizer<ApiResponseStrings>()
        );
        var dispatcher = new NotificationDispatcher(
            helper.UserDataContext,
            sender,
            TestHelper.CreateMockLocalizer<ApiLogStrings>(),
            TestHelper.CreateMockLocalizer<ApiResponseStrings>(),
            Mock.Of<ILogger<NotificationDispatcher>>()
        );
        var config = JsonSerializer.Serialize(new
        {
            Url = "https://example.test/",
            Headers = new Dictionary<string, string>(),
            ContentType = "application/json",
            BodyTemplate = "{\"content\":\"{{message}}\"}",
        });
        helper.UserDataContext.NotificationDestinations.AddRange(
            Enumerable.Range(0, 9).Select(index => new NotificationDestination
            {
                UserID = userId,
                Name = $"destination-{index}",
                Configuration = config,
                EventTypes = "[\"sync_failed\"]",
            })
        );
        await helper.UserDataContext.SaveChangesAsync();

        await dispatcher.DispatchAsync(new NotificationEvent(userId, "sync_failed", "test"));

        handler.Attempts.Should().Be(9);
        handler.MaximumConcurrentRequests.Should().Be(4);
    }

    [Fact]
    public async Task DispatchAsync_UsesTwoSecondBatchTimeoutAndProcessesRemainingDestinations()
    {
        var helper = new TestHelper();
        var userId = helper.demoUser.Id;
        var handler = new HangingCountingHandler();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(NotificationWebhookSender.ClientName))
            .Returns(new HttpClient(handler));
        var sender = new NotificationWebhookSender(
            factory.Object,
            Mock.Of<ILogger<NotificationWebhookSender>>(),
            TestHelper.CreateMockLocalizer<ApiLogStrings>(),
            TestHelper.CreateMockLocalizer<ApiResponseStrings>()
        );
        var dispatcher = new NotificationDispatcher(
            helper.UserDataContext,
            sender,
            TestHelper.CreateMockLocalizer<ApiLogStrings>(),
            TestHelper.CreateMockLocalizer<ApiResponseStrings>(),
            Mock.Of<ILogger<NotificationDispatcher>>()
        );
        var config = JsonSerializer.Serialize(new
        {
            Url = "https://example.test/",
            Headers = new Dictionary<string, string>(),
            ContentType = "application/json",
            BodyTemplate = "{\"content\":\"{{message}}\"}",
        });
        helper.UserDataContext.NotificationDestinations.AddRange(
            Enumerable.Range(0, 10).Select(index => new NotificationDestination
            {
                UserID = userId,
                Name = $"destination-{index}",
                Configuration = config,
                EventTypes = "[\"sync_failed\"]",
            })
        );
        await helper.UserDataContext.SaveChangesAsync();

        await dispatcher
            .DispatchAsync(new NotificationEvent(userId, "sync_failed", "test"))
            .WaitAsync(TimeSpan.FromSeconds(10));

        handler.Attempts.Should().Be(10);
        handler.CancelledRequests.Should().Be(10);
    }

    private sealed class CountingHandler : HttpMessageHandler
    {
        public int Attempts { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Attempts++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        }
    }

    private sealed class HangingCountingHandler : HttpMessageHandler
    {
        private int _attempts;
        private int _cancelledRequests;

        public int Attempts => _attempts;
        public int CancelledRequests => _cancelledRequests;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Interlocked.Increment(ref _attempts);
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                Interlocked.Increment(ref _cancelledRequests);
                throw;
            }
        }
    }

    private sealed class ConcurrentCountingHandler : HttpMessageHandler
    {
        private readonly object _gate = new();
        private int _attempts;
        private int _activeRequests;
        private int _maximumConcurrentRequests;

        public int Attempts => _attempts;
        public int MaximumConcurrentRequests => _maximumConcurrentRequests;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Interlocked.Increment(ref _attempts);
            var activeRequests = Interlocked.Increment(ref _activeRequests);
            lock (_gate)
            {
                _maximumConcurrentRequests = Math.Max(
                    _maximumConcurrentRequests,
                    activeRequests
                );
            }

            try
            {
                await Task.Delay(50, cancellationToken);
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }
            finally
            {
                Interlocked.Decrement(ref _activeRequests);
            }
        }
    }
}
