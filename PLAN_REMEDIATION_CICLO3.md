# PLAN DE REMEDIACIÓN — PQ-Talent (RRHH_Objetivos) · Ciclo 3
**Fecha:** Mayo 2026  
**Proyecto:** `C:\Development\Antigravity\RRHH_Objetivos`  
**Stack:** Blazor Server · .NET 10 · Radzen · EF Core · SQLite  
**Auditor:** Arquitecto de Software — Auditoría Ciclo 3

---

## INSTRUCCIONES PARA EL AGENTE

1. **Leer `CONTEXT.md` completo antes de cada etapa.** Es la fuente de verdad del proyecto.
2. **Usar `IDbContextFactory<AppDbContext>`** en todos los servicios nuevos (patrón ya usado en `EvaluacionService` y `UsuarioService`).
3. **No agregar paquetes NuGet** que no estén en `CONTEXT.md` (Radzen.Blazor, EF Core Sqlite, EF Core Design).
4. **Ejecutar `dotnet build`** al finalizar cada etapa. Corregir errores antes de avanzar.
5. **Generar migraciones** cuando se modifique el modelo de dominio: `dotnet ef migrations add NombreDescriptivo --project Objetivos.Web`.
6. **Solo LINQ sobre DbSet** — nunca SQL raw ni funciones específicas de SQLite (portabilidad SQLite ↔ SQL Server).
7. **El agente NO debe modificar `CONTEXT.md`** salvo en la Etapa 0 donde se indica explícitamente.
8. Las etapas están ordenadas por prioridad y dependencia. Ejecutar en orden.

---

## RESUMEN DE HALLAZGOS A RESOLVER

| ID | Severidad | Título resumido | Etapa |
|----|-----------|-----------------|-------|
| A-14 | ALTO | Slugs de EstadoObjetivoConfig ausentes en seed | 0 |
| M-15 | MEDIO | Índice UNIQUE vs regla de múltiples objetivos por pilar | 0 |
| C-01 | CRÍTICO | CONTEXT.md desactualizado (Q1/Q2/Q3 vs FEEDBACK_MITAD_ANIO) | 0 |
| A-07 | ALTO | AppDbContext inyectado directamente en 7 páginas Razor | 1 |
| A-08/A-15 | ALTO | DataScopeService overloads existentes ignorados por las páginas | 1 |
| M-08 | MEDIO | MainLayout hace queries EF directamente | 1 |
| C-03 | CRÍTICO | Panel de notificaciones ausente | 2 |
| A-13 | ALTO | Chat timer sin protección ObjectDisposedException | 3 |
| M-13 | MEDIO | VAL-01 rechaza silenciosamente sin ofrecer reemplazo | 3 |
| M-14 | MEDIO | Campos ActualizadoEn nunca se actualizan en ediciones | 3 |
| C-04 | CRÍTICO | Calendario de eventos sin UI | 4 |
| M-01 | MEDIO | Dashboard Donut con datos engañosos | 4 |
| M-10 | MEDIO | EvaluarDialog con evidencias hardcodeadas | 4 |
| C-02 | CRÍTICO | Módulo Cursos sin funcionalidad real | 5 |
| A-01/A-02 | ALTO | Ruta /misobjetivos en vez de /objetivos | 6 |
| A-12 | ALTO | UploadController sin validación de seguridad | 6 |
| M-02/M-03 | MEDIO | Queries N+1 en Seguimientos | 7 |
| C-05 | CRÍTICO | ExportService incompleto | 8 |
| A-10 | ALTO | Sin migración InitialCreate | 8 |
| M-04 | MEDIO | Entidad Pais sin seed de fallback verificado | 8 |
| M-11 | MEDIO | CrearObjetivoDialog/EditarObjetivoDialog no auditados | 9 |
| M-16 | MEDIO | Typo en nombre AdminConfiguracionPlatformaService | 9 |
| A-09 | ALTO | Login sin clase CSS .login-logo-img | 9 |

---

## ETAPA 0 — CORRECCIONES CRÍTICAS DE INTEGRIDAD (Ejecutar primero, sin excepción)

**Objetivo:** Resolver inconsistencias que pueden hacer fallar silenciosamente la lógica de negocio central.

### 0.1 Verificar y completar SeedData.cs (A-14, M-04)

**Archivo:** `Objetivos.Web/Data/SeedData.cs`

Verificar que el método `InitializeAsync` incluya seed **idempotente** (verificar existencia antes de insertar) para:

**EstadoObjetivoConfig** — los servicios dependen de estos slugs exactos:
```csharp
// REQUERIDO por ObjetivoService.CrearObjetivoAsync
{ Slug = "pendiente_aprobacion", Nombre = "Pendiente Aprobación", ColorHex = "#F59E0B", Orden = 1, Activo = true }

// REQUERIDO por ObjetivoService.AprobarObjetivoAsync
{ Slug = "aprobado", Nombre = "Aprobado", ColorHex = "#10B981", Orden = 2, Activo = true }

// RECOMENDADO
{ Slug = "rechazado", Nombre = "Rechazado", ColorHex = "#EF4444", Orden = 3, Activo = true }
```

