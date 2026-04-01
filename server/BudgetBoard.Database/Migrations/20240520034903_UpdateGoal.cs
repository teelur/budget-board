using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGoal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "InitialAmount",
                table: "Goal",
                type: "real",
                nullable: false,
                defaultValue: 0f
            );

            migrationBuilder.CreateTable(
                name: "AccountGoal",
                columns: table => new
                {
                    AccountsID = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalsID = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountGoal", x => new { x.AccountsID, x.GoalsID });
                    table.ForeignKey(
                        name: "FK_AccountGoal_Account_AccountsID",
                        column: x => x.AccountsID,
                        principalTable: "Account",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_AccountGoal_Goal_GoalsID",
                        column: x => x.GoalsID,
                        principalTable: "Goal",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_AccountGoal_GoalsID",
                table: "AccountGoal",
                column: "GoalsID"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AccountGoal");

            migrationBuilder.DropColumn(name: "InitialAmount", table: "Goal");
        }
    }
}
