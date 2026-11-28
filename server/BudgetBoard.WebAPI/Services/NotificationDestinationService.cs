using System.Text.Json;
using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;
using BudgetBoard.WebAPI.Models;
using BudgetBoard.WebAPI.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Services;

public sealed class NotificationDestinationService(
    UserDataContext context,
    IEnumerable<INotificationDestinationProvider> providers,
    IStringLocalizer<ApiResponseStrings> localizer
) : INotificationDestinationService
{
    private const int MaxDestinationsPerUser = 10;
    private const string DestinationLimitLockPrefix = "budget-board:notification-destination-limit:";
    private readonly IReadOnlyDictionary<string, INotificationDestinationProvider> providerMap =
        providers.ToDictionary(provider => provider.Provider, StringComparer.Ordinal);

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
        var provider = GetProvider(request.Provider);
        var configuration = provider.BuildConfiguration(request, null);
        var events = ValidateEvents(request.EventTypes);
        var name = ValidateName(request.Name);
        var destination = new NotificationDestination
        {
            UserID = userId,
            Name = name,
            Provider = provider.Provider,
            Configuration = configuration,
            EventTypes = JsonSerializer.Serialize(events),
            IncludeFinancialDetails = request.IncludeFinancialDetails,
        };

        if (!context.Database.IsNpgsql())
        {
            return await SaveDestinationIfWithinLimitAsync(destination, cancellationToken);
        }

        async Task<NotificationDestinationResponse> CreateWithinTransactionAsync(
            CancellationToken operationCancellationToken
        )
        {
            await using var transaction = await context.Database.BeginTransactionAsync(
                operationCancellationToken
            );
            var lockKey = $"{DestinationLimitLockPrefix}{userId:D}";
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtextextended({lockKey}, 0))",
                operationCancellationToken
            );

            var response = await SaveDestinationIfWithinLimitAsync(
                destination,
                operationCancellationToken
            );
            await transaction.CommitAsync(operationCancellationToken);
            return response;
        }

        return await new SingleAttemptExecutionStrategy(context).ExecuteAsync(
            CreateWithinTransactionAsync,
            cancellationToken
        );
    }

    private async Task<NotificationDestinationResponse> SaveDestinationIfWithinLimitAsync(
        NotificationDestination destination,
        CancellationToken cancellationToken
    )
    {
        var destinationCount = await context.NotificationDestinations.CountAsync(
            d => d.UserID == destination.UserID,
            cancellationToken
        );
        if (destinationCount >= MaxDestinationsPerUser)
        {
            throw new NotificationDestinationLimitException();
        }

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

        if (!string.Equals(request.Provider, destination.Provider, StringComparison.Ordinal))
        {
            throw new ArgumentException(localizer["NotificationProviderImmutable"].Value);
        }

        var provider = GetProvider(destination.Provider);
        var configuration = provider.BuildConfiguration(request, destination.Configuration);
        destination.Name = ValidateName(request.Name);
        destination.Configuration = configuration;
        destination.EventTypes = JsonSerializer.Serialize(ValidateEvents(request.EventTypes));
        destination.IncludeFinancialDetails = request.IncludeFinancialDetails;
        await context.SaveChangesAsync(cancellationToken);
        return ToResponse(destination);
    }

    public async Task<NotificationDestinationResponse?> SetEnabledAsync(
        Guid userId,
        Guid id,
        bool enabled,
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

        destination.Enabled = enabled;
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
            : await GetProvider(destination.Provider).SendAsync(
                destination,
                new NotificationEvent(userId, "test", localizer["NotificationTestMessage"]),
                cancellationToken
            );
    }

    private NotificationDestinationResponse ToResponse(NotificationDestination destination) =>
        new(
            destination.ID,
            destination.Name,
            destination.Provider,
            destination.Enabled,
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
        var configuration = GetProvider(destination.Provider).ReadConfiguration(
            destination.Configuration
        );

        return new NotificationDestinationDetailsResponse(
            destination.ID,
            destination.Name,
            destination.Provider,
            destination.Enabled,
            JsonSerializer.Deserialize<string[]>(destination.EventTypes)
                ?? throw new InvalidOperationException(
                    localizer["NotificationEventConfigurationInvalid"].Value
                ),
            destination.IncludeFinancialDetails,
            configuration.Url,
            configuration.Headers,
            configuration.ContentType,
            configuration.BodyTemplate,
            configuration.NtfyServerUrl,
            configuration.NtfyTopic,
            configuration.NtfyAccessToken,
            configuration.DiscordWebhookUrl
        );
    }

    private INotificationDestinationProvider GetProvider(string? provider)
    {
        if (provider is not null && providerMap.TryGetValue(provider, out var destinationProvider))
        {
            return destinationProvider;
        }

        throw new ArgumentException(localizer["NotificationProviderInvalid"].Value);
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

    private sealed class SingleAttemptExecutionStrategy(DbContext context)
        : ExecutionStrategy(context, maxRetryCount: 0, maxRetryDelay: TimeSpan.Zero)
    {
        protected override bool ShouldRetryOn(Exception exception) => false;
    }
}

internal sealed class NotificationDestinationLimitException : InvalidOperationException { }
