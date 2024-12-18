using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetApiCars.Migrations
{
    /// <inheritdoc />
    public partial class ReadyToReturn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReadyToReturn",
                table: "Rents",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReadyToReturn",
                table: "Rents");
        }
    }
}
