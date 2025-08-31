using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateToAutomaticRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutomaticCategorizationRule");

            migrationBuilder.CreateTable(
                name: "AutomaticRule",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomaticRule", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AutomaticRule_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RuleParameter",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Field = table.Column<string>(type: "text", nullable: false),
                    Operator = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    RuleID = table.Column<Guid>(type: "uuid", nullable: false),
                    ParameterKind = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleParameter", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RuleParameter_AutomaticRule_RuleID",
                        column: x => x.RuleID,
                        principalTable: "AutomaticRule",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticRule_UserID",
                table: "AutomaticRule",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_RuleParameter_RuleID",
                table: "RuleParameter",
                column: "RuleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RuleParameter");

            migrationBuilder.DropTable(
                name: "AutomaticRule");

            migrationBuilder.CreateTable(
                name: "AutomaticCategorizationRule",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    CategorizationRule = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomaticCategorizationRule", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AutomaticCategorizationRule_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticCategorizationRule_UserID",
                table: "AutomaticCategorizationRule",
                column: "UserID");
        }
    }
}