**EstadoEvaluacionConfig** — para revisiones y evaluaciones finales:
```csharp
{ Slug = "completada", Nombre = "Completada", ColorHex = "#10B981", Orden = 1, Activo = true }
{ Slug = "en_proceso", Nombre = "En Proceso", ColorHex = "#3B82F6", Orden = 2, Activo = true }
{ Slug = "pendiente",  Nombre = "Pendiente",  ColorHex = "#6B7280", Orden = 3, Activo = true }
```

**EscalaValoracion** — requerida por revisiones y autoevaluaciones:
```csharp
{ Etiqueta = "No cumplido",         ValorNumerico = 1, Orden = 1, Activo = true }
{ Etiqueta = "Parcialmente",        ValorNumerico = 2, Orden = 2, Activo = true }
{ Etiqueta = "En desarrollo",       ValorNumerico = 3, Orden = 3, Activo = true }
{ Etiqueta = "Cumplido",            ValorNumerico = 4, Orden = 4, Activo = true }
{ Etiqueta = "Superado ampliamente",ValorNumerico = 5, Orden = 5, Activo = true }
```

**ConfiguracionPlataforma** — requerida por `ConfiguracionService.ObtenerConfiguracionBoolAsync`:
```csharp
{ Clave = "jefe_puede_crear_objetivos", Valor = "true",  Tipo = "bool", Descripcion = "Permite a jefes crear objetivos para sus empleados" }
{ Clave = "empleado_puede_autoevaluar", Valor = "true",  Tipo = "bool", Descripcion = "Habilita el módulo de autoevaluación para empleados" }
{ Clave = "nombre_empresa",             Valor = "PQ-Talent", Tipo = "string", Descripcion = "Nombre de la empresa mostrado en la plataforma" }
```

**Verificar también:** que `Paises` tenga al menos `Argentina`, `Chile`, `Uruguay` (ya existe en el código visto, confirmar que llega a ejecutarse).

### 0.2 Resolver inconsistencia del índice UNIQUE (M-15)

**Archivo:** `Objetivos.Web/Data/AppDbContext.cs`

El índice `UNIQUE(PilarId, EmpleadoId, Anio)` fue diseñado para un modelo de 1 objetivo por pilar, pero la lógica actual permite múltiples objetivos por pilar. Hay dos opciones — elegir una:

**Opción A (recomendada si el negocio permite múltiples objetivos por pilar):**
Reemplazar el índice UNIQUE por un índice simple (no único):
```csharp
// ANTES:
modelBuilder.Entity<Objetivo>()
    .HasIndex(o => new { o.PilarId, o.EmpleadoId, o.Anio })
    .IsUnique();

// DESPUÉS (Opción A):
modelBuilder.Entity<Objetivo>()
    .HasIndex(o => new { o.PilarId, o.EmpleadoId, o.Anio });
// Agregar comentario explicativo:
// Índice no único: se permiten múltiples objetivos por pilar/empleado/año.
// La validación de negocio (suma de PorcentajePilar <= 100) se aplica en ObjetivoService.
```
Luego generar migración: `dotnet ef migrations add RemoveUniqueConstraintObjetivoPilar`

**Opción B (si el negocio requiere máximo 1 objetivo por pilar):**
Restaurar la verificación VAL-01 en `ObjetivoService.CrearObjetivoAsync` con `AnyAsync` y retornar `(false, true)` cuando sea duplicado.

**Agregar en el catch de `CrearObjetivoAsync`** para distinguir errores:
```csharp
catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true)
{
    await transaction.RollbackAsync();
    return (false, true); // Duplicado detectado a nivel BD
}
catch
{
    await transaction.RollbackAsync();
    return (false, false);
}
```

### 0.3 Actualizar CONTEXT.md (C-01)

**Archivo:** `CONTEXT.md`

Actualizar las secciones que describen el modelo de revisiones para reflejar el estado actual del código:

- Reemplazar `Q1_ABRIL`, `Q2_AGOSTO`, `Q3_NOVIEMBRE` por `FEEDBACK_MITAD_ANIO` en el enum de `PeriodoRevision`.
- Actualizar la fórmula RN-07 de `(q1*0.2)+(q2*0.3)+(q3*0.3)+(fin*0.2)` a `(feedback*0.5)+(fin*0.5)`.
- Actualizar RN-02 (Completar Revisión): eliminar la precondición de secuencia Q1→Q2→Q3, ya que solo existe una revisión.
- Actualizar RN-03 (Evaluación Final): cambiar "Las 3 RevisionCuatrimestral tienen Completada==true" por "El FEEDBACK_MITAD_ANIO tiene Completada==true".
- Documentar la decisión: "Modelo simplificado adoptado por decisión de negocio. Si se restauran Q1/Q2/Q3, actualizar este documento, el enum y RendimientoService."

**Verificación de Etapa 0:** `dotnet build` debe compilar sin errores. Ejecutar la aplicación y crear un objetivo nuevo — verificar en BD que `EstadoObjetivoConfigId` queda asignado correctamente.

---

## ETAPA 1 — REFACTORIZAR DBCONTEXT FUERA DE PÁGINAS RAZOR

