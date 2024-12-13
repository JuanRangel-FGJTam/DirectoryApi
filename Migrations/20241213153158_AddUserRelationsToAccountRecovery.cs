using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRelationsToAccountRecovery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "finishedAt",
                schema: "Recv",
                table: "AccountRecovery");

            migrationBuilder.AddColumn<int>(
                name: "attendingBy",
                schema: "Recv",
                table: "AccountRecovery",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "deletedBy",
                schema: "Recv",
                table: "AccountRecovery",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountRecovery_attendingBy",
                schema: "Recv",
                table: "AccountRecovery",
                column: "attendingBy");

            migrationBuilder.CreateIndex(
                name: "IX_AccountRecovery_deletedBy",
                schema: "Recv",
                table: "AccountRecovery",
                column: "deletedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountRecovery_Users_attendingBy",
                schema: "Recv",
                table: "AccountRecovery",
                column: "attendingBy",
                principalSchema: "System",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountRecovery_Users_deletedBy",
                schema: "Recv",
                table: "AccountRecovery",
                column: "deletedBy",
                principalSchema: "System",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountRecovery_Users_attendingBy",
                schema: "Recv",
                table: "AccountRecovery");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountRecovery_Users_deletedBy",
                schema: "Recv",
                table: "AccountRecovery");

            migrationBuilder.DropIndex(
                name: "IX_AccountRecovery_attendingBy",
                schema: "Recv",
                table: "AccountRecovery");

            migrationBuilder.DropIndex(
                name: "IX_AccountRecovery_deletedBy",
                schema: "Recv",
                table: "AccountRecovery");

            migrationBuilder.DropColumn(
                name: "attendingBy",
                schema: "Recv",
                table: "AccountRecovery");

            migrationBuilder.DropColumn(
                name: "deletedBy",
                schema: "Recv",
                table: "AccountRecovery");

            migrationBuilder.AddColumn<DateTime>(
                name: "finishedAt",
                schema: "Recv",
                table: "AccountRecovery",
                type: "datetime2",
                nullable: true);
        }
    }
}
