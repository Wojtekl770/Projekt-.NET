using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetApiCars.Migrations
{
    /// <inheritdoc />
    public partial class RentState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RentState",
                table: "Rents",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RentState",
                table: "Rents");
        }
    }
}