**Objetivo:** Resolver A-07, A-08/A-15, M-08. Esta es la etapa de mayor impacto arquitectónico.

**Archivos a crear:**
- `Objetivos.Web/Services/NotificacionService.cs`
- `Objetivos.Web/Services/SeguimientoService.cs` (ampliar el existente si ya tiene contenido)
- `Objetivos.Web/Services/AutoevaluacionService.cs` (ampliar el existente si ya tiene contenido)

**Archivos a modificar:**
- `Objetivos.Web/Services/DataScopeService.cs`
- `Objetivos.Web/Services/RevisionService.cs`
- `Objetivos.Web/Components/Layout/MainLayout.razor`
- `Objetivos.Web/Components/Pages/Seguimientos/Index.razor`
- `Objetivos.Web/Components/Pages/Seguimientos/EmpleadoDetalle.razor`
- `Objetivos.Web/Components/Pages/Autoevaluaciones/Index.razor`
- `Objetivos.Web/Components/Pages/Autoevaluaciones/AutoevaluarDialog.razor`
- `Objetivos.Web/Components/Pages/Evaluacion/EvaluarDialog.razor`
- `Objetivos.Web/Components/Pages/Evaluacion/EvaluarFinalDialog.razor`
- `Objetivos.Web/Program.cs`

### 1.1 Crear NotificacionService.cs

```csharp
// Usar IDbContextFactory<AppDbContext> — NO AppDbContext directo
public class NotificacionService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    // Métodos requeridos:
    Task<List<Notificacion>> GetNotificacionesAsync(int usuarioId, int take = 20);
    Task<int> GetNoLeidasCountAsync(int usuarioId);
    Task MarcarTodasComoLeidasAsync(int usuarioId);
    Task MarcarComoLeidaAsync(int notificacionId);
}
```

### 1.2 Ampliar SeguimientoService.cs

Verificar si el archivo ya existe. Si existe, agregar los métodos faltantes. Si no existe, crearlo.

```csharp
public class SeguimientoService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly DataScopeService _dataScope;
    private readonly RendimientoService _rendimiento;

    // Cargar empleados del equipo con promedios en UNA query (resolver N+1 de M-02/M-03)
    Task<List<EmpleadoConPromedio>> GetEmpleadosEquipoConPromediosAsync(ICurrentUserService user, int anio);

    // Empleado completo con objetivos, revisiones y radar — todo en memoria
    Task<EmpleadoDetalleCompleto?> GetEmpleadoDetalleAsync(int empleadoId, int anio);
}

// DTOs:
public class EmpleadoConPromedio
{
    public Empleado Empleado { get; set; } = null!;
    public double PromedioGeneral { get; set; }
    public int ObjetivosActivos { get; set; }
    public int ObjetivosEnRiesgo { get; set; }
}

public class EmpleadoDetalleCompleto
{
    public Empleado Empleado { get; set; } = null!;
    public List<Objetivo> Objetivos { get; set; } = [];
    public Dictionary<int, double> PonderadosPorObjetivo { get; set; } = new();
    public List<MensajeChat> UltimosMensajes { get; set; } = [];
}
```

**Patrón para resolver N+1 en `GetEmpleadosEquipoConPromediosAsync`:**
```csharp
// Cargar todo en UNA query con Include
var empleados = await db.Empleados
    .Include(e => e.Objetivos.Where(o => o.Anio == anio))
        .ThenInclude(o => o.Revisiones)
    .Include(e => e.Objetivos.Where(o => o.Anio == anio))
        .ThenInclude(o => o.EvaluacionFinal)
    .Where(...)
    .ToListAsync();

// Calcular ponderados EN MEMORIA usando el método estático ya existente
foreach (var emp in empleados)
{
    double promedio = emp.Objetivos
        .Where(o => o.Estado != EstadoObjetivo.CANCELADO)
        .Select(o => RendimientoService.CalcularPonderadoStatic(o))
        .Where(v => v > 0)
        .DefaultIfEmpty(0)
        .Average();
    // ...
}
```

### 1.3 Ampliar AutoevaluacionService.cs

Verificar si el archivo ya existe. Si existe, agregar los métodos faltantes.

```csharp
public class AutoevaluacionService
{
    // Métodos mínimos requeridos:
    Task<List<Autoevaluacion>> GetAutoevaluacionesEquipoAsync(ICurrentUserService user, int anio);
    Task<Objetivo?> GetObjetivoParaAutoevAsync(int objetivoId); // con SoftSkills incluidos
    Task<bool> GuardarAutoevaluacionAsync(Autoevaluacion ae);
}
```

### 1.4 Verificar DataScopeService.cs (A-15)

Al revisar el código en auditoría, los overloads para `IQueryable<Empleado>` y `IQueryable<Autoevaluacion>` **ya existen** en `DataScopeService`. La tarea es verificar que las páginas los usen en vez de duplicar la lógica. Si las páginas tienen lógica como:

```csharp
// EN PÁGINAS (a eliminar):
if (CurrentUser.Rol == "DIRECTOR") query = query.Where(e => e.AreaId == CurrentUser.AreaId);
else if (CurrentUser.EsJefe) query = query.Where(e => e.JefeId == CurrentUser.UsuarioId);
```

