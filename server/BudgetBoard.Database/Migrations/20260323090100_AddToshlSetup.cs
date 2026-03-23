using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations;

public partial class AddToshlSetup : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ToshlAccessToken",
            table: "User",
            type: "text",
            nullable: false,
            defaultValue: string.Empty
        );

        migrationBuilder.AddColumn<DateTime>(
            name: "ToshlLastSync",
            table: "User",
            type: "timestamp with time zone",
            nullable: false,
            defaultValue: DateTime.MinValue
        );

        migrationBuilder.AddColumn<string>(
            name: "ToshlMetadataSyncDirection",
            table: "UserSettings",
            type: "text",
            nullable: false,
            defaultValue: "budgetboard"
        );

        migrationBuilder.AddColumn<int>(
            name: "ToshlAutoSyncIntervalHours",
            table: "UserSettings",
            type: "integer",
            nullable: false,
            defaultValue: 8
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ToshlAccessToken", table: "User");
        migrationBuilder.DropColumn(name: "ToshlLastSync", table: "User");
        migrationBuilder.DropColumn(name: "ToshlMetadataSyncDirection", table: "UserSettings");
        migrationBuilder.DropColumn(name: "ToshlAutoSyncIntervalHours", table: "UserSettings");
    }
}
