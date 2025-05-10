using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcBitirmeProjesi.Migrations
{
    /// <inheritdoc />
    public partial class a104 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Products",
                newName: "Unit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "Products",
                newName: "Price");
        }
    }
}
