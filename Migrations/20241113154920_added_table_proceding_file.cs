using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthApi.Migrations
{
    /// <inheritdoc />
    public partial class added_table_proceding_file : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProceedingFiles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    filePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fileType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fileSize = table.Column<long>(type: "bigint", nullable: false),
                    proceedingId = table.Column<int>(type: "int", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getDate()"),
                    updatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getDate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProceedingFiles", x => x.id);
                    table.ForeignKey(
                        name: "FK_ProceedingFiles_Proceedings_proceedingId",
                        column: x => x.proceedingId,
                        principalTable: "Proceedings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProceedingFiles_proceedingId",
                table: "ProceedingFiles",
                column: "proceedingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProceedingFiles");
        }
    }
}
