using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthApi.Migrations
{
    /// <inheritdoc />
    public partial class upadte_person_and_preregister_tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "date",
                table: "Preregistrations",
                newName: "validTo");

            migrationBuilder.RenameColumn(
                name: "birthDate",
                table: "People",
                newName: "birthdate");

            migrationBuilder.AddColumn<DateTime>(
                name: "createdAt",
                table: "Preregistrations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "rfc",
                table: "People",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "password",
                table: "People",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "validatedAt",
                table: "People",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "createdAt",
                table: "Preregistrations");

            migrationBuilder.DropColumn(
                name: "validatedAt",
                table: "People");

            migrationBuilder.RenameColumn(
                name: "validTo",
                table: "Preregistrations",
                newName: "date");

            migrationBuilder.RenameColumn(
                name: "birthdate",
                table: "People",
                newName: "birthDate");

            migrationBuilder.AlterColumn<string>(
                name: "rfc",
                table: "People",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
