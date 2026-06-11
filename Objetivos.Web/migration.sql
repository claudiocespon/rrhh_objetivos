CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "Areas" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Areas" PRIMARY KEY AUTOINCREMENT,
    "Nombre" TEXT NOT NULL,
    "Descripcion" TEXT NOT NULL
);

CREATE TABLE "AuditoriaLogs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AuditoriaLogs" PRIMARY KEY AUTOINCREMENT,
    "Entidad" TEXT NOT NULL,
    "EntidadId" INTEGER NOT NULL,
    "Accion" TEXT NOT NULL,
    "UsuarioId" INTEGER NOT NULL,
    "Fecha" TEXT NOT NULL,
    "CambiosJson" TEXT NOT NULL
);

CREATE TABLE "Cursos" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Cursos" PRIMARY KEY AUTOINCREMENT,
    "Nombre" TEXT NOT NULL,
    "Descripcion" TEXT NOT NULL,
    "UrlImagen" TEXT NOT NULL,
    "Categoria" TEXT NOT NULL,
    "DuracionHoras" INTEGER NOT NULL,
    "EsObligatorio" INTEGER NOT NULL
);

CREATE TABLE "MensajesChat" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MensajesChat" PRIMARY KEY AUTOINCREMENT,
    "RemitenteId" INTEGER NOT NULL,
    "RemitenteEsJefe" INTEGER NOT NULL,
    "DestinatarioEmpleadoId" INTEGER NOT NULL,
    "JefeId" INTEGER NOT NULL,
    "Texto" TEXT NOT NULL,
    "Fecha" TEXT NOT NULL,
    "Leido" INTEGER NOT NULL
);

CREATE TABLE "Notificaciones" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Notificaciones" PRIMARY KEY AUTOINCREMENT,
    "UsuarioId" INTEGER NOT NULL,
    "Tipo" INTEGER NOT NULL,
    "Mensaje" TEXT NOT NULL,
    "Fecha" TEXT NOT NULL,
    "Leida" INTEGER NOT NULL
);

CREATE TABLE "Paises" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Paises" PRIMARY KEY AUTOINCREMENT,
    "Nombre" TEXT NOT NULL
);

CREATE TABLE "Pilares" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Pilares" PRIMARY KEY AUTOINCREMENT,
    "Nombre" TEXT NOT NULL,
    "Descripcion" TEXT NOT NULL,
    "ColorHex" TEXT NOT NULL
);

CREATE TABLE "SoftSkills" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_SoftSkills" PRIMARY KEY AUTOINCREMENT,
    "Nombre" TEXT NOT NULL,
    "Descripcion" TEXT NOT NULL
);

CREATE TABLE "Jefes" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Jefes" PRIMARY KEY AUTOINCREMENT,
    "Nombre" TEXT NOT NULL,
    "Apellido" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "Legajo" TEXT NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "DebeCambiarPassword" INTEGER NOT NULL,
    "AreaId" INTEGER NOT NULL,
    "PaisId" INTEGER NOT NULL,
    "Rol" TEXT NOT NULL,
    "Activo" INTEGER NOT NULL,
    "EsSuperusuario" INTEGER NOT NULL,
    "FechaCreacion" TEXT NOT NULL,
    CONSTRAINT "FK_Jefes_Areas_AreaId" FOREIGN KEY ("AreaId") REFERENCES "Areas" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Jefes_Paises_PaisId" FOREIGN KEY ("PaisId") REFERENCES "Paises" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Empleados" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Empleados" PRIMARY KEY AUTOINCREMENT,
    "Nombre" TEXT NOT NULL,
    "Apellido" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "Legajo" TEXT NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "DebeCambiarPassword" INTEGER NOT NULL,
    "Puesto" TEXT NOT NULL,
    "AreaId" INTEGER NOT NULL,
    "JefeId" INTEGER NOT NULL,
    "PaisId" INTEGER NOT NULL,
    "Activo" INTEGER NOT NULL,
    "EsSuperusuario" INTEGER NOT NULL,
    "FechaIngreso" TEXT NOT NULL,
    CONSTRAINT "FK_Empleados_Areas_AreaId" FOREIGN KEY ("AreaId") REFERENCES "Areas" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Empleados_Jefes_JefeId" FOREIGN KEY ("JefeId") REFERENCES "Jefes" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Empleados_Paises_PaisId" FOREIGN KEY ("PaisId") REFERENCES "Paises" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Objetivos" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Objetivos" PRIMARY KEY AUTOINCREMENT,
    "Nombre" TEXT NOT NULL,
    "Descripcion" TEXT NOT NULL,
    "PilarId" INTEGER NOT NULL,
    "EmpleadoId" INTEGER NOT NULL,
    "Anio" INTEGER NOT NULL,
    "Deadline" TEXT NOT NULL,
    "SoftSkill1Id" INTEGER NOT NULL,
    "SoftSkill2Id" INTEGER NOT NULL,
    "Estado" INTEGER NOT NULL,
    "Progreso" INTEGER NOT NULL,
    "FechaCreacion" TEXT NOT NULL,
    "CreadoPorId" INTEGER NOT NULL,
    CONSTRAINT "FK_Objetivos_Empleados_EmpleadoId" FOREIGN KEY ("EmpleadoId") REFERENCES "Empleados" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Objetivos_Pilares_PilarId" FOREIGN KEY ("PilarId") REFERENCES "Pilares" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Objetivos_SoftSkills_SoftSkill1Id" FOREIGN KEY ("SoftSkill1Id") REFERENCES "SoftSkills" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Objetivos_SoftSkills_SoftSkill2Id" FOREIGN KEY ("SoftSkill2Id") REFERENCES "SoftSkills" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Autoevaluaciones" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Autoevaluaciones" PRIMARY KEY AUTOINCREMENT,
    "ObjetivoId" INTEGER NOT NULL,
    "EmpleadoId" INTEGER NOT NULL,
    "Score" INTEGER NOT NULL,
    "Comentario" TEXT NOT NULL,
    "EvidenciasMencionadasJson" TEXT NOT NULL,
    "ArchivosAdjuntosJson" TEXT NOT NULL,
    "SoftSkill1Score" INTEGER NOT NULL,
    "SoftSkill1Comentario" TEXT NOT NULL,
    "SoftSkill2Score" INTEGER NOT NULL,
    "SoftSkill2Comentario" TEXT NOT NULL,
    "FechaAutoevaluacion" TEXT NOT NULL,
    CONSTRAINT "FK_Autoevaluaciones_Objetivos_ObjetivoId" FOREIGN KEY ("ObjetivoId") REFERENCES "Objetivos" ("Id") ON DELETE CASCADE
);

