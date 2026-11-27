using System.Net;
using System.Text.Json;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using BudgetBoard.WebAPI.Models;
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
        var dispatcher = CreateDispatcher(helper, factory.Object);
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
    public async Task DispatchAsync_RoutesNativeDestinationsToTheirProviders()
    {
        var helper = new TestHelper();
        var userId = helper.demoUser.Id;
        var ntfyHandler = new CountingHandler();
        var discordHandler = new CountingHandler();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(clientFactory => clientFactory.CreateClient(NotificationNtfySender.ClientName))
            .Returns(new HttpClient(ntfyHandler));
        factory.Setup(clientFactory =>
                clientFactory.CreateClient(NotificationDiscordSender.ClientName)
            )
            .Returns(new HttpClient(discordHandler));
        var apiLogLocalizer = TestHelper.CreateMockLocalizer<ApiLogStrings>();
        var apiResponseLocalizer = TestHelper.CreateMockLocalizer<ApiResponseStrings>();
        var ntfySender = new NotificationNtfySender(
            factory.Object,
            Mock.Of<ILogger<NotificationNtfySender>>(),
            apiLogLocalizer,
            apiResponseLocalizer,
            TestHelper.CreateMockLocalizer<ResponseStrings>()
        );
        var discordSender = new NotificationDiscordSender(
            factory.Object,
            Mock.Of<ILogger<NotificationDiscordSender>>(),
            apiLogLocalizer,
            apiResponseLocalizer,
            TestHelper.CreateMockLocalizer<ResponseStrings>()
        );
        helper.UserDataContext.NotificationDestinations.AddRange(
            new NotificationDestination
            {
                UserID = userId,
                Name = "ntfy",
                Provider = NotificationDestinationProviders.Ntfy,
                Configuration = ntfySender.BuildConfiguration(
                    new NotificationDestinationRequest
                    {
                        Name = "ntfy",
                        Provider = NotificationDestinationProviders.Ntfy,
                        NtfyTopic = "budget-board",
                        EventTypes = [NotificationEventTypes.SyncFailed],
                    },
                    null
                ),
                EventTypes = "[\"sync_failed\"]",
            },
            new NotificationDestination
            {
                UserID = userId,
                Name = "discord",
                Provider = NotificationDestinationProviders.Discord,
                Configuration = discordSender.BuildConfiguration(
                    new NotificationDestinationRequest
                    {
                        Name = "discord",
                        Provider = NotificationDestinationProviders.Discord,
                        DiscordWebhookUrl =
                            "https://discord.com/api/webhooks/123456789012345678/secret",
                        EventTypes = [NotificationEventTypes.SyncFailed],
                    },
                    null
                ),
                EventTypes = "[\"sync_failed\"]",
            }
        );
        await helper.UserDataContext.SaveChangesAsync();
        var dispatcher = CreateDispatcher(helper, factory.Object);

        await dispatcher.DispatchAsync(new NotificationEvent(userId, "sync_failed", "test"));

        ntfyHandler.Attempts.Should().Be(1);
        discordHandler.Attempts.Should().Be(1);
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
        var dispatcher = CreateDispatcher(helper, factory.Object);
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
        var dispatcher = CreateDispatcher(helper, factory.Object);
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

    private static NotificationDispatcher CreateDispatcher(
        TestHelper helper,
        IHttpClientFactory factory
    )
    {
        var apiLogLocalizer = TestHelper.CreateMockLocalizer<ApiLogStrings>();
        var apiResponseLocalizer = TestHelper.CreateMockLocalizer<ApiResponseStrings>();
        return new NotificationDispatcher(
            helper.UserDataContext,
            [
                new NotificationWebhookSender(
                    factory,
                    Mock.Of<ILogger<NotificationWebhookSender>>(),
                    apiLogLocalizer,
                    apiResponseLocalizer
                ),
                new NotificationNtfySender(
                    factory,
                    Mock.Of<ILogger<NotificationNtfySender>>(),
                    apiLogLocalizer,
                    apiResponseLocalizer,
                    TestHelper.CreateMockLocalizer<ResponseStrings>()
                ),
                new NotificationDiscordSender(
                    factory,
                    Mock.Of<ILogger<NotificationDiscordSender>>(),
                    apiLogLocalizer,
                    apiResponseLocalizer,
                    TestHelper.CreateMockLocalizer<ResponseStrings>()
                ),
            ],
            apiLogLocalizer,
            apiResponseLocalizer,
            Mock.Of<ILogger<NotificationDispatcher>>()
        );
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
