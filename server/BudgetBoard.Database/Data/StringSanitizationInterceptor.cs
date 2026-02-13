using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BudgetBoard.Database.Data;

/// <summary>
/// EF Core interceptor that automatically removes null bytes from all string properties
/// before saving to prevent PostgreSQL UTF8 encoding errors (22021).
/// </summary>
public class StringSanitizationInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        SanitizeStrings(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        SanitizeStrings(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void SanitizeStrings(DbContext? context)
    {
        if (context == null)
            return;

        var entries = context
            .ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            foreach (var property in entry.Properties)
            {
                if (property.CurrentValue is string stringValue && stringValue.Contains('\0'))
                {
                    property.CurrentValue = stringValue.Replace("\0", string.Empty);
                }
            }
        }
    }
}