CREATE TABLE "BitacoraEntradas" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_BitacoraEntradas" PRIMARY KEY AUTOINCREMENT,
    "ObjetivoId" INTEGER NOT NULL,
    "EmpleadoId" INTEGER NOT NULL,
    "Fecha" TEXT NOT NULL,
    "Texto" TEXT NOT NULL,
    "AdjuntosJson" TEXT NOT NULL,
    "Estado" INTEGER NOT NULL,
    "FeedbackJefe" TEXT NULL,
    "FechaFeedback" TEXT NULL,
    CONSTRAINT "FK_BitacoraEntradas_Empleados_EmpleadoId" FOREIGN KEY ("EmpleadoId") REFERENCES "Empleados" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_BitacoraEntradas_Objetivos_ObjetivoId" FOREIGN KEY ("ObjetivoId") REFERENCES "Objetivos" ("Id") ON DELETE CASCADE
);

CREATE TABLE "EvaluacionesFinales" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_EvaluacionesFinales" PRIMARY KEY AUTOINCREMENT,
    "ObjetivoId" INTEGER NOT NULL,
    "Anio" INTEGER NOT NULL,
    "PuntajeFinal" REAL NOT NULL,
    "ComentarioJefe" TEXT NOT NULL,
    "ResultadoFinal" INTEGER NOT NULL,
    "FechaEvaluacion" TEXT NOT NULL,
    "EvaluadorId" INTEGER NOT NULL,
    CONSTRAINT "FK_EvaluacionesFinales_Objetivos_ObjetivoId" FOREIGN KEY ("ObjetivoId") REFERENCES "Objetivos" ("Id") ON DELETE CASCADE
);

CREATE TABLE "EventosCalendario" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_EventosCalendario" PRIMARY KEY AUTOINCREMENT,
    "Titulo" TEXT NOT NULL,
    "Fecha" TEXT NOT NULL,
    "Tipo" INTEGER NOT NULL,
    "ObjetivoId" INTEGER NULL,
    "AreaId" INTEGER NOT NULL,
    CONSTRAINT "FK_EventosCalendario_Objetivos_ObjetivoId" FOREIGN KEY ("ObjetivoId") REFERENCES "Objetivos" ("Id")
);

CREATE TABLE "RevisionesCuatrimestrales" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_RevisionesCuatrimestrales" PRIMARY KEY AUTOINCREMENT,
    "ObjetivoId" INTEGER NOT NULL,
    "Periodo" INTEGER NOT NULL,
    "Anio" INTEGER NOT NULL,
    "Puntaje" INTEGER NULL,
    "ComentarioJefe" TEXT NOT NULL,
    "Resultado" INTEGER NULL,
    "EvidenciasRevisadasJson" TEXT NOT NULL,
    "Completada" INTEGER NOT NULL,
    "FechaEvaluacion" TEXT NULL,
    "EvaluadorId" INTEGER NULL,
    CONSTRAINT "FK_RevisionesCuatrimestrales_Objetivos_ObjetivoId" FOREIGN KEY ("ObjetivoId") REFERENCES "Objetivos" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_Autoevaluaciones_ObjetivoId" ON "Autoevaluaciones" ("ObjetivoId");

CREATE INDEX "IX_BitacoraEntradas_EmpleadoId" ON "BitacoraEntradas" ("EmpleadoId");

CREATE INDEX "IX_BitacoraEntradas_ObjetivoId" ON "BitacoraEntradas" ("ObjetivoId");

CREATE INDEX "IX_Empleados_AreaId" ON "Empleados" ("AreaId");

CREATE UNIQUE INDEX "IX_Empleados_Email" ON "Empleados" ("Email");

CREATE INDEX "IX_Empleados_JefeId" ON "Empleados" ("JefeId");

CREATE INDEX "IX_Empleados_PaisId" ON "Empleados" ("PaisId");

CREATE UNIQUE INDEX "IX_EvaluacionesFinales_ObjetivoId" ON "EvaluacionesFinales" ("ObjetivoId");

CREATE INDEX "IX_EventosCalendario_ObjetivoId" ON "EventosCalendario" ("ObjetivoId");

CREATE INDEX "IX_Jefes_AreaId" ON "Jefes" ("AreaId");

CREATE UNIQUE INDEX "IX_Jefes_Email" ON "Jefes" ("Email");

CREATE INDEX "IX_Jefes_PaisId" ON "Jefes" ("PaisId");

CREATE INDEX "IX_Objetivos_EmpleadoId" ON "Objetivos" ("EmpleadoId");

CREATE UNIQUE INDEX "IX_Objetivos_PilarId_EmpleadoId_Anio" ON "Objetivos" ("PilarId", "EmpleadoId", "Anio");

CREATE INDEX "IX_Objetivos_SoftSkill1Id" ON "Objetivos" ("SoftSkill1Id");

CREATE INDEX "IX_Objetivos_SoftSkill2Id" ON "Objetivos" ("SoftSkill2Id");

