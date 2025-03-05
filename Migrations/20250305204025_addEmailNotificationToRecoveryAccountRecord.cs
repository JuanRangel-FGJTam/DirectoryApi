using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthApi.Migrations
{
    /// <inheritdoc />
    public partial class addEmailNotificationToRecoveryAccountRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "notificationEmailContent",
                schema: "Recv",
                table: "AccountRecovery",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notificationEmailResponse",
                schema: "Recv",
                table: "AccountRecovery",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "notificationEmailContent",
                schema: "Recv",
                table: "AccountRecovery");

            migrationBuilder.DropColumn(
                name: "notificationEmailResponse",
                schema: "Recv",
                table: "AccountRecovery");
        }
    }
}
