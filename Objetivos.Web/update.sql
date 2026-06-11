BEGIN TRANSACTION;
DROP TABLE "Empleados";

DROP TABLE "Jefes";

ALTER TABLE "Objetivos" RENAME COLUMN "EmpleadoId" TO "UsuarioId";

DROP INDEX "IX_Objetivos_PilarId_EmpleadoId_Anio";

CREATE INDEX "IX_Objetivos_PilarId_UsuarioId_Anio" ON "Objetivos" ("PilarId", "UsuarioId", "Anio");

DROP INDEX "IX_Objetivos_EmpleadoId";

CREATE INDEX "IX_Objetivos_UsuarioId" ON "Objetivos" ("UsuarioId");

ALTER TABLE "MensajesChat" RENAME COLUMN "DestinatarioEmpleadoId" TO "DestinatarioUsuarioId";

ALTER TABLE "CursoAsignaciones" RENAME COLUMN "EmpleadoId" TO "UsuarioId";

DROP INDEX "IX_CursoAsignaciones_EmpleadoId";

CREATE INDEX "IX_CursoAsignaciones_UsuarioId" ON "CursoAsignaciones" ("UsuarioId");

DROP INDEX "IX_CursoAsignaciones_CursoId_EmpleadoId";

CREATE UNIQUE INDEX "IX_CursoAsignaciones_CursoId_UsuarioId" ON "CursoAsignaciones" ("CursoId", "UsuarioId");

ALTER TABLE "BitacoraEntradas" RENAME COLUMN "EmpleadoId" TO "UsuarioId";

DROP INDEX "IX_BitacoraEntradas_EmpleadoId";

CREATE INDEX "IX_BitacoraEntradas_UsuarioId" ON "BitacoraEntradas" ("UsuarioId");

ALTER TABLE "Autoevaluaciones" RENAME COLUMN "EmpleadoId" TO "UsuarioId";

ALTER TABLE "RevisionesCuatrimestrales" ADD "ComentarioUsuario" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Objetivos" ADD "FechaIngreso" TEXT NOT NULL DEFAULT '0001-01-01 00:00:00';

ALTER TABLE "EvaluacionesFinales" ADD "ComentarioUsuario" TEXT NOT NULL DEFAULT '';

ALTER TABLE "BitacoraEntradas" ADD "FeedbackUsuario" TEXT NULL;

CREATE TABLE "Usuarios" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Usuarios" PRIMARY KEY AUTOINCREMENT,
    "Nombre" TEXT NOT NULL,
    "Apellido" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "Legajo" TEXT NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "DebeCambiarPassword" INTEGER NOT NULL,
    "PuestoId" INTEGER NULL,
    "AreaId" INTEGER NOT NULL,
    "PaisId" INTEGER NOT NULL,
    "JefeId" INTEGER NULL,
    "Rol" TEXT NOT NULL,
    "Activo" INTEGER NOT NULL,
    "FechaBaja" TEXT NULL,
    "EsSuperusuario" INTEGER NOT NULL,
    "FechaCreacion" TEXT NOT NULL,
    "FechaIngreso" TEXT NOT NULL,
    CONSTRAINT "FK_Usuarios_Areas_AreaId" FOREIGN KEY ("AreaId") REFERENCES "Areas" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Usuarios_Paises_PaisId" FOREIGN KEY ("PaisId") REFERENCES "Paises" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Usuarios_Puestos_PuestoId" FOREIGN KEY ("PuestoId") REFERENCES "Puestos" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Usuarios_Usuarios_JefeId" FOREIGN KEY ("JefeId") REFERENCES "Usuarios" ("Id")
);

CREATE INDEX "IX_Usuarios_AreaId" ON "Usuarios" ("AreaId");

CREATE UNIQUE INDEX "IX_Usuarios_Email" ON "Usuarios" ("Email");

CREATE INDEX "IX_Usuarios_JefeId" ON "Usuarios" ("JefeId");

CREATE INDEX "IX_Usuarios_PaisId" ON "Usuarios" ("PaisId");

CREATE INDEX "IX_Usuarios_PuestoId" ON "Usuarios" ("PuestoId");

CREATE TABLE "ef_temp_BitacoraEntradas" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_BitacoraEntradas" PRIMARY KEY AUTOINCREMENT,
    "AdjuntosJson" TEXT NOT NULL,
    "Estado" INTEGER NOT NULL,
    "Fecha" TEXT NOT NULL,
    "FechaFeedback" TEXT NULL,
    "FeedbackJefe" TEXT NULL,
    "FeedbackUsuario" TEXT NULL,
    "ObjetivoId" INTEGER NOT NULL,
    "Texto" TEXT NOT NULL,
    "UsuarioId" INTEGER NOT NULL,
    CONSTRAINT "FK_BitacoraEntradas_Objetivos_ObjetivoId" FOREIGN KEY ("ObjetivoId") REFERENCES "Objetivos" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_BitacoraEntradas_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_BitacoraEntradas" ("Id", "AdjuntosJson", "Estado", "Fecha", "FechaFeedback", "FeedbackJefe", "FeedbackUsuario", "ObjetivoId", "Texto", "UsuarioId")
SELECT "Id", "AdjuntosJson", "Estado", "Fecha", "FechaFeedback", "FeedbackJefe", "FeedbackUsuario", "ObjetivoId", "Texto", "UsuarioId"
FROM "BitacoraEntradas";

