using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class EnforceAccountSourceValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"Account\" ALTER COLUMN \"Source\" SET DEFAULT 'Manual';"
            );

            migrationBuilder.Sql(
                """
                UPDATE \"Account\"
                SET \"Source\" = 'Manual'
                WHERE \"Source\" IS NULL
                    OR \"Source\" = ''
                    OR \"Source\" NOT IN ('Manual', 'SimpleFIN', 'LunchFlow');
                """
            );

            migrationBuilder.AddCheckConstraint(
                name: "CK_Account_Source_Valid",
                table: "Account",
                sql: "\"Source\" IN ('Manual', 'SimpleFIN', 'LunchFlow')"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(name: "CK_Account_Source_Valid", table: "Account");
            migrationBuilder.Sql(
                "ALTER TABLE \"Account\" ALTER COLUMN \"Source\" SET DEFAULT ''; "
            );
        }
    }
}
