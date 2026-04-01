using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class LunchFlowUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "LunchFlowAccount",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "BalanceDate",
                table: "LunchFlowAccount",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSync",
                table: "LunchFlowAccount",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SyncID",
                table: "LunchFlowAccount",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Balance",
                table: "LunchFlowAccount");

            migrationBuilder.DropColumn(
                name: "BalanceDate",
                table: "LunchFlowAccount");

            migrationBuilder.DropColumn(
                name: "LastSync",
                table: "LunchFlowAccount");

            migrationBuilder.DropColumn(
                name: "SyncID",
                table: "LunchFlowAccount");
        }
    }
}
