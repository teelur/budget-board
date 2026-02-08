using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddMinimumProbability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AutoCategorizerMinimumProbabilityPercentage",
                table: "UserSettings",
                type: "integer",
                nullable: false,
                defaultValue: 70
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoCategorizerMinimumProbabilityPercentage",
                table: "UserSettings"
            );
        }
    }
}
