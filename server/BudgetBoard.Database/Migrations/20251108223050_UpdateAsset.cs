using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAsset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SoldPrice",
                table: "Asset",
                newName: "SellPrice");

            migrationBuilder.RenameColumn(
                name: "SoldDate",
                table: "Asset",
                newName: "SellDate");

            migrationBuilder.RenameColumn(
                name: "PurchasedDate",
                table: "Asset",
                newName: "PurchaseDate");

            migrationBuilder.RenameColumn(
                name: "HideProperty",
                table: "Asset",
                newName: "Hide");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SellPrice",
                table: "Asset",
                newName: "SoldPrice");

            migrationBuilder.RenameColumn(
                name: "SellDate",
                table: "Asset",
                newName: "SoldDate");

            migrationBuilder.RenameColumn(
                name: "PurchaseDate",
                table: "Asset",
                newName: "PurchasedDate");

            migrationBuilder.RenameColumn(
                name: "Hide",
                table: "Asset",
                newName: "HideProperty");
        }
    }
}
