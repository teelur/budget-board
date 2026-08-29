using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class RecurringRuleCadenceDefinition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "RecurringRule"
                ALTER COLUMN "Cadence" TYPE jsonb
                USING CASE "Cadence"
                    WHEN 'Weekly' THEN '{"version":1,"unit":"Week","interval":1}'::jsonb
                    WHEN 'Biweekly' THEN '{"version":1,"unit":"Week","interval":2}'::jsonb
                    WHEN 'Monthly' THEN '{"version":1,"unit":"Month","interval":1}'::jsonb
                    WHEN 'Yearly' THEN '{"version":1,"unit":"Year","interval":1}'::jsonb
                    ELSE "Cadence"::jsonb
                END;
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "RecurringRule"
                ALTER COLUMN "Cadence" TYPE character varying(32)
                USING CASE
                    WHEN "Cadence"->>'unit' = 'Week' AND ("Cadence"->>'interval')::integer = 1 THEN 'Weekly'
                    WHEN "Cadence"->>'unit' = 'Week' AND ("Cadence"->>'interval')::integer = 2 THEN 'Biweekly'
                    WHEN "Cadence"->>'unit' = 'Month' AND ("Cadence"->>'interval')::integer = 1 THEN 'Monthly'
                    WHEN "Cadence"->>'unit' = 'Year' AND ("Cadence"->>'interval')::integer = 1 THEN 'Yearly'
                    ELSE 'Monthly'
                END;
                """
            );
        }
    }
}
