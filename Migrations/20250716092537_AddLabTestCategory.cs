using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarinaRegSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddLabTestCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LabTestCategoryId",
                table: "LabTests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LabTestCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabTestCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabTests_LabTestCategoryId",
                table: "LabTests",
                column: "LabTestCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabTests_LabTestCategories_LabTestCategoryId",
                table: "LabTests",
                column: "LabTestCategoryId",
                principalTable: "LabTestCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabTests_LabTestCategories_LabTestCategoryId",
                table: "LabTests");

            migrationBuilder.DropTable(
                name: "LabTestCategories");

            migrationBuilder.DropIndex(
                name: "IX_LabTests_LabTestCategoryId",
                table: "LabTests");

            migrationBuilder.DropColumn(
                name: "LabTestCategoryId",
                table: "LabTests");
        }
    }
}
