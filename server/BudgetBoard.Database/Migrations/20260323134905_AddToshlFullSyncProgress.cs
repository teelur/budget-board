using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddToshlFullSyncProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ToshlFullSyncProgressDescription",
                table: "UserSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ToshlFullSyncProgressPercent",
                table: "UserSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ToshlFullSyncProgressDescription",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ToshlFullSyncProgressPercent",
                table: "UserSettings");
        }
    }
}
