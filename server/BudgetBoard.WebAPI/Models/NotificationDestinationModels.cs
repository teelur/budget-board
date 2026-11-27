using System.ComponentModel.DataAnnotations;
using BudgetBoard.Database.Models;

namespace BudgetBoard.WebAPI.Models;

/// <summary>Configuration for a notification destination.</summary>
public sealed record NotificationDestinationRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }

    public string Provider { get; init; } = NotificationDestinationProviders.Webhook;
    public string? Url { get; init; }
    public Dictionary<string, string>? Headers { get; init; }
    public string? ContentType { get; init; }
    public string? BodyTemplate { get; init; }
    public string? NtfyServerUrl { get; init; }
    public string? NtfyTopic { get; init; }
    public string? NtfyAccessToken { get; init; }
    public string? DiscordWebhookUrl { get; init; }
    public required string[] EventTypes { get; init; }
    public bool IncludeFinancialDetails { get; init; }
}

/// <summary>A notification destination without its webhook configuration.</summary>
public sealed record NotificationDestinationResponse(
    Guid Id,
    string Name,
    string Provider,
    string[] EventTypes,
    bool IncludeFinancialDetails
);

/// <summary>A notification destination with its configuration for its authenticated owner.</summary>
public sealed record NotificationDestinationDetailsResponse(
    Guid Id,
    string Name,
    string Provider,
    string[] EventTypes,
    bool IncludeFinancialDetails,
    string? Url,
    Dictionary<string, string>? Headers,
    string? ContentType,
    string? BodyTemplate,
    string? NtfyServerUrl,
    string? NtfyTopic,
    string? NtfyAccessToken,
    string? DiscordWebhookUrl
);

public sealed record NotificationProviderConfigurationDetails(
    string? Url = null,
    Dictionary<string, string>? Headers = null,
    string? ContentType = null,
    string? BodyTemplate = null,
    string? NtfyServerUrl = null,
    string? NtfyTopic = null,
    string? NtfyAccessToken = null,
    string? DiscordWebhookUrl = null
);

internal sealed record WebhookConfiguration(
    string Url,
    Dictionary<string, string> Headers,
    string ContentType,
    string BodyTemplate
);

internal sealed record NtfyConfiguration(string ServerUrl, string Topic, string? AccessToken);

internal sealed record DiscordConfiguration(string WebhookUrl);
