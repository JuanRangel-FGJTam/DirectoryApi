using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthApi.Migrations
{
    /// <inheritdoc />
    public partial class add_columns_validated_at : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "validatedAt",
                table: "People",
                type: "datetime",
                nullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "rfc",
                table: "People",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "validatedAt",
                table: "People"
            );
        }
    }
}
