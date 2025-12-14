using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class FixBudget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Deleted", table: "Budget");

            migrationBuilder.RenameColumn(name: "Created", table: "Budget", newName: "Date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "Date", table: "Budget", newName: "Created");

            migrationBuilder.AddColumn<DateTime>(
                name: "Deleted",
                table: "Budget",
                type: "timestamp with time zone",
                nullable: true
            );
        }
    }
}
