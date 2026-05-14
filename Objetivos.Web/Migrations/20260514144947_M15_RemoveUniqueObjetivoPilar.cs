using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Objetivos.Web.Migrations
{
    /// <inheritdoc />
    public partial class M15_RemoveUniqueObjetivoPilar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Objetivos_PilarId_EmpleadoId_Anio",
                table: "Objetivos");

            migrationBuilder.CreateIndex(
                name: "IX_Objetivos_PilarId_EmpleadoId_Anio",
                table: "Objetivos",
                columns: new[] { "PilarId", "EmpleadoId", "Anio" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Objetivos_PilarId_EmpleadoId_Anio",
                table: "Objetivos");

            migrationBuilder.CreateIndex(
                name: "IX_Objetivos_PilarId_EmpleadoId_Anio",
                table: "Objetivos",
                columns: new[] { "PilarId", "EmpleadoId", "Anio" },
                unique: true);
        }
    }
}
