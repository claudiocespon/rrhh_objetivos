using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Objetivos.Web.Migrations
{
    /// <inheritdoc />
    public partial class UnificarUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BitacoraEntradas_Empleados_EmpleadoId",
                table: "BitacoraEntradas");

            migrationBuilder.DropForeignKey(
                name: "FK_CursoAsignaciones_Empleados_EmpleadoId",
                table: "CursoAsignaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Objetivos_Empleados_EmpleadoId",
                table: "Objetivos");

            migrationBuilder.DropTable(
                name: "Empleados");

            migrationBuilder.DropTable(
                name: "Jefes");

            migrationBuilder.RenameColumn(
                name: "EmpleadoId",
                table: "Objetivos",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_Objetivos_PilarId_EmpleadoId_Anio",
                table: "Objetivos",
                newName: "IX_Objetivos_PilarId_UsuarioId_Anio");

            migrationBuilder.RenameIndex(
                name: "IX_Objetivos_EmpleadoId",
                table: "Objetivos",
                newName: "IX_Objetivos_UsuarioId");

            migrationBuilder.RenameColumn(
                name: "DestinatarioEmpleadoId",
                table: "MensajesChat",
                newName: "DestinatarioUsuarioId");

            migrationBuilder.RenameColumn(
                name: "EmpleadoId",
                table: "CursoAsignaciones",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_CursoAsignaciones_EmpleadoId",
                table: "CursoAsignaciones",
                newName: "IX_CursoAsignaciones_UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_CursoAsignaciones_CursoId_EmpleadoId",
                table: "CursoAsignaciones",
                newName: "IX_CursoAsignaciones_CursoId_UsuarioId");

            migrationBuilder.RenameColumn(
                name: "EmpleadoId",
                table: "BitacoraEntradas",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_BitacoraEntradas_EmpleadoId",
                table: "BitacoraEntradas",
                newName: "IX_BitacoraEntradas_UsuarioId");

            migrationBuilder.RenameColumn(
                name: "EmpleadoId",
                table: "Autoevaluaciones",
                newName: "UsuarioId");

            migrationBuilder.AddColumn<string>(
                name: "ComentarioUsuario",
                table: "RevisionesCuatrimestrales",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaIngreso",
                table: "Objetivos",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ComentarioUsuario",
                table: "EvaluacionesFinales",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FeedbackUsuario",
                table: "BitacoraEntradas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Apellido = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Legajo = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    DebeCambiarPassword = table.Column<bool>(type: "INTEGER", nullable: false),
                    PuestoId = table.Column<int>(type: "INTEGER", nullable: true),
                    AreaId = table.Column<int>(type: "INTEGER", nullable: false),
                    PaisId = table.Column<int>(type: "INTEGER", nullable: false),
                    JefeId = table.Column<int>(type: "INTEGER", nullable: true),
                    Rol = table.Column<string>(type: "TEXT", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaBaja = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EsSuperusuario = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaIngreso = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Usuarios_Paises_PaisId",
                        column: x => x.PaisId,
                        principalTable: "Paises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Usuarios_Puestos_PuestoId",
                        column: x => x.PuestoId,
                        principalTable: "Puestos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Usuarios_Usuarios_JefeId",
                        column: x => x.JefeId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_AreaId",
                table: "Usuarios",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_JefeId",
                table: "Usuarios",
                column: "JefeId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_PaisId",
                table: "Usuarios",
                column: "PaisId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_PuestoId",
                table: "Usuarios",
                column: "PuestoId");

            migrationBuilder.AddForeignKey(
                name: "FK_BitacoraEntradas_Usuarios_UsuarioId",
                table: "BitacoraEntradas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CursoAsignaciones_Usuarios_UsuarioId",
                table: "CursoAsignaciones",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Objetivos_Usuarios_UsuarioId",
                table: "Objetivos",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BitacoraEntradas_Usuarios_UsuarioId",
                table: "BitacoraEntradas");

            migrationBuilder.DropForeignKey(
                name: "FK_CursoAsignaciones_Usuarios_UsuarioId",
                table: "CursoAsignaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Objetivos_Usuarios_UsuarioId",
                table: "Objetivos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropColumn(
                name: "ComentarioUsuario",
                table: "RevisionesCuatrimestrales");

            migrationBuilder.DropColumn(
                name: "FechaIngreso",
                table: "Objetivos");

            migrationBuilder.DropColumn(
                name: "ComentarioUsuario",
                table: "EvaluacionesFinales");

            migrationBuilder.DropColumn(
                name: "FeedbackUsuario",
                table: "BitacoraEntradas");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "Objetivos",
                newName: "EmpleadoId");

            migrationBuilder.RenameIndex(
                name: "IX_Objetivos_UsuarioId",
                table: "Objetivos",
                newName: "IX_Objetivos_EmpleadoId");

            migrationBuilder.RenameIndex(
                name: "IX_Objetivos_PilarId_UsuarioId_Anio",
                table: "Objetivos",
                newName: "IX_Objetivos_PilarId_EmpleadoId_Anio");

            migrationBuilder.RenameColumn(
                name: "DestinatarioUsuarioId",
                table: "MensajesChat",
                newName: "DestinatarioEmpleadoId");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "CursoAsignaciones",
                newName: "EmpleadoId");

            migrationBuilder.RenameIndex(
                name: "IX_CursoAsignaciones_UsuarioId",
                table: "CursoAsignaciones",
                newName: "IX_CursoAsignaciones_EmpleadoId");

            migrationBuilder.RenameIndex(
                name: "IX_CursoAsignaciones_CursoId_UsuarioId",
                table: "CursoAsignaciones",
                newName: "IX_CursoAsignaciones_CursoId_EmpleadoId");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "BitacoraEntradas",
                newName: "EmpleadoId");

            migrationBuilder.RenameIndex(
                name: "IX_BitacoraEntradas_UsuarioId",
                table: "BitacoraEntradas",
                newName: "IX_BitacoraEntradas_EmpleadoId");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "Autoevaluaciones",
                newName: "EmpleadoId");

            migrationBuilder.CreateTable(
                name: "Jefes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AreaId = table.Column<int>(type: "INTEGER", nullable: false),
                    PaisId = table.Column<int>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    Apellido = table.Column<string>(type: "TEXT", nullable: false),
                    DebeCambiarPassword = table.Column<bool>(type: "INTEGER", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    EsSuperusuario = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaBaja = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Legajo = table.Column<string>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Rol = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jefes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jefes_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Jefes_Paises_PaisId",
                        column: x => x.PaisId,
                        principalTable: "Paises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Empleados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AreaId = table.Column<int>(type: "INTEGER", nullable: false),
                    JefeId = table.Column<int>(type: "INTEGER", nullable: false),
                    PaisId = table.Column<int>(type: "INTEGER", nullable: false),
                    PuestoId = table.Column<int>(type: "INTEGER", nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    Apellido = table.Column<string>(type: "TEXT", nullable: false),
                    DebeCambiarPassword = table.Column<bool>(type: "INTEGER", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    EsSuperusuario = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaBaja = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaIngreso = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Legajo = table.Column<string>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empleados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Empleados_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Empleados_Jefes_JefeId",
                        column: x => x.JefeId,
                        principalTable: "Jefes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Empleados_Paises_PaisId",
                        column: x => x.PaisId,
                        principalTable: "Paises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Empleados_Puestos_PuestoId",
                        column: x => x.PuestoId,
                        principalTable: "Puestos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_AreaId",
                table: "Empleados",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_Email",
                table: "Empleados",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_JefeId",
                table: "Empleados",
                column: "JefeId");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_PaisId",
                table: "Empleados",
                column: "PaisId");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_PuestoId",
                table: "Empleados",
                column: "PuestoId");

            migrationBuilder.CreateIndex(
                name: "IX_Jefes_AreaId",
                table: "Jefes",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Jefes_Email",
                table: "Jefes",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jefes_PaisId",
                table: "Jefes",
                column: "PaisId");

            migrationBuilder.AddForeignKey(
                name: "FK_BitacoraEntradas_Empleados_EmpleadoId",
                table: "BitacoraEntradas",
                column: "EmpleadoId",
                principalTable: "Empleados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CursoAsignaciones_Empleados_EmpleadoId",
                table: "CursoAsignaciones",
                column: "EmpleadoId",
                principalTable: "Empleados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Objetivos_Empleados_EmpleadoId",
                table: "Objetivos",
                column: "EmpleadoId",
                principalTable: "Empleados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
