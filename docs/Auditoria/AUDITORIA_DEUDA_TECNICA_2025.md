# AUDITORÍA DE DEUDA FUNCIONAL Y TÉCNICA — PQ-Talent (RRHH_Objetivos)

**Fecha:** 15 de Abril de 2026  
**Proyecto:** `C:\Development\Antigravity\RRHH_Objetivos`  
**Stack:** Blazor Server (.NET 10) + Radzen + EF Core + SQLite  
**Auditor:** Arquitectura de Software — Revisión profunda de código fuente

---

## RESUMEN EJECUTIVO

Se identificaron **38 hallazgos** clasificados en 4 niveles de severidad:

| Severidad | Cantidad | Descripción |
|-----------|----------|-------------|
| CRÍTICO   | 6        | Funcionalidad definida pero NO implementada, o lógica de negocio rota |
| ALTO      | 12       | Divergencias con CONTEXT.md, riesgos de integridad de datos |
| MEDIO     | 11       | Deuda técnica, código duplicado, patrones subóptimos |
| BAJO      | 9        | Mejoras de calidad, consistencia, UX |

---

## SECCIÓN 1 — HALLAZGOS CRÍTICOS (Funcionalidad ausente o rota)

### C-01: Modelo de Revisiones diverge del CONTEXT.md (Q1/Q2/Q3 → FEEDBACK_MITAD_ANIO)

**Archivo:** `Domain/Enums/Enums.cs`, `Services/ObjetivoService.cs`

**Problema:** El CONTEXT.md define 3 revisiones cuatrimestrales (Q1_ABRIL, Q2_AGOSTO, Q3_NOVIEMBRE) con pesos ponderados (0.2, 0.3, 0.3, 0.2). El código actual reemplazó esto por un único `PeriodoRevision.FEEDBACK_MITAD_ANIO`. Esto rompe:
- La fórmula de cálculo ponderado (RN-07) — ahora es 50/50 en vez de 20/30/30/20
- La secuencia obligatoria de revisiones (RN-02 — Q1 antes de Q2, Q2 antes de Q3)
- Los eventos de calendario por período
- La lógica de evaluación final que requería 3 revisiones completadas

**Decisión necesaria:** ¿Se mantiene el modelo simplificado (1 feedback) o se restaura el modelo original (3 cuatrimestrales)? El resto de la auditoría asume que la decisión ya fue tomada (1 feedback). Si se restaura Q1/Q2/Q3, la Etapa 1 es mucho más extensa.

**Impacto:** Toda la lógica de rendimiento, evaluación y progreso.

---

### C-02: Cursos — Módulo esqueleto sin funcionalidad real

**Archivos:** `Components/Pages/Cursos/Index.razor`, `Services/CursoService.cs`, `Domain/Entities/Entities.cs`

**Problema:** La página de Cursos solo lista cursos de la BD con columnas básicas. Falta por completo:
- No hay asignación de cursos a empleados (falta entidad `CursoEmpleado` o `InscripcionCurso`)
- No hay tracking de progreso por empleado
- No hay evaluaciones ni calificaciones por curso
- El botón "Ver Curso" no hace nada (no tiene `Click` handler con lógica)
- No hay estados (EN_CURSO, COMPLETADO, NO_INICIADO) como define el CONTEXT.md seed
- No hay relación curso-empleado en el modelo de datos
- El CONTEXT.md definía cursos mock hardcodeados con progreso y evaluaciones, pero la implementación solo tiene un CRUD vacío

**Severidad:** CRÍTICO — módulo entero definido pero no programado.

---

### C-03: Notificaciones — Solo INSERT, sin panel de visualización

**Archivos:** `Services/BitacoraService.cs`, `Components/Layout/MainLayout.razor`

