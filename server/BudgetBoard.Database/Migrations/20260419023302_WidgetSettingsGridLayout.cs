using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class WidgetSettingsGridLayout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsVisible", table: "WidgetSettings");

            migrationBuilder.AddColumn<int>(
                name: "H",
                table: "WidgetSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<int>(
                name: "W",
                table: "WidgetSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<int>(
                name: "X",
                table: "WidgetSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<int>(
                name: "Y",
                table: "WidgetSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            // Set proper default grid coordinates for any pre-existing NetWorth widgets.
            migrationBuilder.Sql(
                "UPDATE \"WidgetSettings\" SET \"X\" = 0, \"Y\" = 5, \"W\" = 4, \"H\" = 5 WHERE \"WidgetType\" = 'NetWorth';"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "H", table: "WidgetSettings");

            migrationBuilder.DropColumn(name: "W", table: "WidgetSettings");

            migrationBuilder.DropColumn(name: "X", table: "WidgetSettings");

            migrationBuilder.DropColumn(name: "Y", table: "WidgetSettings");

            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "WidgetSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );
        }
    }
}