CREATE INDEX "IX_RevisionesCuatrimestrales_ObjetivoId" ON "RevisionesCuatrimestrales" ("ObjetivoId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260407123311_UpdateAutoevaluacion', '10.0.3');

COMMIT;

BEGIN TRANSACTION;
ALTER TABLE "RevisionesCuatrimestrales" ADD "SoftSkill1Comentario" TEXT NOT NULL DEFAULT '';

ALTER TABLE "RevisionesCuatrimestrales" ADD "SoftSkill1Puntaje" INTEGER NULL;

ALTER TABLE "RevisionesCuatrimestrales" ADD "SoftSkill2Comentario" TEXT NOT NULL DEFAULT '';

ALTER TABLE "RevisionesCuatrimestrales" ADD "SoftSkill2Puntaje" INTEGER NULL;

ALTER TABLE "EvaluacionesFinales" ADD "SoftSkill1Comentario" TEXT NOT NULL DEFAULT '';

ALTER TABLE "EvaluacionesFinales" ADD "SoftSkill1Puntaje" INTEGER NULL;

ALTER TABLE "EvaluacionesFinales" ADD "SoftSkill2Comentario" TEXT NOT NULL DEFAULT '';

ALTER TABLE "EvaluacionesFinales" ADD "SoftSkill2Puntaje" INTEGER NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260410190112_AddSoftSkillsEvaluation', '10.0.3');

COMMIT;

BEGIN TRANSACTION;
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260416123052_AddCursoAsignacion', '10.0.3');

COMMIT;

BEGIN TRANSACTION;
CREATE TABLE "CursoAsignaciones" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CursoAsignaciones" PRIMARY KEY AUTOINCREMENT,
    "CursoId" INTEGER NOT NULL,
    "EmpleadoId" INTEGER NOT NULL,
    "FechaAsignacion" TEXT NOT NULL,
    "FechaCompletado" TEXT NULL,
    "Completado" INTEGER NOT NULL,
    "AsignadoPorId" INTEGER NULL,
    "Notas" TEXT NULL,
    CONSTRAINT "FK_CursoAsignaciones_Cursos_CursoId" FOREIGN KEY ("CursoId") REFERENCES "Cursos" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_CursoAsignaciones_Empleados_EmpleadoId" FOREIGN KEY ("EmpleadoId") REFERENCES "Empleados" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_CursoAsignaciones_CursoId_EmpleadoId" ON "CursoAsignaciones" ("CursoId", "EmpleadoId");

CREATE INDEX "IX_CursoAsignaciones_EmpleadoId" ON "CursoAsignaciones" ("EmpleadoId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260420132357_SyncModelSnapshot', '10.0.3');

COMMIT;

BEGIN TRANSACTION;
CREATE TABLE "EscalasValoracion" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_EscalasValoracion" PRIMARY KEY AUTOINCREMENT,
    "Etiqueta" TEXT NOT NULL,
    "ValorNumerico" REAL NULL,
    "Orden" INTEGER NOT NULL,
    "Activo" INTEGER NOT NULL,
    "CreadoEn" TEXT NOT NULL,
    "ActualizadoEn" TEXT NOT NULL
);

CREATE TABLE "EstadosObjetivoConfig" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_EstadosObjetivoConfig" PRIMARY KEY AUTOINCREMENT,
    "Nombre" TEXT NOT NULL,
    "Slug" TEXT NOT NULL,
    "ColorHex" TEXT NOT NULL,
    "Orden" INTEGER NOT NULL,
    "Activo" INTEGER NOT NULL,
    "CreadoEn" TEXT NOT NULL
);

CREATE TABLE "EstadosEvaluacionConfig" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_EstadosEvaluacionConfig" PRIMARY KEY AUTOINCREMENT,
    "Nombre" TEXT NOT NULL,
    "Slug" TEXT NOT NULL,
    "ColorHex" TEXT NOT NULL,
    "Orden" INTEGER NOT NULL,
    "Activo" INTEGER NOT NULL,
    "CreadoEn" TEXT NOT NULL
);

CREATE TABLE "ConfiguracionesPlataforma" (
    "Clave" TEXT NOT NULL CONSTRAINT "PK_ConfiguracionesPlataforma" PRIMARY KEY,
    "Valor" TEXT NOT NULL,
    "Descripcion" TEXT NOT NULL,
    "Tipo" TEXT NOT NULL,
    "ActualizadoEn" TEXT NOT NULL,
    "ActualizadoPorId" INTEGER NULL
);

ALTER TABLE "Pilares" ADD "Activo" INTEGER NOT NULL DEFAULT 1;

ALTER TABLE "Pilares" ADD "Orden" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Pilares" ADD "CreadoEn" TEXT NOT NULL DEFAULT '2026-04-28 09:49:21';

ALTER TABLE "Pilares" ADD "ActualizadoEn" TEXT NOT NULL DEFAULT '2026-04-28 09:49:21';

ALTER TABLE "SoftSkills" ADD "Activo" INTEGER NOT NULL DEFAULT 1;

ALTER TABLE "SoftSkills" ADD "Orden" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "SoftSkills" ADD "CreadoEn" TEXT NOT NULL DEFAULT '2026-04-28 09:49:21';

ALTER TABLE "SoftSkills" ADD "ActualizadoEn" TEXT NOT NULL DEFAULT '2026-04-28 09:49:21';

ALTER TABLE "Areas" ADD "Activo" INTEGER NOT NULL DEFAULT 1;

ALTER TABLE "Areas" ADD "CreadoEn" TEXT NOT NULL DEFAULT '2026-04-28 09:49:21';

ALTER TABLE "Areas" ADD "ActualizadoEn" TEXT NOT NULL DEFAULT '2026-04-28 09:49:21';

ALTER TABLE "Objetivos" ADD "PorcentajePilar" REAL NOT NULL DEFAULT '0.0';

ALTER TABLE "Objetivos" ADD "AprobadoPorJefe" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Objetivos" ADD "EstadoObjetivoConfigId" INTEGER NULL;

ALTER TABLE "RevisionesCuatrimestrales" ADD "EscalaValoracionId" INTEGER NULL;

ALTER TABLE "RevisionesCuatrimestrales" ADD "EstadoEvaluacionConfigId" INTEGER NULL;

ALTER TABLE "RevisionesCuatrimestrales" ADD "SoftSkill1EscalaValoracionId" INTEGER NULL;

ALTER TABLE "RevisionesCuatrimestrales" ADD "SoftSkill2EscalaValoracionId" INTEGER NULL;

ALTER TABLE "EvaluacionesFinales" ADD "EscalaValoracionIdFinal" INTEGER NULL;

ALTER TABLE "EvaluacionesFinales" ADD "EstadoEvaluacionConfigId" INTEGER NULL;

ALTER TABLE "EvaluacionesFinales" ADD "SoftSkill1EscalaValoracionId" INTEGER NULL;

ALTER TABLE "EvaluacionesFinales" ADD "SoftSkill2EscalaValoracionId" INTEGER NULL;

ALTER TABLE "Autoevaluaciones" ADD "EscalaValoracionIdScore" INTEGER NULL;

ALTER TABLE "Autoevaluaciones" ADD "EstadoEvaluacionConfigId" INTEGER NULL;

ALTER TABLE "Autoevaluaciones" ADD "SoftSkill1EscalaValoracionId" INTEGER NULL;

ALTER TABLE "Autoevaluaciones" ADD "SoftSkill2EscalaValoracionId" INTEGER NULL;

CREATE INDEX "IX_EscalasValoracion_Orden" ON "EscalasValoracion" ("Orden");

CREATE UNIQUE INDEX "IX_EstadosObjetivoConfig_Slug" ON "EstadosObjetivoConfig" ("Slug");