**Problema:** Las notificaciones se crean correctamente en la BD, y el MainLayout muestra un badge con el conteo de no leídas. Sin embargo:
- Al hacer click en el ícono de notificaciones, se marcan TODAS como leídas sin mostrar cuáles eran
- No existe un panel/dropdown/página que liste las notificaciones del usuario
- No hay navegación desde la notificación al recurso relacionado (ej: ir al objetivo cuya bitácora requiere ajuste)
- El usuario nunca ve el contenido de sus notificaciones

**Severidad:** CRÍTICO — el usuario pierde información importante sin verla.

---

### C-04: Calendario de Eventos — Entidad existe, UI no existe

**Archivos:** `Domain/Entities/Entities.cs`, `Data/AppDbContext.cs`

**Problema:** La entidad `EventoCalendario` está modelada, se crean eventos al crear objetivos, pero:
- No existe ninguna página ni componente que muestre un calendario
- El CONTEXT.md especifica usar `RadzenScheduler`
- Los eventos de tipo FEEDBACK_MITAD_ANIO y EVALUACION_FINAL nunca se crean
- No hay vista de calendario en el Dashboard ni en ninguna otra parte

**Severidad:** CRÍTICO — funcionalidad definida en modelo pero sin UI.

---

### C-05: ExportService solo exporta CSV del equipo — falta export completo

**Archivo:** `Services/ExportService.cs`

**Problema:**
- Solo exporta objetivos a CSV, pero no incluye revisiones, evaluaciones finales ni autoevaluaciones
- No hay exportación de reporte individual de empleado
- No hay exportación para RRHH del estado general de la organización
- Los datos exportados no incluyen el puntaje ponderado ni el semáforo
- El botón de exportación solo es visible para RRHH/DG/Super, pero no hay opción de exportar "mis datos" para un empleado normal

---

### C-06: Autoevaluación — EmpleadoId se toma del CurrentUser sin validar Jefe vs Empleado

**Archivo:** `Components/Pages/Autoevaluaciones/AutoevaluarDialog.razor`

**Problema:** En línea `autoevaluacion.EmpleadoId = CurrentUser.UsuarioId`, si el CurrentUser es un Jefe, el `UsuarioId` corresponde a la tabla `Jefes`, no a `Empleados`. Esto corrompe la FK `EmpleadoId` porque apunta a un ID que puede no existir en la tabla Empleados o peor, apuntar al empleado equivocado.

**Impacto:** Integridad referencial rota en producción cuando un jefe intenta autoevaluarse.

---

## SECCIÓN 2 — HALLAZGOS DE SEVERIDAD ALTA

### A-01: Ruta de la página Objetivos no coincide con CONTEXT.md

**Archivo:** `Components/Pages/MisObjetivos/Index.razor`

**Problema:** La ruta es `@page "/misobjetivos"` pero el CONTEXT.md especifica `@page "/objetivos"`. El NavMenu apunta a `Path="misobjetivos"`. Esto rompe cualquier link externo o bookmark que use `/objetivos`.

---

### A-02: Página `@page "/objetivos"` faltante en la especificación de rutas

El CONTEXT.md define las rutas:
```
/dashboard, /objetivos, /seguimientos, /seguimientos/{id}, 
/seguimientos/{id}/objetivo/{id}, /autoevaluacion, /evaluacion, /cursos
```
La ruta `/objetivos` ahora es `/misobjetivos`. No hay redirect.

---

### A-03: ObjetivoService.CrearObjetivoAsync — No valida VAL-01 (duplicado pilar+empleado+año)

**Archivo:** `Services/ObjetivoService.cs`

**Problema:** La regla VAL-01 exige verificar si ya existe un objetivo para el mismo pilar+empleado+año y ofrecer reemplazo. El código actual no hace esta verificación antes del INSERT — confía únicamente en el índice UNIQUE de la BD, que lanzaría una excepción no controlada.

---

### A-04: ObjetivoService.CrearObjetivoAsync — No valida VAL-02/VAL-03 (SoftSkills)

**Archivo:** `Services/ObjetivoService.cs`

