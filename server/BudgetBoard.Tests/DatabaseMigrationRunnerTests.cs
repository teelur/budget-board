using BudgetBoard.Service.Resources;
using BudgetBoard.WebAPI.Services;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;

namespace BudgetBoard.IntegrationTests;

public class DatabaseMigrationRunnerTests
{
    [Fact]
    public async Task RunAsync_WhenPasswordAuthenticationFails_ReturnsFalseWithoutRetrying()
    {
        var logger = new TestLogger<DatabaseMigrationRunner>();
        var logLocalizer = new Mock<IStringLocalizer<LogStrings>>();
        logLocalizer
            .Setup(localizer => localizer[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));
        var runner = new DatabaseMigrationRunner(logger, logLocalizer.Object);
        var attempts = 0;

        var result = await runner.RunAsync(_ =>
        {
            attempts++;
            return Task.FromException(
                new PostgresException("password authentication failed", "FATAL", "FATAL", "28P01")
            );
        });

        result.Should().BeFalse();
        attempts.Should().Be(1);
        logger
            .Messages.Should()
            .ContainSingle(message => message.Contains("DatabaseMigrationInvalidCredentialsLog"));
        logger.Messages.Should().NotContain(message => message.Contains("secret-password"));
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        )
        {
            Messages.Add(formatter(state, exception));
        }
    }
}
