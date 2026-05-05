using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryTypeToTransactionCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoryType",
                table: "TransactionCategory",
                type: "text",
                nullable: true
            );

            migrationBuilder.Sql(
                "UPDATE \"TransactionCategory\" SET \"CategoryType\" = 'expense' WHERE \"CategoryType\" IS NULL;"
            );

            migrationBuilder.AlterColumn<string>(
                name: "CategoryType",
                table: "TransactionCategory",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CategoryType", table: "TransactionCategory");
        }
    }
}