**Problema:** No se verifica que `SoftSkill1Id != SoftSkill2Id` en el servicio. Si el diálogo de creación no lo impide en UI, se puede guardar un objetivo con dos soft skills iguales.

---

### A-05: RevisionService.CompletarEvaluacionFinalAsync — Precondición de deadline removida intencionalmente

**Archivo:** `Services/RevisionService.cs` (línea ~79)

**Problema:** El código tiene `if (DateTime.Today < objetivo.Deadline) return false;` pero un comentario en `EvaluacionService.cs` dice "Removed DateTime restriction as per user request". Hay inconsistencia: el servicio sí verifica el deadline, pero la query de `EvaluacionService` ya muestra objetivos para evaluar antes del deadline. Esto resulta en que el botón "Evaluar Final" aparece pero falla silenciosamente si el deadline no pasó.

---

### A-06: RendimientoService.CalcularPonderadoInterno — Fórmula circular

**Archivo:** `Services/RendimientoService.cs`

**Problema:** La fórmula calcula `(feedback * 0.5) + (fin * 0.5)` donde `fin = EvaluacionFinal.PuntajeFinal`. Pero `CompletarEvaluacionFinalAsync` llama a `CalcularPonderadoAsync` para obtener el puntajeFinal antes de guardarlo. En ese momento `EvaluacionFinal` aún no existe, así que `fin = 0`, y el resultado es solo `feedback * 0.5`. El puntajeFinal guardado en la evaluación siempre será la mitad del feedback, nunca incorporará su propio valor (lo cual es correcto pero confuso — el nombre sugiere un promedio, pero en la práctica es solo 50% del feedback).

**Recomendación:** Documentar explícitamente la fórmula o ajustarla para que tenga sentido semántico.

---

### A-07: Inyección directa de AppDbContext en páginas Razor

**Archivos:** `Seguimientos/Index.razor`, `Seguimientos/EmpleadoDetalle.razor`, `Autoevaluaciones/Index.razor`, `Autoevaluaciones/AutoevaluarDialog.razor`, `Evaluacion/EvaluarDialog.razor`, `Evaluacion/EvaluarFinalDialog.razor`

**Problema:** Múltiples páginas inyectan `@inject AppDbContext Db` directamente y ejecutan queries EF Core con Includes complejos. Esto viola la arquitectura definida en CONTEXT.md ("Servicios directos inyectados en páginas Razor — toda la lógica en Services/"). Causa:
- Lógica de negocio dispersa en UI
- Queries no reutilizables
- Imposible testear o refactorizar sin tocar las páginas
- Riesgo de lifetime mismatch (DbContext Scoped en Blazor Server)

---

### A-08: DataScopeService no cubre Empleados ni Autoevaluaciones

**Archivo:** `Services/DataScopeService.cs`

**Problema:** Solo tiene overloads para `IQueryable<Objetivo>` y `IQueryable<RevisionCuatrimestral>`. Las páginas de Seguimientos y Autoevaluaciones replican manualmente la lógica de scope (if DIRECTOR → filtrar por AreaId, if JEFE → filtrar por JefeId, etc.). Esto está duplicado en al menos 4 archivos.

---

### A-09: Login.razor — La imagen del logo referencia `logo.png` pero puede faltar

**Archivo:** `Components/Pages/Login.razor`

**Problema:** Referencia `<img src="logo.png" class="login-logo-img">` pero la clase CSS `.login-logo-img` no está definida en el `<style>` del componente. El logo podría renderizarse a tamaño original (potencialmente enorme) rompiendo el layout del login.

---

### A-10: No hay migración inicial — solo migraciones parciales

**Directorio:** `Migrations/`

**Problema:** Solo existen dos migraciones:
- `20260407123311_UpdateAutoevaluacion.cs`
- `20260410190112_AddSoftSkillsEvaluation.cs`

No hay una migración `InitialCreate`. Esto sugiere que la BD se crea por `EnsureCreated` o se generó manualmente. Si alguien clona el repo y ejecuta `dotnet ef database update`, podría fallar.

