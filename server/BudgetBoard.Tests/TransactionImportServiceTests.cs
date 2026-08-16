using System.Text.Json;
using BudgetBoard.Database.Models;
using BudgetBoard.Service;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using FluentAssertions;
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

        var response = await service.EnqueueAsync(
            helper.demoUser.Id,
            new TransactionImportRequest
            {
                Transactions =
                [
                    new TransactionImport { Account = "Checking", Amount = 10 },
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

    private static TransactionImportService CreateService(
        TestHelper helper,
        INowProvider nowProvider
    )
    {
        return new TransactionImportService(
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            nowProvider,
            Mock.Of<ILogger<TransactionImportService>>()
        );
    }
}
