using BudgetBoard.Database.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations;

[DbContext(typeof(UserDataContext))]
[Migration("20261024181335_RenameSpendingTrendsWidgetType")]
public partial class RenameSpendingTrendsWidgetType : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """UPDATE "WidgetSettings" SET "WidgetType" = 'TransactionTrends' WHERE "WidgetType" = 'SpendingTrends';"""
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """UPDATE "WidgetSettings" SET "WidgetType" = 'SpendingTrends' WHERE "WidgetType" = 'TransactionTrends';"""
        );
    }
}