---

### A-11: AuditoriaLog.Accion usa "CANCEL" en vez de "DELETE"

**Archivo:** `Services/ObjetivoService.cs` (CancelarObjetivoAsync)

**Problema:** El CONTEXT.md RN-04 especifica `Accion="DELETE" (semántico)` pero el código usa `Accion="CANCEL"`. Cualquier reporte o query sobre auditoría que busque "DELETE" no encontrará cancelaciones.

---

### A-12: Upload de archivos sin validación de seguridad

**Archivo:** `Controllers/UploadController.cs`

**Problema:**
- No valida tipo de archivo (podría subir .exe, .dll, .sh)
- No valida tamaño máximo
- No sanitiza el nombre de archivo contra path traversal (solo agrega timestamp)
- No verifica autenticación/autorización del usuario
- Los archivos se guardan directamente en wwwroot (accesibles públicamente sin control)

---

## SECCIÓN 3 — HALLAZGOS DE SEVERIDAD MEDIA

### M-01: Dashboard — Gráfico Donut muestra datos incorrectos
El Donut muestra "Activo", "Vencen Pronto" y "Pendientes Revisión" pero no muestra estados EN_RIESGO, COMPLETADO. Los "Vencen Pronto" son un subconjunto de "Activo", produciendo datos visualmente engañosos.

### M-02: EmpleadoDetalle.razor — N+1 en RendimientoPorPilarAsync
Se ejecuta `RendimientoPorPilarAsync` en un loop por cada pilar, y luego `CalcularPonderadoAsync` por cada objetivo. Cada uno hace una query independiente a la BD.

### M-03: Seguimientos/Index.razor — N+1 en PromedioGeneralAsync
Loop de `PromedioGeneralAsync` para cada empleado del equipo. Con 50 empleados = 50+ queries a BD.

### M-04: No existe entidad Pais en el seed ni lógica de países
La entidad `Pais` existe pero no se ve referenciada en la lógica de scope. No se filtra por país en ninguna parte.

### M-05: `EstadoBadge.razor` acepta `EstadoObj` y `EstadoBit` pero no hay documentación de cuál usar
El componente compartido tiene dos parámetros pero no está claro cuándo pasar uno u otro.

### M-06: SeedData.cs no pudo ser leído pero el CONTEXT.md muestra datos seed extensos
Si el seed no incluye Países, Cursos con asignación a empleados, y mensajes de chat, habrá datos faltantes en producción.

### M-07: `ObjetivoService.UpdateObjetivoAsync` no llama a `EvaluarEstadoRiesgoAsync`
Después de actualizar un objetivo (especialmente el campo Progreso), no se re-evalúa si debe pasar a EN_RIESGO.

### M-08: `MainLayout.razor` — NotificationCount query usa `ExecuteUpdateAsync` que es EF Core 7+ feature
Funciona en .NET 10, pero el uso de `ExecuteUpdateAsync` directamente en el layout sin servicio es un anti-patrón.

### M-09: El chat no se auto-refresca
No hay SignalR, polling, ni timer para actualizar mensajes nuevos. El chat solo se actualiza al enviar un mensaje.

### M-10: `EvaluarDialog.razor` — Evidencias hardcodeadas
La lista de evidencias disponibles está hardcodeada como `{ "Reporte de avance", "Documentación técnica", ... }` en vez de venir de las entradas de bitácora reales del objetivo.

### M-11: `CrearObjetivoDialog.razor` y `EditarObjetivoDialog.razor` no fueron auditados en contenido
Existen en el directorio pero su implementación interna puede tener gaps adicionales.

---

## SECCIÓN 4 — HALLAZGOS DE SEVERIDAD BAJA

