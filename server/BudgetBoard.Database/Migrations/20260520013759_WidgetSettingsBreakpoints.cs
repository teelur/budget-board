using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class WidgetSettingsBreakpoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "X", table: "WidgetSettings", newName: "LgX");

            migrationBuilder.RenameColumn(name: "Y", table: "WidgetSettings", newName: "LgY");

            migrationBuilder.RenameColumn(name: "W", table: "WidgetSettings", newName: "LgW");

            migrationBuilder.RenameColumn(name: "H", table: "WidgetSettings", newName: "LgH");

            migrationBuilder.AddColumn<int>(
                name: "SmY",
                table: "WidgetSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<int>(
                name: "SmH",
                table: "WidgetSettings",
                type: "integer",
                nullable: false,
                defaultValue: 5
            );

            // Initialise small-screen positions from large-screen values for any existing rows.
            migrationBuilder.Sql(
                "UPDATE \"WidgetSettings\" SET \"SmY\" = \"LgY\", \"SmH\" = \"LgH\";"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "SmY", table: "WidgetSettings");

            migrationBuilder.DropColumn(name: "SmH", table: "WidgetSettings");

            migrationBuilder.RenameColumn(name: "LgX", table: "WidgetSettings", newName: "X");

            migrationBuilder.RenameColumn(name: "LgY", table: "WidgetSettings", newName: "Y");

            migrationBuilder.RenameColumn(name: "LgW", table: "WidgetSettings", newName: "W");

            migrationBuilder.RenameColumn(name: "LgH", table: "WidgetSettings", newName: "H");
        }
    }
}
