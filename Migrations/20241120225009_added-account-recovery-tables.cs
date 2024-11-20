using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuthApi.Migrations
{
    /// <inheritdoc />
    public partial class addedaccountrecoverytables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Recv");

            migrationBuilder.CreateTable(
                name: "AccountRecovery",
                schema: "Recv",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    personId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    firstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    birthDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    genderId = table.Column<int>(type: "int", nullable: true),
                    curp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nationalityId = table.Column<int>(type: "int", nullable: true),
                    contactEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    contactEmail2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    contactPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    contactPhone2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    requestComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    responseComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getDate()"),
                    attendingAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    finishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountRecovery", x => x.id);
                    table.ForeignKey(
                        name: "FK_AccountRecovery_Gender_genderId",
                        column: x => x.genderId,
                        principalTable: "Gender",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_AccountRecovery_Nationality_nationalityId",
                        column: x => x.nationalityId,
                        principalTable: "Nationality",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getDate()"),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getDate()"),
                    deletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AccountRecoveryFiles",
                schema: "Recv",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    fileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    filePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fileType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fileSize = table.Column<long>(type: "bigint", nullable: false),
                    accountRecoveryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    documentTypeId = table.Column<int>(type: "int", nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getDate()"),
                    deletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountRecoveryFiles", x => x.id);
                    table.ForeignKey(
                        name: "FK_AccountRecoveryFiles_AccountRecovery_accountRecoveryId",
                        column: x => x.accountRecoveryId,
                        principalSchema: "Recv",
                        principalTable: "AccountRecovery",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountRecoveryFiles_DocumentTypes_documentTypeId",
                        column: x => x.documentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                table: "DocumentTypes",
                columns: new[] { "id", "deletedAt", "name" },
                values: new object[,]
                {
                    { 1, null, "INE" },
                    { 2, null, "CURP" },
                    { 3, null, "Acta de nacimiento" },
                    { 4, null, "Pasaporte" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountRecovery_genderId",
                schema: "Recv",
                table: "AccountRecovery",
                column: "genderId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountRecovery_nationalityId",
                schema: "Recv",
                table: "AccountRecovery",
                column: "nationalityId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountRecoveryFiles_accountRecoveryId",
                schema: "Recv",
                table: "AccountRecoveryFiles",
                column: "accountRecoveryId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountRecoveryFiles_documentTypeId",
                schema: "Recv",
                table: "AccountRecoveryFiles",
                column: "documentTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountRecoveryFiles",
                schema: "Recv");

            migrationBuilder.DropTable(
                name: "AccountRecovery",
                schema: "Recv");

            migrationBuilder.DropTable(
                name: "DocumentTypes");
        }
    }
}
