using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;
using BudgetBoard.WebAPI.Models;
using BudgetBoard.WebAPI.Resources;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Services;

public sealed partial class NotificationWebhookSender(
    IHttpClientFactory httpClientFactory,
    ILogger<NotificationWebhookSender> logger,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : INotificationDestinationProvider
{
    public const string ClientName = "NotificationWebhook";

    public string Provider => NotificationDestinationProviders.Webhook;

    [GeneratedRegex(@"\{\{(event_type|message|financial_details)\}\}")]
    private static partial Regex PlaceholderRegex();

    public string BuildConfiguration(
        NotificationDestinationRequest request,
        string? existingConfiguration
    )
    {
        var previous = existingConfiguration is null
            ? null
            : JsonSerializer.Deserialize<WebhookConfiguration>(existingConfiguration)
                ?? throw new InvalidOperationException(
                    responseLocalizer["NotificationInvalidConfiguration"].Value
                );
        var url = request.Url ?? previous?.Url;
        var headers = request.Headers ?? previous?.Headers ?? new Dictionary<string, string>();
        var contentType = request.ContentType ?? previous?.ContentType ?? "application/json";
        var template =
            request.BodyTemplate ?? previous?.BodyTemplate ?? "{\"content\":\"{{message}}\"}";
        if (!IsValidUrl(url))
        {
            throw new ArgumentException(responseLocalizer["NotificationWebhookUrlInvalid"].Value);
        }
        if (contentType is not ("application/json" or "text/plain"))
        {
            throw new ArgumentException(
                responseLocalizer["NotificationWebhookContentTypeInvalid"].Value
            );
        }
        if (
            template.Length > 8192
            || headers.Count > 20
            || headers.Any(h =>
                h.Key.Length > 128
                || h.Value is null
                || h.Value.Length > 2048
                || h.Key.Contains('\r')
                || h.Key.Contains('\n')
                || h.Value.Contains('\r')
                || h.Value.Contains('\n')
                || h.Key.Equals("Host", StringComparison.OrdinalIgnoreCase)
                || h.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)
                || h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)
                || h.Key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            throw new ArgumentException(
                responseLocalizer["NotificationWebhookTemplateOrHeadersInvalid"].Value
            );
        }
        using var validationRequest = new HttpRequestMessage();
        if (
            headers.Any(header =>
                !validationRequest.Headers.TryAddWithoutValidation(header.Key, header.Value)
            )
        )
        {
            throw new ArgumentException(
                responseLocalizer["NotificationWebhookHeadersInvalid"].Value
            );
        }

        var configuration = new WebhookConfiguration(url!, headers, contentType, template);
        if (contentType == "application/json")
        {
            try
            {
                var sample = new NotificationEvent(Guid.Empty, "test", "test", "test");
                using var _ = JsonDocument.Parse(
                    Render(configuration, sample, request.IncludeFinancialDetails)
                );
            }
            catch (JsonException exception)
            {
                throw new ArgumentException(
                    responseLocalizer["NotificationWebhookJsonTemplateInvalid"].Value,
                    exception
                );
            }
        }

        return JsonSerializer.Serialize(configuration);
    }

    public NotificationProviderConfigurationDetails ReadConfiguration(string configuration)
    {
        var webhookConfiguration = JsonSerializer.Deserialize<WebhookConfiguration>(configuration)
            ?? throw new InvalidOperationException(
                responseLocalizer["NotificationInvalidConfiguration"].Value
            );
        return new NotificationProviderConfigurationDetails(
            Url: webhookConfiguration.Url,
            Headers: webhookConfiguration.Headers,
            ContentType: webhookConfiguration.ContentType,
            BodyTemplate: webhookConfiguration.BodyTemplate
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
            var configuration = JsonSerializer.Deserialize<WebhookConfiguration>(
                destination.Configuration
            )
                ?? throw new InvalidOperationException(
                    responseLocalizer["NotificationInvalidConfiguration"].Value
                );
            if (!IsValidUrl(configuration.Url))
            {
                throw new InvalidOperationException(
                    responseLocalizer["NotificationDestinationInvalid"].Value
                );
            }

            var body = Render(configuration, notification, destination.IncludeFinancialDetails);
            if (configuration.ContentType == "application/json")
            {
                using var _ = JsonDocument.Parse(body);
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, configuration.Url);
            request.Content = new StringContent(body, Encoding.UTF8, configuration.ContentType);
            foreach (var header in configuration.Headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(5));
            using var response = await httpClientFactory.CreateClient(ClientName).SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                timeout.Token
            );
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "{LogMessage}",
                    logLocalizer[
                        "NotificationDestinationHttpFailureLog",
                        destination.ID,
                        (int)response.StatusCode
                    ]
                );
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

    internal static bool IsValidUrl(string? value)
    {
        if (value is null || value.Length > 4096 || !Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var host = uri.Host.Trim('[', ']');
        if (IPAddress.TryParse(host, out var address))
        {
            var normalized = address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;
            if (
                normalized.Equals(IPAddress.Parse("169.254.169.254"))
                || normalized.Equals(IPAddress.Parse("fd00:ec2::254"))
            )
            {
                return false;
            }
        }

        return uri.Scheme is "http" or "https"
            && string.IsNullOrEmpty(uri.UserInfo)
            && string.IsNullOrEmpty(uri.Fragment)
            && !uri.Host.Equals("metadata.google.internal", StringComparison.OrdinalIgnoreCase);
    }

    internal static string Render(
        WebhookConfiguration configuration,
        NotificationEvent notification,
        bool includeFinancialDetails
    )
    {
        string Encode(string value) =>
            configuration.ContentType == "application/json"
                ? JsonEncodedText.Encode(value).ToString()
                : value;

        return PlaceholderRegex().Replace(
            configuration.BodyTemplate,
            match => Encode(match.Groups[1].Value switch
            {
                "event_type" => notification.Type,
                "message" => notification.Message,
                "financial_details" when includeFinancialDetails => notification.FinancialDetails ?? "",
                _ => "",
            })
        );
    }
}
