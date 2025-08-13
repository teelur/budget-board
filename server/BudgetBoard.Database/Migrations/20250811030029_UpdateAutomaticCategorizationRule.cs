using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAutomaticCategorizationRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AutomaticCategorizationRule_UserID",
                table: "AutomaticCategorizationRule",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_AutomaticCategorizationRule_User_UserID",
                table: "AutomaticCategorizationRule",
                column: "UserID",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AutomaticCategorizationRule_User_UserID",
                table: "AutomaticCategorizationRule");

            migrationBuilder.DropIndex(
                name: "IX_AutomaticCategorizationRule_UserID",
                table: "AutomaticCategorizationRule");
        }
    }
}