### B-01: CSS del login embebido en el componente en vez de `site.css`
### B-02: `Shared/` en raíz de `Objetivos.Web/` está vacío — directorio muerto
### B-03: `Pages/Error.cshtml`, `_Host.cshtml`, `_Layout.cshtml` son legacy Blazor Server patterns — revisar si se usan
### B-04: `antigravityDownloadFile` JS function referenciada en export pero no se ve definida en `wwwroot`
### B-05: No hay `favicon.ico` actualizado (se ve genérico en wwwroot)
### B-06: `.gitignore` puede no estar excluyendo `bin/` y `obj/` (hay que verificar)
### B-07: Archivos de auditoría previos en raíz (`AUDITORIA_EJECUTADA.md`, `AUDITORIA_EXTERNA_TIER1.md`, `AUDITORIA_TECNICA.md`) — limpiar o mover a carpeta docs
### B-08: La entidad `Curso` no tiene relación con SoftSkills — podría vincularse para recomendaciones
### B-09: `appsettings.json` — verificar que EmailSettings tenga valores por defecto seguros

---

## PLAN DE CORRECCIÓN EN ETAPAS — OPTIMIZADO PARA CLAUDE CODE

Cada etapa está diseñada para ejecutarse como un prompt independiente en Claude Code. Incluye los archivos a modificar y el resultado esperado.

---

### ETAPA 0 — DECISIÓN ARQUITECTÓNICA (Manual, no Claude Code)

**Acción humana requerida:** Decidir si se mantiene el modelo de 1 revisión (`FEEDBACK_MITAD_ANIO`) o se restauran las 3 revisiones (`Q1_ABRIL, Q2_AGOSTO, Q3_NOVIEMBRE`). Esta decisión impacta las etapas 1 y 2.

---

### ETAPA 1 — INTEGRIDAD DE DATOS Y MODELOS (Prioridad máxima)

**Prompt para Claude Code:**
```
Lee CONTEXT.md y los archivos indicados. Ejecuta las siguientes correcciones:

1. En Domain/Enums/Enums.cs:
   - Confirmar que PeriodoRevision coincida con la decisión tomada en Etapa 0
   - Si se decidió restaurar Q1/Q2/Q3, actualizar el enum y todos los archivos que lo usan

2. En Services/ObjetivoService.cs → CrearObjetivoAsync:
   - ANTES del INSERT, verificar si existe un objetivo con mismo PilarId+EmpleadoId+Anio
   - Si existe, retornar un resultado que permita al UI mostrar VAL-01
   - Validar que SoftSkill1Id != SoftSkill2Id (VAL-03)
   - Validar que Deadline > DateTime.Today (VAL-04)

3. En Services/ObjetivoService.cs → CancelarObjetivoAsync:
   - Cambiar Accion de "CANCEL" a "DELETE" para coincidir con CONTEXT.md RN-04

4. En Services/ObjetivoService.cs → UpdateObjetivoAsync:
   - Llamar a EvaluarEstadoRiesgoAsync(objetivo.Id) después del SaveChanges

5. En Components/Pages/Autoevaluaciones/AutoevaluarDialog.razor:
   - Cambiar la asignación de EmpleadoId para buscar el Empleado por email
     del CurrentUser en vez de usar CurrentUser.UsuarioId directamente
   - Usar: var emp = await Db.Empleados.FirstOrDefaultAsync(e => e.Email == CurrentUser.Email);
     autoevaluacion.EmpleadoId = emp.Id;

Archivos a modificar:
- Objetivos.Web/Domain/Enums/Enums.cs
- Objetivos.Web/Services/ObjetivoService.cs
- Objetivos.Web/Components/Pages/Autoevaluaciones/AutoevaluarDialog.razor
```

**Verificación:** Compilar. Crear un objetivo duplicado y verificar que el UI muestre el mensaje VAL-01.

---

### ETAPA 2 — FÓRMULAS Y LÓGICA DE RENDIMIENTO

