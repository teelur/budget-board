using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddInstitution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Institution", table: "Account");

            migrationBuilder.AddColumn<Guid>(
                name: "InstitutionID",
                table: "Account",
                type: "uuid",
                nullable: true
            );

            migrationBuilder.CreateTable(
                name: "Institution",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Institution", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Institution_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Account_InstitutionID",
                table: "Account",
                column: "InstitutionID"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Institution_UserID",
                table: "Institution",
                column: "UserID"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Account_Institution_InstitutionID",
                table: "Account",
                column: "InstitutionID",
                principalTable: "Institution",
                principalColumn: "ID"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Account_Institution_InstitutionID",
                table: "Account"
            );

            migrationBuilder.DropTable(name: "Institution");

            migrationBuilder.DropIndex(name: "IX_Account_InstitutionID", table: "Account");

            migrationBuilder.DropColumn(name: "InstitutionID", table: "Account");

            migrationBuilder.AddColumn<string>(
                name: "Institution",
                table: "Account",
                type: "text",
                nullable: false,
                defaultValue: ""
            );
        }
    }
}
