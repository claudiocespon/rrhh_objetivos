using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Objetivos.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftSkillsEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SoftSkill1Comentario",
                table: "RevisionesCuatrimestrales",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SoftSkill1Puntaje",
                table: "RevisionesCuatrimestrales",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoftSkill2Comentario",
                table: "RevisionesCuatrimestrales",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SoftSkill2Puntaje",
                table: "RevisionesCuatrimestrales",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoftSkill1Comentario",
                table: "EvaluacionesFinales",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SoftSkill1Puntaje",
                table: "EvaluacionesFinales",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoftSkill2Comentario",
                table: "EvaluacionesFinales",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SoftSkill2Puntaje",
                table: "EvaluacionesFinales",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoftSkill1Comentario",
                table: "RevisionesCuatrimestrales");

            migrationBuilder.DropColumn(
                name: "SoftSkill1Puntaje",
                table: "RevisionesCuatrimestrales");

            migrationBuilder.DropColumn(
                name: "SoftSkill2Comentario",
                table: "RevisionesCuatrimestrales");

            migrationBuilder.DropColumn(
                name: "SoftSkill2Puntaje",
                table: "RevisionesCuatrimestrales");

            migrationBuilder.DropColumn(
                name: "SoftSkill1Comentario",
                table: "EvaluacionesFinales");

            migrationBuilder.DropColumn(
                name: "SoftSkill1Puntaje",
                table: "EvaluacionesFinales");

            migrationBuilder.DropColumn(
                name: "SoftSkill2Comentario",
                table: "EvaluacionesFinales");

            migrationBuilder.DropColumn(
                name: "SoftSkill2Puntaje",
                table: "EvaluacionesFinales");
        }
    }
}
