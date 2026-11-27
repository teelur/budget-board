using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LargeTransactionThreshold",
                table: "UserSettings",
                type: "numeric",
                nullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "RecurringReminderLeadTimeDays",
                table: "UserSettings",
                type: "integer",
                nullable: false,
                defaultValue: 3
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "LargeTransactionThreshold", table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "RecurringReminderLeadTimeDays",
                table: "UserSettings"
            );
        }
    }
}