**Prompt para Claude Code:**
```
Lee CONTEXT.md sección RN-07 y corrige:

1. En Services/RendimientoService.cs:
   - Documentar con comentarios XML la fórmula actual y por qué difiere del CONTEXT.md
   - Si se restauraron Q1/Q2/Q3 en Etapa 1, restaurar la fórmula:
     (q1 * 0.2) + (q2 * 0.3) + (q3 * 0.3) + (fin * 0.2)
   - Si se mantiene FEEDBACK_MITAD_ANIO, documentar explícitamente:
     "El puntaje final almacenado es feedback*0.5 porque la evaluación 
      final no existe al momento del cálculo. Esto es by design."

2. En Services/RevisionService.cs → CompletarEvaluacionFinalAsync:
   - Resolver la inconsistencia: si EvaluacionService muestra objetivos
     para evaluar antes del deadline, el servicio debe permitirlo también
   - O bien: remover la verificación de deadline en el servicio
   - O bien: agregar la verificación en el EvaluacionService query
   - Documentar la decisión con comentario

Archivos:
- Objetivos.Web/Services/RendimientoService.cs
- Objetivos.Web/Services/RevisionService.cs
- Objetivos.Web/Services/EvaluacionService.cs
```

---

### ETAPA 3 — ELIMINAR INYECCIÓN DIRECTA DE DBCONTEXT EN PÁGINAS

**Prompt para Claude Code:**
```
Refactoriza para que NINGUNA página Razor inyecte AppDbContext directamente. 
Toda la lógica debe ir en Services/. Archivos a corregir:

1. Components/Pages/Seguimientos/Index.razor
   → Crear método en un servicio (ej: SeguimientoService o ampliar DataScopeService)
     que retorne los empleados del equipo con sus promedios pre-calculados

2. Components/Pages/Seguimientos/EmpleadoDetalle.razor
   → Mover las queries a ObjetivoService o crear EmpleadoService
   → El método debe retornar el empleado con objetivos, radar data, y ponderados
     en UNA sola llamada (eliminar N+1)

3. Components/Pages/Autoevaluaciones/Index.razor
   → Crear AutoevaluacionService con método GetAutoevaluacionesAsync(ICurrentUserService)

4. Components/Pages/Autoevaluaciones/AutoevaluarDialog.razor
   → Mover la lógica de guardado a AutoevaluacionService

5. Components/Pages/Evaluacion/EvaluarDialog.razor
   → Mover la carga de la revisión a RevisionService.GetRevisionDetalleAsync(id)

6. Components/Pages/Evaluacion/EvaluarFinalDialog.razor
   → Mover la carga del objetivo a ObjetivoService.GetByIdAsync (ya existe)

7. Components/Layout/MainLayout.razor
   → Mover LoadNotifications a un NotificacionService
   → Mover el marcado de leídas al mismo servicio

Cada servicio debe usar IDbContextFactory<AppDbContext> para evitar 
problemas de lifetime en Blazor Server.

Archivos a modificar: todos los listados arriba + crear nuevos servicios
Archivos a crear:
- Services/SeguimientoService.cs (o ampliar existentes)
- Services/AutoevaluacionService.cs
- Services/NotificacionService.cs
```

---

### ETAPA 4 — MÓDULO DE NOTIFICACIONES (UI)

**Prompt para Claude Code:**
```
Implementa un panel de notificaciones funcional:

1. Crear Components/Shared/NotificacionPanel.razor:
   - Mostrar como dropdown/panel lateral al click del ícono de campanita
   - Listar las últimas 20 notificaciones del usuario
   - Mostrar tipo, mensaje, fecha, estado leída/no-leída
   - Al hacer click en una notificación, navegar al recurso relacionado
   - Botón "Marcar todas como leídas"

2. Modificar MainLayout.razor:
   - Reemplazar el onClick actual por la apertura del panel
   - Usar NotificacionService creado en Etapa 3

3. En NotificacionService:
   - GetNotificacionesAsync(int usuarioId, int take = 20)
   - MarcarTodasComoLeidasAsync(int usuarioId)
   - MarcarComoLeidaAsync(int notificacionId)

Usar RadzenPanel o un RadzenDialog lateral para el panel.
```

