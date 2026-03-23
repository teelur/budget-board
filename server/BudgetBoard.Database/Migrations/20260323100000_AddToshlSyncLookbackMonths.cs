using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations;

public partial class AddToshlSyncLookbackMonths : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "ToshlSyncLookbackMonths",
            table: "UserSettings",
            type: "integer",
            nullable: false,
            defaultValue: 0
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ToshlSyncLookbackMonths", table: "UserSettings");
    }
}
