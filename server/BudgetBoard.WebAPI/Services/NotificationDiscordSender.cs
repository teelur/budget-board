using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using BudgetBoard.WebAPI.Models;
using BudgetBoard.WebAPI.Resources;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Services;

public sealed class NotificationDiscordSender(
    IHttpClientFactory httpClientFactory,
    ILogger<NotificationDiscordSender> logger,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer,
    IStringLocalizer<ResponseStrings> messageLocalizer
) : INotificationDestinationProvider
{
    public const string ClientName = "NotificationDiscord";
    private const int MaxDescriptionLength = 4000;
    private const int MaxDetailsLength = 1000;

    public string Provider => NotificationDestinationProviders.Discord;

    public string BuildConfiguration(
        NotificationDestinationRequest request,
        string? existingConfiguration
    )
    {
        var previous = existingConfiguration is null
            ? null
            : JsonSerializer.Deserialize<DiscordConfiguration>(existingConfiguration)
                ?? throw new InvalidOperationException(
                    responseLocalizer["NotificationInvalidConfiguration"].Value
                );
        var webhookUrl = request.DiscordWebhookUrl ?? previous?.WebhookUrl;
        if (!IsValidWebhookUrl(webhookUrl))
        {
            throw new ArgumentException(
                responseLocalizer["NotificationDiscordWebhookUrlInvalid"].Value
            );
        }

        return JsonSerializer.Serialize(new DiscordConfiguration(webhookUrl!));
    }

    public NotificationProviderConfigurationDetails ReadConfiguration(string configuration)
    {
        var discordConfiguration = JsonSerializer.Deserialize<DiscordConfiguration>(configuration)
            ?? throw new InvalidOperationException(
                responseLocalizer["NotificationInvalidConfiguration"].Value
            );
        return new NotificationProviderConfigurationDetails(
            DiscordWebhookUrl: discordConfiguration.WebhookUrl
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
            var configuration = JsonSerializer.Deserialize<DiscordConfiguration>(
                destination.Configuration
            )
                ?? throw new InvalidOperationException(
                    responseLocalizer["NotificationInvalidConfiguration"].Value
                );
            if (!IsValidWebhookUrl(configuration.WebhookUrl))
            {
                throw new InvalidOperationException(
                    responseLocalizer["NotificationDiscordWebhookUrlInvalid"].Value
                );
            }

            var financialDetails = NotificationMessageFormatter.GetFinancialDetails(
                notification,
                destination.IncludeFinancialDetails,
                MaxDetailsLength
            );
            var payload = new DiscordWebhookPayload(
                [
                    new DiscordEmbed(
                        NotificationMessageFormatter.Truncate(
                            NotificationMessageFormatter.GetTitle(messageLocalizer),
                            256
                        ),
                        NotificationMessageFormatter.Truncate(
                            notification.Message,
                            MaxDescriptionLength
                        ),
                        financialDetails is null
                            ? []
                            : [new DiscordEmbedField("Details", financialDetails)]
                    ),
                ],
                new DiscordAllowedMentions([])
            );

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                new UriBuilder(configuration.WebhookUrl) { Query = "wait=true" }.Uri
            )
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                ),
            };
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

    private static bool IsValidWebhookUrl(string? value)
    {
        if (
            !NotificationWebhookSender.IsValidUrl(value)
            || !Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || !uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrEmpty(uri.Query)
            || !(
                uri.Host.Equals("discord.com", StringComparison.OrdinalIgnoreCase)
                || uri.Host.Equals("discordapp.com", StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            return false;
        }

        var path = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return path.Length == 4
            && path[0].Equals("api", StringComparison.OrdinalIgnoreCase)
            && path[1].Equals("webhooks", StringComparison.OrdinalIgnoreCase)
            && ulong.TryParse(path[2], out _)
            && path[3].Length is > 0 and <= 256
            && !path[3].Contains('%');
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

    private sealed record DiscordWebhookPayload(
        [property: JsonPropertyName("embeds")] DiscordEmbed[] Embeds,
        [property: JsonPropertyName("allowed_mentions")] DiscordAllowedMentions AllowedMentions
    );

    private sealed record DiscordEmbed(
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("fields")] DiscordEmbedField[] Fields
    );

    private sealed record DiscordEmbedField(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("value")] string Value
    );

    private sealed record DiscordAllowedMentions(
        [property: JsonPropertyName("parse")] string[] Parse
    );
}