Reemplazar por:
```csharp
// USO CORRECTO DEL SERVICIO:
query = _dataScope.AplicarScope(query, CurrentUser);
```

### 1.5 Refactorizar páginas para usar servicios

Para cada página listada abajo, **eliminar `@inject AppDbContext Db`** y reemplazar por inyección del servicio correspondiente:

| Página | Servicio a usar |
|--------|----------------|
| `Seguimientos/Index.razor` | `SeguimientoService` |
| `Seguimientos/EmpleadoDetalle.razor` | `SeguimientoService` |
| `Autoevaluaciones/Index.razor` | `AutoevaluacionService` |
| `Autoevaluaciones/AutoevaluarDialog.razor` | `AutoevaluacionService` |
| `Evaluacion/EvaluarDialog.razor` | `RevisionService.GetRevisionDetalleAsync(id)` |
| `Evaluacion/EvaluarFinalDialog.razor` | `ObjetivoService.GetByIdAsync(id)` (ya existe) |
| `Layout/MainLayout.razor` | `NotificacionService` |

### 1.6 Registrar servicios nuevos en Program.cs

```csharp
builder.Services.AddScoped<NotificacionService>();
// SeguimientoService y AutoevaluacionService: verificar si ya están registrados.
// Si no están:
builder.Services.AddScoped<SeguimientoService>();
builder.Services.AddScoped<AutoevaluacionService>();
```

**Verificación de Etapa 1:** `dotnet build` sin errores. Ningún archivo `.razor` debe contener `@inject AppDbContext`.

---

## ETAPA 2 — PANEL DE NOTIFICACIONES (C-03)

**Objetivo:** El usuario pueda ver el contenido de sus notificaciones, no solo el badge.

**Archivos a crear:**
- `Objetivos.Web/Components/Shared/NotificacionPanel.razor`

**Archivos a modificar:**
- `Objetivos.Web/Components/Layout/MainLayout.razor`

### 2.1 Crear NotificacionPanel.razor

El componente debe:
- Renderizarse como panel lateral o dropdown al click de la campanita (usar `RadzenDialog` o `RadzenPanel`).
- Listar las últimas 20 notificaciones del usuario con: ícono por tipo, mensaje, fecha relativa, estado leída/no leída.
- Notificaciones no leídas con fondo diferente (usar `var(--color-background-secondary)` para no leídas).
- Botón "Marcar todas como leídas" usando `NotificacionService.MarcarTodasComoLeidasAsync`.
- Click en notificación individual: marcar como leída y navegar si aplica.

**Íconos por tipo** (usar Radzen Icons o Material Icons ya disponibles):
```
SOLICITUD_ACTUALIZACION → "update" / "edit_note"
NUEVA_EVALUACION        → "grading" / "assignment_turned_in"
DEADLINE_PROXIMO        → "schedule" / "alarm"
```

**Parámetros del componente:**
```razor
[Parameter] public EventCallback OnCerrar { get; set; }
@inject NotificacionService NotifService
@inject ICurrentUserService CurrentUser
@inject NavigationManager Nav
```

### 2.2 Modificar MainLayout.razor

Reemplazar el `onclick` que marca todo como leído sin mostrar nada, por la apertura del `NotificacionPanel`.

**Verificación de Etapa 2:** Click en campanita muestra lista de notificaciones. "Marcar todas" actualiza el badge a 0.

---

## ETAPA 3 — CORRECCIONES DE LÓGICA DE NEGOCIO

**Objetivo:** Resolver bugs concretos de lógica: A-13, M-13, M-14.

### 3.1 Proteger timer del chat contra ObjectDisposedException (A-13)

**Archivo:** Buscar el componente que contiene el chat con timer (probablemente `Seguimientos/EmpleadoDetalle.razor` u `ObjetivoDetalle.razor`).

Localizar la declaración del `Timer` y envolver el callback:

```csharp
_chatTimer = new Timer(async _ =>
{
    try
    {
        await InvokeAsync(async () =>
        {
            // lógica de refresh del chat
            mensajes = await ChatService.GetConversacionAsync(...);
            StateHasChanged();
        });
    }
    catch (ObjectDisposedException) { /* Circuito desconectado — el Dispose lo detendrá */ }
    catch (Exception ex)
    {
        // Log opcional, no re-throw
    }
}, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
```

### 3.2 Implementar VAL-01 con confirmación de reemplazo (M-13)

**Archivo:** `Objetivos.Web/Components/Pages/MisObjetivos/CrearObjetivoDialog.razor`

Localizar la llamada a `ObjetivoService.CrearObjetivoAsync`. Después de la llamada, manejar el caso `Duplicado=true`:

```csharp
var (ok, duplicado) = await ObjetivoService.CrearObjetivoAsync(nuevo);

if (duplicado)
{
    var confirmar = await DialogService.Confirm(
        "Ya existe un objetivo de este pilar para el empleado en este año. ¿Desea cancelarlo y crear el nuevo?",
        "Objetivo duplicado",
        new ConfirmOptions { OkButtonText = "Sí, reemplazar", CancelButtonText = "No, cancelar" }
    );
    if (confirmar == true)
    {
        (ok, _) = await ObjetivoService.CrearObjetivoAsync(nuevo, reemplazar: true);
    }
}

if (ok)
{
    NotificationService.Notify(NotificationSeverity.Success, "Éxito", "Objetivo creado correctamente");
    await DialogService.CloseSideDialog(true);
}
```

