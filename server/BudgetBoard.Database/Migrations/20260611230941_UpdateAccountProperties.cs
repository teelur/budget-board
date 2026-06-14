using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAccountProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Account_Institution_InstitutionID",
                table: "Account"
            );

            migrationBuilder.Sql("UPDATE \"Account\" SET \"Type\" = '' WHERE \"Type\" IS NULL;");

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

            migrationBuilder.Sql(
                "UPDATE \"Account\" SET \"InterestRate\" = 0 WHERE \"InterestRate\" IS NULL;"
            );

            // Backfill missing or orphaned InstitutionID values before making the FK required.
            // Orphaned accounts are assigned to a dedicated per-user "Orphans" institution.
            migrationBuilder.Sql(
                @"
                INSERT INTO ""Institution"" (""ID"", ""Name"", ""Index"", ""UserID"")
                SELECT
                    (
                        substr(md5(a.""UserID""::text || '-orphans-institution'), 1, 8) || '-' ||
                        substr(md5(a.""UserID""::text || '-orphans-institution'), 9, 4) || '-' ||
                        substr(md5(a.""UserID""::text || '-orphans-institution'), 13, 4) || '-' ||
                        substr(md5(a.""UserID""::text || '-orphans-institution'), 17, 4) || '-' ||
                        substr(md5(a.""UserID""::text || '-orphans-institution'), 21, 12)
                    )::uuid AS ""ID"",
                    'Orphans' AS ""Name"",
                    0 AS ""Index"",
                    a.""UserID""
                FROM ""Account"" a
                WHERE (a.""InstitutionID"" IS NULL
                    OR NOT EXISTS (
                        SELECT 1 FROM ""Institution"" i WHERE i.""ID"" = a.""InstitutionID""
                    ))
                    AND NOT EXISTS (
                        SELECT 1 FROM ""Institution"" i2
                        WHERE i2.""ID"" = (
                            (
                                substr(md5(a.""UserID""::text || '-orphans-institution'), 1, 8) || '-' ||
                                substr(md5(a.""UserID""::text || '-orphans-institution'), 9, 4) || '-' ||
                                substr(md5(a.""UserID""::text || '-orphans-institution'), 13, 4) || '-' ||
                                substr(md5(a.""UserID""::text || '-orphans-institution'), 17, 4) || '-' ||
                                substr(md5(a.""UserID""::text || '-orphans-institution'), 21, 12)
                            )::uuid
                        )
                    )
                GROUP BY a.""UserID"";

                UPDATE ""Institution"" i
                SET ""Deleted"" = COALESCE(i.""Deleted"", NOW())
                WHERE i.""ID"" IN (
                    SELECT DISTINCT (
                        (
                            substr(md5(a.""UserID""::text || '-orphans-institution'), 1, 8) || '-' ||
                            substr(md5(a.""UserID""::text || '-orphans-institution'), 9, 4) || '-' ||
                            substr(md5(a.""UserID""::text || '-orphans-institution'), 13, 4) || '-' ||
                            substr(md5(a.""UserID""::text || '-orphans-institution'), 17, 4) || '-' ||
                            substr(md5(a.""UserID""::text || '-orphans-institution'), 21, 12)
                        )::uuid
                    )
                    FROM ""Account"" a
                    WHERE a.""InstitutionID"" IS NULL
                        OR NOT EXISTS (
                            SELECT 1 FROM ""Institution"" i2 WHERE i2.""ID"" = a.""InstitutionID""
                        )
                );

                UPDATE ""Account"" a
                SET ""InstitutionID"" = (
                    (
                        substr(md5(a.""UserID""::text || '-orphans-institution'), 1, 8) || '-' ||
                        substr(md5(a.""UserID""::text || '-orphans-institution'), 9, 4) || '-' ||
                        substr(md5(a.""UserID""::text || '-orphans-institution'), 13, 4) || '-' ||
                        substr(md5(a.""UserID""::text || '-orphans-institution'), 17, 4) || '-' ||
                        substr(md5(a.""UserID""::text || '-orphans-institution'), 21, 12)
                    )::uuid
                ),
                    ""Deleted"" = COALESCE(a.""Deleted"", NOW())
                WHERE a.""InstitutionID"" IS NULL
                    OR NOT EXISTS (
                        SELECT 1 FROM ""Institution"" i2 WHERE i2.""ID"" = a.""InstitutionID""
                    );
                "
            );

            migrationBuilder.AlterColumn<decimal>(
                name: "InterestRate",
                table: "Account",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "InstitutionID",
                table: "Account",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Account_Institution_InstitutionID",
                table: "Account",
                column: "InstitutionID",
                principalTable: "Institution",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Account_Institution_InstitutionID",
                table: "Account"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Account",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text"
            );

            migrationBuilder.AlterColumn<decimal>(
                name: "InterestRate",
                table: "Account",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric"
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "InstitutionID",
                table: "Account",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Account_Institution_InstitutionID",
                table: "Account",
                column: "InstitutionID",
                principalTable: "Institution",
                principalColumn: "ID"
            );
        }
    }
}
