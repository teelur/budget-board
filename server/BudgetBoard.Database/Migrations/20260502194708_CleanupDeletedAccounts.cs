using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class CleanupDeletedAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Null out Type and reset Source to 'Manual' for all soft-deleted accounts
            migrationBuilder.Sql(
                """
                UPDATE "Account"
                SET "Type" = NULL, "Source" = 'Manual'
                WHERE "Deleted" IS NOT NULL;
                """
            );

            // Sever SimpleFIN links for deleted accounts
            migrationBuilder.Sql(
                """
                UPDATE "SimpleFinAccount"
                SET "LinkedAccountId" = NULL, "LastSync" = NULL
                WHERE "LinkedAccountId" IN (
                    SELECT "ID" FROM "Account" WHERE "Deleted" IS NOT NULL
                );
                """
            );

            // Sever LunchFlow links for deleted accounts
            migrationBuilder.Sql(
                """
                UPDATE "LunchFlowAccount"
                SET "LinkedAccountId" = NULL, "LastSync" = NULL
                WHERE "LinkedAccountId" IN (
                    SELECT "ID" FROM "Account" WHERE "Deleted" IS NOT NULL
                );
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data cleanup migrations are not reversible
        }
    }
}
