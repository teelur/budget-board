using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BudgetBoard.Database.Data;

public class UserDataContextFactory : IDesignTimeDbContextFactory<UserDataContext>
{
    public UserDataContext CreateDbContext(string[] args)
    {
        var postgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var postgresDatabase =
            Environment.GetEnvironmentVariable("POSTGRES_DATABASE") ?? "budgetboard";
        var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
        var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? string.Empty;
        var postgresPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";

        var connectionString =
            $"Host={postgresHost};Port={postgresPort};Database={postgresDatabase};Username={postgresUser};Password={postgresPassword}";

        var optionsBuilder = new DbContextOptionsBuilder<UserDataContext>();
        optionsBuilder.UseNpgsql(connectionString).AddInterceptors(new StringSanitizationInterceptor());

        return new UserDataContext(optionsBuilder.Options);
    }
}
