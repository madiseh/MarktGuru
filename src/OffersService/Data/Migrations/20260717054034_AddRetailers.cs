using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OffersService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRetailers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RetailerId",
                table: "Offers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Retailers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Country = table.Column<string>(type: "char(2)", fixedLength: true, maxLength: 2, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Retailers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Offers",
                keyColumn: "Id",
                keyValue: 1,
                column: "RetailerId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Offers",
                keyColumn: "Id",
                keyValue: 2,
                column: "RetailerId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Offers",
                keyColumn: "Id",
                keyValue: 3,
                column: "RetailerId",
                value: 1);

            migrationBuilder.InsertData(
                table: "Retailers",
                columns: new[] { "Id", "Country", "CreatedAt", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "AT", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Rewe" },
                    { 2, "AT", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Billa" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Offers_RetailerId",
                table: "Offers",
                column: "RetailerId");

            migrationBuilder.CreateIndex(
                name: "IX_Retailers_Name",
                table: "Retailers",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Retailers_RetailerId",
                table: "Offers",
                column: "RetailerId",
                principalTable: "Retailers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Retailers_RetailerId",
                table: "Offers");

            migrationBuilder.DropTable(
                name: "Retailers");

            migrationBuilder.DropIndex(
                name: "IX_Offers_RetailerId",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "RetailerId",
                table: "Offers");
        }
    }
}
