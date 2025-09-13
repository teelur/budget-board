using BudgetBoard.Database.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCurrencyToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "UserSettings",
                type: "text",
                nullable: false,
                oldClrType: typeof(Currency),
                oldType: "currency");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Currency>(
                name: "Currency",
                table: "UserSettings",
                type: "currency",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
