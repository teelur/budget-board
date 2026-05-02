using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class MergeAccountSubtypeIntoType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Promote Subtype into Type where Subtype is non-empty.
            migrationBuilder.Sql(
                "UPDATE \"Account\" SET \"Type\" = \"Subtype\" WHERE \"Subtype\" IS NOT NULL AND \"Subtype\" <> '';"
            );

            migrationBuilder.DropColumn(name: "Subtype", table: "Account");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Account",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Account",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "Subtype",
                table: "Account",
                type: "text",
                nullable: false,
                defaultValue: ""
            );
        }
    }
}
