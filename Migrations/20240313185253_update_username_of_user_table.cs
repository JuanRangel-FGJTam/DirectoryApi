using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuthApi.Migrations
{
    /// <inheritdoc />
    public partial class update_username_of_user_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ContactTypes",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ContactTypes",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ContactTypes",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ContactTypes",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Gender",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Gender",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MaritalStatus",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MaritalStatus",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MaritalStatus",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MaritalStatus",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "MaritalStatus",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.RenameColumn(
                name: "username",
                schema: "System",
                table: "Users",
                newName: "email");

            migrationBuilder.UpdateData(
                schema: "System",
                table: "Users",
                keyColumn: "id",
                keyValue: 1,
                column: "email",
                value: "system@email.com");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "email",
                schema: "System",
                table: "Users",
                newName: "username");

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

            migrationBuilder.UpdateData(
                schema: "System",
                table: "Users",
                keyColumn: "id",
                keyValue: 1,
                column: "username",
                value: "system");
        }
    }
}
