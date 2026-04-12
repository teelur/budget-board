using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class BalanceDateOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Delete duplicate balances per (AccountID, date), keeping the one with the latest DateTime.
            migrationBuilder.Sql(
                """
                DELETE FROM "Balance"
                WHERE "ID" NOT IN (
                    SELECT DISTINCT ON ("AccountID", DATE("DateTime")) "ID"
                    FROM "Balance"
                    ORDER BY "AccountID", DATE("DateTime"), "DateTime" DESC
                );
                """
            );

            migrationBuilder.DropColumn(
                name: "DateTime",
                table: "Balance");

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "Balance",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "Balance");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTime",
                table: "Balance",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
