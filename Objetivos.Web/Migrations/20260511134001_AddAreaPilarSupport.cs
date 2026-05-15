using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Objetivos.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAreaPilarSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsGlobal",
                table: "Pilares",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AreaPilares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AreaId = table.Column<int>(type: "INTEGER", nullable: false),
                    PilarId = table.Column<int>(type: "INTEGER", nullable: false),
                    PesoPorcentual = table.Column<decimal>(type: "TEXT", nullable: false),
                    EsObligatorio = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AreaPilares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AreaPilares_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AreaPilares_Pilares_PilarId",
                        column: x => x.PilarId,
                        principalTable: "Pilares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AreaPilares_AreaId_PilarId",
                table: "AreaPilares",
                columns: new[] { "AreaId", "PilarId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AreaPilares_PilarId",
                table: "AreaPilares",
                column: "PilarId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AreaPilares");

            migrationBuilder.DropColumn(
                name: "EsGlobal",
                table: "Pilares");
        }
    }
}
