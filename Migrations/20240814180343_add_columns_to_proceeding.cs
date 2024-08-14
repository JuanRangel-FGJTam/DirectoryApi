using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthApi.Migrations
{
    /// <inheritdoc />
    public partial class add_columns_to_proceeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "createdAt",
                table: "Proceedings",
                type: "datetime",
                nullable: false,
                defaultValueSql: "getDate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "denunciaId",
                table: "Proceedings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "observations",
                table: "Proceedings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updatedAt",
                table: "Proceedings",
                type: "datetime",
                nullable: false,
                defaultValueSql: "getDate()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "denunciaId",
                table: "Proceedings");

            migrationBuilder.DropColumn(
                name: "observations",
                table: "Proceedings");

            migrationBuilder.DropColumn(
                name: "updatedAt",
                table: "Proceedings");

            migrationBuilder.AlterColumn<DateTime>(
                name: "createdAt",
                table: "Proceedings",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "getDate()");
        }
    }
}
