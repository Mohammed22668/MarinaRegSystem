using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarinaRegSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomFieldForStockEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockEntries_AspNetUsers_CreatedByUserId",
                table: "StockEntries");

            migrationBuilder.AlterColumn<long>(
                name: "CreatedByUserId",
                table: "StockEntries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "StockEntries",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockEntries_ApplicationUserId",
                table: "StockEntries",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockEntries_AspNetUsers_ApplicationUserId",
                table: "StockEntries",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockEntries_cUsers_CreatedByUserId",
                table: "StockEntries",
                column: "CreatedByUserId",
                principalTable: "cUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockEntries_AspNetUsers_ApplicationUserId",
                table: "StockEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_StockEntries_cUsers_CreatedByUserId",
                table: "StockEntries");

            migrationBuilder.DropIndex(
                name: "IX_StockEntries_ApplicationUserId",
                table: "StockEntries");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "StockEntries");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "StockEntries",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_StockEntries_AspNetUsers_CreatedByUserId",
                table: "StockEntries",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
