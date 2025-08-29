using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategorizationRule",
                table: "AutomaticCategorizationRule");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "AutomaticCategorizationRule");

            migrationBuilder.CreateTable(
                name: "RuleParameter",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Field = table.Column<string>(type: "text", nullable: false),
                    Operator = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    RuleID = table.Column<Guid>(type: "uuid", nullable: false),
                    ParameterKind = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleParameter", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RuleParameter_AutomaticCategorizationRule_RuleID",
                        column: x => x.RuleID,
                        principalTable: "AutomaticCategorizationRule",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.AddColumn<string>(
                name: "CategorizationRule",
                table: "AutomaticCategorizationRule",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "AutomaticCategorizationRule",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
