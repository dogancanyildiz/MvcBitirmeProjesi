using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcBitirmeProjesi.Migrations
{
    /// <inheritdoc />
    public partial class a105 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tc",
                table: "Users",
                newName: "TC");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "TC",
                table: "Users",
                newName: "Tc");
        }
    }
}
