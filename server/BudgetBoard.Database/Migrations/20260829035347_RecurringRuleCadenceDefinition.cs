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

            migrationBuilder.AddCheckConstraint(
                name: "CK_RecurringRule_Cadence_IsObject",
                table: "RecurringRule",
                sql: "jsonb_typeof(\"Cadence\") = 'object'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RecurringRule_Cadence_IsObject",
                table: "RecurringRule");

            migrationBuilder.Sql(
                """
                ALTER TABLE "RecurringRule"
                ALTER COLUMN "Cadence" TYPE character varying(32)
                USING CASE
                    WHEN "Cadence" = '{"version":1,"unit":"Week","interval":1}'::jsonb THEN 'Weekly'
                    WHEN "Cadence" = '{"version":1,"unit":"Week","interval":2}'::jsonb THEN 'Biweekly'
                    WHEN "Cadence" = '{"version":1,"unit":"Month","interval":1}'::jsonb THEN 'Monthly'
                    WHEN "Cadence" = '{"version":1,"unit":"Year","interval":1}'::jsonb THEN 'Yearly'
                    ELSE 'Monthly'
                END;
                """
            );
        }
    }
}
