using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class EnforceAccountTypeClassification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"AccountType\" ALTER COLUMN \"Classification\" SET DEFAULT 'asset';"
            );

            migrationBuilder.Sql(
                """
                UPDATE "AccountType"
                SET "Classification" = 'asset'
                WHERE "Classification" IS NULL
                    OR "Classification" = ''
                    OR "Classification" NOT IN ('asset', 'liability');
                """
            );

            migrationBuilder.AddCheckConstraint(
                name: "CK_AccountType_Classification_Valid",
                table: "AccountType",
                sql: "\"Classification\" IN ('asset', 'liability')"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_AccountType_Classification_Valid",
                table: "AccountType"
            );

            migrationBuilder.Sql(
                "ALTER TABLE \"AccountType\" ALTER COLUMN \"Classification\" SET DEFAULT 'asset';"
            );
        }
    }
}