CREATE INDEX "IX_EstadosObjetivoConfig_Orden" ON "EstadosObjetivoConfig" ("Orden");

CREATE UNIQUE INDEX "IX_EstadosEvaluacionConfig_Slug" ON "EstadosEvaluacionConfig" ("Slug");

CREATE INDEX "IX_EstadosEvaluacionConfig_Orden" ON "EstadosEvaluacionConfig" ("Orden");

CREATE INDEX "IX_Pilares_Orden" ON "Pilares" ("Orden");

CREATE INDEX "IX_SoftSkills_Orden" ON "SoftSkills" ("Orden");

CREATE INDEX "IX_Objetivos_EstadoObjetivoConfigId" ON "Objetivos" ("EstadoObjetivoConfigId");

CREATE INDEX "IX_RevisionesCuatrimestrales_EscalaValoracionId" ON "RevisionesCuatrimestrales" ("EscalaValoracionId");

CREATE INDEX "IX_RevisionesCuatrimestrales_EstadoEvaluacionConfigId" ON "RevisionesCuatrimestrales" ("EstadoEvaluacionConfigId");

CREATE INDEX "IX_RevisionesCuatrimestrales_SoftSkill1EscalaValoracionId" ON "RevisionesCuatrimestrales" ("SoftSkill1EscalaValoracionId");

CREATE INDEX "IX_RevisionesCuatrimestrales_SoftSkill2EscalaValoracionId" ON "RevisionesCuatrimestrales" ("SoftSkill2EscalaValoracionId");

CREATE INDEX "IX_EvaluacionesFinales_EscalaValoracionIdFinal" ON "EvaluacionesFinales" ("EscalaValoracionIdFinal");

CREATE INDEX "IX_EvaluacionesFinales_EstadoEvaluacionConfigId" ON "EvaluacionesFinales" ("EstadoEvaluacionConfigId");

CREATE INDEX "IX_EvaluacionesFinales_SoftSkill1EscalaValoracionId" ON "EvaluacionesFinales" ("SoftSkill1EscalaValoracionId");

CREATE INDEX "IX_EvaluacionesFinales_SoftSkill2EscalaValoracionId" ON "EvaluacionesFinales" ("SoftSkill2EscalaValoracionId");

CREATE INDEX "IX_Autoevaluaciones_EscalaValoracionIdScore" ON "Autoevaluaciones" ("EscalaValoracionIdScore");

CREATE INDEX "IX_Autoevaluaciones_EstadoEvaluacionConfigId" ON "Autoevaluaciones" ("EstadoEvaluacionConfigId");

CREATE INDEX "IX_Autoevaluaciones_SoftSkill1EscalaValoracionId" ON "Autoevaluaciones" ("SoftSkill1EscalaValoracionId");

CREATE INDEX "IX_Autoevaluaciones_SoftSkill2EscalaValoracionId" ON "Autoevaluaciones" ("SoftSkill2EscalaValoracionId");

CREATE TABLE "ef_temp_Objetivos" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Objetivos" PRIMARY KEY AUTOINCREMENT,
    "Anio" INTEGER NOT NULL,
    "AprobadoPorJefe" INTEGER NOT NULL,
    "CreadoPorId" INTEGER NOT NULL,
    "Deadline" TEXT NOT NULL,
    "Descripcion" TEXT NOT NULL,
    "EmpleadoId" INTEGER NOT NULL,
    "Estado" INTEGER NOT NULL,
    "EstadoObjetivoConfigId" INTEGER NULL,
    "FechaCreacion" TEXT NOT NULL,
    "Nombre" TEXT NOT NULL,
    "PilarId" INTEGER NOT NULL,
    "PorcentajePilar" REAL NOT NULL,
    "Progreso" INTEGER NOT NULL,
    "SoftSkill1Id" INTEGER NOT NULL,
    "SoftSkill2Id" INTEGER NOT NULL,
    CONSTRAINT "FK_Objetivos_Empleados_EmpleadoId" FOREIGN KEY ("EmpleadoId") REFERENCES "Empleados" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Objetivos_EstadosObjetivoConfig_EstadoObjetivoConfigId" FOREIGN KEY ("EstadoObjetivoConfigId") REFERENCES "EstadosObjetivoConfig" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Objetivos_Pilares_PilarId" FOREIGN KEY ("PilarId") REFERENCES "Pilares" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Objetivos_SoftSkills_SoftSkill1Id" FOREIGN KEY ("SoftSkill1Id") REFERENCES "SoftSkills" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Objetivos_SoftSkills_SoftSkill2Id" FOREIGN KEY ("SoftSkill2Id") REFERENCES "SoftSkills" ("Id") ON DELETE RESTRICT
);

INSERT INTO "ef_temp_Objetivos" ("Id", "Anio", "AprobadoPorJefe", "CreadoPorId", "Deadline", "Descripcion", "EmpleadoId", "Estado", "EstadoObjetivoConfigId", "FechaCreacion", "Nombre", "PilarId", "PorcentajePilar", "Progreso", "SoftSkill1Id", "SoftSkill2Id")
SELECT "Id", "Anio", "AprobadoPorJefe", "CreadoPorId", "Deadline", "Descripcion", "EmpleadoId", "Estado", "EstadoObjetivoConfigId", "FechaCreacion", "Nombre", "PilarId", "PorcentajePilar", "Progreso", "SoftSkill1Id", "SoftSkill2Id"
FROM "Objetivos";