**Nota:** Solo hacer este cambio si la Opción B fue elegida en Etapa 0.2. Si se eligió Opción A (índice no único), la validación de duplicados ya no aplica en el sentido original.

### 3.3 Actualizar automáticamente ActualizadoEn (M-14)

**Archivo:** `Objetivos.Web/Data/AppDbContext.cs`

Sobrescribir `SaveChangesAsync` para actualizar automáticamente el campo `ActualizadoEn` en todas las entidades que lo tengan:

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    var ahora = DateTime.UtcNow;
    foreach (var entry in ChangeTracker.Entries())
    {
        if (entry.State == EntityState.Modified)
        {
            // Actualizar ActualizadoEn en entidades que lo tengan
            var prop = entry.Entity.GetType().GetProperty("ActualizadoEn");
            if (prop != null && prop.CanWrite)
                prop.SetValue(entry.Entity, ahora);
        }
    }
    return await base.SaveChangesAsync(cancellationToken);
}
```

**Entidades afectadas:** `Area`, `Pilar`, `SoftSkill`, `EscalaValoracion`, `ConfiguracionPlataforma`.

**Verificación de Etapa 3:** Editar un Área en AdminConfiguracion y verificar que `ActualizadoEn` cambia. Crear un objetivo duplicado y verificar que aparece el dialog de confirmación.

---

## ETAPA 4 — CALENDARIO, DASHBOARD Y EVALUACIÓN

**Objetivo:** Resolver C-04, M-01, M-10.

### 4.1 Implementar CalendarioService y vista de calendario (C-04)

**Archivos a crear:**
- `Objetivos.Web/Services/CalendarioService.cs` (verificar si existe — si existe, ampliar)
- `Objetivos.Web/Components/Pages/Calendario/Index.razor`

**CalendarioService:**
```csharp
public class CalendarioService
{
    Task<List<EventoCalendario>> GetEventosAsync(ICurrentUserService user, int anio);
    // Usa DataScopeService internamente para filtrar por rol
}
```

**Página de calendario** (`@page "/calendario"`):
```razor
@* Usar RadzenScheduler según CONTEXT.md *@
<RadzenScheduler Data="@eventos" TItem="EventoCalendario"
                 StartProperty="Fecha" EndProperty="Fecha"
                 TextProperty="Titulo"
                 AppointmentSelect="@OnEventoClick">
    <RadzenMonthView />
</RadzenScheduler>
```

**Colores por tipo de evento** (usar `AppointmentRender`):
```
DEADLINE_OBJETIVO      → rojo / danger
FEEDBACK_MITAD_ANIO    → azul / info
EVALUACION_FINAL       → verde / success
```

**En `ObjetivoService.CrearObjetivoAsync`:** verificar que se crea el evento de tipo `FEEDBACK_MITAD_ANIO` (con fecha = 15 de Julio del año en curso, según RN-08 adaptado al modelo de 1 revisión). Si no se crea, agregarlo en la transacción.

**Agregar al NavMenu:**
```razor
<RadzenPanelMenuItem Text="Calendario" Icon="event" Path="calendario" />
```

**Registrar en Program.cs si no está:**
```csharp
builder.Services.AddScoped<CalendarioService>();
```

### 4.2 Corregir gráfico Donut del Dashboard (M-01)

**Archivos:**
- `Objetivos.Web/Services/DashboardService.cs`
- `Objetivos.Web/Components/Pages/Dashboard.razor`

**En DashboardService**, agregar al DTO de datos del dashboard:
```csharp
public int EnRiesgo { get; set; }
public int Completados { get; set; }
public int Cancelados { get; set; }
// (EnCurso ya debería existir)
```

Y calcularlos en la query:
```csharp
EnRiesgo   = objetivos.Count(o => o.Estado == EstadoObjetivo.EN_RIESGO);
Completados = objetivos.Count(o => o.Estado == EstadoObjetivo.COMPLETADO);
Cancelados  = objetivos.Count(o => o.Estado == EstadoObjetivo.CANCELADO);
```

**En Dashboard.razor**, cambiar el `donutData` para mostrar distribución real:
```csharp
var donutData = new[]
{
    new { Estado = "En curso",    Cantidad = data.EnCurso,    Color = "#3B82F6" },
    new { Estado = "En riesgo",   Cantidad = data.EnRiesgo,   Color = "#EF4444" },
    new { Estado = "Completados", Cantidad = data.Completados, Color = "#10B981" },
};
// "Vencen Pronto" permanece como KPI card separada — no incluirlo en el donut.
```

### 4.3 Cargar evidencias reales en EvaluarDialog (M-10)

**Archivo:** `Objetivos.Web/Components/Pages/Evaluacion/EvaluarDialog.razor`

Localizar la lista de evidencias hardcodeadas. Reemplazar por carga desde `BitacoraEntradas` del objetivo:

```csharp
// ANTES (hardcodeado):
var evidenciasDisponibles = new List<string> { "Reporte de avance", "Documentación técnica", ... };

