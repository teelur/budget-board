using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class TransactionLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransactionLink",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceTransactionID = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetTransactionID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionLink", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TransactionLink_Transaction_SourceTransactionID",
                        column: x => x.SourceTransactionID,
                        principalTable: "Transaction",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionLink_Transaction_TargetTransactionID",
                        column: x => x.TargetTransactionID,
                        principalTable: "Transaction",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionLink_SourceTransactionID",
                table: "TransactionLink",
                column: "SourceTransactionID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionLink_TargetTransactionID",
                table: "TransactionLink",
                column: "TargetTransactionID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionLink");
        }
    }
}
