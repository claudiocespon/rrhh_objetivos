using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Objetivos.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAreaEspecificaToObjetivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AreaEspecificaId",
                table: "Objetivos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PorcentajeArea",
                table: "Objetivos",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Objetivos_AreaEspecificaId",
                table: "Objetivos",
                column: "AreaEspecificaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Objetivos_Areas_AreaEspecificaId",
                table: "Objetivos",
                column: "AreaEspecificaId",
                principalTable: "Areas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Objetivos_Areas_AreaEspecificaId",
                table: "Objetivos");

            migrationBuilder.DropIndex(
                name: "IX_Objetivos_AreaEspecificaId",
                table: "Objetivos");

            migrationBuilder.DropColumn(
                name: "AreaEspecificaId",
                table: "Objetivos");

            migrationBuilder.DropColumn(
                name: "PorcentajeArea",
                table: "Objetivos");
        }
    }
}