---

### ETAPA 5 — CALENDARIO DE EVENTOS

**Prompt para Claude Code:**
```
Implementa la vista de calendario usando RadzenScheduler:

1. Crear Components/Pages/Calendario/Index.razor (o integrar en Dashboard):
   - @page "/calendario" (agregar al NavMenu)
   - Mostrar RadzenScheduler con eventos del área del usuario
   - Filtrar por scope (DataScopeService)
   - Colores diferentes por TipoEvento

2. Crear Services/CalendarioService.cs:
   - GetEventosAsync(int areaId, int anio) → List<EventoCalendario>
   - CrearEventoRevision cuando se crea un objetivo (ya parcialmente hecho)
   - Agregar creación de eventos para fechas fijas de revisión (RN-08)

3. En ObjetivoService.CrearObjetivoAsync:
   - Agregar eventos de calendario para las revisiones cuatrimestrales
     (si aplica según decisión de Etapa 0)

4. En NavMenu.razor:
   - Agregar item: RadzenPanelMenuItem Text="Calendario" Icon="event" Path="calendario"
```

---

### ETAPA 6 — MÓDULO DE CURSOS COMPLETO

**Prompt para Claude Code:**
```
Implementa el módulo de Cursos completo:

1. Crear entidad Domain/Entities/CursoAsignacion.cs:
   - Id, CursoId, EmpleadoId, Progreso (0-100), Estado (NO_INICIADO, EN_CURSO, COMPLETADO)
   - Calificacion (nullable double), FechaInicio, FechaCompletado
   - Agregar DbSet en AppDbContext, crear migración

2. Ampliar CursoService.cs:
   - GetCursosConAsignacionesAsync(int empleadoId)
   - AsignarCursoAsync(int cursoId, int empleadoId)
   - ActualizarProgresoAsync(int asignacionId, int progreso)
   - CompletarCursoAsync(int asignacionId, double calificacion)

3. Rediseñar Components/Pages/Cursos/Index.razor:
   - Tab "Catálogo de Cursos" — lista general con botón Inscribirse
   - Tab "Mis Cursos" — cursos del empleado logueado con progreso
   - Tab "Cursos del Equipo" (solo jefes) — ver progreso de subordinados
   - El botón "Ver Curso" debe mostrar detalle con progreso

4. Crear migración: dotnet ef migrations add AddCursoAsignaciones
```

---

### ETAPA 7 — SEGURIDAD Y UPLOAD

**Prompt para Claude Code:**
```
Corrige la seguridad del upload y rutas:

1. En Controllers/UploadController.cs:
   - Agregar [Authorize] o validar sesión manualmente
   - Validar tipos MIME permitidos: .pdf, .docx, .xlsx, .jpg, .png
   - Limitar tamaño a 10MB por archivo
   - Sanitizar nombre de archivo (remover caracteres especiales, path traversal)
   - Mover uploads fuera de wwwroot a una carpeta protegida
   - Crear endpoint separado para servir archivos con validación de acceso

2. Corregir ruta de Objetivos:
   - En Components/Pages/MisObjetivos/Index.razor: cambiar @page "/misobjetivos" 
     a @page "/objetivos" (o agregar alias @page "/objetivos")
   - En NavMenu.razor: cambiar Path="misobjetivos" a Path="objetivos"

3. Agregar función JS faltante en wwwroot:
   - Verificar si antigravityDownloadFile está definida
   - Si no, crear en wwwroot/js/app.js e incluir en _Host.cshtml
```

---

### ETAPA 8 — OPTIMIZACIÓN DE RENDIMIENTO (N+1)

