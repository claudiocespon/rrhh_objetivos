# AUDITORÍA EJECUTADA — RRHH_Objetivos
**Fecha:** 2026-03-30
**Estado:** Fase 1, 2 y 3 Completadas.

Este documento detalla las tareas realizadas para mitigar los hallazgos de la auditoría técnica.

---

## TAREAS COMPLETADAS

### FASE 1 — Seguridad Crítica
- **[SEC-04] Task 1.1:** Actualización de `.gitignore` para excluir bases de datos SQLite y archivos de nómina (CSV/XLSX).
- **[SEC-03] Task 1.2:** Reemplazo de hashing SHA-256 por **PBKDF2 con salt** (Rfc2898DeriveBytes) con 100,000 iteraciones.
- **[SEC-01] Task 1.3:** Eliminación de la exposición de contraseñas en texto plano en la recuperación. Se cambió la firma a `Task<bool>`.
- **[SEC-06] Task 1.4:** Migración de emails de superusuarios hardcodeados a `appsettings.json`.
- **[SEC-08] Task 1.5:** Ajuste de niveles de log en `appsettings.json` (Warning en prod, Debug en dev).
- **[SEC-05] Task 1.6:** Implementación de política de contraseña mínima (8 caracteres, letra y número).

### FASE 2 — Arquitectura
- **[ARQ-01] Task 2.1:** Creación de `EvaluacionService` y eliminación de inyección de `AppDbContext` en `Evaluacion/Index.razor`.
- **[ARQ-03] Task 2.2:** Centralización de lógica de visibilidad en `DataScopeService`. Aplicado en `ObjetivoService` y `DashboardService`.
- **[PAT-02] Task 2.3:** Refactorización de `DashboardService` para usar Inyección de Dependencias por constructor para `ICurrentUserService`.

### FASE 3 — Deuda Técnica
- **[DT-04] Task 3.1:** Resolución de N+1 queries en `RendimientoService.PromedioGeneralAsync()`.
- **[DT-01] Task 3.2:** Movimiento del seed de cursos de `CursoService` a `SeedData.InitializeAsync()`.
- **[DT-05] Task 3.3:** Corrección de logs de auditoría en cancelación de objetivos (Action: "CANCEL", JSON serializado).
- **[DT-06] Task 3.4:** Implementación de protección contra **CSV Injection** en `ExportService`.
- **[DT-03] Task 3.6:** Inclusión de WARNING en consola para fallbacks de asignación de jefe en el seed.

---

## ARCHIVOS MODIFICADOS

- `.gitignore`
- `Objetivos.Web/appsettings.json`
- `Objetivos.Web/appsettings.Development.json`
- `Objetivos.Web/Program.cs`
- `Objetivos.Web/Data/SeedData.cs`
- `Objetivos.Web/Services/AuthService.cs`
- `Objetivos.Web/Services/ObjetivoService.cs`
- `Objetivos.Web/Services/DashboardService.cs`
- `Objetivos.Web/Services/RendimientoService.cs`
- `Objetivos.Web/Services/CursoService.cs`
- `Objetivos.Web/Services/ExportService.cs`
- `Objetivos.Web/Services/EvaluacionService.cs` (Nuevo)
- `Objetivos.Web/Services/DataScopeService.cs` (Nuevo)
- `Objetivos.Web/Components/Pages/Login.razor`
- `Objetivos.Web/Components/Pages/Dashboard.razor`
- `Objetivos.Web/Components/Pages/Evaluacion/Index.razor`

---

## DEUDA REMANENTE (HALLAZGOS PENDIENTES)

Los siguientes hallazgos de la auditoría técnica requieren intervenciones mayores o decisiones de producto y no fueron cubiertos en esta fase de mitigación:

- **[SEC-02]** Implementación de ASP.NET Core Identity / Cookie Authentication real (actualmente se usa un servicio Scoped con estado).
- **[SEC-07]** Implementación de CSRF protection.
- **[ARQ-02]** Unificación de tablas `Jefe` y `Empleado` en una única tabla de `Usuario`.
- **[ARQ-04]** Tipado fuerte de Roles (Enum en lugar de string libre en DB).
- **[ARQ-05]** Consistencia total en el uso de `IDbContextFactory` en todos los servicios (algunos aún usan el Scoped DbContext inyectado).
- **[DT-02]** Conversión de JSON strings en entidades de dominio a objetos tipados usando EF Core Value Converters.
- **[DT-07]** Resolución de dependencia circular entre `RevisionService` y `ObjetivoService`.
- **[PAT-01]** Thread-safety de `ICurrentUserService` en Blazor Server.
- **[PAT-03]** Implementación de Repositorios / Unit of Work para facilitar unit testing.
- **[DT-10]** Migración de `EnsureCreatedAsync()` a EF Core Migrations.
