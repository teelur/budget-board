using System.Net;
using System.Text.Json;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using BudgetBoard.WebAPI.Models;
using BudgetBoard.WebAPI.Resources;
using BudgetBoard.WebAPI.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using Moq;

namespace BudgetBoard.IntegrationTests;

public class NotificationNativeProviderTests
{
    [Fact]
    public async Task NtfySender_PostsToConfiguredServerWithBearerToken()
    {
        var handler = new CapturingHandler();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(clientFactory => clientFactory.CreateClient(NotificationNtfySender.ClientName))
            .Returns(new HttpClient(handler));
        var sender = new NotificationNtfySender(
            factory.Object,
            Mock.Of<ILogger<NotificationNtfySender>>(),
            TestHelper.CreateMockLocalizer<ApiLogStrings>(),
            TestHelper.CreateMockLocalizer<ApiResponseStrings>(),
            CreateMessageLocalizer()
        );
        var configuration = sender.BuildConfiguration(
            new NotificationDestinationRequest
            {
                Name = "ntfy",
                Provider = NotificationDestinationProviders.Ntfy,
                NtfyServerUrl = "https://ntfy.example/",
                NtfyTopic = "budget-board",
                NtfyAccessToken = "secret",
                EventTypes = [],
            },
            null
        );

        var sent = await sender.SendAsync(
            CreateDestination(NotificationDestinationProviders.Ntfy, configuration),
            new NotificationEvent(Guid.NewGuid(), "sync_failed", "Sync failed"),
            CancellationToken.None
        );

        sent.Should().BeTrue();
        handler.Method.Should().Be(HttpMethod.Post);
        handler.RequestUri.Should().Be(new Uri("https://ntfy.example/budget-board"));
        handler.Authorization.Should().Be("Bearer secret");
        handler.Title.Should().Be("Budget Board");
        handler.ContentType.Should().Be("text/plain; charset=utf-8");
        handler.Body.Should().Be("Sync failed");
    }

    [Fact]
    public async Task DiscordSender_SendsEmbedAndDisablesAllowedMentions()
    {
        var handler = new CapturingHandler();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(clientFactory =>
                clientFactory.CreateClient(NotificationDiscordSender.ClientName)
            )
            .Returns(new HttpClient(handler));
        var sender = new NotificationDiscordSender(
            factory.Object,
            Mock.Of<ILogger<NotificationDiscordSender>>(),
            TestHelper.CreateMockLocalizer<ApiLogStrings>(),
            TestHelper.CreateMockLocalizer<ApiResponseStrings>(),
            CreateMessageLocalizer()
        );
        var configuration = sender.BuildConfiguration(
            new NotificationDestinationRequest
            {
                Name = "discord",
                Provider = NotificationDestinationProviders.Discord,
                DiscordWebhookUrl =
                    "https://discord.com/api/webhooks/123456789012345678/secret",
                EventTypes = [],
            },
            null
        );

        var sent = await sender.SendAsync(
            CreateDestination(NotificationDestinationProviders.Discord, configuration),
            new NotificationEvent(Guid.NewGuid(), "sync_failed", "Sync failed"),
            CancellationToken.None
        );

        sent.Should().BeTrue();
        handler.Method.Should().Be(HttpMethod.Post);
        handler.RequestUri!.Host.Should().Be("discord.com");
        handler.RequestUri.Query.Should().Be("?wait=true");
        using var payload = JsonDocument.Parse(handler.Body!);
        payload.RootElement.GetProperty("embeds")[0]
            .GetProperty("title").GetString().Should().Be("Budget Board");
        payload.RootElement.GetProperty("embeds")[0]
            .GetProperty("description").GetString().Should().Be("Sync failed");
        payload.RootElement.GetProperty("allowed_mentions")
            .GetProperty("parse").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public void DiscordSender_RejectsNonDiscordWebhookUrls()
    {
        var sender = new NotificationDiscordSender(
            Mock.Of<IHttpClientFactory>(),
            Mock.Of<ILogger<NotificationDiscordSender>>(),
            TestHelper.CreateMockLocalizer<ApiLogStrings>(),
            TestHelper.CreateMockLocalizer<ApiResponseStrings>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>()
        );

        var action = () =>
            sender.BuildConfiguration(
                new NotificationDestinationRequest
                {
                    Name = "discord",
                    Provider = NotificationDestinationProviders.Discord,
                    DiscordWebhookUrl =
                        "https://discord.com.example/api/webhooks/123456789012345678/secret",
                    EventTypes = [],
                },
                null
            );

        action.Should().Throw<ArgumentException>();
    }

    private static NotificationDestination CreateDestination(string provider, string configuration) =>
        new()
        {
            ID = Guid.NewGuid(),
            UserID = Guid.NewGuid(),
            Name = provider,
            Provider = provider,
            Configuration = configuration,
            EventTypes = "[]",
        };

    private static IStringLocalizer<ResponseStrings> CreateMessageLocalizer()
    {
        var localizer = new Mock<IStringLocalizer<ResponseStrings>>();
        localizer
            .Setup(item => item[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));
        localizer
            .Setup(item => item["NotificationTitle"])
            .Returns(new LocalizedString("NotificationTitle", "Budget Board"));
        return localizer.Object;
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }
        public Uri? RequestUri { get; private set; }
        public string? Authorization { get; private set; }
        public string? Title { get; private set; }
        public string? ContentType { get; private set; }
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Method = request.Method;
            RequestUri = request.RequestUri;
            Authorization = request.Headers.Authorization?.ToString();
            Title = request.Headers.TryGetValues("Title", out var titleValues)
                ? string.Join(", ", titleValues)
                : null;
            ContentType = request.Content?.Headers.ContentType?.ToString();
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
