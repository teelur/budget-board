using BudgetBoard.Database.Data;
using BudgetBoard.Service.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Npgsql;

namespace BudgetBoard.WebAPI.Services;

public sealed class DatabaseMigrationRunner(
    ILogger<DatabaseMigrationRunner> logger,
    IStringLocalizer<LogStrings> logLocalizer
)
{
    private const string InvalidPasswordSqlState = "28P01";

    public async Task<bool> RunAsync(
        UserDataContext dbContext,
        CancellationToken cancellationToken = default
    )
    {
        return await RunAsync(dbContext.Database.MigrateAsync, cancellationToken);
    }

    public async Task<bool> RunAsync(
        Func<CancellationToken, Task> migrateAsync,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await migrateAsync(cancellationToken);
            logger.LogInformation("{LogMessage}", logLocalizer["DatabaseMigrationCompletedLog"]);
            return true;
        }
        catch (Exception exception) when (IsInvalidPasswordException(exception))
        {
            logger.LogCritical(
                "{LogMessage}",
                logLocalizer["DatabaseMigrationInvalidCredentialsLog"]
            );
            return false;
        }
        catch (Exception exception)
        {
            logger.LogCritical(
                exception,
                "{LogMessage}",
                logLocalizer["DatabaseMigrationFailedLog"]
            );
            return false;
        }
    }

    private static bool IsInvalidPasswordException(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException { SqlState: InvalidPasswordSqlState })
            {
                return true;
            }
        }

        return false;
    }
}
