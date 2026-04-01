using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddLunchFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LunchFlowApiKey",
                table: "User",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "LunchFlowAccount",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    InstitutionName = table.Column<string>(type: "text", nullable: false),
                    InstitutionLogo = table.Column<string>(type: "text", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    LinkedAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LunchFlowAccount", x => x.ID);
                    table.ForeignKey(
                        name: "FK_LunchFlowAccount_Account_LinkedAccountId",
                        column: x => x.LinkedAccountId,
                        principalTable: "Account",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_LunchFlowAccount_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LunchFlowAccount_LinkedAccountId",
                table: "LunchFlowAccount",
                column: "LinkedAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LunchFlowAccount_UserID",
                table: "LunchFlowAccount",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LunchFlowAccount");

            migrationBuilder.DropColumn(
                name: "LunchFlowApiKey",
                table: "User");
        }
    }
}
