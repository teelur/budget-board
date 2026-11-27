using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using BudgetBoard.WebAPI.Models;
using BudgetBoard.WebAPI.Resources;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Services;

public sealed class NotificationNtfySender(
    IHttpClientFactory httpClientFactory,
    ILogger<NotificationNtfySender> logger,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer,
    IStringLocalizer<ResponseStrings> messageLocalizer
) : INotificationDestinationProvider
{
    public const string ClientName = "NotificationNtfy";
    private const int MaxMessageLength = 4000;

    public string Provider => NotificationDestinationProviders.Ntfy;

    public string BuildConfiguration(
        NotificationDestinationRequest request,
        string? existingConfiguration
    )
    {
        var previous = existingConfiguration is null
            ? null
            : JsonSerializer.Deserialize<NtfyConfiguration>(existingConfiguration)
                ?? throw new InvalidOperationException(
                    responseLocalizer["NotificationInvalidConfiguration"].Value
                );
        var serverUrl = request.NtfyServerUrl ?? previous?.ServerUrl ?? "https://ntfy.sh";
        var topic = request.NtfyTopic ?? previous?.Topic;
        var accessToken = request.NtfyAccessToken is null
            ? previous?.AccessToken
            : string.IsNullOrWhiteSpace(request.NtfyAccessToken)
                ? null
                : request.NtfyAccessToken.Trim();

        if (
            !NotificationWebhookSender.IsValidUrl(serverUrl)
            || !Uri.TryCreate(serverUrl, UriKind.Absolute, out var serverUri)
            || !string.IsNullOrEmpty(serverUri.Query)
        )
        {
            throw new ArgumentException(responseLocalizer["NotificationNtfyServerUrlInvalid"].Value);
        }
        if (
            string.IsNullOrWhiteSpace(topic)
            || topic.Length > 128
            || topic.Any(character =>
                char.IsWhiteSpace(character)
                || char.IsControl(character)
                || character is '/' or '?' or '#'
            )
        )
        {
            throw new ArgumentException(responseLocalizer["NotificationNtfyTopicInvalid"].Value);
        }
        if (
            accessToken is { Length: > 2048 }
            || accessToken?.Any(char.IsControl) is true
        )
        {
            throw new ArgumentException(
                responseLocalizer["NotificationNtfyAccessTokenInvalid"].Value
            );
        }

        return JsonSerializer.Serialize(
            new NtfyConfiguration(serverUri.AbsoluteUri.TrimEnd('/'), topic, accessToken)
        );
    }

    public NotificationProviderConfigurationDetails ReadConfiguration(string configuration)
    {
        var ntfyConfiguration = JsonSerializer.Deserialize<NtfyConfiguration>(configuration)
            ?? throw new InvalidOperationException(
                responseLocalizer["NotificationInvalidConfiguration"].Value
            );
        return new NotificationProviderConfigurationDetails(
            NtfyServerUrl: ntfyConfiguration.ServerUrl,
            NtfyTopic: ntfyConfiguration.Topic,
            NtfyAccessToken: ntfyConfiguration.AccessToken
        );
    }

    public async Task<bool> SendAsync(
        NotificationDestination destination,
        NotificationEvent notification,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var configuration = JsonSerializer.Deserialize<NtfyConfiguration>(
                destination.Configuration
            )
                ?? throw new InvalidOperationException(
                    responseLocalizer["NotificationInvalidConfiguration"].Value
                );
            var endpoint = new Uri(
                $"{configuration.ServerUrl.TrimEnd('/')}/{Uri.EscapeDataString(configuration.Topic)}"
            );
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(
                    NotificationMessageFormatter.Truncate(
                        BuildMessage(notification, destination.IncludeFinancialDetails),
                        MaxMessageLength
                    ),
                    Encoding.UTF8,
                    "text/plain"
                ),
            };
            request.Headers.TryAddWithoutValidation(
                "Title",
                NotificationMessageFormatter.GetTitle(notification, messageLocalizer)
            );
            if (!string.IsNullOrEmpty(configuration.AccessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    configuration.AccessToken
                );
            }

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken
            );
            timeout.CancelAfter(TimeSpan.FromSeconds(5));
            using var response = await httpClientFactory.CreateClient(ClientName).SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                timeout.Token
            );
            if (!response.IsSuccessStatusCode)
            {
                LogHttpFailure(destination.ID, response.StatusCode);
                return false;
            }

            return true;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                "{LogMessage}",
                logLocalizer[
                    "NotificationDestinationSendFailedLog",
                    destination.ID,
                    GetFailureCategory(exception)
                ]
            );
            return false;
        }
    }

    private static string BuildMessage(NotificationEvent notification, bool includeDetails)
    {
        var details = NotificationMessageFormatter.GetFinancialDetails(
            notification,
            includeDetails,
            MaxMessageLength
        );
        return details is null
            ? notification.Message
            : $"{notification.Message}{Environment.NewLine}{Environment.NewLine}{details}";
    }

    private void LogHttpFailure(Guid destinationId, HttpStatusCode statusCode) =>
        logger.LogWarning(
            "{LogMessage}",
            logLocalizer[
                "NotificationDestinationHttpFailureLog",
                destinationId,
                (int)statusCode
            ]
        );

    private static SendFailureCategory GetFailureCategory(Exception exception) =>
        exception switch
        {
            OperationCanceledException => SendFailureCategory.TimeoutOrCancellation,
            HttpRequestException
            {
                HttpRequestError: HttpRequestError.NameResolutionError,
            } => SendFailureCategory.NameResolution,
            HttpRequestException
            {
                HttpRequestError: HttpRequestError.SecureConnectionError,
            } => SendFailureCategory.SecureConnection,
            HttpRequestException => SendFailureCategory.Network,
            JsonException or InvalidOperationException => SendFailureCategory.Configuration,
            _ => SendFailureCategory.Unexpected,
        };

    private enum SendFailureCategory
    {
        TimeoutOrCancellation,
        NameResolution,
        SecureConnection,
        Network,
        Configuration,
        Unexpected,
    }
}
