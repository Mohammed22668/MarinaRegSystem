using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarinaRegSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDepartmentStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Departments",
                newName: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Departments",
                newName: "IsActive");
        }
    }
}
