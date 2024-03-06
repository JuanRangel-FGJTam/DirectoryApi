using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthApi.Migrations
{
    /// <inheritdoc />
    public partial class fix_columns_auto_calculated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "iD",
                table: "ContactInformations",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "iD",
                table: "Addresses",
                newName: "id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updatedAt",
                table: "People",
                type: "datetime",
                nullable: false,
                defaultValueSql: "getDate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldComputedColumnSql: "getDate()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "createdAt",
                table: "People",
                type: "datetime",
                nullable: false,
                defaultValueSql: "getDate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "getDate()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updatedAt",
                table: "ContactInformations",
                type: "datetime",
                nullable: false,
                defaultValueSql: "getDate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldComputedColumnSql: "getDate()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "createdAt",
                table: "ContactInformations",
                type: "datetime",
                nullable: false,
                defaultValueSql: "getDate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldComputedColumnSql: "getDate()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updatedAt",
                table: "Addresses",
                type: "datetime",
                nullable: false,
                defaultValueSql: "getDate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldComputedColumnSql: "getDate()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "createdAt",
                table: "Addresses",
                type: "datetime",
                nullable: false,
                defaultValueSql: "getDate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldComputedColumnSql: "getDate()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                table: "ContactInformations",
                newName: "iD");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Addresses",
                newName: "iD");

            migrationBuilder.AlterColumn<DateTime>(
                name: "createdAt",
                table: "People",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "getDate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "getDate()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updatedAt",
                table: "People",
                type: "datetime2",
                nullable: false,
                computedColumnSql: "getDate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "getDate()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updatedAt",
                table: "ContactInformations",
                type: "datetime2",
                nullable: false,
                computedColumnSql: "getDate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "getDate()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "createdAt",
                table: "ContactInformations",
                type: "datetime2",
                nullable: false,
                computedColumnSql: "getDate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "getDate()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updatedAt",
                table: "Addresses",
                type: "datetime2",
                nullable: false,
                computedColumnSql: "getDate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "getDate()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "createdAt",
                table: "Addresses",
                type: "datetime2",
                nullable: false,
                computedColumnSql: "getDate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "getDate()");
        }
    }
}
