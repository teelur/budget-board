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
                throw;
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
        var job = await userDataContext.TransactionImportJobs.FindAsync([jobId], cancellationToken);
        if (job is null)
        {
            throw new InvalidOperationException($"Transaction import job {jobId} was not found.");
        }

        job.Status =
            errors.Count == 0
                ? TransactionImportJobStatuses.Completed
                : TransactionImportJobStatuses.CompletedWithErrors;
        job.CompletedAt = nowProvider.UtcNow;
        job.LastHeartbeatAt = null;
        job.LeaseExpiresAt = null;
        await userDataContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<TransactionImportJob?> ClaimNextJobAsync(CancellationToken cancellationToken)
    {
        var now = nowProvider.UtcNow;
        await userDataContext
            .TransactionImportJobs.Where(job =>
                job.Status == TransactionImportJobStatuses.Running
                && job.LeaseExpiresAt != null
                && job.LeaseExpiresAt < now
            )
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(job => job.Status, TransactionImportJobStatuses.Pending)
                        .SetProperty(job => job.LeaseExpiresAt, (DateTime?)null)
                        .SetProperty(job => job.LastHeartbeatAt, (DateTime?)null),
                cancellationToken
            );

        await using var databaseTransaction = await userDataContext.Database.BeginTransactionAsync(
            cancellationToken
        );

        var job = await userDataContext
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
            .FirstOrDefaultAsync(cancellationToken);

        if (job is null)
        {
            await databaseTransaction.CommitAsync(cancellationToken);
            return null;
        }

        job.Status = TransactionImportJobStatuses.Running;
        job.AttemptCount++;
        job.StartedAt ??= now;
        job.LastHeartbeatAt = now;
        job.LeaseExpiresAt = now.Add(JobLeaseDuration);
        await userDataContext.SaveChangesAsync(cancellationToken);
        await databaseTransaction.CommitAsync(cancellationToken);
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
        };
}
