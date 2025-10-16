using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoctorAppointmentSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordToPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "Patients");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Patients",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "Patients");

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "Patients",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
