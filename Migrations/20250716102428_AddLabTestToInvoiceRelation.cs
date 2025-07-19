using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarinaRegSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddLabTestToInvoiceRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LabTestId1",
                table: "LabInvoiceTests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabInvoiceTests_LabTestId1",
                table: "LabInvoiceTests",
                column: "LabTestId1");

            migrationBuilder.AddForeignKey(
                name: "FK_LabInvoiceTests_LabTests_LabTestId1",
                table: "LabInvoiceTests",
                column: "LabTestId1",
                principalTable: "LabTests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabInvoiceTests_LabTests_LabTestId1",
                table: "LabInvoiceTests");

            migrationBuilder.DropIndex(
                name: "IX_LabInvoiceTests_LabTestId1",
                table: "LabInvoiceTests");

            migrationBuilder.DropColumn(
                name: "LabTestId1",
                table: "LabInvoiceTests");
        }
    }
}
