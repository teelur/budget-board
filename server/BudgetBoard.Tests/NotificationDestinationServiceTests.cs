using System.Data.Common;
using System.Text.Json;
using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;
using BudgetBoard.WebAPI.Models;
using BudgetBoard.WebAPI.Services;
using BudgetBoard.WebAPI.Resources;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;

namespace BudgetBoard.IntegrationTests;

public class NotificationDestinationServiceTests
{
    [Fact]
    public async Task Destinations_AreUserScopedAndSummaryOmitsConfiguration()
    {
        var helper = new TestHelper();
        var service = CreateService(helper);
        var userId = helper.demoUser.Id;
        var otherUserId = Guid.NewGuid();
        var request = new NotificationDestinationRequest
        {
            Name = "Example",
            Url = "https://example.test/secret",
            Headers = new Dictionary<string, string> { ["Authorization"] = "Bearer secret" },
            EventTypes = [NotificationEventTypes.SyncFailed],
        };

        var created = await service.CreateAsync(userId, request, CancellationToken.None);
        var stored = await helper.UserDataContext.NotificationDestinations.SingleAsync();

        var storedConfiguration = JsonDocument.Parse(stored.Configuration);
        storedConfiguration.RootElement.GetProperty("Url").GetString()
            .Should().Be("https://example.test/secret");
        storedConfiguration.RootElement.GetProperty("Headers")
            .GetProperty("Authorization").GetString().Should().Be("Bearer secret");
        JsonSerializer.Serialize(created).Should().NotContain("secret");
        (await service.ReadAsync(otherUserId, CancellationToken.None)).Should().BeEmpty();
        (await service.ReadAsync(otherUserId, created.Id, CancellationToken.None)).Should().BeNull();
        (await service.UpdateAsync(otherUserId, created.Id, request, CancellationToken.None)).Should().BeNull();
        (await service.DeleteAsync(otherUserId, created.Id, CancellationToken.None)).Should().BeFalse();
    }

    [Fact]
    public async Task Update_WhenSecretsAreOmitted_RetainsExistingConfiguration()
    {
        var helper = new TestHelper();
        var service = CreateService(helper);
        var userId = helper.demoUser.Id;
        var created = await service.CreateAsync(
            userId,
            new NotificationDestinationRequest
            {
                Name = "Example",
                Url = "https://example.test/secret",
                EventTypes = [NotificationEventTypes.SyncFailed],
            },
            CancellationToken.None
        );

        var updated = await service.UpdateAsync(
            userId,
            created.Id,
            new NotificationDestinationRequest { Name = "Updated", EventTypes = [] },
            CancellationToken.None
        );

        updated!.Name.Should().Be("Updated");
        updated.EventTypes.Should().BeEmpty();
        (await helper.UserDataContext.NotificationDestinations.SingleAsync())
            .Configuration.Should().Contain("https://example.test/secret");
    }

    [Fact]
    public async Task Create_RejectsMoreThanTenDestinationsForUser()
    {
        var helper = new TestHelper();
        var service = CreateService(helper);
        var userId = helper.demoUser.Id;

        for (var index = 0; index < 10; index++)
        {
            await service.CreateAsync(
                userId,
                new NotificationDestinationRequest
                {
                    Name = $"Destination {index}",
                    Url = "https://example.test/",
                    EventTypes = [],
                },
                CancellationToken.None
            );
        }

        var action = () =>
            service.CreateAsync(
                userId,
                new NotificationDestinationRequest
                {
                    Name = "Destination 11",
                    Url = "https://example.test/",
                    EventTypes = [],
                },
                CancellationToken.None
            );

        await action.Should().ThrowAsync<InvalidOperationException>();
        (await helper.UserDataContext.NotificationDestinations.CountAsync()).Should().Be(10);
    }