CREATE TABLE "ef_temp_RevisionesCuatrimestrales" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_RevisionesCuatrimestrales" PRIMARY KEY AUTOINCREMENT,
    "Anio" INTEGER NOT NULL,
    "ComentarioJefe" TEXT NOT NULL,
    "Completada" INTEGER NOT NULL,
    "EscalaValoracionId" INTEGER NULL,
    "EstadoEvaluacionConfigId" INTEGER NULL,
    "EvaluadorId" INTEGER NULL,
    "EvidenciasRevisadasJson" TEXT NOT NULL,
    "FechaEvaluacion" TEXT NULL,
    "ObjetivoId" INTEGER NOT NULL,
    "Periodo" INTEGER NOT NULL,
    "Puntaje" INTEGER NULL,
    "Resultado" INTEGER NULL,
    "SoftSkill1Comentario" TEXT NOT NULL,
    "SoftSkill1EscalaValoracionId" INTEGER NULL,
    "SoftSkill1Puntaje" INTEGER NULL,
    "SoftSkill2Comentario" TEXT NOT NULL,
    "SoftSkill2EscalaValoracionId" INTEGER NULL,
    "SoftSkill2Puntaje" INTEGER NULL,
    CONSTRAINT "FK_RevisionesCuatrimestrales_EscalasValoracion_EscalaValoracionId" FOREIGN KEY ("EscalaValoracionId") REFERENCES "EscalasValoracion" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_RevisionesCuatrimestrales_EscalasValoracion_SoftSkill1EscalaValoracionId" FOREIGN KEY ("SoftSkill1EscalaValoracionId") REFERENCES "EscalasValoracion" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_RevisionesCuatrimestrales_EscalasValoracion_SoftSkill2EscalaValoracionId" FOREIGN KEY ("SoftSkill2EscalaValoracionId") REFERENCES "EscalasValoracion" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_RevisionesCuatrimestrales_EstadosEvaluacionConfig_EstadoEvaluacionConfigId" FOREIGN KEY ("EstadoEvaluacionConfigId") REFERENCES "EstadosEvaluacionConfig" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_RevisionesCuatrimestrales_Objetivos_ObjetivoId" FOREIGN KEY ("ObjetivoId") REFERENCES "Objetivos" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_RevisionesCuatrimestrales" ("Id", "Anio", "ComentarioJefe", "Completada", "EscalaValoracionId", "EstadoEvaluacionConfigId", "EvaluadorId", "EvidenciasRevisadasJson", "FechaEvaluacion", "ObjetivoId", "Periodo", "Puntaje", "Resultado", "SoftSkill1Comentario", "SoftSkill1EscalaValoracionId", "SoftSkill1Puntaje", "SoftSkill2Comentario", "SoftSkill2EscalaValoracionId", "SoftSkill2Puntaje")
SELECT "Id", "Anio", "ComentarioJefe", "Completada", "EscalaValoracionId", "EstadoEvaluacionConfigId", "EvaluadorId", "EvidenciasRevisadasJson", "FechaEvaluacion", "ObjetivoId", "Periodo", "Puntaje", "Resultado", "SoftSkill1Comentario", "SoftSkill1EscalaValoracionId", "SoftSkill1Puntaje", "SoftSkill2Comentario", "SoftSkill2EscalaValoracionId", "SoftSkill2Puntaje"
FROM "RevisionesCuatrimestrales";

CREATE TABLE "ef_temp_EvaluacionesFinales" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_EvaluacionesFinales" PRIMARY KEY AUTOINCREMENT,
    "Anio" INTEGER NOT NULL,
    "ComentarioJefe" TEXT NOT NULL,
    "EscalaValoracionIdFinal" INTEGER NOT NULL,
    "EstadoEvaluacionConfigId" INTEGER NULL,
    "EvaluadorId" INTEGER NOT NULL,
    "FechaEvaluacion" TEXT NOT NULL,
    "ObjetivoId" INTEGER NOT NULL,
    "PuntajeFinal" REAL NOT NULL,
    "ResultadoFinal" INTEGER NOT NULL,
    "SoftSkill1Comentario" TEXT NOT NULL,
    "SoftSkill1EscalaValoracionId" INTEGER NULL,
    "SoftSkill1Puntaje" INTEGER NULL,
    "SoftSkill2Comentario" TEXT NOT NULL,
    "SoftSkill2EscalaValoracionId" INTEGER NULL,
    "SoftSkill2Puntaje" INTEGER NULL,
    CONSTRAINT "FK_EvaluacionesFinales_EscalasValoracion_EscalaValoracionIdFinal" FOREIGN KEY ("EscalaValoracionIdFinal") REFERENCES "EscalasValoracion" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_EvaluacionesFinales_EscalasValoracion_SoftSkill1EscalaValoracionId" FOREIGN KEY ("SoftSkill1EscalaValoracionId") REFERENCES "EscalasValoracion" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_EvaluacionesFinales_EscalasValoracion_SoftSkill2EscalaValoracionId" FOREIGN KEY ("SoftSkill2EscalaValoracionId") REFERENCES "EscalasValoracion" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_EvaluacionesFinales_EstadosEvaluacionConfig_EstadoEvaluacionConfigId" FOREIGN KEY ("EstadoEvaluacionConfigId") REFERENCES "EstadosEvaluacionConfig" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_EvaluacionesFinales_Objetivos_ObjetivoId" FOREIGN KEY ("ObjetivoId") REFERENCES "Objetivos" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_EvaluacionesFinales" ("Id", "Anio", "ComentarioJefe", "EscalaValoracionIdFinal", "EstadoEvaluacionConfigId", "EvaluadorId", "FechaEvaluacion", "ObjetivoId", "PuntajeFinal", "ResultadoFinal", "SoftSkill1Comentario", "SoftSkill1EscalaValoracionId", "SoftSkill1Puntaje", "SoftSkill2Comentario", "SoftSkill2EscalaValoracionId", "SoftSkill2Puntaje")
SELECT "Id", "Anio", "ComentarioJefe", "EscalaValoracionIdFinal", "EstadoEvaluacionConfigId", "EvaluadorId", "FechaEvaluacion", "ObjetivoId", "PuntajeFinal", "ResultadoFinal", "SoftSkill1Comentario", "SoftSkill1EscalaValoracionId", "SoftSkill1Puntaje", "SoftSkill2Comentario", "SoftSkill2EscalaValoracionId", "SoftSkill2Puntaje"
FROM "EvaluacionesFinales";

