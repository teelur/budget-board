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
}
