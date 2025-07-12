using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarinaRegSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddLabInventoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LabInventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LabTestId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabInventory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabInventory_LabTests_LabTestId",
                        column: x => x.LabTestId,
                        principalTable: "LabTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabInventory_LabTestId",
                table: "LabInventory",
                column: "LabTestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabInventory");
        }
    }
}
