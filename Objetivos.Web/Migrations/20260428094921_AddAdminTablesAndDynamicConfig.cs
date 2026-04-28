using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Objetivos.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminTablesAndDynamicConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Crear tablas administrativas
            migrationBuilder.CreateTable(
                name: "EscalasValoracion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Etiqueta = table.Column<string>(type: "TEXT", nullable: false),
                    ValorNumerico = table.Column<decimal>(type: "REAL", nullable: true),
                    Orden = table.Column<int>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActualizadoEn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscalasValoracion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstadosObjetivoConfig",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    ColorHex = table.Column<string>(type: "TEXT", nullable: false),
                    Orden = table.Column<int>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosObjetivoConfig", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstadosEvaluacionConfig",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    ColorHex = table.Column<string>(type: "TEXT", nullable: false),
                    Orden = table.Column<int>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosEvaluacionConfig", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionesPlataforma",
                columns: table => new
                {
                    Clave = table.Column<string>(type: "TEXT", nullable: false),
                    Valor = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    ActualizadoEn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActualizadoPorId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesPlataforma", x => x.Clave);
                });

            // Agregar columnas a tablas existentes
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Pilares",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "Orden",
                table: "Pilares",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreadoEn",
                table: "Pilares",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 9, 49, 21, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualizadoEn",
                table: "Pilares",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 9, 49, 21, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "SoftSkills",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "Orden",
                table: "SoftSkills",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreadoEn",
                table: "SoftSkills",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 9, 49, 21, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualizadoEn",
                table: "SoftSkills",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 9, 49, 21, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Areas",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreadoEn",
                table: "Areas",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 9, 49, 21, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualizadoEn",
                table: "Areas",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 28, 9, 49, 21, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<decimal>(
                name: "PorcentajePilar",
                table: "Objetivos",
                type: "REAL",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "AprobadoPorJefe",
                table: "Objetivos",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "EstadoObjetivoConfigId",
                table: "Objetivos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EscalaValoracionId",
                table: "RevisionesCuatrimestrales",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstadoEvaluacionConfigId",
                table: "RevisionesCuatrimestrales",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoftSkill1EscalaValoracionId",
                table: "RevisionesCuatrimestrales",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoftSkill2EscalaValoracionId",
                table: "RevisionesCuatrimestrales",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EscalaValoracionIdFinal",
                table: "EvaluacionesFinales",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstadoEvaluacionConfigId",
                table: "EvaluacionesFinales",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoftSkill1EscalaValoracionId",
                table: "EvaluacionesFinales",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoftSkill2EscalaValoracionId",
                table: "EvaluacionesFinales",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EscalaValoracionIdScore",
                table: "Autoevaluaciones",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstadoEvaluacionConfigId",
                table: "Autoevaluaciones",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoftSkill1EscalaValoracionId",
                table: "Autoevaluaciones",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoftSkill2EscalaValoracionId",
                table: "Autoevaluaciones",
                type: "INTEGER",
                nullable: true);

            // Crear índices
            migrationBuilder.CreateIndex(
                name: "IX_EscalasValoracion_Orden",
                table: "EscalasValoracion",
                column: "Orden");

            migrationBuilder.CreateIndex(
                name: "IX_EstadosObjetivoConfig_Slug",
                table: "EstadosObjetivoConfig",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstadosObjetivoConfig_Orden",
                table: "EstadosObjetivoConfig",
                column: "Orden");

            migrationBuilder.CreateIndex(
                name: "IX_EstadosEvaluacionConfig_Slug",
                table: "EstadosEvaluacionConfig",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstadosEvaluacionConfig_Orden",
                table: "EstadosEvaluacionConfig",
                column: "Orden");

            migrationBuilder.CreateIndex(
                name: "IX_Pilares_Orden",
                table: "Pilares",
                column: "Orden");

            migrationBuilder.CreateIndex(
                name: "IX_SoftSkills_Orden",
                table: "SoftSkills",
                column: "Orden");

            migrationBuilder.CreateIndex(
                name: "IX_Objetivos_EstadoObjetivoConfigId",
                table: "Objetivos",
                column: "EstadoObjetivoConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionesCuatrimestrales_EscalaValoracionId",
                table: "RevisionesCuatrimestrales",
                column: "EscalaValoracionId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionesCuatrimestrales_EstadoEvaluacionConfigId",
                table: "RevisionesCuatrimestrales",
                column: "EstadoEvaluacionConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionesCuatrimestrales_SoftSkill1EscalaValoracionId",
                table: "RevisionesCuatrimestrales",
                column: "SoftSkill1EscalaValoracionId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionesCuatrimestrales_SoftSkill2EscalaValoracionId",
                table: "RevisionesCuatrimestrales",
                column: "SoftSkill2EscalaValoracionId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluacionesFinales_EscalaValoracionIdFinal",
                table: "EvaluacionesFinales",
                column: "EscalaValoracionIdFinal");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluacionesFinales_EstadoEvaluacionConfigId",
                table: "EvaluacionesFinales",
                column: "EstadoEvaluacionConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluacionesFinales_SoftSkill1EscalaValoracionId",
                table: "EvaluacionesFinales",
                column: "SoftSkill1EscalaValoracionId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluacionesFinales_SoftSkill2EscalaValoracionId",
                table: "EvaluacionesFinales",
                column: "SoftSkill2EscalaValoracionId");

            migrationBuilder.CreateIndex(
                name: "IX_Autoevaluaciones_EscalaValoracionIdScore",
                table: "Autoevaluaciones",
                column: "EscalaValoracionIdScore");

            migrationBuilder.CreateIndex(
                name: "IX_Autoevaluaciones_EstadoEvaluacionConfigId",
                table: "Autoevaluaciones",
                column: "EstadoEvaluacionConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_Autoevaluaciones_SoftSkill1EscalaValoracionId",
                table: "Autoevaluaciones",
                column: "SoftSkill1EscalaValoracionId");

            migrationBuilder.CreateIndex(
                name: "IX_Autoevaluaciones_SoftSkill2EscalaValoracionId",
                table: "Autoevaluaciones",
                column: "SoftSkill2EscalaValoracionId");

            // Crear foreign keys
            migrationBuilder.AddForeignKey(
                name: "FK_Objetivos_EstadosObjetivoConfig_EstadoObjetivoConfigId",
                table: "Objetivos",
                column: "EstadoObjetivoConfigId",
                principalTable: "EstadosObjetivoConfig",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RevisionesCuatrimestrales_EscalasValoracion_EscalaValoracionId",
                table: "RevisionesCuatrimestrales",
                column: "EscalaValoracionId",
                principalTable: "EscalasValoracion",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RevisionesCuatrimestrales_EstadosEvaluacionConfig_EstadoEvaluacionConfigId",
                table: "RevisionesCuatrimestrales",
                column: "EstadoEvaluacionConfigId",
                principalTable: "EstadosEvaluacionConfig",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RevisionesCuatrimestrales_EscalasValoracion_SoftSkill1EscalaValoracionId",
                table: "RevisionesCuatrimestrales",
                column: "SoftSkill1EscalaValoracionId",
                principalTable: "EscalasValoracion",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RevisionesCuatrimestrales_EscalasValoracion_SoftSkill2EscalaValoracionId",
                table: "RevisionesCuatrimestrales",
                column: "SoftSkill2EscalaValoracionId",
                principalTable: "EscalasValoracion",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluacionesFinales_EscalasValoracion_EscalaValoracionIdFinal",
                table: "EvaluacionesFinales",
                column: "EscalaValoracionIdFinal",
                principalTable: "EscalasValoracion",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluacionesFinales_EstadosEvaluacionConfig_EstadoEvaluacionConfigId",
                table: "EvaluacionesFinales",
                column: "EstadoEvaluacionConfigId",
                principalTable: "EstadosEvaluacionConfig",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluacionesFinales_EscalasValoracion_SoftSkill1EscalaValoracionId",
                table: "EvaluacionesFinales",
                column: "SoftSkill1EscalaValoracionId",
                principalTable: "EscalasValoracion",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluacionesFinales_EscalasValoracion_SoftSkill2EscalaValoracionId",
                table: "EvaluacionesFinales",
                column: "SoftSkill2EscalaValoracionId",
                principalTable: "EscalasValoracion",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Autoevaluaciones_EscalasValoracion_EscalaValoracionIdScore",
                table: "Autoevaluaciones",
                column: "EscalaValoracionIdScore",
                principalTable: "EscalasValoracion",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Autoevaluaciones_EstadosEvaluacionConfig_EstadoEvaluacionConfigId",
                table: "Autoevaluaciones",
                column: "EstadoEvaluacionConfigId",
                principalTable: "EstadosEvaluacionConfig",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Autoevaluaciones_EscalasValoracion_SoftSkill1EscalaValoracionId",
                table: "Autoevaluaciones",
                column: "SoftSkill1EscalaValoracionId",
                principalTable: "EscalasValoracion",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Autoevaluaciones_EscalasValoracion_SoftSkill2EscalaValoracionId",
                table: "Autoevaluaciones",
                column: "SoftSkill2EscalaValoracionId",
                principalTable: "EscalasValoracion",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Objetivos_EstadosObjetivoConfig_EstadoObjetivoConfigId",
                table: "Objetivos");

            migrationBuilder.DropForeignKey(
                name: "FK_RevisionesCuatrimestrales_EscalasValoracion_EscalaValoracionId",
                table: "RevisionesCuatrimestrales");

            migrationBuilder.DropForeignKey(
                name: "FK_RevisionesCuatrimestrales_EstadosEvaluacionConfig_EstadoEvaluacionConfigId",
                table: "RevisionesCuatrimestrales");

            migrationBuilder.DropForeignKey(
                name: "FK_RevisionesCuatrimestrales_EscalasValoracion_SoftSkill1EscalaValoracionId",
                table: "RevisionesCuatrimestrales");

            migrationBuilder.DropForeignKey(
                name: "FK_RevisionesCuatrimestrales_EscalasValoracion_SoftSkill2EscalaValoracionId",
                table: "RevisionesCuatrimestrales");

            migrationBuilder.DropForeignKey(
                name: "FK_EvaluacionesFinales_EscalasValoracion_EscalaValoracionIdFinal",
                table: "EvaluacionesFinales");

            migrationBuilder.DropForeignKey(
                name: "FK_EvaluacionesFinales_EstadosEvaluacionConfig_EstadoEvaluacionConfigId",
                table: "EvaluacionesFinales");

            migrationBuilder.DropForeignKey(
                name: "FK_EvaluacionesFinales_EscalasValoracion_SoftSkill1EscalaValoracionId",
                table: "EvaluacionesFinales");

            migrationBuilder.DropForeignKey(
                name: "FK_EvaluacionesFinales_EscalasValoracion_SoftSkill2EscalaValoracionId",
                table: "EvaluacionesFinales");

            migrationBuilder.DropForeignKey(
                name: "FK_Autoevaluaciones_EscalasValoracion_EscalaValoracionIdScore",
                table: "Autoevaluaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Autoevaluaciones_EstadosEvaluacionConfig_EstadoEvaluacionConfigId",
                table: "Autoevaluaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Autoevaluaciones_EscalasValoracion_SoftSkill1EscalaValoracionId",
                table: "Autoevaluaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Autoevaluaciones_EscalasValoracion_SoftSkill2EscalaValoracionId",
                table: "Autoevaluaciones");

            migrationBuilder.DropIndex(
                name: "IX_EscalasValoracion_Orden",
                table: "EscalasValoracion");

            migrationBuilder.DropIndex(
                name: "IX_EstadosObjetivoConfig_Orden",
                table: "EstadosObjetivoConfig");

            migrationBuilder.DropIndex(
                name: "IX_EstadosObjetivoConfig_Slug",
                table: "EstadosObjetivoConfig");

            migrationBuilder.DropIndex(
                name: "IX_EstadosEvaluacionConfig_Orden",
                table: "EstadosEvaluacionConfig");

            migrationBuilder.DropIndex(
                name: "IX_EstadosEvaluacionConfig_Slug",
                table: "EstadosEvaluacionConfig");

            migrationBuilder.DropIndex(
                name: "IX_Pilares_Orden",
                table: "Pilares");

            migrationBuilder.DropIndex(
                name: "IX_SoftSkills_Orden",
                table: "SoftSkills");

            migrationBuilder.DropIndex(
                name: "IX_Objetivos_EstadoObjetivoConfigId",
                table: "Objetivos");

            migrationBuilder.DropIndex(
                name: "IX_RevisionesCuatrimestrales_EscalaValoracionId",
                table: "RevisionesCuatrimestrales");

            migrationBuilder.DropIndex(
                name: "IX_RevisionesCuatrimestrales_EstadoEvaluacionConfigId",
                table: "RevisionesCuatrimestrales");

            migrationBuilder.DropIndex(
                name: "IX_RevisionesCuatrimestrales_SoftSkill1EscalaValoracionId",
                table: "RevisionesCuatrimestrales");

            migrationBuilder.DropIndex(
                name: "IX_RevisionesCuatrimestrales_SoftSkill2EscalaValoracionId",
                table: "RevisionesCuatrimestrales");

            migrationBuilder.DropIndex(
                name: "IX_EvaluacionesFinales_EscalaValoracionIdFinal",
                table: "EvaluacionesFinales");

            migrationBuilder.DropIndex(
                name: "IX_EvaluacionesFinales_EstadoEvaluacionConfigId",
                table: "EvaluacionesFinales");

            migrationBuilder.DropIndex(
                name: "IX_EvaluacionesFinales_SoftSkill1EscalaValoracionId",
                table: "EvaluacionesFinales");

            migrationBuilder.DropIndex(
                name: "IX_EvaluacionesFinales_SoftSkill2EscalaValoracionId",
                table: "EvaluacionesFinales");

            migrationBuilder.DropIndex(
                name: "IX_Autoevaluaciones_EscalaValoracionIdScore",
                table: "Autoevaluaciones");

            migrationBuilder.DropIndex(
                name: "IX_Autoevaluaciones_EstadoEvaluacionConfigId",
                table: "Autoevaluaciones");

            migrationBuilder.DropIndex(
                name: "IX_Autoevaluaciones_SoftSkill1EscalaValoracionId",
                table: "Autoevaluaciones");

            migrationBuilder.DropIndex(
                name: "IX_Autoevaluaciones_SoftSkill2EscalaValoracionId",
                table: "Autoevaluaciones");

            migrationBuilder.DropTable(
                name: "ConfiguracionesPlataforma");

            migrationBuilder.DropTable(
                name: "EstadosEvaluacionConfig");

            migrationBuilder.DropTable(
                name: "EstadosObjetivoConfig");

            migrationBuilder.DropTable(
                name: "EscalasValoracion");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Pilares");

            migrationBuilder.DropColumn(
                name: "Orden",
                table: "Pilares");

            migrationBuilder.DropColumn(
                name: "CreadoEn",
                table: "Pilares");

            migrationBuilder.DropColumn(
                name: "ActualizadoEn",
                table: "Pilares");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "SoftSkills");

            migrationBuilder.DropColumn(
                name: "Orden",
                table: "SoftSkills");

            migrationBuilder.DropColumn(
                name: "CreadoEn",
                table: "SoftSkills");

            migrationBuilder.DropColumn(
                name: "ActualizadoEn",
                table: "SoftSkills");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "CreadoEn",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "ActualizadoEn",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "PorcentajePilar",
                table: "Objetivos");

            migrationBuilder.DropColumn(
                name: "AprobadoPorJefe",
                table: "Objetivos");

            migrationBuilder.DropColumn(
                name: "EstadoObjetivoConfigId",
                table: "Objetivos");

            migrationBuilder.DropColumn(
                name: "EscalaValoracionId",
                table: "RevisionesCuatrimestrales");

            migrationBuilder.DropColumn(
                name: "EstadoEvaluacionConfigId",
                table: "RevisionesCuatrimestrales");

            migrationBuilder.DropColumn(
                name: "SoftSkill1EscalaValoracionId",
                table: "RevisionesCuatrimestrales");

            migrationBuilder.DropColumn(
                name: "SoftSkill2EscalaValoracionId",
                table: "RevisionesCuatrimestrales");

            migrationBuilder.DropColumn(
                name: "EscalaValoracionIdFinal",
                table: "EvaluacionesFinales");

            migrationBuilder.DropColumn(
                name: "EstadoEvaluacionConfigId",
                table: "EvaluacionesFinales");

            migrationBuilder.DropColumn(
                name: "SoftSkill1EscalaValoracionId",
                table: "EvaluacionesFinales");

            migrationBuilder.DropColumn(
                name: "SoftSkill2EscalaValoracionId",
                table: "EvaluacionesFinales");

            migrationBuilder.DropColumn(
                name: "EscalaValoracionIdScore",
                table: "Autoevaluaciones");

            migrationBuilder.DropColumn(
                name: "EstadoEvaluacionConfigId",
                table: "Autoevaluaciones");

            migrationBuilder.DropColumn(
                name: "SoftSkill1EscalaValoracionId",
                table: "Autoevaluaciones");

            migrationBuilder.DropColumn(
                name: "SoftSkill2EscalaValoracionId",
                table: "Autoevaluaciones");
        }
    }
}
