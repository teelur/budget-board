using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations;

public partial class NormalizeUsernameOnlyEmailConfirmation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE "User"
            SET "EmailConfirmed" = FALSE
            WHERE "Email" IS NULL OR BTRIM("Email") = '';
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE "User"
            SET "EmailConfirmed" = TRUE
            WHERE "Email" IS NULL OR BTRIM("Email") = '';
            """
        );
    }
}
