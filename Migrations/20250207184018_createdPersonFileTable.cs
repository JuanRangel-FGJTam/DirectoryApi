using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthApi.Migrations
{
    /// <inheritdoc />
    public partial class createdPersonFileTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonFiles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    personId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    documentTypeId = table.Column<int>(type: "int", nullable: false),
                    fileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    filePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mimmeType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fileSize = table.Column<long>(type: "bigint", nullable: false),
                    validation = table.Column<DateTime>(type: "date", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getDate()"),
                    deletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonFiles", x => x.id);
                    table.ForeignKey(
                        name: "FK_PersonFiles_DocumentTypes_documentTypeId",
                        column: x => x.documentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonFiles_People_personId",
                        column: x => x.personId,
                        principalTable: "People",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonFiles_documentTypeId",
                table: "PersonFiles",
                column: "documentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonFiles_personId",
                table: "PersonFiles",
                column: "personId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonFiles");
        }
    }
}
