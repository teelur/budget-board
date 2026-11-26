using System.ComponentModel.DataAnnotations;

namespace BudgetBoard.WebAPI.Models;

/// <summary>Configuration for a notification destination.</summary>
public sealed record NotificationDestinationRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }

    public string? Url { get; init; }
    public Dictionary<string, string>? Headers { get; init; }
    public string? ContentType { get; init; }
    public string? BodyTemplate { get; init; }
    public required string[] EventTypes { get; init; }
    public bool IncludeFinancialDetails { get; init; }
}

/// <summary>A notification destination without its webhook configuration.</summary>
public sealed record NotificationDestinationResponse(
    Guid Id,
    string Name,
    string[] EventTypes,
    bool IncludeFinancialDetails
);

/// <summary>A notification destination with its configuration for its authenticated owner.</summary>
public sealed record NotificationDestinationDetailsResponse(
    Guid Id,
    string Name,
    string[] EventTypes,
    bool IncludeFinancialDetails,
    string Url,
    Dictionary<string, string> Headers,
    string ContentType,
    string BodyTemplate
);

internal sealed record WebhookConfiguration(
    string Url,
    Dictionary<string, string> Headers,
    string ContentType,
    string BodyTemplate
);
