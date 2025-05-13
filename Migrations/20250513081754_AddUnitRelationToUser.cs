using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcBitirmeProjesi.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitRelationToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UnitId",
                table: "Users",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Units_UnitId",
                table: "Users",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Units_UnitId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_UnitId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
