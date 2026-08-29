using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class RecurringRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RecurringRuleID",
                table: "Transaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RecurringRule",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountID = table.Column<Guid>(type: "uuid", nullable: false),
                    MerchantName = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: true),
                    Subcategory = table.Column<string>(type: "text", nullable: true),
                    Cadence = table.Column<string>(type: "jsonb", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AmountMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringRule", x => x.ID);
                    table.CheckConstraint("CK_RecurringRule_Cadence_IsObject", "jsonb_typeof(\"Cadence\") = 'object'");
                    table.ForeignKey(
                        name: "FK_RecurringRule_Account_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Account",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecurringRule_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_RecurringRuleID",
                table: "Transaction",
                column: "RecurringRuleID");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringRule_AccountID",
                table: "RecurringRule",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringRule_UserID_IsActive",
                table: "RecurringRule",
                columns: new[] { "UserID", "IsActive" });

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_RecurringRule_RecurringRuleID",
                table: "Transaction",
                column: "RecurringRuleID",
                principalTable: "RecurringRule",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_RecurringRule_RecurringRuleID",
                table: "Transaction");

            migrationBuilder.DropTable(
                name: "RecurringRule");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_RecurringRuleID",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "RecurringRuleID",
                table: "Transaction");
        }
    }
}
