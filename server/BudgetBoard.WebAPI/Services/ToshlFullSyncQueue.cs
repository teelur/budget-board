using System.Threading.Channels;
using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BudgetBoard.WebAPI.Services;

public sealed class ToshlFullSyncQueue(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ToshlFullSyncQueue> logger
) : BackgroundService, IToshlFullSyncQueue
{
    private static readonly TimeSpan s_staleSyncThreshold = TimeSpan.FromMinutes(10);
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, byte> s_queuedOrRunningUsers =
        new();
    private readonly Channel<Guid> _queue = Channel.CreateUnbounded<Guid>();

    public async Task QueueAsync(Guid userGuid)
    {
        if (!s_queuedOrRunningUsers.TryAdd(userGuid, 0))
        {
            return;
        }

        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var userDataContext = scope.ServiceProvider.GetRequiredService<UserDataContext>();
        try
        {
            var userSettings = await GetOrCreateUserSettingsAsync(userDataContext, userGuid);
            if (IsStaleActiveSync(userSettings, DateTime.UtcNow))
            {
                await MarkStaleSyncAsFailedAsync(userDataContext, userGuid, DateTime.UtcNow);
                await userDataContext.Entry(userSettings).ReloadAsync();
            }

            if (
                userSettings.ToshlFullSyncStatus == ToshlFullSyncStatuses.Queued
                || userSettings.ToshlFullSyncStatus == ToshlFullSyncStatuses.Running
            )
            {
                return;
            }

            userSettings.ToshlFullSyncStatus = ToshlFullSyncStatuses.Queued;
            userSettings.ToshlFullSyncQueuedAt = DateTime.UtcNow;
            userSettings.ToshlFullSyncStartedAt = null;
            userSettings.ToshlFullSyncCompletedAt = null;
            userSettings.ToshlFullSyncError = string.Empty;
            userSettings.ToshlFullSyncProgressPercent = 0;
            userSettings.ToshlFullSyncProgressDescription = "Queued";
            await userDataContext.SaveChangesAsync();

            await _queue.Writer.WriteAsync(userGuid);
        }
        catch
        {
            s_queuedOrRunningUsers.TryRemove(userGuid, out _);
            throw;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RecoverStaleSyncsAsync(stoppingToken);

        await foreach (var userGuid in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessQueuedSyncAsync(userGuid, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Toshl full sync worker failed unexpectedly for user {UserId}",
                    userGuid
                );
            }
        }
    }

    private async Task ProcessQueuedSyncAsync(Guid userGuid, CancellationToken cancellationToken)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var userDataContext = scope.ServiceProvider.GetRequiredService<UserDataContext>();
        var toshlService = scope.ServiceProvider.GetRequiredService<IToshlService>();

        try
        {
            await SetStatusAsync(
                userDataContext,
                userGuid,
                ToshlFullSyncStatuses.Running,
                startedAt: DateTime.UtcNow,
                completedAt: null,
                error: string.Empty,
                progressPercent: 1,
                progressDescription: "Starting",
                cancellationToken
            );

            var errors = await toshlService.SyncAsync(
                userGuid,
                force: true,
                trackFullSyncProgress: true
            );

            await SetStatusAsync(
                userDataContext,
                userGuid,
                errors.Count == 0
                    ? ToshlFullSyncStatuses.Succeeded
                    : ToshlFullSyncStatuses.Failed,
                startedAt: null,
                completedAt: DateTime.UtcNow,
                error: errors.Count == 0
                    ? string.Empty
                    : string.Join("\n", errors.Distinct(StringComparer.OrdinalIgnoreCase)),
                progressPercent: 100,
                progressDescription: errors.Count == 0 ? "Completed" : "Completed with errors",
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Queued Toshl full sync failed for user {UserId}", userGuid);

            await SetStatusAsync(
                userDataContext,
                userGuid,
                ToshlFullSyncStatuses.Failed,
                startedAt: null,
                completedAt: DateTime.UtcNow,
                error: $"{ex.GetBaseException().GetType().Name}: {ex.GetBaseException().Message}",
                progressPercent: null,
                progressDescription: "Failed",
                cancellationToken
            );
        }
        finally
        {
            s_queuedOrRunningUsers.TryRemove(userGuid, out _);
        }
    }

    private static async Task<UserSettings> GetOrCreateUserSettingsAsync(
        UserDataContext userDataContext,
        Guid userGuid
    )
    {
        var userSettings = await userDataContext.UserSettings.FirstOrDefaultAsync(us =>
            us.UserID == userGuid
        );
        if (userSettings != null)
        {
            return userSettings;
        }

        userSettings = new UserSettings { UserID = userGuid };
        userDataContext.UserSettings.Add(userSettings);
        return userSettings;
    }

    private async Task RecoverStaleSyncsAsync(CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;

        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var userDataContext = scope.ServiceProvider.GetRequiredService<UserDataContext>();
        var staleUserIds = await userDataContext
            .UserSettings.Where(us =>
                (us.ToshlFullSyncStatus == ToshlFullSyncStatuses.Queued
                    || us.ToshlFullSyncStatus == ToshlFullSyncStatuses.Running)
                && (
                    (us.ToshlFullSyncStartedAt != null && us.ToshlFullSyncStartedAt < nowUtc - s_staleSyncThreshold)
                    || (us.ToshlFullSyncStartedAt == null && us.ToshlFullSyncQueuedAt != null && us.ToshlFullSyncQueuedAt < nowUtc - s_staleSyncThreshold)
                )
            )
            .Select(us => us.UserID)
            .ToListAsync(cancellationToken);

        foreach (var userGuid in staleUserIds)
        {
            await MarkStaleSyncAsFailedAsync(userDataContext, userGuid, nowUtc, cancellationToken);
        }
    }

    private static bool IsStaleActiveSync(UserSettings userSettings, DateTime nowUtc)
    {
        if (
            userSettings.ToshlFullSyncStatus != ToshlFullSyncStatuses.Queued
            && userSettings.ToshlFullSyncStatus != ToshlFullSyncStatuses.Running
        )
        {
            return false;
        }

        var referenceTime = userSettings.ToshlFullSyncStartedAt ?? userSettings.ToshlFullSyncQueuedAt;
        return referenceTime != null && referenceTime < nowUtc - s_staleSyncThreshold;
    }

    private static async Task MarkStaleSyncAsFailedAsync(
        UserDataContext userDataContext,
        Guid userGuid,
        DateTime nowUtc,
        CancellationToken cancellationToken = default
    )
    {
        await userDataContext
            .UserSettings.Where(us => us.UserID == userGuid)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(us => us.ToshlFullSyncStatus, ToshlFullSyncStatuses.Failed)
                    .SetProperty(us => us.ToshlFullSyncCompletedAt, nowUtc)
                    .SetProperty(
                        us => us.ToshlFullSyncError,
                        "The previous Toshl full sync did not finish. It was likely interrupted by a restart or crash."
                    )
                    .SetProperty(us => us.ToshlFullSyncProgressDescription, "Failed")
                    .SetProperty(us => us.ToshlFullSyncProgressPercent, 100),
                cancellationToken
            );
    }

    private static Task SetStatusAsync(
        UserDataContext userDataContext,
        Guid userGuid,
        string status,
        DateTime? startedAt,
        DateTime? completedAt,
        string error,
        int? progressPercent,
        string progressDescription,
        CancellationToken cancellationToken
    ) => SetStatusInternalAsync(
        userDataContext,
        userGuid,
        status,
        startedAt,
        completedAt,
        error,
        progressPercent,
        progressDescription,
        cancellationToken
    );

    private static async Task SetStatusInternalAsync(
        UserDataContext userDataContext,
        Guid userGuid,
        string status,
        DateTime? startedAt,
        DateTime? completedAt,
        string error,
        int? progressPercent,
        string progressDescription,
        CancellationToken cancellationToken
    )
    {
        await userDataContext
            .UserSettings.Where(us => us.UserID == userGuid)
            .ExecuteUpdateAsync(
                setters => setters
                        .SetProperty(us => us.ToshlFullSyncStatus, status)
                        .SetProperty(us => us.ToshlFullSyncError, error ?? string.Empty)
                        .SetProperty(
                            us => us.ToshlFullSyncProgressDescription,
                            progressDescription ?? string.Empty
                        ),
                cancellationToken
            );

        if (startedAt.HasValue)
        {
            await userDataContext
                .UserSettings.Where(us => us.UserID == userGuid)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        us => us.ToshlFullSyncStartedAt,
                        startedAt.Value
                    ),
                    cancellationToken
                );
        }

        if (completedAt.HasValue)
        {
            await userDataContext
                .UserSettings.Where(us => us.UserID == userGuid)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        us => us.ToshlFullSyncCompletedAt,
                        completedAt.Value
                    ),
                    cancellationToken
                );
        }

        if (progressPercent.HasValue)
        {
            await userDataContext
                .UserSettings.Where(us => us.UserID == userGuid)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        us => us.ToshlFullSyncProgressPercent,
                        progressPercent.Value
                    ),
                    cancellationToken
                );
        }

        if (!completedAt.HasValue && status == ToshlFullSyncStatuses.Running)
        {
            await userDataContext
                .UserSettings.Where(us => us.UserID == userGuid)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        us => us.ToshlFullSyncCompletedAt,
                        (DateTime?)null
                    ),
                    cancellationToken
                );
        }

    }
}
