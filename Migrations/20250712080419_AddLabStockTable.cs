using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarinaRegSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddLabStockTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LabStocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LabTestId = table.Column<int>(type: "int", nullable: false),
                    QuantityAvailable = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabStocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabStocks_LabTests_LabTestId",
                        column: x => x.LabTestId,
                        principalTable: "LabTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabStocks_LabTestId",
                table: "LabStocks",
                column: "LabTestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabStocks");
        }
    }
}