    [Fact]
    public async Task ReadDetails_ReturnsConfigurationOnlyToOwningUser()
    {
        var helper = new TestHelper();
        var service = CreateService(helper);
        var userId = helper.demoUser.Id;
        var created = await service.CreateAsync(
            userId,
            new NotificationDestinationRequest
            {
                Name = "Example",
                Url = "https://example.test/secret",
                Headers = new Dictionary<string, string>
                {
                    ["Authorization"] = "Bearer secret",
                },
                ContentType = "text/plain",
                BodyTemplate = "{{message}}",
                EventTypes = [NotificationEventTypes.SyncFailed],
            },
            CancellationToken.None
        );

        var details = await service.ReadAsync(userId, created.Id, CancellationToken.None);
        var summaries = await service.ReadAsync(userId, CancellationToken.None);

        details.Should().NotBeNull();
        details!.Url.Should().Be("https://example.test/secret");
        details.Headers.Should().ContainKey("Authorization").WhoseValue.Should().Be("Bearer secret");
        details.ContentType.Should().Be("text/plain");
        details.BodyTemplate.Should().Be("{{message}}");
        JsonSerializer.Serialize(summaries).Should().NotContain("secret");
        (await service.ReadAsync(Guid.NewGuid(), created.Id, CancellationToken.None)).Should().BeNull();
    }

