using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthApi.Migrations
{
    /// <inheritdoc />
    public partial class add_password_for_people : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "password",
                table: "People",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.RenameColumn("birthDate", "People", "birthdate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("password", "People");
            migrationBuilder.RenameColumn("birthdate", "People", "birthDate");
        }
    }
}
