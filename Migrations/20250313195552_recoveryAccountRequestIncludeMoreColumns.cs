using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthApi.Migrations
{
    /// <inheritdoc />
    public partial class recoveryAccountRequestIncludeMoreColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "maritalStatusId",
                schema: "Recv",
                table: "AccountRecovery",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "occupationId",
                schema: "Recv",
                table: "AccountRecovery",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rfc",
                schema: "Recv",
                table: "AccountRecovery",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountRecovery_maritalStatusId",
                schema: "Recv",
                table: "AccountRecovery",
                column: "maritalStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountRecovery_occupationId",
                schema: "Recv",
                table: "AccountRecovery",
                column: "occupationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountRecovery_MaritalStatus_maritalStatusId",
                schema: "Recv",
                table: "AccountRecovery",
                column: "maritalStatusId",
                principalTable: "MaritalStatus",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountRecovery_Occupation_occupationId",
                schema: "Recv",
                table: "AccountRecovery",
                column: "occupationId",
                principalTable: "Occupation",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountRecovery_MaritalStatus_maritalStatusId",
                schema: "Recv",
                table: "AccountRecovery");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountRecovery_Occupation_occupationId",
                schema: "Recv",
                table: "AccountRecovery");

            migrationBuilder.DropIndex(
                name: "IX_AccountRecovery_maritalStatusId",
                schema: "Recv",
                table: "AccountRecovery");

            migrationBuilder.DropIndex(
                name: "IX_AccountRecovery_occupationId",
                schema: "Recv",
                table: "AccountRecovery");

            migrationBuilder.DropColumn(
                name: "maritalStatusId",
                schema: "Recv",
                table: "AccountRecovery");

            migrationBuilder.DropColumn(
                name: "occupationId",
                schema: "Recv",
                table: "AccountRecovery");

            migrationBuilder.DropColumn(
                name: "rfc",
                schema: "Recv",
                table: "AccountRecovery");
        }
    }
}
