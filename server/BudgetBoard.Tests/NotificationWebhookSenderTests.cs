using System.Diagnostics;
using System.Net;
using System.Text.Json;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;
using BudgetBoard.WebAPI.Resources;
using BudgetBoard.WebAPI.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

public class NotificationWebhookSenderTests
{
    [Theory]
    [InlineData(false, "")]
    [InlineData(true, "private details")]
    public async Task SendAsync_EscapesJsonAndRespectsFinancialDetailsOptIn(
        bool includeDetails,
        string expectedDetails
    )
    {
        var handler = new CapturingHandler(HttpStatusCode.NoContent);
        var sender = CreateSender(handler);
        var userId = Guid.NewGuid();
        var destination = CreateDestination(userId, includeDetails);

        var result = await sender.SendAsync(
            destination,
            new NotificationEvent(userId, "transaction_created", "A \"quoted\" event", "private details"),
            CancellationToken.None
        );

        result.Should().BeTrue();
        using var payload = JsonDocument.Parse(handler.Body!);
        payload.RootElement.GetProperty("message").GetString().Should().Be("A \"quoted\" event");
        payload.RootElement.GetProperty("details").GetString().Should().Be(expectedDetails);
    }

    [Theory]
    [InlineData(false, "Account sync completed with errors.")]
    [InlineData(true, "Account sync completed with errors.\n\nSimpleFIN: Invalid access token.")]
    public async Task SendAsync_DefaultStyleSyncMessageIncludesErrorsOnlyWhenOptedIn(
        bool includeDetails,
        string expectedMessage
    )
    {
        var handler = new CapturingHandler(HttpStatusCode.NoContent);
        var sender = CreateSender(handler);
        var userId = Guid.NewGuid();
        var destination = CreateDestination(
            userId,
            includeDetails,
            "{\"content\":\"{{message}}\"}"
        );

        var result = await sender.SendAsync(
            destination,
            new NotificationEvent(
                userId,
                NotificationEventTypes.SyncFailed,
                "Account sync completed with errors.",
                "SimpleFIN: Invalid access token."
            ),
            CancellationToken.None
        );

        result.Should().BeTrue();
        using var payload = JsonDocument.Parse(handler.Body!);
        payload.RootElement.GetProperty("content").GetString().Should().Be(expectedMessage);
    }

    [Theory]
    [InlineData(false, "Transaction import failed for 2 rows.")]
    [InlineData(true, "Transaction import failed for 2 rows.\n\nRow 2: Invalid account.")]
    public async Task SendAsync_DefaultStyleImportMessageIncludesErrorsOnlyWhenOptedIn(
        bool includeDetails,
        string expectedMessage
    )
    {
        var handler = new CapturingHandler(HttpStatusCode.NoContent);
        var sender = CreateSender(handler);
        var userId = Guid.NewGuid();
        var destination = CreateDestination(
            userId,
            includeDetails,
            "{\"content\":\"{{message}}\"}"
        );

        var result = await sender.SendAsync(
            destination,
            new NotificationEvent(
                userId,
                NotificationEventTypes.ImportFailed,
                "Transaction import failed for 2 rows.",
                "Row 2: Invalid account."
            ),
            CancellationToken.None
        );

        result.Should().BeTrue();
        using var payload = JsonDocument.Parse(handler.Body!);
        payload.RootElement.GetProperty("content").GetString().Should().Be(expectedMessage);
    }

    [Fact]
    public async Task SendAsync_FailureReturnsFalse()
    {
        var handler = new CapturingHandler(HttpStatusCode.BadRequest);
        var sender = CreateSender(handler);
        var userId = Guid.NewGuid();

        var result = await sender.SendAsync(
            CreateDestination(userId, false),
            new NotificationEvent(userId, "test", "test"),
            CancellationToken.None
        );

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_TimesOutWithoutRetry()
    {
        var handler = new HangingHandler();
        var logger = new CapturingLogger<NotificationWebhookSender>();
        var sender = CreateSender(handler, logger);
        var userId = Guid.NewGuid();
        var elapsed = Stopwatch.StartNew();

        var result = await sender.SendAsync(
            CreateDestination(userId, false),
            new NotificationEvent(userId, "test", "test"),
            CancellationToken.None
        );

        result.Should().BeFalse();
        handler.Attempts.Should().Be(1);
        elapsed.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10));
        logger.Messages.Should().ContainSingle().Which.Should().Contain("TimeoutOrCancellation");
    }

    [Fact]
    public async Task SendAsync_NetworkFailureLogsSanitizedCategory()
    {
        const string sensitiveMessage = "https://example.test/?token=do-not-log";
        var logger = new CapturingLogger<NotificationWebhookSender>();
        var sender = CreateSender(new ThrowingHandler(new HttpRequestException(sensitiveMessage)), logger);
        var userId = Guid.NewGuid();

        var result = await sender.SendAsync(
            CreateDestination(userId, false),
            new NotificationEvent(userId, "test", "test"),
            CancellationToken.None
        );

        result.Should().BeFalse();
        logger.Messages.Should().ContainSingle();
        logger.Messages[0].Should().Contain("Network").And.NotContain(sensitiveMessage);
    }

    private static NotificationWebhookSender CreateSender(
        HttpMessageHandler handler,
        ILogger<NotificationWebhookSender>? logger = null
    )
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(NotificationWebhookSender.ClientName))
            .Returns(new HttpClient(handler));
        return new NotificationWebhookSender(
            factory.Object,
            logger ?? Mock.Of<ILogger<NotificationWebhookSender>>(),
            TestHelper.CreateMockLocalizer<ApiLogStrings>(),
            TestHelper.CreateMockLocalizer<ApiResponseStrings>()
        );
    }

    private static NotificationDestination CreateDestination(
        Guid userId,
        bool includeDetails,
        string bodyTemplate = "{\"message\":\"{{message}}\",\"details\":\"{{financial_details}}\"}"
    ) => new()
    {
        UserID = userId,
        Name = "test",
        EventTypes = "[]",
        IncludeFinancialDetails = includeDetails,
        Configuration = JsonSerializer.Serialize(new
        {
            Url = "https://example.test/secret",
            Headers = new Dictionary<string, string>(),
            ContentType = "application/json",
            BodyTemplate = bodyTemplate,
        }),
    };

    private sealed class ThrowingHandler(Exception exception) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) => Task.FromException<HttpResponseMessage>(exception);
    }

    private sealed class CapturingHandler(HttpStatusCode status) : HttpMessageHandler
    {
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(status);
        }

    }

    private sealed class HangingHandler : HttpMessageHandler
    {
        public int Attempts { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Attempts++;
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        ) => Messages.Add(formatter(state, exception));
    }
}
