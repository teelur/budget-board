using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.WebAPI.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BudgetBoard.IntegrationTests;

public sealed class IdentityEmailUniquenessTests
{
    [Fact]
    public void UserModel_RequiresUniqueNormalizedEmail()
    {
        var options = new DbContextOptionsBuilder<UserDataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new UserDataContext(options);

        var emailIndex = context
            .Model.FindEntityType(typeof(ApplicationUser))!
            .GetIndexes()
            .Single(index =>
                index.GetDatabaseName() == UserDataContext.NormalizedEmailIndexName
            );

        emailIndex.IsUnique.Should().BeTrue();
    }

    [Fact]
    public void IsDuplicateEmailViolation_RecognizesEmailIndexUniqueViolation()
    {
        var exception = CreateUpdateException(
            PostgresErrorCodes.UniqueViolation,
            UserDataContext.NormalizedEmailIndexName
        );

        IdentityEmailUniqueness.IsDuplicateEmailViolation(exception).Should().BeTrue();
    }

    [Fact]
    public void IsDuplicateEmailViolation_IgnoresOtherDatabaseConstraints()
    {
        var exception = CreateUpdateException(
            PostgresErrorCodes.UniqueViolation,
            "UserNameIndex"
        );

        IdentityEmailUniqueness.IsDuplicateEmailViolation(exception).Should().BeFalse();
    }

    private static DbUpdateException CreateUpdateException(
        string sqlState,
        string constraintName
    ) =>
        new(
            "Database update failed.",
            new PostgresException(
                "Duplicate key.",
                "ERROR",
                "ERROR",
                sqlState,
                constraintName: constraintName
            )
        );
}
