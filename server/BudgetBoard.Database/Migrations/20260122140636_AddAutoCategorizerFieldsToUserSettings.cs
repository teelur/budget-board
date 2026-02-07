using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBoard.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoCategorizerFieldsToUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "AutoCategorizerLastTrained",
                table: "UserSettings",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "AutoCategorizerModelEndDate",
                table: "UserSettings",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AutoCategorizerModelOID",
                table: "UserSettings",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "AutoCategorizerModelStartDate",
                table: "UserSettings",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableAutoCategorizer",
                table: "UserSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoCategorizerLastTrained",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "AutoCategorizerModelEndDate",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "AutoCategorizerModelOID",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "AutoCategorizerModelStartDate",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "EnableAutoCategorizer",
                table: "UserSettings");
        }
    }
}
