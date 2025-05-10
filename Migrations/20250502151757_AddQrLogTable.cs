using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcBitirmeProjesi.Migrations
{
    /// <inheritdoc />
    public partial class AddQrLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QrLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QrValue = table.Column<string>(type: "TEXT", nullable: false),
                    GirisZamani = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CikisZamani = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GecenSure = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QrLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QrLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_QrLogs_UserId",
                table: "QrLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QrLogs");
        }
    }
}
