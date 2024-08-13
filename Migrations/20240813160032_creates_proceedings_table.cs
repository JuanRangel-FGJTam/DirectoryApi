using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthApi.Migrations
{
    /// <inheritdoc />
    public partial class creates_proceedings_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    disabled = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ProceedingStatuses",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    disabled = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProceedingStatuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Proceedings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    personId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    folio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    statusId = table.Column<int>(type: "int", nullable: true),
                    areaId = table.Column<int>(type: "int", nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proceedings", x => x.id);
                    table.ForeignKey(
                        name: "FK_Proceedings_Areas_areaId",
                        column: x => x.areaId,
                        principalTable: "Areas",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Proceedings_ProceedingStatuses_statusId",
                        column: x => x.statusId,
                        principalTable: "ProceedingStatuses",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Proceedings_areaId",
                table: "Proceedings",
                column: "areaId");

            migrationBuilder.CreateIndex(
                name: "IX_Proceedings_statusId",
                table: "Proceedings",
                column: "statusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Proceedings");

            migrationBuilder.DropTable(
                name: "Areas");

            migrationBuilder.DropTable(
                name: "ProceedingStatuses");
        }
    }
}