CREATE TABLE "ef_temp_Autoevaluaciones" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Autoevaluaciones" PRIMARY KEY AUTOINCREMENT,
    "ArchivosAdjuntosJson" TEXT NOT NULL,
    "Comentario" TEXT NOT NULL,
    "EmpleadoId" INTEGER NOT NULL,
    "EscalaValoracionIdScore" INTEGER NOT NULL,
    "EstadoEvaluacionConfigId" INTEGER NULL,
    "EvidenciasMencionadasJson" TEXT NOT NULL,
    "FechaAutoevaluacion" TEXT NOT NULL,
    "ObjetivoId" INTEGER NOT NULL,
    "Score" INTEGER NOT NULL,
    "SoftSkill1Comentario" TEXT NOT NULL,
    "SoftSkill1EscalaValoracionId" INTEGER NULL,
    "SoftSkill1Score" INTEGER NOT NULL,
    "SoftSkill2Comentario" TEXT NOT NULL,
    "SoftSkill2EscalaValoracionId" INTEGER NULL,
    "SoftSkill2Score" INTEGER NOT NULL,
    CONSTRAINT "FK_Autoevaluaciones_EscalasValoracion_EscalaValoracionIdScore" FOREIGN KEY ("EscalaValoracionIdScore") REFERENCES "EscalasValoracion" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Autoevaluaciones_EscalasValoracion_SoftSkill1EscalaValoracionId" FOREIGN KEY ("SoftSkill1EscalaValoracionId") REFERENCES "EscalasValoracion" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Autoevaluaciones_EscalasValoracion_SoftSkill2EscalaValoracionId" FOREIGN KEY ("SoftSkill2EscalaValoracionId") REFERENCES "EscalasValoracion" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Autoevaluaciones_EstadosEvaluacionConfig_EstadoEvaluacionConfigId" FOREIGN KEY ("EstadoEvaluacionConfigId") REFERENCES "EstadosEvaluacionConfig" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Autoevaluaciones_Objetivos_ObjetivoId" FOREIGN KEY ("ObjetivoId") REFERENCES "Objetivos" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_Autoevaluaciones" ("Id", "ArchivosAdjuntosJson", "Comentario", "EmpleadoId", "EscalaValoracionIdScore", "EstadoEvaluacionConfigId", "EvidenciasMencionadasJson", "FechaAutoevaluacion", "ObjetivoId", "Score", "SoftSkill1Comentario", "SoftSkill1EscalaValoracionId", "SoftSkill1Score", "SoftSkill2Comentario", "SoftSkill2EscalaValoracionId", "SoftSkill2Score")
SELECT "Id", "ArchivosAdjuntosJson", "Comentario", "EmpleadoId", "EscalaValoracionIdScore", "EstadoEvaluacionConfigId", "EvidenciasMencionadasJson", "FechaAutoevaluacion", "ObjetivoId", "Score", "SoftSkill1Comentario", "SoftSkill1EscalaValoracionId", "SoftSkill1Score", "SoftSkill2Comentario", "SoftSkill2EscalaValoracionId", "SoftSkill2Score"
FROM "Autoevaluaciones";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "Objetivos";

ALTER TABLE "ef_temp_Objetivos" RENAME TO "Objetivos";

DROP TABLE "RevisionesCuatrimestrales";

ALTER TABLE "ef_temp_RevisionesCuatrimestrales" RENAME TO "RevisionesCuatrimestrales";

DROP TABLE "EvaluacionesFinales";

ALTER TABLE "ef_temp_EvaluacionesFinales" RENAME TO "EvaluacionesFinales";

DROP TABLE "Autoevaluaciones";

ALTER TABLE "ef_temp_Autoevaluaciones" RENAME TO "Autoevaluaciones";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_Objetivos_EmpleadoId" ON "Objetivos" ("EmpleadoId");

CREATE INDEX "IX_Objetivos_EstadoObjetivoConfigId" ON "Objetivos" ("EstadoObjetivoConfigId");

CREATE UNIQUE INDEX "IX_Objetivos_PilarId_EmpleadoId_Anio" ON "Objetivos" ("PilarId", "EmpleadoId", "Anio");

CREATE INDEX "IX_Objetivos_SoftSkill1Id" ON "Objetivos" ("SoftSkill1Id");

CREATE INDEX "IX_Objetivos_SoftSkill2Id" ON "Objetivos" ("SoftSkill2Id");

CREATE INDEX "IX_RevisionesCuatrimestrales_EscalaValoracionId" ON "RevisionesCuatrimestrales" ("EscalaValoracionId");

CREATE INDEX "IX_RevisionesCuatrimestrales_EstadoEvaluacionConfigId" ON "RevisionesCuatrimestrales" ("EstadoEvaluacionConfigId");

CREATE INDEX "IX_RevisionesCuatrimestrales_ObjetivoId" ON "RevisionesCuatrimestrales" ("ObjetivoId");

CREATE INDEX "IX_RevisionesCuatrimestrales_SoftSkill1EscalaValoracionId" ON "RevisionesCuatrimestrales" ("SoftSkill1EscalaValoracionId");

CREATE INDEX "IX_RevisionesCuatrimestrales_SoftSkill2EscalaValoracionId" ON "RevisionesCuatrimestrales" ("SoftSkill2EscalaValoracionId");

CREATE INDEX "IX_EvaluacionesFinales_EscalaValoracionIdFinal" ON "EvaluacionesFinales" ("EscalaValoracionIdFinal");

CREATE INDEX "IX_EvaluacionesFinales_EstadoEvaluacionConfigId" ON "EvaluacionesFinales" ("EstadoEvaluacionConfigId");

CREATE UNIQUE INDEX "IX_EvaluacionesFinales_ObjetivoId" ON "EvaluacionesFinales" ("ObjetivoId");

CREATE INDEX "IX_EvaluacionesFinales_SoftSkill1EscalaValoracionId" ON "EvaluacionesFinales" ("SoftSkill1EscalaValoracionId");

CREATE INDEX "IX_EvaluacionesFinales_SoftSkill2EscalaValoracionId" ON "EvaluacionesFinales" ("SoftSkill2EscalaValoracionId");

CREATE INDEX "IX_Autoevaluaciones_EscalaValoracionIdScore" ON "Autoevaluaciones" ("EscalaValoracionIdScore");

CREATE INDEX "IX_Autoevaluaciones_EstadoEvaluacionConfigId" ON "Autoevaluaciones" ("EstadoEvaluacionConfigId");

CREATE UNIQUE INDEX "IX_Autoevaluaciones_ObjetivoId" ON "Autoevaluaciones" ("ObjetivoId");

CREATE INDEX "IX_Autoevaluaciones_SoftSkill1EscalaValoracionId" ON "Autoevaluaciones" ("SoftSkill1EscalaValoracionId");

CREATE INDEX "IX_Autoevaluaciones_SoftSkill2EscalaValoracionId" ON "Autoevaluaciones" ("SoftSkill2EscalaValoracionId");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260428094921_AddAdminTablesAndDynamicConfig', '10.0.3');

BEGIN TRANSACTION;
ALTER TABLE "Objetivos" ADD "AreaEspecificaId" INTEGER NULL;

ALTER TABLE "Objetivos" ADD "PorcentajeArea" TEXT NOT NULL DEFAULT '0.0';