CREATE TABLE "ef_temp_CursoAsignaciones" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CursoAsignaciones" PRIMARY KEY AUTOINCREMENT,
    "AsignadoPorId" INTEGER NULL,
    "Completado" INTEGER NOT NULL,
    "CursoId" INTEGER NOT NULL,
    "FechaAsignacion" TEXT NOT NULL,
    "FechaCompletado" TEXT NULL,
    "Notas" TEXT NULL,
    "UsuarioId" INTEGER NOT NULL,
    CONSTRAINT "FK_CursoAsignaciones_Cursos_CursoId" FOREIGN KEY ("CursoId") REFERENCES "Cursos" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_CursoAsignaciones_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_CursoAsignaciones" ("Id", "AsignadoPorId", "Completado", "CursoId", "FechaAsignacion", "FechaCompletado", "Notas", "UsuarioId")
SELECT "Id", "AsignadoPorId", "Completado", "CursoId", "FechaAsignacion", "FechaCompletado", "Notas", "UsuarioId"
FROM "CursoAsignaciones";

CREATE TABLE "ef_temp_Objetivos" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Objetivos" PRIMARY KEY AUTOINCREMENT,
    "Anio" INTEGER NOT NULL,
    "AprobadoPorJefe" INTEGER NOT NULL,
    "AreaEspecificaId" INTEGER NULL,
    "CreadoPorId" INTEGER NOT NULL,
    "Deadline" TEXT NOT NULL,
    "Descripcion" TEXT NOT NULL,
    "Estado" INTEGER NOT NULL,
    "EstadoObjetivoConfigId" INTEGER NULL,
    "FechaCreacion" TEXT NOT NULL,
    "FechaIngreso" TEXT NOT NULL,
    "Nombre" TEXT NOT NULL,
    "PilarId" INTEGER NOT NULL,
    "PorcentajeArea" TEXT NOT NULL,
    "PorcentajePilar" TEXT NOT NULL,
    "Progreso" INTEGER NOT NULL,
    "SoftSkill1Id" INTEGER NOT NULL,
    "SoftSkill2Id" INTEGER NOT NULL,
    "UsuarioId" INTEGER NOT NULL,
    CONSTRAINT "FK_Objetivos_Areas_AreaEspecificaId" FOREIGN KEY ("AreaEspecificaId") REFERENCES "Areas" ("Id"),
    CONSTRAINT "FK_Objetivos_EstadosObjetivoConfig_EstadoObjetivoConfigId" FOREIGN KEY ("EstadoObjetivoConfigId") REFERENCES "EstadosObjetivoConfig" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Objetivos_Pilares_PilarId" FOREIGN KEY ("PilarId") REFERENCES "Pilares" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Objetivos_SoftSkills_SoftSkill1Id" FOREIGN KEY ("SoftSkill1Id") REFERENCES "SoftSkills" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Objetivos_SoftSkills_SoftSkill2Id" FOREIGN KEY ("SoftSkill2Id") REFERENCES "SoftSkills" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Objetivos_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_Objetivos" ("Id", "Anio", "AprobadoPorJefe", "AreaEspecificaId", "CreadoPorId", "Deadline", "Descripcion", "Estado", "EstadoObjetivoConfigId", "FechaCreacion", "FechaIngreso", "Nombre", "PilarId", "PorcentajeArea", "PorcentajePilar", "Progreso", "SoftSkill1Id", "SoftSkill2Id", "UsuarioId")
SELECT "Id", "Anio", "AprobadoPorJefe", "AreaEspecificaId", "CreadoPorId", "Deadline", "Descripcion", "Estado", "EstadoObjetivoConfigId", "FechaCreacion", "FechaIngreso", "Nombre", "PilarId", "PorcentajeArea", "PorcentajePilar", "Progreso", "SoftSkill1Id", "SoftSkill2Id", "UsuarioId"
FROM "Objetivos";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "BitacoraEntradas";

ALTER TABLE "ef_temp_BitacoraEntradas" RENAME TO "BitacoraEntradas";

DROP TABLE "CursoAsignaciones";

ALTER TABLE "ef_temp_CursoAsignaciones" RENAME TO "CursoAsignaciones";

DROP TABLE "Objetivos";

ALTER TABLE "ef_temp_Objetivos" RENAME TO "Objetivos";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_BitacoraEntradas_ObjetivoId" ON "BitacoraEntradas" ("ObjetivoId");

CREATE INDEX "IX_BitacoraEntradas_UsuarioId" ON "BitacoraEntradas" ("UsuarioId");

CREATE UNIQUE INDEX "IX_CursoAsignaciones_CursoId_UsuarioId" ON "CursoAsignaciones" ("CursoId", "UsuarioId");

CREATE INDEX "IX_CursoAsignaciones_UsuarioId" ON "CursoAsignaciones" ("UsuarioId");

CREATE INDEX "IX_Objetivos_AreaEspecificaId" ON "Objetivos" ("AreaEspecificaId");

CREATE INDEX "IX_Objetivos_EstadoObjetivoConfigId" ON "Objetivos" ("EstadoObjetivoConfigId");

CREATE INDEX "IX_Objetivos_PilarId_UsuarioId_Anio" ON "Objetivos" ("PilarId", "UsuarioId", "Anio");

CREATE INDEX "IX_Objetivos_SoftSkill1Id" ON "Objetivos" ("SoftSkill1Id");

CREATE INDEX "IX_Objetivos_SoftSkill2Id" ON "Objetivos" ("SoftSkill2Id");

CREATE INDEX "IX_Objetivos_UsuarioId" ON "Objetivos" ("UsuarioId");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260609161046_UnificarUsuarios', '10.0.3');

