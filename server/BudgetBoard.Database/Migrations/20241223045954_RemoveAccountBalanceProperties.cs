using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAccountBalanceProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "BalanceDate", table: "Account");

            migrationBuilder.DropColumn(name: "CurrentBalance", table: "Account");

            migrationBuilder.AlterColumn<decimal>(
                name: "MonthlyContribution",
                table: "Goal",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<decimal>(
                name: "InitialAmount",
                table: "Goal",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Goal",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "MonthlyContribution",
                table: "Goal",
                type: "real",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<float>(
                name: "InitialAmount",
                table: "Goal",
                type: "real",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<float>(
                name: "Amount",
                table: "Goal",
                type: "real",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric"
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "BalanceDate",
                table: "Account",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<float>(
                name: "CurrentBalance",
                table: "Account",
                type: "real",
                nullable: false,
                defaultValue: 0f
            );
        }
    }
}
