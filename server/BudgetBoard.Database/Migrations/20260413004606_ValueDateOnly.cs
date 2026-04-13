using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class ValueDateOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Delete duplicate values per (AssetID, date), keeping the one with the latest DateTime.
            migrationBuilder.Sql(
                """
                DELETE FROM "Value"
                WHERE "ID" NOT IN (
                    SELECT DISTINCT ON ("AssetID", DATE("DateTime")) "ID"
                    FROM "Value"
                    ORDER BY "AssetID", DATE("DateTime"), "DateTime" DESC
                );
                """
            );

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "Value",
                type: "date",
                nullable: true
            );

            // Copy existing DateTime values into the new Date column.
            migrationBuilder.Sql(
                """
                UPDATE "Value" SET "Date" = "DateTime"::date;
                """
            );

            migrationBuilder.AlterColumn<DateOnly>(
                name: "Date",
                table: "Value",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true
            );

            migrationBuilder.DropColumn(name: "DateTime", table: "Value");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Date", table: "Value");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTime",
                table: "Value",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
            );
        }
    }
}
