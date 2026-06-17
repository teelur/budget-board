using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class MakeAssetTypeRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "Asset"
                SET "Type" = ''
                WHERE "Type" IS NULL;
                """
            );

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Asset",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Asset",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text"
            );
        }
    }
}
