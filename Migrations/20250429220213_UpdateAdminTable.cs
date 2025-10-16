using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoctorAppointmentSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Role",
                table: "Admins",
                newName: "Password");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Admins",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Admins");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Admins",
                newName: "Role");
        }
    }
}
