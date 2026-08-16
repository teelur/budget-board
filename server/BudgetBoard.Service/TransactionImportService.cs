using System.Runtime.ExceptionServices;
using System.Text.Json;
using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class TransactionImportService(
    UserDataContext userDataContext,
    ITransactionService transactionService,
    INowProvider nowProvider,
    ILogger<TransactionImportService> logger
) : ITransactionImportService
{
    private const int BatchSize = 100;
    private static readonly TimeSpan JobLeaseDuration = TimeSpan.FromMinutes(10);

    public async Task<TransactionImportJobResponse> EnqueueAsync(
        Guid userGuid,
        ITransactionImportRequest request,
        string? idempotencyKey = null
    )
    {
        var transactions = request
            .Transactions.Select(transaction => new TransactionImport
            {
                ID = transaction.ID ?? Guid.NewGuid(),
                Date = transaction.Date,
                MerchantName = transaction.MerchantName,
                Category = transaction.Category,
                Amount = transaction.Amount,
                Account = transaction.Account,
            })
            .ToList();
        var accountNameToIdMap = request.AccountNameToIDMap.ToList();
        var normalizedIdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey)
            ? null
            : idempotencyKey.Trim();

        if (normalizedIdempotencyKey is not null)
        {
            var existingJob = await userDataContext.TransactionImportJobs.FirstOrDefaultAsync(job =>
                job.UserID == userGuid && job.IdempotencyKey == normalizedIdempotencyKey
            );
            if (existingJob is not null)
            {
                return ToResponse(existingJob);
            }
        }

        var now = nowProvider.UtcNow;
        var job = new TransactionImportJob
        {
            UserID = userGuid,
            Status = TransactionImportJobStatuses.Pending,
            Payload = JsonSerializer.Serialize(
                new TransactionImportRequest
                {
                    Transactions = transactions,
                    AccountNameToIDMap = accountNameToIdMap,
                }
            ),
            IdempotencyKey = normalizedIdempotencyKey,
            TotalCount = transactions.Count,
            CreatedAt = now,
        };

        userDataContext.TransactionImportJobs.Add(job);
        await userDataContext.SaveChangesAsync();
        return ToResponse(job);
    }

    public async Task<TransactionImportJobResponse?> ReadStatusAsync(Guid userGuid, Guid jobId)
    {
        var job = await userDataContext
            .TransactionImportJobs.AsNoTracking()
            .FirstOrDefaultAsync(importJob =>
                importJob.ID == jobId && importJob.UserID == userGuid
            );

        return job is null ? null : ToResponse(job);
    }

    public async Task<TransactionImportJobResponse?> RequestCancellationAsync(
        Guid userGuid,
        Guid jobId
    )
    {
        var job = await userDataContext.TransactionImportJobs.FirstOrDefaultAsync(importJob =>
            importJob.ID == jobId && importJob.UserID == userGuid
        );

        if (job is null)
        {
            return null;
        }

        if (IsTerminalStatus(job.Status))
        {
            return ToResponse(job);
        }

        job.CancellationRequestedAt ??= nowProvider.UtcNow;
        if (job.Status == TransactionImportJobStatuses.Pending)
        {
            job.Status = TransactionImportJobStatuses.Cancelled;
            job.CompletedAt = nowProvider.UtcNow;
            job.LastHeartbeatAt = null;
            job.LeaseExpiresAt = null;
        }

        await userDataContext.SaveChangesAsync();
        return ToResponse(job);
    }

    public async Task<bool> ProcessNextAsync(CancellationToken cancellationToken)
    {
        var job = await ClaimNextJobAsync(cancellationToken);
        if (job is null)
        {
            return false;
        }

        try
        {
            var request = JsonSerializer.Deserialize<TransactionImportRequest>(job.Payload);
            if (request is null)
            {
                throw new InvalidOperationException("The import payload could not be read.");
            }

            var transactions = request.Transactions.ToList();
            var errors = new List<string>();
            for (var offset = 0; offset < transactions.Count; offset += BatchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var batch = transactions.Skip(offset).Take(BatchSize).ToList();
                var succeededCount = 0;
                var failedCount = 0;

                try
                {
                    await transactionService.ImportTransactionsAsync(
                        job.UserID,
                        new TransactionImportRequest
                        {
                            Transactions = batch,
                            AccountNameToIDMap = request.AccountNameToIDMap,
                        }
                    );
                    succeededCount = batch.Count;
                }
                catch (Exception batchException)
                {
                    userDataContext.ChangeTracker.Clear();
                    logger.LogWarning(
                        batchException,
                        "Transaction import job {JobID} batch {BatchStart} failed; retrying rows individually",
                        job.ID,
                        offset
                    );

                    foreach (
                        var (transaction, index) in batch.Select(
                            (transaction, index) => (transaction, index)
                        )
                    )
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        try
                        {
                            await transactionService.ImportTransactionsAsync(
                                job.UserID,
                                new TransactionImportRequest
                                {
                                    Transactions = [transaction],
                                    AccountNameToIDMap = request.AccountNameToIDMap,
                                }
                            );
                            succeededCount++;
                        }
                        catch (Exception rowException)
                        {
                            failedCount++;
                            if (errors.Count < 10)
                            {
                                errors.Add($"Row {offset + index + 1}: {rowException.Message}");
                            }
                        }
                        finally
                        {
                            userDataContext.ChangeTracker.Clear();
                        }
                    }
                }

                await UpdateProgressAsync(
                    job.ID,
                    offset + batch.Count,
                    succeededCount,
                    failedCount,
                    errors,
                    cancellationToken
                );

                if (await IsCancellationRequestedAsync(job.ID, cancellationToken))
                {
                    await CancelJobAsync(job.ID, cancellationToken);
                    return true;
                }
            }

            await CompleteJobAsync(job.ID, errors, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            userDataContext.ChangeTracker.Clear();
            job = await userDataContext.TransactionImportJobs.FindAsync(
                [job.ID],
                cancellationToken
            );
            if (job is null)
            {
                ExceptionDispatchInfo.Capture(exception).Throw();
            }

            job.Status = TransactionImportJobStatuses.Failed;
            job.ErrorMessage = exception.Message;
            job.CompletedAt = nowProvider.UtcNow;
            job.LastHeartbeatAt = null;
            job.LeaseExpiresAt = null;
            await userDataContext.SaveChangesAsync(cancellationToken);

            logger.LogError(exception, "Transaction import job {JobID} failed", job.ID);
        }

        return true;
    }

    private async Task UpdateProgressAsync(
        Guid jobId,
        int processedCount,
        int succeededCount,
        int failedCount,
        IReadOnlyList<string> errors,
        CancellationToken cancellationToken
    )
    {
        userDataContext.ChangeTracker.Clear();
        var job = await userDataContext.TransactionImportJobs.FindAsync([jobId], cancellationToken);
        if (job is null)
        {
            throw new InvalidOperationException($"Transaction import job {jobId} was not found.");
        }

        job.ProcessedCount = processedCount;
        job.SucceededCount += succeededCount;
        job.FailedCount += failedCount;
        job.ErrorMessage = errors.Count == 0 ? null : string.Join(" ", errors);
        job.LastHeartbeatAt = nowProvider.UtcNow;
        job.LeaseExpiresAt = job.LastHeartbeatAt.Value.Add(JobLeaseDuration);
        await userDataContext.SaveChangesAsync(cancellationToken);
    }

    private async Task CompleteJobAsync(
        Guid jobId,
        IReadOnlyList<string> errors,
        CancellationToken cancellationToken
    )
    {
        userDataContext.ChangeTracker.Clear();
        var completedAt = nowProvider.UtcNow;
        var completedStatus =
            errors.Count == 0
                ? TransactionImportJobStatuses.Completed
                : TransactionImportJobStatuses.CompletedWithErrors;

        if (!userDataContext.Database.IsRelational())
        {
            var nonRelationalJob = await userDataContext.TransactionImportJobs.FindAsync(
                [jobId],
                cancellationToken
            );
            if (nonRelationalJob is null)
            {
                throw new InvalidOperationException(
                    $"Transaction import job {jobId} was not found."
                );
            }

            if (
                nonRelationalJob.Status == TransactionImportJobStatuses.Running
                && nonRelationalJob.CancellationRequestedAt != null
            )
            {
                await CancelJobAsync(jobId, cancellationToken);
            }
            else if (nonRelationalJob.Status == TransactionImportJobStatuses.Running)
            {
                nonRelationalJob.Status = completedStatus;
                nonRelationalJob.CompletedAt = completedAt;
                nonRelationalJob.LastHeartbeatAt = null;
                nonRelationalJob.LeaseExpiresAt = null;
                await userDataContext.SaveChangesAsync(cancellationToken);
            }

            return;
        }

        var updatedCount = await userDataContext
            .TransactionImportJobs.Where(job =>
                job.ID == jobId
                && job.Status == TransactionImportJobStatuses.Running
                && job.CancellationRequestedAt == null
            )
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(job => job.Status, completedStatus)
                        .SetProperty(job => job.CompletedAt, completedAt)
                        .SetProperty(job => job.LastHeartbeatAt, (DateTime?)null)
                        .SetProperty(job => job.LeaseExpiresAt, (DateTime?)null),
                cancellationToken
            );

        if (updatedCount > 0)
        {
            return;
        }

        userDataContext.ChangeTracker.Clear();
        var job = await userDataContext.TransactionImportJobs.FindAsync([jobId], cancellationToken);
        if (job is null)
        {
            throw new InvalidOperationException($"Transaction import job {jobId} was not found.");
        }

        if (
            job.Status == TransactionImportJobStatuses.Running
            && job.CancellationRequestedAt != null
        )
        {
            await CancelJobAsync(jobId, cancellationToken);
        }
    }

    private async Task<bool> IsCancellationRequestedAsync(
        Guid jobId,
        CancellationToken cancellationToken
    )
    {
        return await userDataContext
            .TransactionImportJobs.AsNoTracking()
            .AnyAsync(
                job => job.ID == jobId && job.CancellationRequestedAt != null,
                cancellationToken
            );
    }

    private async Task CancelJobAsync(Guid jobId, CancellationToken cancellationToken)
    {
        userDataContext.ChangeTracker.Clear();
        var job = await userDataContext.TransactionImportJobs.FindAsync([jobId], cancellationToken);
        if (job is null)
        {
            throw new InvalidOperationException($"Transaction import job {jobId} was not found.");
        }

        if (IsTerminalStatus(job.Status))
        {
            return;
        }

        job.Status = TransactionImportJobStatuses.Cancelled;
        job.CompletedAt ??= nowProvider.UtcNow;
        job.LastHeartbeatAt = null;
        job.LeaseExpiresAt = null;
        await userDataContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<TransactionImportJob?> ClaimNextJobAsync(CancellationToken cancellationToken)
    {
        var now = nowProvider.UtcNow;
        var expiredJobs = userDataContext.TransactionImportJobs.Where(job =>
            job.Status == TransactionImportJobStatuses.Running
            && job.LeaseExpiresAt != null
            && job.LeaseExpiresAt < now
        );
        if (userDataContext.Database.IsRelational())
        {
            await expiredJobs
                .Where(job => job.CancellationRequestedAt != null)
                .ExecuteUpdateAsync(
                    setters =>
                        setters
                            .SetProperty(job => job.Status, TransactionImportJobStatuses.Cancelled)
                            .SetProperty(job => job.CompletedAt, now)
                            .SetProperty(job => job.LeaseExpiresAt, (DateTime?)null)
                            .SetProperty(job => job.LastHeartbeatAt, (DateTime?)null),
                    cancellationToken
                );
            await expiredJobs
                .Where(job => job.CancellationRequestedAt == null)
                .ExecuteUpdateAsync(
                    setters =>
                        setters
                            .SetProperty(job => job.Status, TransactionImportJobStatuses.Pending)
                            .SetProperty(job => job.LeaseExpiresAt, (DateTime?)null)
                            .SetProperty(job => job.LastHeartbeatAt, (DateTime?)null),
                    cancellationToken
                );
        }
        else
        {
            var expiredJobsInMemory = await expiredJobs.ToListAsync(cancellationToken);
            foreach (var expiredJob in expiredJobsInMemory)
            {
                expiredJob.Status =
                    expiredJob.CancellationRequestedAt != null
                        ? TransactionImportJobStatuses.Cancelled
                        : TransactionImportJobStatuses.Pending;
                expiredJob.CompletedAt =
                    expiredJob.CancellationRequestedAt != null ? now : expiredJob.CompletedAt;
                expiredJob.LeaseExpiresAt = null;
                expiredJob.LastHeartbeatAt = null;
            }

            await userDataContext.SaveChangesAsync(cancellationToken);
        }

        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? databaseTransaction = null;
        if (userDataContext.Database.IsRelational())
        {
            databaseTransaction = await userDataContext.Database.BeginTransactionAsync(
                cancellationToken
            );
        }

        var job = userDataContext.Database.IsRelational()
            ? await userDataContext
                .TransactionImportJobs.FromSqlRaw(
                    """
                    SELECT *
                    FROM "TransactionImportJob"
                    WHERE "Status" = 'Pending'
                    ORDER BY "CreatedAt"
                    FOR UPDATE SKIP LOCKED
                    LIMIT 1
                    """
                )
                .FirstOrDefaultAsync(cancellationToken)
            : await userDataContext
                .TransactionImportJobs.Where(importJob =>
                    importJob.Status == TransactionImportJobStatuses.Pending
                )
                .OrderBy(importJob => importJob.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

        if (job is null)
        {
            if (databaseTransaction is not null)
            {
                await databaseTransaction.CommitAsync(cancellationToken);
                await databaseTransaction.DisposeAsync();
            }
            return null;
        }

        job.Status = TransactionImportJobStatuses.Running;
        job.AttemptCount++;
        job.StartedAt ??= now;
        job.LastHeartbeatAt = now;
        job.LeaseExpiresAt = now.Add(JobLeaseDuration);
        await userDataContext.SaveChangesAsync(cancellationToken);
        if (databaseTransaction is not null)
        {
            await databaseTransaction.CommitAsync(cancellationToken);
            await databaseTransaction.DisposeAsync();
        }
        return job;
    }

    private static TransactionImportJobResponse ToResponse(TransactionImportJob job) =>
        new()
        {
            ID = job.ID,
            Status = job.Status,
            TotalCount = job.TotalCount,
            ProcessedCount = job.ProcessedCount,
            SucceededCount = job.SucceededCount,
            FailedCount = job.FailedCount,
            CreatedAt = job.CreatedAt,
            StartedAt = job.StartedAt,
            CompletedAt = job.CompletedAt,
            ErrorMessage = job.ErrorMessage,
            CancellationRequested = job.CancellationRequestedAt != null,
        };

    private static bool IsTerminalStatus(string status) =>
        status
            is TransactionImportJobStatuses.Completed
                or TransactionImportJobStatuses.CompletedWithErrors
                or TransactionImportJobStatuses.Failed
                or TransactionImportJobStatuses.Cancelled;
}
