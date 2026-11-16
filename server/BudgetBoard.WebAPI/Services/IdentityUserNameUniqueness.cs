using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BudgetBoard.WebAPI.Services;

public static class IdentityUserNameUniqueness
{
    public static bool IsDuplicateUserNameViolation(DbUpdateException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        for (Exception? cause = exception; cause is not null; cause = cause.InnerException)
        {
            if (
                cause is PostgresException
                {
                    SqlState: PostgresErrorCodes.UniqueViolation,
                    ConstraintName: "UserNameIndex",
                }
            )
            {
                return true;
            }
        }

        return false;
    }
}
