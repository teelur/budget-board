using BudgetBoard.Database.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BudgetBoard.WebAPI.Services;

public static class IdentityEmailUniqueness
{
    public static bool IsDuplicateEmailViolation(DbUpdateException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        for (Exception? cause = exception; cause is not null; cause = cause.InnerException)
        {
            if (
                cause is PostgresException
                {
                    SqlState: PostgresErrorCodes.UniqueViolation,
                    ConstraintName: UserDataContext.NormalizedEmailIndexName,
                }
            )
            {
                return true;
            }
        }

        return false;
    }
}
