using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarinaRegSystem.Migrations
{
    /// <inheritdoc />
    public partial class Add_StockInvoiceTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockInvoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockInvoices_cUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "cUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockInvoiceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockInvoiceId = table.Column<int>(type: "int", nullable: false),
                    LabTestId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockInvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockInvoiceItems_LabTests_LabTestId",
                        column: x => x.LabTestId,
                        principalTable: "LabTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockInvoiceItems_StockInvoices_StockInvoiceId",
                        column: x => x.StockInvoiceId,
                        principalTable: "StockInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockInvoiceItems_LabTestId",
                table: "StockInvoiceItems",
                column: "LabTestId");

            migrationBuilder.CreateIndex(
                name: "IX_StockInvoiceItems_StockInvoiceId",
                table: "StockInvoiceItems",
                column: "StockInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_StockInvoices_CreatedByUserId",
                table: "StockInvoices",
                column: "CreatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockInvoiceItems");

            migrationBuilder.DropTable(
                name: "StockInvoices");
        }
    }
}
