using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class BudgetMonthDateOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add the new Month column
            migrationBuilder.AddColumn<DateOnly>(
                name: "Month",
                table: "Budget",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1)
            );

            // Migrate data from Date to Month: extract year/month and normalize to first day of month (UTC)
            migrationBuilder.Sql(
                @"UPDATE ""Budget"" SET ""Month"" = make_date(
                    EXTRACT(YEAR FROM (""Date"" AT TIME ZONE 'UTC'))::int,
                    EXTRACT(MONTH FROM (""Date"" AT TIME ZONE 'UTC'))::int,
                    1
                )"
            );

            // Drop the old Date column
            migrationBuilder.DropColumn(name: "Date", table: "Budget");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add the Date column back
            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Budget",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
            );

            // Migrate data from Month back to Date (as UTC timestamp at start of month)
            migrationBuilder.Sql(
                @"UPDATE ""Budget"" SET ""Date"" = (""Month""::timestamp AT TIME ZONE 'UTC')"
            );

            // Drop the Month column
            migrationBuilder.DropColumn(name: "Month", table: "Budget");
        }
    }
}