**Prompt para Claude Code:**
```
Optimiza las queries N+1 identificadas:

1. En el nuevo SeguimientoService (creado en Etapa 3):
   - Cargar todos los empleados del equipo con sus objetivos y revisiones 
     en UNA query con Include/ThenInclude
   - Calcular promedios en memoria en vez de N queries

2. En EmpleadoDetalle:
   - La query ya usa Include pero luego hace N queries a 
     RendimientoPorPilarAsync/CalcularPonderadoAsync
   - Mover el cálculo a in-memory usando los datos ya cargados

3. En RendimientoService:
   - Agregar método batch: CalcularPromediosBatchAsync(List<int> empleadoIds, int anio)
     que cargue todo en una query y retorne Dictionary<int, double>
```

---

### ETAPA 9 — DASHBOARD MEJORADO

**Prompt para Claude Code:**
```
Mejora el Dashboard para ser más informativo:

1. Corregir el gráfico Donut:
   - Mostrar todos los estados: ACTIVO, EN_RIESGO, COMPLETADO, CANCELADO
   - Remover "Vencen Pronto" del donut (es un subconjunto de ACTIVO)
   - Agregar "Vencen Pronto" como un indicador separado (KPI card ya existe)

2. Agregar gráfico de radar de rendimiento del área (si es jefe)
3. Agregar mini-calendario con próximos 5 eventos
4. Agregar lista de "Últimas actividades" (últimas 5 bitácoras/evaluaciones)

Ampliar DashboardService con los datos necesarios.
```

---

### ETAPA 10 — CONSISTENCIA Y LIMPIEZA

**Prompt para Claude Code:**
```
Limpieza general del proyecto:

1. Eliminar directorio vacío Objetivos.Web/Shared/
2. Verificar que .gitignore excluya bin/, obj/, *.db, .vs/
3. Mover CSS del Login.razor a wwwroot/css/site.css
4. Agregar clase CSS .login-logo-img con width/height apropiados
5. Verificar que _Host.cshtml incluya los JS necesarios (antigravityDownloadFile)
6. Eliminar o archivar auditorías previas de la raíz del proyecto
7. Agregar README.md con instrucciones de setup del proyecto
8. Centralizar DataScopeService para cubrir:
   - IQueryable<Empleado>
   - IQueryable<Autoevaluacion>
   - IQueryable<BitacoraEntrada>
9. Verificar que las migraciones estén completas (crear InitialCreate si falta)
10. Revisar CrearObjetivoDialog.razor y EditarObjetivoDialog.razor
    para verificar que implementen todas las validaciones VAL-01 a VAL-05
```

---

## DIAGRAMA DE DEPENDENCIAS ENTRE ETAPAS

```
Etapa 0 (Decisión humana)
   │
   ├──→ Etapa 1 (Modelos e integridad)
   │       │
   │       ├──→ Etapa 2 (Fórmulas)
   │       │
   │       └──→ Etapa 3 (Refactor DbContext) ──→ Etapa 8 (N+1)
   │               │
   │               ├──→ Etapa 4 (Notificaciones UI)
   │               │
   │               └──→ Etapa 9 (Dashboard)
   │
   ├──→ Etapa 5 (Calendario) — independiente tras Etapa 0
   │
   ├──→ Etapa 6 (Cursos) — independiente
   │
   ├──→ Etapa 7 (Seguridad) — independiente
   │
   └──→ Etapa 10 (Limpieza) — ejecutar al final
```

---

## NOTAS PARA CLAUDE CODE

1. **Siempre leer CONTEXT.md primero** antes de cada etapa. Es la fuente de verdad.
2. **Usar `IDbContextFactory<AppDbContext>`** en todos los servicios nuevos (patrón ya usado en EvaluacionService y UsuarioService).
3. **No agregar paquetes NuGet** que no estén en CONTEXT.md.
4. **Mantener portabilidad SQLite ↔ SQL Server** — solo LINQ, nunca SQL raw.
5. **Ejecutar `dotnet build`** después de cada etapa para verificar compilación.
6. **Generar migraciones** si se modifica el modelo: `dotnet ef migrations add NombreDescriptivo`.

---

*Fin de la auditoría — Documento generado para uso con Claude Code*
