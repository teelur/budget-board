using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGoalParams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "MonthlyContribution",
                table: "Goal",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real"
            );

            migrationBuilder.AlterColumn<float>(
                name: "InitialAmount",
                table: "Goal",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real"
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompleteDate",
                table: "Goal",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "MonthlyContribution",
                table: "Goal",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<float>(
                name: "InitialAmount",
                table: "Goal",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompleteDate",
                table: "Goal",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true
            );
        }
    }
}
