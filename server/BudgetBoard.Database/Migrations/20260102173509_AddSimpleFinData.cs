using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSimpleFinData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SyncID",
                table: "Account");

            migrationBuilder.AddColumn<Guid>(
                name: "SimpleFinAccountId",
                table: "Account",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SimpleFinOrganization",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Domain = table.Column<string>(type: "text", nullable: true),
                    SimpleFinUrl = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "text", nullable: true),
                    SyncID = table.Column<string>(type: "text", nullable: true),
                    LastSync = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimpleFinOrganization", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SimpleFinOrganization_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SimpleFinAccount",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    SyncID = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false),
                    BalanceDate = table.Column<int>(type: "integer", nullable: false),
                    LastSync = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    LinkedAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimpleFinAccount", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SimpleFinAccount_Account_LinkedAccountId",
                        column: x => x.LinkedAccountId,
                        principalTable: "Account",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SimpleFinAccount_SimpleFinOrganization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "SimpleFinOrganization",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SimpleFinAccount_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SimpleFinAccount_LinkedAccountId",
                table: "SimpleFinAccount",
                column: "LinkedAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SimpleFinAccount_OrganizationId",
                table: "SimpleFinAccount",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_SimpleFinAccount_UserID",
                table: "SimpleFinAccount",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_SimpleFinOrganization_UserID",
                table: "SimpleFinOrganization",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SimpleFinAccount");

            migrationBuilder.DropTable(
                name: "SimpleFinOrganization");

            migrationBuilder.DropColumn(
                name: "SimpleFinAccountId",
                table: "Account");

            migrationBuilder.AddColumn<string>(
                name: "SyncID",
                table: "Account",
                type: "text",
                nullable: true);
        }
    }
}
