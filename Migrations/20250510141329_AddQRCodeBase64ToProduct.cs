using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcBitirmeProjesi.Migrations
{
    /// <inheritdoc />
    public partial class AddQRCodeBase64ToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QRCodeBase64",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QRCodeBase64",
                table: "Products");
        }
    }
}
