using System.Data.Common;
using System.Text.Json;
using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class TransactionImportServiceTests
{
    [Fact]
    public async Task EnqueueAsync_ShouldPersistPayloadAndCounts()
    {
        var helper = new TestHelper();
        var now = new DateTime(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);
        var nowProvider = new Mock<INowProvider>();
        nowProvider.Setup(provider => provider.UtcNow).Returns(now);
        var service = CreateService(helper, nowProvider.Object);
        var existingRowId = Guid.NewGuid();

        var response = await service.EnqueueAsync(
            helper.demoUser.Id,
            new TransactionImportRequest
            {
                Transactions =
                [
                    new TransactionImport
                    {
                        ID = existingRowId,
                        Account = "Checking",
                        Amount = 10,
                    },
                    new TransactionImport { Account = "Checking", Amount = -5 },
                ],
                AccountNameToIDMap = [],
            }
        );

        var job = await helper.UserDataContext.TransactionImportJobs.FindAsync(response.ID);
        job.Should().NotBeNull();
        job!.Status.Should().Be(TransactionImportJobStatuses.Pending);
        job.TotalCount.Should().Be(2);
        job.Payload.Should().NotBeNullOrWhiteSpace();

        var persistedRequest = JsonSerializer.Deserialize<TransactionImportRequest>(job.Payload);
        persistedRequest.Should().NotBeNull();
        persistedRequest!.Transactions.Should().OnlyContain(transaction => transaction.ID != null);
        persistedRequest.Transactions.First().ID.Should().Be(existingRowId);
        response.ProgressPercentage.Should().Be(0);
        response.CreatedAt.Should().Be(now);
    }

    [Fact]
    public async Task ReadStatusAsync_ShouldOnlyReturnJobsOwnedByUser()
    {
        var helper = new TestHelper();
        var service = CreateService(helper, Mock.Of<INowProvider>());
        var response = await service.EnqueueAsync(
            helper.demoUser.Id,
            new TransactionImportRequest()
        );

        var status = await service.ReadStatusAsync(helper.demoUser.Id, response.ID);
        var otherUserStatus = await service.ReadStatusAsync(Guid.NewGuid(), response.ID);

        status.Should().NotBeNull();
        otherUserStatus.Should().BeNull();
    }

    [Fact]
    public async Task RequestCancellationAsync_ShouldCancelPendingOwnedJob()
    {
        var helper = new TestHelper();
        var now = new DateTime(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);
        var service = CreateService(helper, CreateNowProvider(now).Object);
        var job = AddJob(helper, [new TransactionImport { Account = "Checking" }]);

        var response = await service.RequestCancellationAsync(helper.demoUser.Id, job.ID);

        response.Should().NotBeNull();
        response!.Status.Should().Be(TransactionImportJobStatuses.Cancelled);
        response.CancellationRequested.Should().BeTrue();
        response.CompletedAt.Should().Be(now);
        var cancelledJob = await helper.UserDataContext.TransactionImportJobs.FindAsync(job.ID);
        cancelledJob!.Status.Should().Be(TransactionImportJobStatuses.Cancelled);
    }

    [Fact]
    public async Task RequestCancellationAsync_ShouldNotCancelJobOwnedByAnotherUser()
    {
        var helper = new TestHelper();
        var service = CreateService(helper, Mock.Of<INowProvider>());
        var job = AddJob(helper, [new TransactionImport { Account = "Checking" }]);

        var response = await service.RequestCancellationAsync(Guid.NewGuid(), job.ID);

        response.Should().BeNull();
        job.Status.Should().Be(TransactionImportJobStatuses.Pending);
    }

    [Fact]
    public async Task EnqueueAsync_WithSameIdempotencyKey_ShouldReturnExistingJob()
    {
        var helper = new TestHelper();
        var service = CreateService(helper, Mock.Of<INowProvider>());
        var request = new TransactionImportRequest
        {
            Transactions = [new TransactionImport { Account = "Checking" }],
        };

        var firstResponse = await service.EnqueueAsync(helper.demoUser.Id, request, "import-1");
        var secondResponse = await service.EnqueueAsync(helper.demoUser.Id, request, " import-1 ");

        secondResponse.ID.Should().Be(firstResponse.ID);
        helper.UserDataContext.TransactionImportJobs.Should().ContainSingle();
    }

    [Fact]
    public async Task ProcessNextAsync_WhenNoPendingJobs_ShouldReturnFalse()
    {
        var helper = new TestHelper();
        var service = CreateService(helper, Mock.Of<INowProvider>());

        var result = await service.ProcessNextAsync(CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessNextAsync_WhenJobCompletes_ShouldProcessBatchesAndUpdateProgress()
    {
        var helper = new TestHelper();
        var now = new DateTime(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);
        var nowProvider = CreateNowProvider(now);
        var importServiceMock = new Mock<ITransactionService>();
        var service = CreateService(helper, nowProvider.Object, importServiceMock.Object);
        var job = AddJob(helper, CreateTransactions(101));

        var result = await service.ProcessNextAsync(CancellationToken.None);

        result.Should().BeTrue();
        importServiceMock.Verify(
            transactionService =>
                transactionService.ImportTransactionsAsync(
                    helper.demoUser.Id,
                    It.IsAny<ITransactionImportRequest>()
                ),
            Times.Exactly(2)
        );

        var completedJob = await helper.UserDataContext.TransactionImportJobs.FindAsync(job.ID);
        completedJob.Should().NotBeNull();
        completedJob!.Status.Should().Be(TransactionImportJobStatuses.Completed);
        completedJob.ProcessedCount.Should().Be(101);
        completedJob.SucceededCount.Should().Be(101);
        completedJob.FailedCount.Should().Be(0);
        completedJob.CompletedAt.Should().Be(now);
        completedJob.LeaseExpiresAt.Should().BeNull();
        completedJob.LastHeartbeatAt.Should().BeNull();
    }

    [Fact]
    public async Task ProcessNextAsync_WhenCancellationIsRequestedAfterBatch_ShouldCancelJob()
    {
        var helper = new TestHelper();
        var now = new DateTime(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);
        var nowProvider = CreateNowProvider(now);
        var importServiceMock = new Mock<ITransactionService>();
        var service = CreateService(helper, nowProvider.Object, importServiceMock.Object);
        var job = AddJob(helper, CreateTransactions(101));
        importServiceMock
            .Setup(transactionService =>
                transactionService.ImportTransactionsAsync(
                    helper.demoUser.Id,
                    It.IsAny<ITransactionImportRequest>()
                )
            )
            .Callback(() =>
            {
                if (job.CancellationRequestedAt is null)
                {
                    job.CancellationRequestedAt = now;
                    helper.UserDataContext.SaveChanges();
                }
            })
            .Returns(Task.CompletedTask);

        var result = await service.ProcessNextAsync(CancellationToken.None);

        result.Should().BeTrue();
        var cancelledJob = await helper.UserDataContext.TransactionImportJobs.FindAsync(job.ID);
        cancelledJob.Should().NotBeNull();
        cancelledJob!.Status.Should().Be(TransactionImportJobStatuses.Cancelled);
        cancelledJob.ProcessedCount.Should().Be(100);
        cancelledJob.CompletedAt.Should().Be(now);
        cancelledJob.LeaseExpiresAt.Should().BeNull();
        importServiceMock.Verify(
            transactionService =>
                transactionService.ImportTransactionsAsync(
                    helper.demoUser.Id,
                    It.IsAny<ITransactionImportRequest>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task ProcessNextAsync_WhenBatchFails_ShouldRetryRowsAndCompleteWithErrors()
    {
        var helper = new TestHelper();
        var nowProvider = CreateNowProvider(new DateTime(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc));
        var importServiceMock = new Mock<ITransactionService>();
        importServiceMock
            .Setup(transactionService =>
                transactionService.ImportTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<ITransactionImportRequest>()
                )
            )
            .Returns<Guid, ITransactionImportRequest>(
                (_, request) =>
                {
                    var transactions = request.Transactions.ToList();
                    if (transactions.Count != 1 || transactions[0].Amount == 2)
                    {
                        throw new InvalidOperationException("Invalid imported row");
                    }

                    return Task.CompletedTask;
                }
            );
        var service = CreateService(helper, nowProvider.Object, importServiceMock.Object);
        var job = AddJob(
            helper,
            [
                new TransactionImport { Account = "Checking", Amount = 1 },
                new TransactionImport { Account = "Checking", Amount = 2 },
            ]
        );

        var result = await service.ProcessNextAsync(CancellationToken.None);

        result.Should().BeTrue();
        var completedJob = await helper.UserDataContext.TransactionImportJobs.FindAsync(job.ID);
        completedJob.Should().NotBeNull();
        completedJob!.Status.Should().Be(TransactionImportJobStatuses.CompletedWithErrors);
        completedJob.ProcessedCount.Should().Be(2);
        completedJob.SucceededCount.Should().Be(1);
        completedJob.FailedCount.Should().Be(1);
        completedJob.ErrorMessage.Should().Contain("Row 2");
    }

    [Fact]
    public async Task ProcessNextAsync_WhenMoreThanTenRowsFail_ShouldLimitErrorSummary()
    {
        var helper = new TestHelper();
        var importServiceMock = new Mock<ITransactionService>();
        importServiceMock
            .Setup(transactionService =>
                transactionService.ImportTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<ITransactionImportRequest>()
                )
            )
            .ThrowsAsync(new InvalidOperationException("Invalid imported row"));
        var service = CreateService(helper, Mock.Of<INowProvider>(), importServiceMock.Object);
        var job = AddJob(helper, CreateTransactions(11));

        var result = await service.ProcessNextAsync(CancellationToken.None);

        result.Should().BeTrue();
        var completedJob = await helper.UserDataContext.TransactionImportJobs.FindAsync(job.ID);
        completedJob.Should().NotBeNull();
        completedJob!.Status.Should().Be(TransactionImportJobStatuses.CompletedWithErrors);
        completedJob.ProcessedCount.Should().Be(11);
        completedJob.SucceededCount.Should().Be(0);
        completedJob.FailedCount.Should().Be(11);
        completedJob.ErrorMessage.Should().NotBeNull();
        completedJob.ErrorMessage!.Should().Contain("Row 10");
        completedJob.ErrorMessage.Should().NotContain("Row 11");
    }

    [Fact]
    public async Task ProcessNextAsync_WhenPayloadIsInvalid_ShouldMarkJobFailed()
    {
        var helper = new TestHelper();
        var now = new DateTime(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);
        var nowProvider = CreateNowProvider(now);
        var service = CreateService(helper, nowProvider.Object);
        var job = AddJob(helper, [], "null");

        var result = await service.ProcessNextAsync(CancellationToken.None);

        result.Should().BeTrue();
        var failedJob = await helper.UserDataContext.TransactionImportJobs.FindAsync(job.ID);
        failedJob.Should().NotBeNull();
        failedJob!.Status.Should().Be(TransactionImportJobStatuses.Failed);
        failedJob.ErrorMessage.Should().Be("The import payload could not be read.");
        failedJob.CompletedAt.Should().Be(now);
        failedJob.LeaseExpiresAt.Should().BeNull();
        failedJob.LastHeartbeatAt.Should().BeNull();
    }

    [Fact]
    public async Task ProcessNextAsync_WhenCancelledDuringProcessing_ShouldThrowCancellation()
    {
        var helper = new TestHelper();
        using var cancellationSource = new CancellationTokenSource();
        var importServiceMock = new Mock<ITransactionService>();
        importServiceMock
            .Setup(transactionService =>
                transactionService.ImportTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<ITransactionImportRequest>()
                )
            )
            .Callback(() => cancellationSource.Cancel())
            .Returns(Task.CompletedTask);
        var service = CreateService(helper, Mock.Of<INowProvider>(), importServiceMock.Object);
        AddJob(helper, [new TransactionImport { Account = "Checking", Amount = 1 }]);

        var act = () => service.ProcessNextAsync(cancellationSource.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ProcessNextAsync_ShouldRequeueExpiredRunningJobs()
    {
        var helper = new TestHelper();
        var now = new DateTime(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);
        var nowProvider = CreateNowProvider(now);
        var service = CreateService(helper, nowProvider.Object);
        var expiredJob = new TransactionImportJob
        {
            UserID = helper.demoUser.Id,
            Status = TransactionImportJobStatuses.Running,
            Payload = JsonSerializer.Serialize(new TransactionImportRequest()),
            TotalCount = 0,
            CreatedAt = now.AddMinutes(-20),
            LeaseExpiresAt = now.AddMinutes(-1),
            LastHeartbeatAt = now.AddMinutes(-11),
        };
        helper.UserDataContext.TransactionImportJobs.Add(expiredJob);
        await helper.UserDataContext.SaveChangesAsync();

        var result = await service.ProcessNextAsync(CancellationToken.None);

        result.Should().BeTrue();
        var completedJob = await helper.UserDataContext.TransactionImportJobs.FindAsync(
            expiredJob.ID
        );
        completedJob!.Status.Should().Be(TransactionImportJobStatuses.Completed);
        completedJob.AttemptCount.Should().Be(1);
    }

    [Fact]
    public async Task ProcessNextAsync_ShouldFinalizeExpiredCancelledJobs()
    {
        var helper = new TestHelper();
        var now = new DateTime(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);
        var job = new TransactionImportJob
        {
            UserID = helper.demoUser.Id,
            Status = TransactionImportJobStatuses.Running,
            Payload = JsonSerializer.Serialize(new TransactionImportRequest()),
            TotalCount = 0,
            CreatedAt = now.AddMinutes(-20),
            CancellationRequestedAt = now.AddMinutes(-1),
            LeaseExpiresAt = now.AddSeconds(-1),
            LastHeartbeatAt = now.AddMinutes(-11),
        };
        helper.UserDataContext.TransactionImportJobs.Add(job);
        await helper.UserDataContext.SaveChangesAsync();
        var service = CreateService(helper, CreateNowProvider(now).Object);

        var result = await service.ProcessNextAsync(CancellationToken.None);

        result.Should().BeFalse();
        var cancelledJob = await helper.UserDataContext.TransactionImportJobs.FindAsync(job.ID);
        cancelledJob!.Status.Should().Be(TransactionImportJobStatuses.Cancelled);
        cancelledJob.CompletedAt.Should().Be(now);
        cancelledJob.LeaseExpiresAt.Should().BeNull();
    }

    [Fact]
    public async Task ProcessNextAsync_WhenJobDisappearsWhileHandlingFailure_ShouldRethrowMissingJobError()
    {
        var helper = new TestHelper();
        var job = AddJob(helper, [new TransactionImport { Account = "Checking" }]);
        var importServiceMock = new Mock<ITransactionService>();
        importServiceMock
            .Setup(transactionService =>
                transactionService.ImportTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<ITransactionImportRequest>()
                )
            )
            .Callback(() => RemoveJob(helper, job.ID))
            .ThrowsAsync(new InvalidOperationException("Import failed"));
        var service = CreateService(helper, Mock.Of<INowProvider>(), importServiceMock.Object);

        var act = () => service.ProcessNextAsync(CancellationToken.None);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage($"Transaction import job {job.ID} was not found.");
    }

    [Fact]
    public async Task ProcessNextAsync_WhenJobDisappearsWhileUpdatingProgress_ShouldRethrowMissingJobError()
    {
        var helper = new TestHelper();
        var job = AddJob(helper, [new TransactionImport { Account = "Checking" }]);
        var importServiceMock = new Mock<ITransactionService>();
        importServiceMock
            .Setup(transactionService =>
                transactionService.ImportTransactionsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<ITransactionImportRequest>()
                )
            )
            .Callback(() => RemoveJob(helper, job.ID))
            .Returns(Task.CompletedTask);
        var service = CreateService(helper, Mock.Of<INowProvider>(), importServiceMock.Object);

        var act = () => service.ProcessNextAsync(CancellationToken.None);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage($"Transaction import job {job.ID} was not found.");
    }

    [Fact]
    public async Task ProcessNextAsync_WhenJobDisappearsWhileCompleting_ShouldRethrowMissingJobError()
    {
        var helper = new TestHelper();
        var job = AddJob(helper, [new TransactionImport { Account = "Checking" }]);
        var currentTime = new DateTime(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);
        var nowProvider = new Mock<INowProvider>();
        var utcNowCalls = 0;
        nowProvider
            .Setup(provider => provider.UtcNow)
            .Returns(() =>
            {
                utcNowCalls++;
                if (utcNowCalls == 2)
                {
                    RemoveJobWithoutSaving(helper, job.ID);
                }

                return currentTime;
            });
        var service = CreateService(helper, nowProvider.Object, Mock.Of<ITransactionService>());

        var act = () => service.ProcessNextAsync(CancellationToken.None);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage($"Transaction import job {job.ID} was not found.");
    }

    [Fact]
    public async Task ProcessNextAsync_WithRelationalProvider_ShouldClaimAndCompleteJob()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateSqliteContext(connection);
        var user = new ApplicationUserFaker().Generate();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var now = new DateTime(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);
        var nowProvider = CreateNowProvider(now);
        var service = new TransactionImportService(
            context,
            Mock.Of<ITransactionService>(),
            nowProvider.Object,
            Mock.Of<ILogger<TransactionImportService>>()
        );
        var job = new TransactionImportJob
        {
            UserID = user.Id,
            Status = TransactionImportJobStatuses.Pending,
            Payload = JsonSerializer.Serialize(new TransactionImportRequest()),
            CreatedAt = now,
        };
        context.TransactionImportJobs.Add(job);
        await context.SaveChangesAsync();

        var result = await service.ProcessNextAsync(CancellationToken.None);

        result.Should().BeTrue();
        var completedJob = await context.TransactionImportJobs.FindAsync(job.ID);
        completedJob!.Status.Should().Be(TransactionImportJobStatuses.Completed);
        completedJob.AttemptCount.Should().Be(1);
    }

    [Fact]
    public async Task ProcessNextAsync_WithRelationalProviderAndNoJobs_ShouldReturnFalse()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateSqliteContext(connection);
        var service = new TransactionImportService(
            context,
            Mock.Of<ITransactionService>(),
            Mock.Of<INowProvider>(),
            Mock.Of<ILogger<TransactionImportService>>()
        );

        var result = await service.ProcessNextAsync(CancellationToken.None);

        result.Should().BeFalse();
    }

    private static TransactionImportService CreateService(
        TestHelper helper,
        INowProvider nowProvider,
        ITransactionService? transactionService = null
    )
    {
        return new TransactionImportService(
            helper.UserDataContext,
            transactionService ?? Mock.Of<ITransactionService>(),
            nowProvider,
            Mock.Of<ILogger<TransactionImportService>>()
        );
    }

    private static Mock<INowProvider> CreateNowProvider(DateTime now)
    {
        var nowProvider = new Mock<INowProvider>();
        nowProvider.Setup(provider => provider.UtcNow).Returns(now);
        return nowProvider;
    }

    private static TransactionImportJob AddJob(
        TestHelper helper,
        IEnumerable<TransactionImport> transactions,
        string? payload = null
    )
    {
        var transactionList = transactions.ToList();
        var job = new TransactionImportJob
        {
            UserID = helper.demoUser.Id,
            Status = TransactionImportJobStatuses.Pending,
            Payload =
                payload
                ?? JsonSerializer.Serialize(
                    new TransactionImportRequest { Transactions = transactionList }
                ),
            TotalCount = transactionList.Count,
            CreatedAt = DateTime.UtcNow,
        };
        helper.UserDataContext.TransactionImportJobs.Add(job);
        helper.UserDataContext.SaveChanges();
        return job;
    }

    private static IReadOnlyList<TransactionImport> CreateTransactions(int count) =>
        Enumerable
            .Range(1, count)
            .Select(index => new TransactionImport { Account = "Checking", Amount = index })
            .ToList();

    private static UserDataContext CreateSqliteContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<UserDataContext>()
            .UseSqlite(connection)
            .AddInterceptors(new SqliteImportClaimInterceptor())
            .Options;
        var context = new UserDataContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private static void RemoveJob(TestHelper helper, Guid jobId)
    {
        RemoveJobWithoutSaving(helper, jobId);
        helper.UserDataContext.SaveChanges();
    }

    private static void RemoveJobWithoutSaving(TestHelper helper, Guid jobId)
    {
        var job = helper.UserDataContext.TransactionImportJobs.SingleOrDefault(importJob =>
            importJob.ID == jobId
        );
        if (job is not null)
        {
            helper.UserDataContext.TransactionImportJobs.Remove(job);
        }
    }

    private sealed class SqliteImportClaimInterceptor : DbCommandInterceptor
    {
        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result
        )
        {
            RemovePostgresLockSyntax(command);
            return base.ReaderExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default
        )
        {
            RemovePostgresLockSyntax(command);
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        private static void RemovePostgresLockSyntax(DbCommand command)
        {
            command.CommandText = command.CommandText.Replace(
                "FOR UPDATE SKIP LOCKED",
                string.Empty,
                StringComparison.OrdinalIgnoreCase
            );
        }
    }
}
