using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Objetivos.Web.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyPilarAreaRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AreaPilares");

            migrationBuilder.RenameColumn(
                name: "EsGlobal",
                table: "Pilares",
                newName: "EsObligatorio");

            migrationBuilder.AddColumn<int>(
                name: "AreaId",
                table: "Pilares",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PesoPorcentual",
                table: "Pilares",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Pilares_AreaId",
                table: "Pilares",
                column: "AreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pilares_Areas_AreaId",
                table: "Pilares",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pilares_Areas_AreaId",
                table: "Pilares");

            migrationBuilder.DropIndex(
                name: "IX_Pilares_AreaId",
                table: "Pilares");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "Pilares");

            migrationBuilder.DropColumn(
                name: "PesoPorcentual",
                table: "Pilares");

            migrationBuilder.RenameColumn(
                name: "EsObligatorio",
                table: "Pilares",
                newName: "EsGlobal");

            migrationBuilder.CreateTable(
                name: "AreaPilares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AreaId = table.Column<int>(type: "INTEGER", nullable: false),
                    PilarId = table.Column<int>(type: "INTEGER", nullable: false),
                    EsObligatorio = table.Column<bool>(type: "INTEGER", nullable: false),
                    PesoPorcentual = table.Column<decimal>(type: "TEXT", nullable: false)
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
    }
}
