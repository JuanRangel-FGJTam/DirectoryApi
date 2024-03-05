using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuthApi.Migrations
{
    /// <inheritdoc />
    public partial class init_database : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactTypes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactTypes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    iSO = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phoneCode = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Gender",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gender", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "MaritalStatus",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaritalStatus", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Nationality",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nationality", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Occupation",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Occupation", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Preregistrations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    mail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preregistrations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "States",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    countryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States", x => x.id);
                    table.ForeignKey(
                        name: "FK_States_Countries_countryId",
                        column: x => x.countryId,
                        principalTable: "Countries",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    rfc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    curp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    firstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    birthDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getDate()"),
                    genderId = table.Column<int>(type: "int", nullable: true),
                    maritalStatusId = table.Column<int>(type: "int", nullable: true),
                    nationalityId = table.Column<int>(type: "int", nullable: true),
                    occupationId = table.Column<int>(type: "int", nullable: true),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, computedColumnSql: "getDate()"),
                    deletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.id);
                    table.ForeignKey(
                        name: "FK_People_Gender_genderId",
                        column: x => x.genderId,
                        principalTable: "Gender",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_People_MaritalStatus_maritalStatusId",
                        column: x => x.maritalStatusId,
                        principalTable: "MaritalStatus",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_People_Nationality_nationalityId",
                        column: x => x.nationalityId,
                        principalTable: "Nationality",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_People_Occupation_occupationId",
                        column: x => x.occupationId,
                        principalTable: "Occupation",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Municipalities",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    stateId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Municipalities", x => x.id);
                    table.ForeignKey(
                        name: "FK_Municipalities_States_stateId",
                        column: x => x.stateId,
                        principalTable: "States",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ContactInformations",
                columns: table => new
                {
                    iD = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    personId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    contactTypeId = table.Column<int>(type: "int", nullable: false),
                    value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false, computedColumnSql: "getDate()"),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, computedColumnSql: "getDate()"),
                    deletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactInformations", x => x.iD);
                    table.ForeignKey(
                        name: "FK_ContactInformations_ContactTypes_contactTypeId",
                        column: x => x.contactTypeId,
                        principalTable: "ContactTypes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContactInformations_People_personId",
                        column: x => x.personId,
                        principalTable: "People",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Colonies",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    zipCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    municipalityId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colonies", x => x.id);
                    table.ForeignKey(
                        name: "FK_Colonies_Municipalities_municipalityId",
                        column: x => x.municipalityId,
                        principalTable: "Municipalities",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    iD = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    personId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    countryId = table.Column<int>(type: "int", nullable: false),
                    stateId = table.Column<int>(type: "int", nullable: false),
                    municipalityId = table.Column<int>(type: "int", nullable: false),
                    colonyId = table.Column<int>(type: "int", nullable: true),
                    street = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    numberInside = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    zipCode = table.Column<int>(type: "int", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false, computedColumnSql: "getDate()"),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, computedColumnSql: "getDate()"),
                    deletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.iD);
                    table.ForeignKey(
                        name: "FK_Addresses_Colonies_colonyId",
                        column: x => x.colonyId,
                        principalTable: "Colonies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Addresses_Countries_countryId",
                        column: x => x.countryId,
                        principalTable: "Countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Addresses_Municipalities_municipalityId",
                        column: x => x.municipalityId,
                        principalTable: "Municipalities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Addresses_People_personId",
                        column: x => x.personId,
                        principalTable: "People",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Addresses_States_stateId",
                        column: x => x.stateId,
                        principalTable: "States",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ContactTypes",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "TELEFONO CELULAR" },
                    { 2, "TELEFONO DE CASA" },
                    { 3, "TELEFONO DE TRABAJO" },
                    { 4, "CORREO ELECTRONICO" }
                });

            migrationBuilder.InsertData(
                table: "Gender",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "Masculino" },
                    { 2, "Femenino" }
                });

            migrationBuilder.InsertData(
                table: "MaritalStatus",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "SOLTERO(A)" },
                    { 2, "CASADO(A)" },
                    { 3, "DIVORCIADO(A)" },
                    { 4, "UDO(A)" },
                    { 5, "UNION LIBRE" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_colonyId",
                table: "Addresses",
                column: "colonyId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_countryId",
                table: "Addresses",
                column: "countryId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_municipalityId",
                table: "Addresses",
                column: "municipalityId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_personId",
                table: "Addresses",
                column: "personId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_stateId",
                table: "Addresses",
                column: "stateId");

            migrationBuilder.CreateIndex(
                name: "IX_Colonies_municipalityId",
                table: "Colonies",
                column: "municipalityId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactInformations_contactTypeId",
                table: "ContactInformations",
                column: "contactTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactInformations_personId",
                table: "ContactInformations",
                column: "personId");

            migrationBuilder.CreateIndex(
                name: "IX_Municipalities_stateId",
                table: "Municipalities",
                column: "stateId");

            migrationBuilder.CreateIndex(
                name: "IX_People_genderId",
                table: "People",
                column: "genderId");

            migrationBuilder.CreateIndex(
                name: "IX_People_maritalStatusId",
                table: "People",
                column: "maritalStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_People_nationalityId",
                table: "People",
                column: "nationalityId");

            migrationBuilder.CreateIndex(
                name: "IX_People_occupationId",
                table: "People",
                column: "occupationId");

            migrationBuilder.CreateIndex(
                name: "IX_States_countryId",
                table: "States",
                column: "countryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "ContactInformations");

            migrationBuilder.DropTable(
                name: "Preregistrations");

            migrationBuilder.DropTable(
                name: "Colonies");

            migrationBuilder.DropTable(
                name: "ContactTypes");

            migrationBuilder.DropTable(
                name: "People");

            migrationBuilder.DropTable(
                name: "Municipalities");

            migrationBuilder.DropTable(
                name: "Gender");

            migrationBuilder.DropTable(
                name: "MaritalStatus");

            migrationBuilder.DropTable(
                name: "Nationality");

            migrationBuilder.DropTable(
                name: "Occupation");

            migrationBuilder.DropTable(
                name: "States");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