// DESPUÉS (desde BD):
var bitacora = await Db.BitacoraEntradas // o usar servicio
    .Where(b => b.ObjetivoId == revision.ObjetivoId)
    .OrderByDescending(b => b.Fecha)
    .ToListAsync();

var evidenciasDisponibles = bitacora
    .Select(b => $"{b.Fecha:dd/MM/yyyy} — {b.Texto.Length > 80 ? b.Texto[..80] + "…" : b.Texto}")
    .ToList();
```

Si la bitácora está vacía, mostrar un mensaje: `"El empleado aún no ha cargado entradas de bitácora para este objetivo."` y deshabilitar el botón de completar revisión (o permitir escribir evidencias manualmente).

**Verificación de Etapa 4:** El calendario muestra eventos. El donut muestra EN_RIESGO y COMPLETADOS. EvaluarDialog muestra entradas de bitácora reales.

---

## ETAPA 5 — MÓDULO DE CURSOS COMPLETO (C-02)

**Objetivo:** El módulo de Cursos debe ser funcional end-to-end.

**Archivos a verificar/modificar:**
- `Objetivos.Web/Domain/Entities/Entities.cs` — verificar si `CursoAsignacion` ya existe
- `Objetivos.Web/Data/AppDbContext.cs` — verificar si `CursoAsignaciones` DbSet ya existe
- `Objetivos.Web/Services/CursoService.cs` — ampliar con métodos de asignación
- `Objetivos.Web/Components/Pages/Cursos/Index.razor` — rediseñar con tabs

### 5.1 Verificar entidad CursoAsignacion

Al revisar el código, **`CursoAsignacion` ya existe en `Entities.cs` y en `AppDbContext.cs`** con el constraint único `CursoId+EmpleadoId`. Verificar que existe la migración correspondiente en `Migrations/`. Si no existe la migración para esta entidad, generarla:

```
dotnet ef migrations add AddCursoAsignaciones --project Objetivos.Web
```

### 5.2 Ampliar CursoService.cs

Agregar los métodos que faltan:

```csharp
// Obtener cursos con estado de asignación del empleado actual
Task<List<CursoConAsignacion>> GetCursosParaEmpleadoAsync(int empleadoId);

// Obtener asignaciones del equipo (para jefes)
Task<List<CursoAsignacion>> GetAsignacionesEquipoAsync(ICurrentUserService user);

// Inscribir empleado en curso
Task<bool> InscribirAsync(int cursoId, int empleadoId, int asignadoPorId);

// Actualizar progreso
Task<bool> ActualizarProgresoAsync(int asignacionId, int progreso);

// Completar curso con calificación
Task<bool> CompletarCursoAsync(int asignacionId, double calificacion);

// DTO:
public class CursoConAsignacion
{
    public Curso Curso { get; set; } = null!;
    public CursoAsignacion? Asignacion { get; set; } // null si no está inscrito
}
```

### 5.3 Rediseñar Components/Pages/Cursos/Index.razor

Implementar con 3 tabs usando `RadzenTabs`:

```
Tab 1 "Catálogo"    → Lista de todos los cursos con botón "Inscribirse" (si no está ya inscrito)
Tab 2 "Mis Cursos"  → Cursos del empleado logueado con RadzenProgressBar y estado badge
Tab 3 "Equipo"      → Solo visible para jefes. Grid con empleado, curso, progreso (RadzenDataGrid)
```

El botón "Ver Curso" en el catálogo debe abrir un dialog con detalle del curso y opción de inscripción.

**Verificación de Etapa 5:** Un usuario puede inscribirse en un curso. Los cursos inscritos aparecen en "Mis Cursos" con progreso.

---

## ETAPA 6 — SEGURIDAD Y RUTA DE OBJETIVOS

**Objetivo:** Resolver A-01/A-02 y A-12.

### 6.1 Corregir ruta /objetivos (A-01, A-02)

**Archivo:** `Objetivos.Web/Components/Pages/MisObjetivos/Index.razor`

Agregar la ruta canónica del CONTEXT.md sin romper la existente:
```razor
@page "/objetivos"
@page "/misobjetivos"
```

**Archivo:** `Objetivos.Web/Components/Layout/NavMenu.razor`

Cambiar:
```razor
// ANTES:
<RadzenPanelMenuItem Text="Objetivos y Competencias" Icon="flag" Path="misobjetivos" />

// DESPUÉS:
<RadzenPanelMenuItem Text="Objetivos y Competencias" Icon="flag" Path="objetivos" />
```

### 6.2 Asegurar UploadController (A-12)

**Archivo:** `Objetivos.Web/Controllers/UploadController.cs`

Agregar las siguientes validaciones en el endpoint de upload:

```csharp
// 1. Validar autenticación (usar ICurrentUserService)
if (!currentUser.EstaAutenticado) return Unauthorized();

// 2. Validar tipo de archivo
var extensionesPermitidas = new[] { ".pdf", ".docx", ".xlsx", ".jpg", ".jpeg", ".png" };
var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
if (!extensionesPermitidas.Contains(extension))
    return BadRequest("Tipo de archivo no permitido");