CREATE INDEX "IX_Objetivos_AreaEspecificaId" ON "Objetivos" ("AreaEspecificaId");

CREATE TABLE "ef_temp_Objetivos" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Objetivos" PRIMARY KEY AUTOINCREMENT,
    "Anio" INTEGER NOT NULL,
    "AprobadoPorJefe" INTEGER NOT NULL,
    "AreaEspecificaId" INTEGER NULL,
    "CreadoPorId" INTEGER NOT NULL,
    "Deadline" TEXT NOT NULL,
    "Descripcion" TEXT NOT NULL,
    "EmpleadoId" INTEGER NOT NULL,
    "Estado" INTEGER NOT NULL,
    "EstadoObjetivoConfigId" INTEGER NULL,
    "FechaCreacion" TEXT NOT NULL,
    "Nombre" TEXT NOT NULL,
    "PilarId" INTEGER NOT NULL,
    "PorcentajeArea" TEXT NOT NULL,
    "PorcentajePilar" TEXT NOT NULL,
    "Progreso" INTEGER NOT NULL,
    "SoftSkill1Id" INTEGER NOT NULL,
    "SoftSkill2Id" INTEGER NOT NULL,
    CONSTRAINT "FK_Objetivos_Areas_AreaEspecificaId" FOREIGN KEY ("AreaEspecificaId") REFERENCES "Areas" ("Id"),
    CONSTRAINT "FK_Objetivos_Empleados_EmpleadoId" FOREIGN KEY ("EmpleadoId") REFERENCES "Empleados" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Objetivos_EstadosObjetivoConfig_EstadoObjetivoConfigId" FOREIGN KEY ("EstadoObjetivoConfigId") REFERENCES "EstadosObjetivoConfig" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Objetivos_Pilares_PilarId" FOREIGN KEY ("PilarId") REFERENCES "Pilares" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Objetivos_SoftSkills_SoftSkill1Id" FOREIGN KEY ("SoftSkill1Id") REFERENCES "SoftSkills" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Objetivos_SoftSkills_SoftSkill2Id" FOREIGN KEY ("SoftSkill2Id") REFERENCES "SoftSkills" ("Id") ON DELETE RESTRICT
);

INSERT INTO "ef_temp_Objetivos" ("Id", "Anio", "AprobadoPorJefe", "AreaEspecificaId", "CreadoPorId", "Deadline", "Descripcion", "EmpleadoId", "Estado", "EstadoObjetivoConfigId", "FechaCreacion", "Nombre", "PilarId", "PorcentajeArea", "PorcentajePilar", "Progreso", "SoftSkill1Id", "SoftSkill2Id")
SELECT "Id", "Anio", "AprobadoPorJefe", "AreaEspecificaId", "CreadoPorId", "Deadline", "Descripcion", "EmpleadoId", "Estado", "EstadoObjetivoConfigId", "FechaCreacion", "Nombre", "PilarId", "PorcentajeArea", "PorcentajePilar", "Progreso", "SoftSkill1Id", "SoftSkill2Id"
FROM "Objetivos";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "Objetivos";

ALTER TABLE "ef_temp_Objetivos" RENAME TO "Objetivos";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_Objetivos_AreaEspecificaId" ON "Objetivos" ("AreaEspecificaId");

CREATE INDEX "IX_Objetivos_EmpleadoId" ON "Objetivos" ("EmpleadoId");

CREATE INDEX "IX_Objetivos_EstadoObjetivoConfigId" ON "Objetivos" ("EstadoObjetivoConfigId");

CREATE UNIQUE INDEX "IX_Objetivos_PilarId_EmpleadoId_Anio" ON "Objetivos" ("PilarId", "EmpleadoId", "Anio");

CREATE INDEX "IX_Objetivos_SoftSkill1Id" ON "Objetivos" ("SoftSkill1Id");

CREATE INDEX "IX_Objetivos_SoftSkill2Id" ON "Objetivos" ("SoftSkill2Id");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260428135626_AddAreaEspecificaToObjetivo', '10.0.3');

BEGIN TRANSACTION;
ALTER TABLE "Pilares" ADD "EsGlobal" INTEGER NOT NULL DEFAULT 0;

CREATE TABLE "AreaPilares" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AreaPilares" PRIMARY KEY AUTOINCREMENT,
    "AreaId" INTEGER NOT NULL,
    "PilarId" INTEGER NOT NULL,
    "PesoPorcentual" TEXT NOT NULL,
    "EsObligatorio" INTEGER NOT NULL,
    CONSTRAINT "FK_AreaPilares_Areas_AreaId" FOREIGN KEY ("AreaId") REFERENCES "Areas" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AreaPilares_Pilares_PilarId" FOREIGN KEY ("PilarId") REFERENCES "Pilares" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_AreaPilares_AreaId_PilarId" ON "AreaPilares" ("AreaId", "PilarId");

CREATE INDEX "IX_AreaPilares_PilarId" ON "AreaPilares" ("PilarId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260511134001_AddAreaPilarSupport', '10.0.3');

COMMIT;

BEGIN TRANSACTION;
DROP TABLE "AreaPilares";

ALTER TABLE "Pilares" RENAME COLUMN "EsGlobal" TO "EsObligatorio";

ALTER TABLE "Pilares" ADD "AreaId" INTEGER NULL;

ALTER TABLE "Pilares" ADD "PesoPorcentual" TEXT NOT NULL DEFAULT '0.0';

CREATE INDEX "IX_Pilares_AreaId" ON "Pilares" ("AreaId");

CREATE TABLE "ef_temp_Pilares" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Pilares" PRIMARY KEY AUTOINCREMENT,
    "Activo" INTEGER NOT NULL,
    "ActualizadoEn" TEXT NOT NULL,
    "AreaId" INTEGER NULL,
    "ColorHex" TEXT NOT NULL,
    "CreadoEn" TEXT NOT NULL,
    "Descripcion" TEXT NOT NULL,
    "EsObligatorio" INTEGER NOT NULL,
    "Nombre" TEXT NOT NULL,
    "Orden" INTEGER NOT NULL,
    "PesoPorcentual" TEXT NOT NULL,
    CONSTRAINT "FK_Pilares_Areas_AreaId" FOREIGN KEY ("AreaId") REFERENCES "Areas" ("Id") ON DELETE RESTRICT
);