    [Theory]
    [InlineData("file:///etc/passwd")]
    [InlineData("http://169.254.169.254/latest/meta-data/")]
    [InlineData("http://[::ffff:169.254.169.254]/")]
    [InlineData("http://[fd00:ec2::254]/")]
    [InlineData("https://user:password@example.test/")]
    public async Task Create_RejectsInvalidDestinations(string url)
    {
        var helper = new TestHelper();
        var service = CreateService(helper);
        var request = new NotificationDestinationRequest { Name = "Example", Url = url, EventTypes = [] };

        var action = () => service.CreateAsync(helper.demoUser.Id, request, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Create_RejectsInvalidJsonTemplate()
    {
        var helper = new TestHelper();
        var service = CreateService(helper);
        var request = new NotificationDestinationRequest
        {
            Name = "Example",
            Url = "https://example.test/",
            BodyTemplate = "{\"message\":{{message}}}",
            EventTypes = [],
        };

        var action = () => service.CreateAsync(helper.demoUser.Id, request, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Create_ValidatesJsonTemplateWithFinancialDetailsOptInSetting()
    {
        var helper = new TestHelper();
        var service = CreateService(helper);
        var request = new NotificationDestinationRequest
        {
            Name = "Example",
            Url = "https://example.test/",
            BodyTemplate = "t{{financial_details}}rue",
            EventTypes = [],
            IncludeFinancialDetails = false,
        };

        var created = await service.CreateAsync(
            helper.demoUser.Id,
            request,
            CancellationToken.None
        );

        created.IncludeFinancialDetails.Should().BeFalse();
    }

    [SuperuserServiceTests.PostgreSqlFact]
    public async Task Create_ConcurrentRequestsRespectDestinationLimit()
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "BUDGET_BOARD_TEST_POSTGRES_CONNECTION_STRING"
        )!;
        var schema = $"notification_destination_limit_test_{Guid.NewGuid():N}";
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using (
            var createSchema = new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"", connection)
        )
        {
            await createSchema.ExecuteNonQueryAsync();
        }

        try
        {
            var scopedConnectionString = new NpgsqlConnectionStringBuilder(connectionString)
            {
                SearchPath = schema,
            }.ConnectionString;
            var lockInterceptor = new DestinationLimitLockInterceptor();
            var options = new DbContextOptionsBuilder<UserDataContext>()
                .UseNpgsql(scopedConnectionString, npgsqlOptions => npgsqlOptions.EnableRetryOnFailure())
                .AddInterceptors(lockInterceptor)
                .Options;
            var userId = Guid.NewGuid();

            await using (var setup = new UserDataContext(options))
            {
                await setup.Database.ExecuteSqlRawAsync(setup.Database.GenerateCreateScript());
                setup.Users.Add(new ApplicationUser { Id = userId, UserName = "notification-test" });
                setup.NotificationDestinations.AddRange(
                    Enumerable.Range(0, 9).Select(index => new NotificationDestination
                    {
                        UserID = userId,
                        Name = $"destination-{index}",
                        Configuration = """{"Url":"https://example.test/","Headers":{},"ContentType":"application/json","BodyTemplate":"{{message}}"}""",
                        EventTypes = "[]",
                    })
                );
                await setup.SaveChangesAsync();
            }

            await using var firstContext = new UserDataContext(options);
            await using var secondContext = new UserDataContext(options);
            var firstCreate = CreateService(firstContext).CreateAsync(
                userId,
                CreateRequest("first"),
                CancellationToken.None
            );
            Task<NotificationDestinationResponse>? secondCreate = null;
            try
            {
                await lockInterceptor.FirstLockAcquired.WaitAsync(TimeSpan.FromSeconds(10));
                secondCreate = CreateService(secondContext).CreateAsync(
                    userId,
                    CreateRequest("second"),
                    CancellationToken.None
                );
                await lockInterceptor.SecondLockAttempted.WaitAsync(TimeSpan.FromSeconds(10));
                secondCreate.IsCompleted.Should().BeFalse();
            }
            finally
            {
                lockInterceptor.ReleaseFirst();
            }

            await firstCreate.WaitAsync(TimeSpan.FromSeconds(10));
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await secondCreate!.WaitAsync(TimeSpan.FromSeconds(10));
            });

            await using var verify = new UserDataContext(options);
            (await verify.NotificationDestinations.CountAsync(d => d.UserID == userId))
                .Should()
                .Be(10);
        }
        finally
        {
            await using var dropSchema = new NpgsqlCommand(
                $"DROP SCHEMA \"{schema}\" CASCADE",
                connection
            );
            await dropSchema.ExecuteNonQueryAsync();
        }
    }

    private static NotificationDestinationRequest CreateRequest(string name) =>
        new()
        {
            Name = name,
            Url = "https://example.test/",
            EventTypes = [],
        };

    private static NotificationDestinationService CreateService(TestHelper helper) =>
        CreateService(helper.UserDataContext);

    private static NotificationDestinationService CreateService(UserDataContext context)
    {
        var sender = new NotificationWebhookSender(
            Mock.Of<IHttpClientFactory>(),
            Mock.Of<ILogger<NotificationWebhookSender>>(),
            TestHelper.CreateMockLocalizer<ApiLogStrings>(),
            TestHelper.CreateMockLocalizer<ApiResponseStrings>()
        );
        return new NotificationDestinationService(
            context,
            sender,
            TestHelper.CreateMockLocalizer<ApiResponseStrings>()
        );
    }

    private sealed class DestinationLimitLockInterceptor : DbCommandInterceptor
    {
        private readonly TaskCompletionSource _firstLockAcquired = new(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        private readonly TaskCompletionSource _secondLockAttempted = new(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        private readonly TaskCompletionSource _releaseFirst = new(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        private int _lockAttempts;
        private int _lockExecutions;

        public Task FirstLockAcquired => _firstLockAcquired.Task;
        public Task SecondLockAttempted => _secondLockAttempted.Task;

        public void ReleaseFirst() => _releaseFirst.TrySetResult();

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default
        )
        {
            if (
                command.CommandText.Contains("pg_advisory_xact_lock", StringComparison.Ordinal)
                && Interlocked.Increment(ref _lockAttempts) == 2
            )
            {
                _secondLockAttempted.TrySetResult();
            }
            return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override async ValueTask<int> NonQueryExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result,
            CancellationToken cancellationToken = default
        )
        {
            if (
                command.CommandText.Contains("pg_advisory_xact_lock", StringComparison.Ordinal)
                && Interlocked.Increment(ref _lockExecutions) == 1
            )
            {
                _firstLockAcquired.TrySetResult();
                await _releaseFirst.Task.WaitAsync(cancellationToken);
            }
            return await base.NonQueryExecutedAsync(
                command,
                eventData,
                result,
                cancellationToken
            );
        }
    }
}
