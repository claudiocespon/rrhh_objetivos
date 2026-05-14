using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Objetivos.Web.Migrations
{
    /// <inheritdoc />
    public partial class Stage0IntegrityFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PesoPorcentual",
                table: "Pilares");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PesoPorcentual",
                table: "Pilares",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
