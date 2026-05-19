using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DisableBuiltInAssetTypes",
                table: "UserSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Asset",
                type: "text",
                nullable: true
            );

            migrationBuilder.CreateTable(
                name: "AssetType",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Parent = table.Column<string>(type: "text", nullable: false),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetType", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AssetType_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_AssetType_UserID",
                table: "AssetType",
                column: "UserID"
            );

            // Null out Type for all soft-deleted assets
            migrationBuilder.Sql(
                """
                UPDATE "Asset"
                SET "Type" = NULL
                WHERE "Deleted" IS NOT NULL;
                """
            );

            // Null out Type for all soft-deleted accounts
            migrationBuilder.Sql(
                """
                UPDATE "Account"
                SET "Type" = NULL
                WHERE "Deleted" IS NOT NULL;
                """
            );

            // Null out Category and Subcategory for all soft-deleted transactions
            migrationBuilder.Sql(
                """
                UPDATE "Transaction"
                SET "Category" = NULL, "Subcategory" = NULL
                WHERE "Deleted" IS NOT NULL;
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AssetType");

            migrationBuilder.DropColumn(name: "DisableBuiltInAssetTypes", table: "UserSettings");

            migrationBuilder.DropColumn(name: "Type", table: "Asset");
        }
    }
}