INSERT INTO "ef_temp_Pilares" ("Id", "Activo", "ActualizadoEn", "AreaId", "ColorHex", "CreadoEn", "Descripcion", "EsObligatorio", "Nombre", "Orden", "PesoPorcentual")
SELECT "Id", "Activo", "ActualizadoEn", "AreaId", "ColorHex", "CreadoEn", "Descripcion", "EsObligatorio", "Nombre", "Orden", "PesoPorcentual"
FROM "Pilares";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "Pilares";

ALTER TABLE "ef_temp_Pilares" RENAME TO "Pilares";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_Pilares_AreaId" ON "Pilares" ("AreaId");

CREATE INDEX "IX_Pilares_Orden" ON "Pilares" ("Orden");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260511135025_SimplifyPilarAreaRelationship', '10.0.3');

BEGIN TRANSACTION;
CREATE TABLE "ef_temp_Pilares" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Pilares" PRIMARY KEY AUTOINCREMENT,
    "Activo" INTEGER NOT NULL,
    "ActualizadoEn" TEXT NOT NULL,
    "AreaId" INTEGER NULL,
    "ColorHex" TEXT NOT NULL,
    "CreadoEn" TEXT NOT NULL,
    "Descripcion" TEXT NOT NULL,
    "EsObligatorio" INTEGER NOT NULL,
    "Nombre" TEXT NOT NULL,
    "Orden" INTEGER NOT NULL,
    CONSTRAINT "FK_Pilares_Areas_AreaId" FOREIGN KEY ("AreaId") REFERENCES "Areas" ("Id") ON DELETE RESTRICT
);

INSERT INTO "ef_temp_Pilares" ("Id", "Activo", "ActualizadoEn", "AreaId", "ColorHex", "CreadoEn", "Descripcion", "EsObligatorio", "Nombre", "Orden")
SELECT "Id", "Activo", "ActualizadoEn", "AreaId", "ColorHex", "CreadoEn", "Descripcion", "EsObligatorio", "Nombre", "Orden"
FROM "Pilares";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "Pilares";

ALTER TABLE "ef_temp_Pilares" RENAME TO "Pilares";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_Pilares_AreaId" ON "Pilares" ("AreaId");

CREATE INDEX "IX_Pilares_Orden" ON "Pilares" ("Orden");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260514121411_Stage0IntegrityFixes', '10.0.3');

BEGIN TRANSACTION;
DROP INDEX "IX_Objetivos_PilarId_EmpleadoId_Anio";

CREATE INDEX "IX_Objetivos_PilarId_EmpleadoId_Anio" ON "Objetivos" ("PilarId", "EmpleadoId", "Anio");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260514144947_M15_RemoveUniqueObjetivoPilar', '10.0.3');

COMMIT;

BEGIN TRANSACTION;
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260514150000_RemoveUniqueObjetivoPilar', '10.0.3');

COMMIT;

BEGIN TRANSACTION;
CREATE TABLE "Puestos" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Puestos" PRIMARY KEY AUTOINCREMENT,
    "Nombre" TEXT NOT NULL,
    "Descripcion" TEXT NOT NULL,
    "Activo" INTEGER NOT NULL,
    "CreadoEn" TEXT NOT NULL,
    "ActualizadoEn" TEXT NOT NULL
);


                INSERT INTO Puestos (Nombre, Descripcion, Activo, CreadoEn, ActualizadoEn)
                SELECT DISTINCT Puesto, Puesto, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP 
                FROM Empleados 
                WHERE Puesto IS NOT NULL AND Puesto != '';
            

ALTER TABLE "Empleados" ADD "PuestoId" INTEGER NULL;


                UPDATE Empleados
                SET PuestoId = (SELECT Id FROM Puestos WHERE Puestos.Nombre = Empleados.Puesto)
                WHERE Puesto IS NOT NULL AND Puesto != '';
            

CREATE INDEX "IX_Empleados_PuestoId" ON "Empleados" ("PuestoId");

CREATE TABLE "ef_temp_Empleados" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Empleados" PRIMARY KEY AUTOINCREMENT,
    "Activo" INTEGER NOT NULL,
    "Apellido" TEXT NOT NULL,
    "AreaId" INTEGER NOT NULL,
    "DebeCambiarPassword" INTEGER NOT NULL,
    "Email" TEXT NOT NULL,
    "EsSuperusuario" INTEGER NOT NULL,
    "FechaIngreso" TEXT NOT NULL,
    "JefeId" INTEGER NOT NULL,
    "Legajo" TEXT NOT NULL,
    "Nombre" TEXT NOT NULL,
    "PaisId" INTEGER NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "PuestoId" INTEGER NULL,
    CONSTRAINT "FK_Empleados_Areas_AreaId" FOREIGN KEY ("AreaId") REFERENCES "Areas" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Empleados_Jefes_JefeId" FOREIGN KEY ("JefeId") REFERENCES "Jefes" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Empleados_Paises_PaisId" FOREIGN KEY ("PaisId") REFERENCES "Paises" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Empleados_Puestos_PuestoId" FOREIGN KEY ("PuestoId") REFERENCES "Puestos" ("Id") ON DELETE SET NULL
);

INSERT INTO "ef_temp_Empleados" ("Id", "Activo", "Apellido", "AreaId", "DebeCambiarPassword", "Email", "EsSuperusuario", "FechaIngreso", "JefeId", "Legajo", "Nombre", "PaisId", "PasswordHash", "PuestoId")
SELECT "Id", "Activo", "Apellido", "AreaId", "DebeCambiarPassword", "Email", "EsSuperusuario", "FechaIngreso", "JefeId", "Legajo", "Nombre", "PaisId", "PasswordHash", "PuestoId"
FROM "Empleados";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "Empleados";

ALTER TABLE "ef_temp_Empleados" RENAME TO "Empleados";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_Empleados_AreaId" ON "Empleados" ("AreaId");

CREATE UNIQUE INDEX "IX_Empleados_Email" ON "Empleados" ("Email");

CREATE INDEX "IX_Empleados_JefeId" ON "Empleados" ("JefeId");

CREATE INDEX "IX_Empleados_PaisId" ON "Empleados" ("PaisId");

CREATE INDEX "IX_Empleados_PuestoId" ON "Empleados" ("PuestoId");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260527133325_AddPuestoEntity', '10.0.3');

BEGIN TRANSACTION;
ALTER TABLE "Jefes" ADD "FechaBaja" TEXT NULL;

ALTER TABLE "Empleados" ADD "FechaBaja" TEXT NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260609133153_AddFechaBaja', '10.0.3');

COMMIT;

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

