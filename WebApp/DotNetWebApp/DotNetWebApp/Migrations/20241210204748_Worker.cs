using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetWebApp.Migrations
{
    /// <inheritdoc />
    public partial class Worker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Worker",
                schema: "Identity",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Worker",
                schema: "Identity",
                table: "User");
        }
    }
}