// 3. Validar tamaño (10 MB máximo)
const long maxBytes = 10 * 1024 * 1024;
if (file.Length > maxBytes)
    return BadRequest("El archivo supera el tamaño máximo de 10 MB");

// 4. Sanitizar nombre de archivo
var nombreSanitizado = Path.GetFileNameWithoutExtension(file.FileName)
    .Replace("..", "").Replace("/", "").Replace("\\", "")
    .Replace(":", "").Replace("*", "").Replace("?", "")
    .Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");
if (string.IsNullOrWhiteSpace(nombreSanitizado)) nombreSanitizado = "archivo";
var nombreFinal = $"{nombreSanitizado}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
```

**Verificación de Etapa 6:** Navegar a `/objetivos` y verificar que carga. Intentar subir un `.exe` y verificar que retorna error 400.

---

## ETAPA 7 — OPTIMIZACIÓN N+1 (M-02, M-03)

**Objetivo:** Resolver las queries N+1 en Seguimientos usando el SeguimientoService creado en Etapa 1.

**Esta etapa solo aplica si en Etapa 1 se creó `SeguimientoService` pero las páginas todavía tienen lógica de N+1.**

### 7.1 Verificar que SeguimientoService resuelve el N+1

Confirmar que `GetEmpleadosEquipoConPromediosAsync` carga empleados+objetivos+revisiones en UNA sola query con `Include/ThenInclude` y calcula ponderados en memoria usando `RendimientoService.CalcularPonderadoStatic`.

### 7.2 Verificar que EmpleadoDetalle no hace queries adicionales en loop

En `Seguimientos/EmpleadoDetalle.razor` (o en `SeguimientoService.GetEmpleadoDetalleAsync`), confirmar que después de cargar el empleado con los Includes, los cálculos de radar por pilar se hacen en memoria:

```csharp
// NO hacer esto (N+1):
foreach (var pilar in pilares)
    radar[pilar.Id] = await RendimientoService.RendimientoPorPilarAsync(empleadoId, pilar.Id, anio);

// HACER esto (en memoria):
var objetivosCargados = empleadoConIncludes.Objetivos;
foreach (var pilar in pilares)
{
    var obj = objetivosCargados.FirstOrDefault(o => o.PilarId == pilar.Id);
    radar[pilar.Id] = RendimientoService.CalcularPonderadoStatic(obj);
}
```

**Verificación de Etapa 7:** Acceder a Seguimientos con 10+ empleados. El tiempo de carga debe ser notablemente menor.

---

## ETAPA 8 — EXPORTACIÓN Y MIGRACIONES (C-05, A-10, M-04)

### 8.1 Completar ExportService (C-05)

**Archivo:** `Objetivos.Web/Services/ExportService.cs`

Ampliar con los siguientes métodos:

```csharp
// Exportar reporte completo de un empleado (para jefe)
Task<byte[]> ExportarReporteEmpleadoAsync(int empleadoId, int anio);
// Incluir: datos personales, objetivos, puntaje por pilar, ponderados, semáforo, revisiones, autoevaluación

// Exportar estado general del área (para RRHH/DG)
Task<byte[]> ExportarReporteAreaAsync(int areaId, int anio);
// Incluir: tabla de empleados con promedios, colores semáforo, objetivos por estado

// Exportar datos propios (para empleado)
Task<byte[]> ExportarMisDatosAsync(int empleadoId, int anio);
```

**Formato de exportación:** CSV con columnas bien nombradas. Incluir:
- `Empleado`, `Pilar`, `Objetivo`, `Progreso`, `Feedback Score`, `Resultado`, `Puntaje Ponderado`, `Semáforo`

**Verificar función JS:** En `wwwroot/` buscar si existe `antigravityDownloadFile`. Si no existe, crear `wwwroot/js/app.js`:
```javascript
window.antigravityDownloadFile = function(filename, contentType, content) {
    const link = document.createElement('a');
    link.href = 'data:' + contentType + ';base64,' + content;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};
