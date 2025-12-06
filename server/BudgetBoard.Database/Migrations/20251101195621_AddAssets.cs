using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Asset",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PurchasedDate = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    PurchasePrice = table.Column<decimal>(type: "numeric", nullable: true),
                    SoldDate = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    SoldPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    HideProperty = table.Column<bool>(type: "boolean", nullable: false),
                    Deleted = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Asset_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "Value",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    DateTime = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    AssetID = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Value", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Value_Asset_AssetID",
                        column: x => x.AssetID,
                        principalTable: "Asset",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(name: "IX_Asset_UserID", table: "Asset", column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Value_AssetID",
                table: "Value",
                column: "AssetID"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Value");

            migrationBuilder.DropTable(name: "Asset");
        }
    }
}
