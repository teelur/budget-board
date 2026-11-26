using System.Text.Json;
using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;
using BudgetBoard.WebAPI.Models;
using BudgetBoard.WebAPI.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Services;

public sealed class NotificationDestinationService(
    UserDataContext context,
    NotificationWebhookSender sender,
    IStringLocalizer<ApiResponseStrings> localizer
) : INotificationDestinationService
{
    public async Task<IReadOnlyList<NotificationDestinationResponse>> ReadAsync(
        Guid userId,
        CancellationToken cancellationToken
    ) =>
        [
            .. (
                await context
                    .NotificationDestinations.AsNoTracking()
                    .Where(d => d.UserID == userId)
                    .OrderBy(d => d.Name)
                    .ToListAsync(cancellationToken)
            ).Select(ToResponse),
        ];

    public async Task<NotificationDestinationDetailsResponse?> ReadAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var destination = await context
            .NotificationDestinations.AsNoTracking()
            .SingleOrDefaultAsync(d => d.UserID == userId && d.ID == id, cancellationToken);
        return destination is null ? null : ToDetailsResponse(destination);
    }

    public async Task<NotificationDestinationResponse> CreateAsync(
        Guid userId,
        NotificationDestinationRequest request,
        CancellationToken cancellationToken
    )
    {
        var configuration = BuildConfiguration(request, null);
        var events = ValidateEvents(request.EventTypes);
        var destination = new NotificationDestination
        {
            UserID = userId,
            Name = ValidateName(request.Name),
            Configuration = JsonSerializer.Serialize(configuration),
            EventTypes = JsonSerializer.Serialize(events),
            IncludeFinancialDetails = request.IncludeFinancialDetails,
        };
        context.NotificationDestinations.Add(destination);
        await context.SaveChangesAsync(cancellationToken);
        return ToResponse(destination);
    }

    public async Task<NotificationDestinationResponse?> UpdateAsync(
        Guid userId,
        Guid id,
        NotificationDestinationRequest request,
        CancellationToken cancellationToken
    )
    {
        var destination = await context.NotificationDestinations.SingleOrDefaultAsync(
            d => d.UserID == userId && d.ID == id,
            cancellationToken
        );
        if (destination is null)
        {
            return null;
        }

        var previous =
            JsonSerializer.Deserialize<WebhookConfiguration>(destination.Configuration)
            ?? throw new InvalidOperationException(
                localizer["NotificationInvalidConfiguration"].Value
            );
        var configuration = BuildConfiguration(request, previous);
        destination.Name = ValidateName(request.Name);
        destination.Configuration = JsonSerializer.Serialize(configuration);
        destination.EventTypes = JsonSerializer.Serialize(ValidateEvents(request.EventTypes));
        destination.IncludeFinancialDetails = request.IncludeFinancialDetails;
        await context.SaveChangesAsync(cancellationToken);
        return ToResponse(destination);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var destination = await context.NotificationDestinations.SingleOrDefaultAsync(
            d => d.UserID == userId && d.ID == id,
            cancellationToken
        );
        if (destination is null)
        {
            return false;
        }

        context.NotificationDestinations.Remove(destination);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool?> TestAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var destination = await context
            .NotificationDestinations.AsNoTracking()
            .SingleOrDefaultAsync(d => d.UserID == userId && d.ID == id, cancellationToken);
        return destination is null
            ? null
            : await sender.SendAsync(
                destination,
                new NotificationEvent(userId, "test", localizer["NotificationTestMessage"]),
                cancellationToken
            );
    }

    private NotificationDestinationResponse ToResponse(NotificationDestination destination) =>
        new(
            destination.ID,
            destination.Name,
            JsonSerializer.Deserialize<string[]>(destination.EventTypes)
                ?? throw new InvalidOperationException(
                    localizer["NotificationEventConfigurationInvalid"].Value
                ),
            destination.IncludeFinancialDetails
        );

    private NotificationDestinationDetailsResponse ToDetailsResponse(
        NotificationDestination destination
    )
    {
        var configuration =
            JsonSerializer.Deserialize<WebhookConfiguration>(destination.Configuration)
            ?? throw new InvalidOperationException(
                localizer["NotificationInvalidConfiguration"].Value
            );

        return new NotificationDestinationDetailsResponse(
            destination.ID,
            destination.Name,
            JsonSerializer.Deserialize<string[]>(destination.EventTypes)
                ?? throw new InvalidOperationException(
                    localizer["NotificationEventConfigurationInvalid"].Value
                ),
            destination.IncludeFinancialDetails,
            configuration.Url,
            configuration.Headers,
            configuration.ContentType,
            configuration.BodyTemplate
        );
    }

    private string ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
        {
            throw new ArgumentException(localizer["NotificationNameInvalid"].Value);
        }
        return name.Trim();
    }

    private string[] ValidateEvents(string[]? events)
    {
        if (events is null || events.Any(e => !NotificationEventTypes.All.Contains(e)))
        {
            throw new ArgumentException(localizer["NotificationEventTypesInvalid"].Value);
        }
        return events.Distinct().ToArray();
    }

    private WebhookConfiguration BuildConfiguration(
        NotificationDestinationRequest request,
        WebhookConfiguration? previous
    )
    {
        var url = request.Url ?? previous?.Url;
        var headers = request.Headers ?? previous?.Headers ?? new Dictionary<string, string>();
        var contentType = request.ContentType ?? previous?.ContentType ?? "application/json";
        var template =
            request.BodyTemplate ?? previous?.BodyTemplate ?? "{\"content\":\"{{message}}\"}";
        if (!NotificationWebhookSender.IsValidUrl(url))
        {
            throw new ArgumentException(localizer["NotificationWebhookUrlInvalid"].Value);
        }
        if (contentType is not ("application/json" or "text/plain"))
        {
            throw new ArgumentException(localizer["NotificationWebhookContentTypeInvalid"].Value);
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
                localizer["NotificationWebhookTemplateOrHeadersInvalid"].Value
            );
        }
        using var validationRequest = new HttpRequestMessage();
        if (
            headers.Any(header =>
                !validationRequest.Headers.TryAddWithoutValidation(header.Key, header.Value)
            )
        )
        {
            throw new ArgumentException(localizer["NotificationWebhookHeadersInvalid"].Value);
        }

        var configuration = new WebhookConfiguration(url!, headers, contentType, template);
        if (contentType == "application/json")
        {
            try
            {
                var sample = new NotificationEvent(Guid.Empty, "test", "test", "test");
                using var _ = JsonDocument.Parse(
                    NotificationWebhookSender.Render(configuration, sample, true)
                );
            }
            catch (JsonException exception)
            {
                throw new ArgumentException(
                    localizer["NotificationWebhookJsonTemplateInvalid"].Value,
                    exception
                );
            }
        }
        return configuration;
    }
}
