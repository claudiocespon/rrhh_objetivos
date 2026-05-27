using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Objetivos.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPuestoEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Puestos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActualizadoEn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Puestos", x => x.Id);
                });

            // MIGRACIÓN DE DATOS (OPCIÓN 1): Preservar puestos existentes
            migrationBuilder.Sql(@"
                INSERT INTO Puestos (Nombre, Descripcion, Activo, CreadoEn, ActualizadoEn)
                SELECT DISTINCT Puesto, Puesto, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP 
                FROM Empleados 
                WHERE Puesto IS NOT NULL AND Puesto != '';
            ");

            migrationBuilder.AddColumn<int>(
                name: "PuestoId",
                table: "Empleados",
                type: "INTEGER",
                nullable: true);

            // MIGRACIÓN DE DATOS: Asignar PuestoId
            migrationBuilder.Sql(@"
                UPDATE Empleados
                SET PuestoId = (SELECT Id FROM Puestos WHERE Puestos.Nombre = Empleados.Puesto)
                WHERE Puesto IS NOT NULL AND Puesto != '';
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_PuestoId",
                table: "Empleados",
                column: "PuestoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Empleados_Puestos_PuestoId",
                table: "Empleados",
                column: "PuestoId",
                principalTable: "Puestos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // Ahora sí, eliminamos la columna vieja de texto libre
            migrationBuilder.DropColumn(
                name: "Puesto",
                table: "Empleados");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Empleados_Puestos_PuestoId",
                table: "Empleados");

            migrationBuilder.DropTable(
                name: "Puestos");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_PuestoId",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "PuestoId",
                table: "Empleados");

            migrationBuilder.AddColumn<string>(
                name: "Puesto",
                table: "Empleados",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
