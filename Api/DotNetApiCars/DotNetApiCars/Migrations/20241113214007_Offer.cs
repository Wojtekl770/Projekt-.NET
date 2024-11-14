using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetApiCars.Migrations
{
    /// <inheritdoc />
    public partial class Offer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rents_Cars_CarId",
                table: "Rents");

            migrationBuilder.RenameColumn(
                name: "CarId",
                table: "Rents",
                newName: "OfferId");

            migrationBuilder.RenameIndex(
                name: "IX_Rents_CarId",
                table: "Rents",
                newName: "IX_Rents_OfferId");

            migrationBuilder.CreateTable(
                name: "OffersDB",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PriceDay = table.Column<int>(type: "int", nullable: false),
                    PriceInsurance = table.Column<int>(type: "int", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WhenOfferWasMade = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CarId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OffersDB", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OffersDB_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OffersDB_CarId",
                table: "OffersDB",
                column: "CarId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rents_OffersDB_OfferId",
                table: "Rents",
                column: "OfferId",
                principalTable: "OffersDB",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rents_OffersDB_OfferId",
                table: "Rents");

            migrationBuilder.DropTable(
                name: "OffersDB");

            migrationBuilder.RenameColumn(
                name: "OfferId",
                table: "Rents",
                newName: "CarId");

            migrationBuilder.RenameIndex(
                name: "IX_Rents_OfferId",
                table: "Rents",
                newName: "IX_Rents_CarId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rents_Cars_CarId",
                table: "Rents",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
