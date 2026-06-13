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
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
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