```

Y agregar en `Objetivos.Web/Pages/_Host.cshtml`:
```html
<script src="~/js/app.js"></script>
```

### 8.2 Verificar y crear migración InitialCreate (A-10)

Revisar si en `Migrations/` existe una migración que cubra el esquema completo inicial. Si solo hay migraciones parciales desde `20260407`, documentar en `README.md` el proceso de setup:

```markdown
## Setup inicial
1. Clonar el repositorio
2. dotnet restore
3. dotnet ef database update --project Objetivos.Web
4. dotnet run --project Objetivos.Web
```

Si el `dotnet ef database update` falla por falta de migración base, evaluar:
- Opción A: Crear `InitialCreate` desde el snapshot actual.
- Opción B: Documentar que se debe correr en un servidor con la BD ya existente.

### 8.3 Verificar seed de Países (M-04)

El código de `SeedData.cs` ya incluye Argentina, Chile y Uruguay. Verificar que:
1. Se ejecuta correctamente en bases de datos nuevas.
2. Los Jefes y Empleados del CSV tienen `PaisId` asignado correctamente.
3. Si un Empleado tiene país no mapeado en el CSV, se asigna un país por defecto (Argentina, Id=1).

**Verificación de Etapa 8:** El botón de exportar genera un archivo descargable. `dotnet ef database update` en una BD limpia funciona sin errores.

---

## ETAPA 9 — LIMPIEZA Y CONSISTENCIA (M-11, M-16, A-09)

### 9.1 Auditar CrearObjetivoDialog y EditarObjetivoDialog (M-11)

**Archivos:**
- `Objetivos.Web/Components/Pages/MisObjetivos/CrearObjetivoDialog.razor`
- `Objetivos.Web/Components/Pages/MisObjetivos/EditarObjetivoDialog.razor`

Verificar que implementen correctamente:
- VAL-02: Exactamente 2 soft skills seleccionadas
- VAL-03: SoftSkill1 ≠ SoftSkill2 (validación en UI antes de llamar al servicio)
- VAL-04: Deadline > hoy
- Campo `PorcentajePilar`: presente, entre 0 y 100, no excede el total disponible
- Campo `AreaEspecificaId`: opcional pero funcional si existe

Si falta alguna validación, agregar antes del submit del formulario.

### 9.2 Renombrar AdminConfiguracionPlatformaService (M-16)

Este es un refactor de bajo riesgo. Si se decide hacer:

1. Renombrar el archivo a `ConfiguracionPlataformaService.cs` (sin prefijo Admin, para ser consistente con `ConfiguracionPlataforma` la entidad).
2. Actualizar la clase: `public class ConfiguracionPlataformaService`
3. Actualizar `Program.cs`: `builder.Services.AddScoped<ConfiguracionPlataformaService>()`
4. Actualizar todas las inyecciones en páginas que lo usen (buscar `AdminConfiguracionPlatformaService` en el proyecto).

**Alternativamente:** Dejar el nombre actual y agregar un comentario XML explicando la inconsistencia — decisión del equipo.

### 9.3 Agregar clase CSS faltante en Login (A-09)

**Archivo:** `Objetivos.Web/wwwroot/css/site.css`

Agregar al final del archivo:
```css
.login-logo-img {
    width: 80px;
    height: auto;
    display: block;
    margin: 0 auto 1rem;
}
```

### 9.4 Limpieza general

- Eliminar el directorio vacío `Objetivos.Web/Shared/` si existe y no contiene archivos.
- Verificar que `.gitignore` excluya: `bin/`, `obj/`, `*.db`, `.vs/`, `*.user`.
- Mover CSS embebido del componente `Login.razor` a `site.css` si no se hizo en 9.3.
- Verificar que `_Host.cshtml` incluya `~/js/app.js` si fue creado en Etapa 8.

**Verificación de Etapa 9:** `dotnet build` limpio. El logo del login se muestra a tamaño razonable (80px).

---

## VERIFICACIÓN FINAL

Al completar todas las etapas, ejecutar el siguiente checklist:

```
[ ] dotnet build → 0 errores, 0 warnings relevantes
[ ] Ningún archivo .razor contiene @inject AppDbContext Db
[ ] Login funciona con usuario del seed
[ ] Crear objetivo → aparece en Mis Objetivos
[ ] Duplicado de objetivo → aparece dialog de confirmación
[ ] Seguimientos → carga sin N+1 (verificar con SQL logging si está habilitado)
[ ] Campanita → muestra panel de notificaciones
[ ] Calendario → muestra eventos en RadzenScheduler
[ ] Cursos → tab Catálogo muestra cursos, se puede inscribir
[ ] Dashboard Donut → muestra EN_RIESGO y COMPLETADOS
[ ] EvaluarDialog → muestra entradas de bitácora reales
[ ] Exportar → descarga archivo CSV
[ ] Subir .exe al UploadController → retorna 400
[ ] Navegar a /objetivos → carga correctamente
[ ] SeedData → EstadoObjetivoConfig tiene slugs "pendiente_aprobacion" y "aprobado"
[ ] CONTEXT.md actualizado con modelo de 1 revisión
```

---

## DIAGRAMA DE DEPENDENCIAS

```
Etapa 0 (Integridad de datos y CONTEXT.md) ← SIEMPRE PRIMERO
    │
    └──→ Etapa 1 (Refactor DbContext) ← DESBLOQUEA TODO
            │
            ├──→ Etapa 2 (Panel Notificaciones)
            │
            ├──→ Etapa 7 (N+1 — solo si Etapa 1 no lo resolvió)
            │
            └──→ Etapa 3 (Fixes de lógica) ← paralelo con Etapa 2

Etapa 4 (Calendario + Dashboard + Evaluación) ← Luego de Etapa 1
Etapa 5 (Cursos) ← Independiente, luego de Etapa 0
Etapa 6 (Seguridad + Ruta) ← Independiente, luego de Etapa 0
Etapa 8 (Exportación + Migraciones) ← Luego de Etapa 1
Etapa 9 (Limpieza) ← SIEMPRE AL FINAL
```

---

*Plan generado por auditoría Ciclo 3 — Mayo 2026 — Arquitecto de Software*  
*Fuente de verdad del proyecto: `CONTEXT.md` y `RULES.md`*
