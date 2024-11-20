using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetApiCars.Migrations
{
    /// <inheritdoc />
    public partial class NameSurnameEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Rents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Rents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Surname",
                table: "Rents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Rents");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Rents");

            migrationBuilder.DropColumn(
                name: "Surname",
                table: "Rents");
        }
    }
}
