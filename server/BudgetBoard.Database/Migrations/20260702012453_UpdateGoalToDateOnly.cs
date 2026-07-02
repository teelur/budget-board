using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGoalToDateOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "Completed",
                table: "Goal",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<DateOnly>(
                name: "CompleteDate",
                table: "Goal",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Completed",
                table: "Goal",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompleteDate",
                table: "Goal",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true
            );
        }
    }
}
