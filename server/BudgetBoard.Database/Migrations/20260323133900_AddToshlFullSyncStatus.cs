using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddToshlFullSyncStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ToshlFullSyncCompletedAt",
                table: "UserSettings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToshlFullSyncError",
                table: "UserSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ToshlFullSyncQueuedAt",
                table: "UserSettings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ToshlFullSyncStartedAt",
                table: "UserSettings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToshlFullSyncStatus",
                table: "UserSettings",
                type: "text",
                nullable: false,
                defaultValue: "idle");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ToshlFullSyncCompletedAt",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ToshlFullSyncError",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ToshlFullSyncQueuedAt",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ToshlFullSyncStartedAt",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ToshlFullSyncStatus",
                table: "UserSettings");
        }
    }
}
