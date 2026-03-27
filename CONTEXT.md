# CONTEXT.md — Plataforma de Evaluación de Objetivos
## Contrato técnico para agente de desarrollo

---

## STACK — SIN EXCEPCIONES

| Capa | Tecnología |
|------|-----------|
| Framework | ASP.NET Blazor Server — monolítico — .NET 10 |
| UI Components | Radzen Blazor (última versión estable) |
| ORM | Entity Framework Core 10 |
| Base de datos | SQLite (archivo local) |
| Lenguaje | C# 13 |
| Auth | Sesión persistente vía ProtectedSessionStorage — Service ICurrentUserService |
| Roles | JEFE, LIDER, GERENTE, DIRECTOR, DIRECTOR_GENERAL, RRHH, COLABORADOR + SUPERUSUARIO flag |
| Arquitectura | Monolito. NO Web API separada. NO MediatR. NO CQRS. Servicios directos inyectados en páginas Razor. |

**PROHIBIDO usar:** React, Angular, Vue, PostgreSQL, SQL Server, MediatR, repositorios genéricos, Web API controllers separados.

---

## ESTRUCTURA DEL PROYECTO

```
Objetivos.sln
└── Objetivos.Web/                        ← único proyecto
    ├── Data/
    │   ├── AppDbContext.cs
    │   └── SeedData.cs
    ├── Domain/
    │   ├── Entities/                     ← clases C# puras, sin lógica
    │   └── Enums/
    ├── Services/                         ← toda la lógica de negocio
    │   ├── ObjetivoService.cs
    │   ├── RevisionService.cs
    │   ├── BitacoraService.cs
    │   ├── ChatService.cs
    │   ├── DashboardService.cs
    │   └── RendimientoService.cs         ← TODOS los cálculos de métricas
    ├── Components/
    │   ├── Layout/
    │   │   ├── MainLayout.razor
    │   │   └── NavMenu.razor
    │   ├── Pages/
    │   │   ├── Dashboard.razor
    │   │   ├── Objetivos/
    │   │   │   ├── Index.razor
    │   │   │   └── CrearObjetivoDialog.razor
    │   │   ├── Seguimientos/
    │   │   │   ├── Index.razor
    │   │   │   ├── EmpleadoDetalle.razor
    │   │   │   └── ObjetivoDetalle.razor
    │   │   ├── Autoevaluacion/
    │   │   │   └── Index.razor
    │   │   ├── Evaluacion/
    │   │   │   ├── Index.razor
    │   │   │   └── EvaluarDialog.razor
    │   │   └── Cursos/
    │   │       └── Index.razor
    │   └── Shared/
    │       ├── SemaforoIndicador.razor
    │       ├── EstadoBadge.razor
    │       └── EmptyState.razor
    └── wwwroot/
        └── app.css
```

---

## ENTIDADES — C# EXACTO

```csharp
// Domain/Enums/
public enum EstadoObjetivo    { BORRADOR, ACTIVO, EN_RIESGO, COMPLETADO, CANCELADO }
public enum PeriodoRevision   { Q1_ABRIL, Q2_AGOSTO, Q3_NOVIEMBRE }
public enum ResultadoEval     { CUMPLIDO, PARCIAL, NO_CUMPLIDO, EN_RIESGO }
public enum EstadoBitacora    { PENDIENTE_REVISION, COMENTADO_JEFE, REQUIERE_AJUSTE, CERRADO }
public enum TipoEvento        { DEADLINE_OBJETIVO, REVISION_Q1, REVISION_Q2, REVISION_Q3, EVALUACION_FINAL }
public enum TipoNotificacion  { SOLICITUD_ACTUALIZACION, NUEVA_EVALUACION, DEADLINE_PROXIMO }

// Domain/Entities/
public class Area {
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
}

public class Jefe {
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Legajo { get; set; } = "";
    public int AreaId { get; set; }
    public Area Area { get; set; } = null!;
    public int PaisId { get; set; }
    public Pais Pais { get; set; } = null!;
    public string Rol { get; set; } = "JEFE"; // JEFE, LIDER, GERENTE, DIRECTOR, DIRECTOR_GENERAL, RRHH
    public bool Activo { get; set; } = true;
    public bool EsSuperusuario { get; set; } = false;
    public bool DebeCambiarPassword { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

public class Empleado {
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Legajo { get; set; } = "";
    public string Puesto { get; set; } = "";
    public int AreaId { get; set; }
    public Area Area { get; set; } = null!;
    public int PaisId { get; set; }
    public Pais Pais { get; set; } = null!;
    public int JefeId { get; set; }
    public Jefe Jefe { get; set; } = null!;
    public bool Activo { get; set; } = true;
    public bool EsSuperusuario { get; set; } = false;
    public bool DebeCambiarPassword { get; set; } = true;
    public DateTime FechaIngreso { get; set; }
}

public class Pilar {
    public int Id { get; set; }
    public string Nombre { get; set; } = ""; // "EXCELENCIA_ORGANIZACIONAL" | "INNOVACION_MEJORA" | "ORIENTACION_CLIENTE"
    public string Descripcion { get; set; } = "";
    public string ColorHex { get; set; } = "#000000";
}

public class SoftSkill {
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
}

public class Objetivo {
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public int PilarId { get; set; }
    public Pilar Pilar { get; set; } = null!;
    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;
    public int Anio { get; set; }
    public DateTime Deadline { get; set; }
    public int SoftSkill1Id { get; set; }
    public SoftSkill SoftSkill1 { get; set; } = null!;
    public int SoftSkill2Id { get; set; }
    public SoftSkill SoftSkill2 { get; set; } = null!;
    public EstadoObjetivo Estado { get; set; } = EstadoObjetivo.ACTIVO;
    public int Progreso { get; set; } = 0; // 0-100
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public int CreadoPorId { get; set; }
    // Nav
    public List<RevisionCuatrimestral> Revisiones { get; set; } = new();
    public EvaluacionFinal? EvaluacionFinal { get; set; }
    public List<BitacoraEntrada> Bitacora { get; set; } = new();
    public Autoevaluacion? Autoevaluacion { get; set; }
}

public class RevisionCuatrimestral {
    public int Id { get; set; }
    public int ObjetivoId { get; set; }
    public Objetivo Objetivo { get; set; } = null!;
    public PeriodoRevision Periodo { get; set; }
    public int Anio { get; set; }
    public int? Puntaje { get; set; }           // null = pendiente, 1-5 = completada
    public string ComentarioJefe { get; set; } = "";
    public ResultadoEval? Resultado { get; set; }
    public string EvidenciasRevisadasJson { get; set; } = "[]"; // JSON serializado
    public bool Completada { get; set; } = false;
    public DateTime? FechaEvaluacion { get; set; }
    public int? EvaluadorId { get; set; }
}

public class EvaluacionFinal {
    public int Id { get; set; }
    public int ObjetivoId { get; set; }
    public Objetivo Objetivo { get; set; } = null!;
    public int Anio { get; set; }
    public double PuntajeFinal { get; set; }    // resultado del promedio ponderado
    public string ComentarioJefe { get; set; } = "";
    public ResultadoEval ResultadoFinal { get; set; }
    public DateTime FechaEvaluacion { get; set; }
    public int EvaluadorId { get; set; }
}

public class Autoevaluacion {
    public int Id { get; set; }
    public int ObjetivoId { get; set; }
    public Objetivo Objetivo { get; set; } = null!;
    public int EmpleadoId { get; set; }
    public int Score { get; set; }              // 1-5
    public string Comentario { get; set; } = "";
    public string EvidenciasMencionadasJson { get; set; } = "[]";
    public DateTime FechaAutoevaluacion { get; set; }
}

public class BitacoraEntrada {
    public int Id { get; set; }
    public int ObjetivoId { get; set; }
    public Objetivo Objetivo { get; set; } = null!;
    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;
    public DateTime Fecha { get; set; }
    public string Texto { get; set; } = "";
    public string AdjuntosJson { get; set; } = "[]"; // JSON array de strings
    public EstadoBitacora Estado { get; set; } = EstadoBitacora.PENDIENTE_REVISION;
    public string? FeedbackJefe { get; set; }
    public DateTime? FechaFeedback { get; set; }
}

public class MensajeChat {
    public int Id { get; set; }
    public int RemitenteId { get; set; }        // ID del remitente en su respectiva tabla
    public bool RemitenteEsJefe { get; set; }   // true = Jefe, false = Empleado
    public int DestinatarioEmpleadoId { get; set; }
    public int JefeId { get; set; }             // ID del Jefe en la conversación
    public string Texto { get; set; } = "";
    public DateTime Fecha { get; set; }
    public bool Leido { get; set; } = false;
}

public class EventoCalendario {
    public int Id { get; set; }
    public string Titulo { get; set; } = "";
    public DateTime Fecha { get; set; }
    public TipoEvento Tipo { get; set; }
    public int? ObjetivoId { get; set; }
    public Objetivo? Objetivo { get; set; }
    public int AreaId { get; set; }
}

public class AuditoriaLog {
    public int Id { get; set; }
    public string Entidad { get; set; } = "";
    public int EntidadId { get; set; }
    public string Accion { get; set; } = "";    // "CREATE" | "UPDATE" | "DELETE"
    public int UsuarioId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string CambiosJson { get; set; } = "{}";
}

public class Notificacion {
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public TipoNotificacion Tipo { get; set; }
    public string Mensaje { get; set; } = "";
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public bool Leida { get; set; } = false;
}
```

---

## DBCONTEXT — CONSTRAINTS CRÍTICOS

```csharp
// Constraint único obligatorio — NO OMITIR:
modelBuilder.Entity<Objetivo>()
    .HasIndex(o => new { o.PilarId, o.EmpleadoId, o.Anio })
    .IsUnique();

// Soft skills como FKs directas (no tabla intermedia)
modelBuilder.Entity<Objetivo>()
    .HasOne(o => o.SoftSkill1).WithMany().HasForeignKey(o => o.SoftSkill1Id)
    .OnDelete(DeleteBehavior.Restrict);
modelBuilder.Entity<Objetivo>()
    .HasOne(o => o.SoftSkill2).WithMany().HasForeignKey(o => o.SoftSkill2Id)
    .OnDelete(DeleteBehavior.Restrict);

// EvaluacionFinal: 1 por objetivo
modelBuilder.Entity<EvaluacionFinal>()
    .HasIndex(e => e.ObjetivoId).IsUnique();

// Autoevaluacion: 1 por objetivo
modelBuilder.Entity<Autoevaluacion>()
    .HasIndex(a => a.ObjetivoId).IsUnique();
```

---

## REGLAS DE NEGOCIO — IMPLEMENTACIÓN EXACTA

### RN-01: Crear Objetivo (transacción atómica)

```
PRECONDICIÓN:
  - pilarId + empleadoId + DateTime.Now.Year NO existe en Objetivo (o usuario confirmó reemplazo)
  - softSkill1Id != softSkill2Id
  - deadline > DateTime.Today
  - nombre, descripcion, empleadoId, deadline, softSkill1Id, softSkill2Id → todos presentes

POSTCONDICIÓN — ejecutar todo en una transacción:
  1. INSERT Objetivo → estado = ACTIVO
  2. INSERT RevisionCuatrimestral x3:
       { ObjetivoId, Periodo=Q1_ABRIL,    Anio=objetivo.Anio, Completada=false }
       { ObjetivoId, Periodo=Q2_AGOSTO,   Anio=objetivo.Anio, Completada=false }
       { ObjetivoId, Periodo=Q3_NOVIEMBRE,Anio=objetivo.Anio, Completada=false }
  3. INSERT EventoCalendario:
       { Titulo=$"Deadline: {objetivo.Nombre}", Fecha=objetivo.Deadline,
         Tipo=DEADLINE_OBJETIVO, ObjetivoId=id, AreaId=jefe.AreaId }
  4. INSERT AuditoriaLog { Entidad="Objetivo", Accion="CREATE", UsuarioId=jefeActualId }
  5. SaveChanges()
```

### RN-02: Completar Revisión Cuatrimestral

```
PRECONDICIÓN:
  - revision.Completada == false
  - SECUENCIA: no se puede completar Q2 si Q1.Completada==false
               no se puede completar Q3 si Q2.Completada==false
  - puntaje entre 1 y 5
  - comentarioJefe no vacío
  - resultado seleccionado
  - al menos 1 evidencia marcada

POSTCONDICIÓN:
  1. UPDATE RevisionCuatrimestral:
       Puntaje = puntaje, ComentarioJefe = comentario, Resultado = resultado
       EvidenciasRevisadasJson = JSON.Serialize(evidencias)
       Completada = true, FechaEvaluacion = DateTime.UtcNow, EvaluadorId = jefeActualId
  2. LLAMAR RecalcularProgresoObjetivo(objetivoId)
  3. LLAMAR EvaluarEstadoRiesgo(objetivoId)
  4. INSERT AuditoriaLog { Entidad="RevisionCuatrimestral", Accion="UPDATE" }
  5. SaveChanges()
```

### RN-03: Evaluación Final

```
PRECONDICIÓN:
  - Las 3 RevisionCuatrimestral del objetivo tienen Completada==true
  - DateTime.Today >= objetivo.Deadline
  - No existe EvaluacionFinal para ese objetivoId

POSTCONDICIÓN:
  1. puntajeFinal = CalcularPonderado(objetivoId)  // ver RN-07
  2. INSERT EvaluacionFinal { puntajeFinal, comentario, resultado, fecha=now, evaluadorId }
  3. UPDATE Objetivo.Estado = COMPLETADO
  4. INSERT AuditoriaLog { Entidad="EvaluacionFinal", Accion="CREATE" }
  5. SaveChanges()
```

### RN-04: Cancelar Objetivo (soft delete)

```
NUNCA hacer DELETE físico de Objetivo.
UPDATE Objetivo.Estado = CANCELADO
INSERT AuditoriaLog { Accion="DELETE" (semántico), CambiosJson={"razon": razon} }
```

### RN-05: Actualizar Estado de Bitácora

```
Acción "Comentar":
  UPDATE BitacoraEntrada SET Estado=COMENTADO_JEFE, FeedbackJefe=texto, FechaFeedback=now

Acción "Requiere ajuste":
  UPDATE BitacoraEntrada SET Estado=REQUIERE_AJUSTE, FeedbackJefe=nota, FechaFeedback=now
  INSERT Notificacion { UsuarioId=empleadoId, Tipo=SOLICITUD_ACTUALIZACION }

Acción "Cerrar":
  UPDATE BitacoraEntrada SET Estado=CERRADO

Acción "Solicitar actualización":
  INSERT Notificacion { UsuarioId=empleadoId, Tipo=SOLICITUD_ACTUALIZACION,
                        Mensaje="El jefe solicita una actualización en la bitácora" }
```

### RN-06: Transición Automática de Estado EN_RIESGO

```
// Llamar después de cualquier UPDATE a Objetivo.Progreso o en batch diario
void EvaluarEstadoRiesgo(int objetivoId):
  if objetivo.Estado == ACTIVO:
    diasRestantes = (objetivo.Deadline - DateTime.Today).TotalDays
    if objetivo.Progreso < 50 AND diasRestantes < 60:
      objetivo.Estado = EN_RIESGO
  // no revertir EN_RIESGO automáticamente (requiere acción manual)
```

### RN-07: Fórmulas de Cálculo — EXACTAS

```
// Puntaje ponderado de un objetivo (1-5)
double CalcularPonderado(int objetivoId):
  q1 = revisiones.First(r => r.Periodo == Q1_ABRIL).Puntaje ?? 0
  q2 = revisiones.First(r => r.Periodo == Q2_AGOSTO).Puntaje ?? 0
  q3 = revisiones.First(r => r.Periodo == Q3_NOVIEMBRE).Puntaje ?? 0
  fin = evaluacionFinal?.PuntajeFinal ?? 0
  return (q1 * 0.2) + (q2 * 0.3) + (q3 * 0.3) + (fin * 0.2)

// Rendimiento de empleado por pilar (0-5)
double RendimientoPorPilar(int empleadoId, int pilarId, int anio):
  objetivo = objetivos.FirstOrDefault(o => o.EmpleadoId==empleadoId 
                                        && o.PilarId==pilarId 
                                        && o.Anio==anio)
  if objetivo == null: return 0
  return CalcularPonderado(objetivo.Id)

// Promedio general del empleado (0-5)
double PromedioGeneral(int empleadoId, int anio):
  pilares = [P001, P002, P003]
  valores = pilares.Select(p => RendimientoPorPilar(empleadoId, p, anio))
  objetivosConDatos = valores.Where(v => v > 0)
  if !objetivosConDatos.Any(): return 0
  return objetivosConDatos.Average()

// Semáforo (usar para badges y cards)
SemaforoColor GetSemaforo(double promedio):
  if promedio >= 4.0: return Verde   // 🟢
  if promedio >= 3.0: return Amarillo // 🟡
  return Rojo                         // 🔴

// Display sobre 100
int DisplayScore(double promedio):
  return (int)Math.Round(promedio * 20)
  // Ejemplo: 4.25 → 85/100

// Progreso de objetivo (campo guardado en BD, actualizable manualmente)
// RecalcularProgresoObjetivo: promedio de revisiones completadas * 20
void RecalcularProgresoObjetivo(int objetivoId):
  revisionesCompletadas = revisiones.Where(r => r.Completada && r.Puntaje.HasValue)
  if !revisionesCompletadas.Any(): return
  promedio = revisionesCompletadas.Average(r => r.Puntaje!.Value)
  objetivo.Progreso = (int)Math.Round(promedio * 20)
```

### RN-08: Fechas Fijas de Revisión (por año)

```
Q1_ABRIL    → 15 de Abril    del año del objetivo
Q2_AGOSTO   → 15 de Agosto   del año del objetivo
Q3_NOVIEMBRE→ 15 de Noviembre del año del objetivo
```

---

## USUARIO EN SESIÓN — MOCK (sin auth real)

```csharp
// En Program.cs o como servicio singleton scoped
// El jefe activo siempre es J001 — Roberto Fernández
// Inyectar ICurrentUserService en todos los servicios que necesiten JefeId

public interface ICurrentUserService {
    int UsuarioId { get; }
    int AreaId { get; }
    string NombreCompleto { get; }
    string Email { get; }
    string Rol { get; }
    bool EsJefe { get; }
    bool EstaAutenticado { get; }
    bool DebeCambiarPassword { get; }
    bool EsSuperusuario { get; }
    Task SetUsuarioAsync(int id, string nombreCompleto, string email, string rol, int areaId, bool esJefe, bool debeCambiarPassword, bool esSuperusuario);
    Task CerrarSesionAsync();
    Task InitializeAsync();
}

// El servicio utiliza ProtectedSessionStorage para mantener la sesión del usuario.
// El Login.razor establece el usuario en el servicio al autenticar.
```

---

## RADZEN — COMPONENTES Y PATRONES

### Patrones obligatorios

```razor
@* Diálogo de creación — patrón estándar *@
@inject DialogService DialogService

// Abrir diálogo:
var result = await DialogService.OpenAsync<CrearObjetivoDialog>(
    "Crear Objetivo",
    new Dictionary<string, object> { ["PilarId"] = pilarId },
    new DialogOptions { Width = "600px", CloseDialogOnOverlayClick = false }
);
if (result != null) await RefrescarGrid();

@* Notificación toast *@
@inject NotificationService NotificationService
NotificationService.Notify(NotificationSeverity.Success, "Éxito", "Objetivo creado");
NotificationService.Notify(NotificationSeverity.Error, "Error", mensaje);

@* Confirmación destructiva *@
var confirmado = await DialogService.Confirm(
    "¿Cancelar este objetivo?",
    "Confirmar cancelación",
    new ConfirmOptions { OkButtonText = "Sí, cancelar", CancelButtonText = "No" }
);
```

### Gráficos RadzenChart

```razor
@* Donut — Dashboard rendimiento área *@
<RadzenChart>
    <RadzenDonutSeries Data="@estadosArea" CategoryProperty="Estado" 
                       ValueProperty="Cantidad" Title="Objetivos del Área">
        <RadzenSeriesDataLabels Visible="true" />
    </RadzenDonutSeries>
</RadzenChart>

@* Línea — evolución temporal objetivo *@
<RadzenChart>
    <RadzenLineSeries Data="@evolucionObjetivo" CategoryProperty="Periodo" 
                      ValueProperty="Puntaje" Title="Evolución">
        <RadzenMarkers Visible="true" MarkerType="MarkerType.Circle" />
    </RadzenLineSeries>
    <RadzenCategoryAxis>
        <RadzenAxisTitle Text="Período" />
    </RadzenCategoryAxis>
    <RadzenValueAxis Min="0" Max="5">
        <RadzenAxisTitle Text="Puntaje" />
    </RadzenValueAxis>
</RadzenChart>

@* Radar — rendimiento por pilar *@
<RadzenChart>
    <RadzenRadarSeries Data="@radarEmpleado" CategoryProperty="Pilar" 
                       ValueProperty="Score" Title="Rendimiento por Pilar" />
</RadzenChart>
```

### Rating (evaluación 1-5)

```razor
@* RadzenRating para evaluaciones — Stars *@
<RadzenRating @bind-Value="@puntajeSeleccionado" Stars="5" />
@* puntajeSeleccionado es int, rango 1-5 *@
```

### Calendario

```razor
@* RadzenScheduler para calendario de eventos *@
<RadzenScheduler Data="@eventos" TItem="EventoCalendario"
                 StartProperty="Fecha" EndProperty="Fecha"
                 TextProperty="Titulo"
                 AppointmentSelect="@OnEventoClick">
    <RadzenMonthView />
</RadzenScheduler>
```

---

## DATOS SEED — EXACTOS

```csharp
// SeedData.cs — ejecutar en Program.cs si DB vacía
// Usar IDs enteros que coincidan con las FKs

// Área
{ Id=1, Nombre="Operaciones", Descripcion="Área de operaciones" }

// Jefe
{ Id=1, Nombre="Roberto", Apellido="Fernández", Email="roberto.fernandez@empresa.com",
  AreaId=1, Rol="JEFE" }

// Empleados
{ Id=1, Nombre="Juan",   Apellido="Pérez",      Puesto="Analista Senior",           AreaId=1, JefeId=1 }
{ Id=2, Nombre="María",  Apellido="González",   Puesto="Coordinadora de Proyectos", AreaId=1, JefeId=1 }
{ Id=3, Nombre="Carlos", Apellido="Rodríguez",  Puesto="Asistente Administrativo",  AreaId=1, JefeId=1 }
{ Id=4, Nombre="Laura",  Apellido="Martínez",   Puesto="Especialista en Calidad",   AreaId=1, JefeId=1 }

// Pilares
{ Id=1, Nombre="EXCELENCIA_ORGANIZACIONAL", ColorHex="#2563EB" }
{ Id=2, Nombre="INNOVACION_MEJORA",         ColorHex="#7C3AED" }
{ Id=3, Nombre="ORIENTACION_CLIENTE",       ColorHex="#059669" }

// SoftSkills (20 — IDs 1 a 20)
SS01-Comunicación efectiva, SS02-Liderazgo, SS03-Trabajo en equipo,
SS04-Resolución de problemas, SS05-Pensamiento crítico, SS06-Adaptabilidad,
SS07-Gestión del tiempo, SS08-Creatividad, SS09-Inteligencia emocional,
SS10-Negociación, SS11-Toma de decisiones, SS12-Empatía,
SS13-Proactividad, SS14-Resiliencia, SS15-Orientación a resultados,
SS16-Planificación estratégica, SS17-Delegación efectiva, SS18-Escucha activa,
SS19-Gestión de conflictos, SS20-Mentoría

// Objetivos de Juan Pérez (EmpleadoId=1) — año 2026
{ Id=1, PilarId=1, EmpleadoId=1, Anio=2026, Deadline="2026-12-15",
  SoftSkill1Id=1, SoftSkill2Id=3, Estado=ACTIVO, Progreso=65,
  Nombre="Optimizar procesos de análisis de datos",
  Descripcion="Reducir tiempo de procesamiento de reportes en 30% mediante automatización" }

{ Id=2, PilarId=2, EmpleadoId=1, Anio=2026, Deadline="2026-11-30",
  SoftSkill1Id=5, SoftSkill2Id=8, Estado=ACTIVO, Progreso=80,
  Nombre="Implementar dashboard ejecutivo",
  Descripcion="Crear tablero interactivo con métricas clave usando Power BI" }

{ Id=3, PilarId=3, EmpleadoId=1, Anio=2026, Deadline="2026-12-20",
  SoftSkill1Id=9, SoftSkill2Id=18, Estado=ACTIVO, Progreso=90,
  Nombre="Mejorar satisfacción del cliente interno",
  Descripcion="Aumentar NPS de clientes internos de 7 a 9 mediante mejora en tiempos de respuesta" }

// Revisiones de OBJ001 (ya tiene Q1 completada)
{ ObjetivoId=1, Periodo=Q1_ABRIL,    Completada=true,  Puntaje=3,
  Resultado=PARCIAL, FechaEvaluacion="2026-04-15", EvaluadorId=1 }
{ ObjetivoId=1, Periodo=Q2_AGOSTO,   Completada=false }
{ ObjetivoId=1, Periodo=Q3_NOVIEMBRE,Completada=false }

// Revisiones de OBJ002
{ ObjetivoId=2, Periodo=Q1_ABRIL,    Completada=true,  Puntaje=4,
  Resultado=CUMPLIDO, FechaEvaluacion="2026-04-15", EvaluadorId=1 }
{ ObjetivoId=2, Periodo=Q2_AGOSTO,   Completada=false }
{ ObjetivoId=2, Periodo=Q3_NOVIEMBRE,Completada=false }

// Revisiones de OBJ003
{ ObjetivoId=3, Periodo=Q1_ABRIL,    Completada=true,  Puntaje=5,
  Resultado=CUMPLIDO, FechaEvaluacion="2026-04-15", EvaluadorId=1 }
{ ObjetivoId=3, Periodo=Q2_AGOSTO,   Completada=false }
{ ObjetivoId=3, Periodo=Q3_NOVIEMBRE,Completada=false }

// Bitácora de OBJ001
{ Id=1, ObjetivoId=1, EmpleadoId=1, Fecha="2026-02-02",
  Texto="Implementé el nuevo procedimiento de extracción automática...",
  Estado=PENDIENTE_REVISION, AdjuntosJson='["proceso_automatizado_v1.pdf"]' }
{ Id=2, ObjetivoId=1, EmpleadoId=1, Fecha="2026-01-15",
  Texto="Reunión con equipo comercial para identificar reportes críticos...",
  Estado=COMENTADO_JEFE,
  FeedbackJefe="Excelente enfoque. Asegúrate de documentar los casos de uso.",
  FechaFeedback="2026-01-16" }
{ Id=3, ObjetivoId=1, EmpleadoId=1, Fecha="2026-01-05",
  Texto="Análisis inicial de procesos actuales...",
  Estado=CERRADO,
  FeedbackJefe="Bien identificado. Prioriza el cuello de botella #1.",
  FechaFeedback="2026-01-06", AdjuntosJson='["analisis_procesos.xlsx"]' }

// Mensajes chat (JefeId=1, EmpleadoId=1)
MSG001: RemitenteEsJefe=true,  Texto="Juan, vi tu última entrada... ¿Necesitas apoyo de IT?", Leido=true
MSG002: RemitenteEsJefe=false, Texto="Hola Roberto, sí, necesito acceso al servidor...",       Leido=true
MSG003: RemitenteEsJefe=true,  Texto="Perfecto, ya solicité el acceso...",                      Leido=true
MSG004: RemitenteEsJefe=false, Texto="Genial, gracias! Te mantendré al tanto...",               Leido=true
MSG005: RemitenteEsJefe=true,  Texto="¿Cómo va el tema del dashboard? Vi que estás al 80%.",   Leido=false
MSG006: RemitenteEsJefe=false, Texto="Va muy bien! Solo falta integrar las últimas métricas...",Leido=false

// Autoevaluación de Juan (OBJ001)
{ ObjetivoId=1, EmpleadoId=1, Score=4, FechaAutoevaluacion="2026-01-20",
  Comentario="Logré implementar 80% de las mejoras propuestas...",
  EvidenciasMencionadasJson='["Reducción de tiempos en 25%","Implementación de 4 de 5 controles"]' }

// Cursos mock (hardcoded en Cursos/Index.razor — sin BD)
CUR001: Liderazgo Transformacional | Coursera    | 40h | Progreso:75%  | Eval:8.5 | EN_CURSO
CUR002: Excel Avanzado             | LinkedIn    | 20h | Progreso:100% | Eval:9.0 | COMPLETADO
CUR003: Técnicas de Negociación    | Udemy       | 30h | Progreso:0%   | Eval:null | NO_INICIADO
CUR004: Power BI para Ejecutivos   | Platzi      | 25h | Progreso:60%  | Eval:7.8 | EN_CURSO
CUR005: Gestión de Proyectos       | Udacity     | 50h | Progreso:100% | Eval:9.5 | COMPLETADO
CUR006: Inteligencia Emocional     | edX         | 15h | Progreso:30%  | Eval:null | EN_CURSO
```

---

## VALIDACIONES — MENSAJES EXACTOS

| Código | Condición | Mensaje al usuario |
|--------|-----------|-------------------|
| VAL-01 | pilarId+empleadoId+año duplicado | "Ya existe un objetivo de este pilar para el empleado en {año}. ¿Desea reemplazarlo?" |
| VAL-02 | softSkills seleccionadas != 2 | "Debe seleccionar exactamente 2 soft skills para este objetivo" |
| VAL-03 | softSkill1Id == softSkill2Id | "Las dos soft skills deben ser diferentes" |
| VAL-04 | deadline <= DateTime.Today | "La fecha de cierre debe ser posterior a hoy" |
| VAL-05 | campo obligatorio vacío | "Este campo es obligatorio" |
| VAL-06 | sin empleados en área | "No hay empleados asignados a su área. Contacte a RRHH." |
| VAL-07 | revisión fuera de secuencia | "Debe completar la revisión {periodo anterior} antes de evaluar {periodo actual}" |
| VAL-08 | evaluación final prematura | "Solo puede realizar la evaluación final cuando todas las revisiones cuatrimestrales estén completas" |
| VAL-09 | evaluación final: deadline no alcanzado | "La evaluación final estará disponible a partir del {deadline}" |
| VAL-10 | puntaje fuera de rango | "El puntaje debe estar entre 1 y 5" |
| VAL-11 | sin evidencias marcadas | "Debe marcar al menos una evidencia verificada" |

---

## QUERIES PRINCIPALES — EF CORE (evitar N+1)

```csharp
// Dashboard: objetivos del área con estado
var objetivosArea = await _db.Objetivos
    .Include(o => o.Empleado)
    .Include(o => o.Pilar)
    .Where(o => o.Empleado.AreaId == areaId && o.Anio == DateTime.Now.Year)
    .ToListAsync();

// Empleado detalle completo
var empleado = await _db.Empleados
    .Include(e => e.Objetivos.Where(o => o.Anio == anio))
        .ThenInclude(o => o.Revisiones)
    .Include(e => e.Objetivos)
        .ThenInclude(o => o.EvaluacionFinal)
    .Include(e => e.Objetivos)
        .ThenInclude(o => o.Pilar)
    .FirstOrDefaultAsync(e => e.Id == empleadoId);

// Bitácora de un objetivo (orden descendente)
var bitacora = await _db.BitacoraEntradas
    .Where(b => b.ObjetivoId == objetivoId)
    .OrderByDescending(b => b.Fecha)
    .ToListAsync();

// Chat entre jefe y empleado
var mensajes = await _db.MensajesChat
    .Where(m => m.JefeId == jefeId && m.DestinatarioEmpleadoId == empleadoId)
    .OrderBy(m => m.Fecha)
    .ToListAsync();

// KPIs Dashboard
var hoy = DateTime.Today;
var en30Dias = hoy.AddDays(30);
int totalObjetivos   = objetivosArea.Count(o => o.Estado != EstadoObjetivo.CANCELADO);
int enCurso          = objetivosArea.Count(o => o.Estado == EstadoObjetivo.ACTIVO);
int vencenPronto     = objetivosArea.Count(o => o.Estado == EstadoObjetivo.ACTIVO 
                                              && o.Deadline <= en30Dias);
int pendientesRev    = await _db.RevisionesCuatrimestrales
    .Where(r => !r.Completada 
             && r.Objetivo.Empleado.AreaId == areaId
             && PeriodoAFecha(r.Periodo, r.Anio) < hoy)
    .CountAsync();
```

---

## ESTADOS VACÍOS — TEXTO EXACTO

| Pantalla | Condición | Mensaje |
|----------|-----------|---------|
| Dashboard | Sin objetivos | "No hay objetivos creados aún" |
| Objetivos | Grid vacío | "Aún no has creado objetivos. ¡Comienza agregando uno con el botón +" |
| Seguimientos | Empleado sin objetivos | "Este empleado no tiene objetivos asignados" |
| Seguimientos | Bitácora vacía | "Sin entradas de bitácora" |
| Seguimientos | Chat vacío | "Inicia la conversación" |
| Autoevaluación | Sin autoevaluaciones | "El empleado aún no ha realizado autoevaluaciones" |
| Evaluación | Sin pendientes | "Todas las revisiones del período están completas ✅" |

---

## LO QUE NO SE IMPLEMENTA

- Autenticación real (Windows Auth, Azure AD) → mock hardcodeado
- Portal de empleados → no existe ninguna pantalla para empleados
- Carga de archivos adjuntos reales → guardar solo nombre como string en JSON
- Notificaciones push → solo INSERT en tabla Notificacion
- CDN, caché distribuida, particionamiento
- Tests unitarios (fase posterior)
- El campo `Autoevaluacion.EvidenciasMencionadasJson` lo carga solo el seed — el jefe no puede modificarlo

---

## RUTAS BLAZOR — EXACTAS

```
@page "/dashboard"
@page "/objetivos"
@page "/seguimientos"
@page "/seguimientos/{EmpleadoId:int}"
@page "/seguimientos/{EmpleadoId:int}/objetivo/{ObjetivoId:int}"
@page "/autoevaluacion"
@page "/evaluacion"
@page "/cursos"
```

Ruta por defecto: redirigir "/" → "/dashboard"

```razor
@* App.razor o Routes.razor *@
<Router AppAssembly="typeof(App).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="routeData" DefaultLayout="typeof(MainLayout)" />
    </Found>
    <NotFound>
        <PageTitle>No encontrado</PageTitle>
        <LayoutView Layout="typeof(MainLayout)">
            <RadzenText>Página no encontrada.</RadzenText>
        </LayoutView>
    </NotFound>
</Router>
```

Navegación programática en páginas:
```csharp
@inject NavigationManager Nav
Nav.NavigateTo($"/seguimientos/{empleadoId}");
Nav.NavigateTo($"/seguimientos/{empleadoId}/objetivo/{objetivoId}");
```

---

## NAVMENU — PATRÓN RADZEN

Usar `RadzenPanelMenu` dentro de `RadzenSidebar`. Estructura fija, sin submenús.

```razor
@* Layout/NavMenu.razor *@
<RadzenPanelMenu>
    <RadzenPanelMenuItem Text="Dashboard"               Icon="dashboard" Path="dashboard" />
    <RadzenPanelMenuItem Text="Objetivos y Competencias" Icon="flag"     Path="objetivos" />
    <RadzenPanelMenuItem Text="Seguimientos"            Icon="people"    Path="seguimientos" />
    <RadzenPanelMenuItem Text="Autoevaluación"          Icon="assignment" Path="autoevaluacion" />
    <RadzenPanelMenuItem Text="Evaluación"              Icon="grading"   Path="evaluacion" />
    <RadzenPanelMenuItem Text="Cursos"                  Icon="school"    Path="cursos" />
    @if (CurrentUser.EsSuperusuario)
    {
        <RadzenPanelMenuItem Text="Administración Usuarios" Icon="admin_panel_settings" Path="admin/usuarios" />
    }
</RadzenPanelMenu>
```

```razor
@* Layout/MainLayout.razor *@
<RadzenLayout>
    <RadzenSidebar>
        <NavMenu />
    </RadzenSidebar>
    <RadzenBody>
        <RadzenHeader>
            <RadzenText TextStyle="TextStyle.H6">
                Bienvenido, @currentUser.NombreCompleto
            </RadzenText>
        </RadzenHeader>
        <RadzenContentContainer>
            @Body
        </RadzenContentContainer>
    </RadzenBody>
</RadzenLayout>
```

`RadzenPanelMenu` maneja el estado activo del ítem automáticamente según la URL actual. **No implementar lógica de selección manual.**

---

## REGLA DE ORO PARA EL AGENTE

> Ante cualquier duda de implementación, priorizar en este orden:
> 1. Lo que dice este CONTEXT.md
> 2. Lo que dice la especificación técnica original (PDF)
> 3. El comportamiento más simple posible

**No inventar patrones, no agregar capas, no introducir paquetes NuGet que no estén en este documento.**

Paquetes NuGet a usar:
- `Radzen.Blazor`
- `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.EntityFrameworkCore.Design`
- `System.Text.Json` (incluido en .NET 10)

---

## PORTABILIDAD SQLite → SQL SERVER

La base de datos actual es SQLite. En el futuro puede migrar a SQL Server. **Todo el código debe ser portable sin reescritura.**

### Regla principal
**Usar exclusivamente LINQ sobre DbSet.** EF Core traduce al dialecto correcto. Nunca escribir SQL que dependa del motor.

### Prohibido (rompe en SQL Server)
```csharp
// ❌ Funciones de fecha SQLite-específicas
.Where(o => o.Deadline.ToString("yyyy-MM-dd") < DateTime.Today.ToString())

// ❌ SQL raw con sintaxis SQLite
_db.Objetivos.FromSqlRaw("SELECT * FROM Objetivos WHERE strftime('%Y', Deadline) = '2026'")

// ❌ PRAGMA o comandos SQLite
_db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL")
```

### Correcto (funciona en ambos motores)
```csharp
// ✅ Comparación de fechas con LINQ puro
.Where(o => o.Deadline < DateTime.Today)
.Where(o => o.Deadline.Year == 2026)

// ✅ Ordenamiento, agrupamiento, conteo — siempre LINQ
.GroupBy(o => o.Estado).Select(g => new { Estado = g.Key, Cantidad = g.Count() })
```

### Campos JSON (EvidenciasRevisadasJson, AdjuntosJson, etc.)
- Se almacenan como `TEXT` / `NVARCHAR(MAX)` — EF Core lo maneja igual en ambos motores.
- La serialización/deserialización se hace **siempre en el servicio C#** con `System.Text.Json`.
- **Nunca** hacer parsing de JSON dentro de una query EF Core.

```csharp
// ✅ Correcto: deserializar en memoria después del query
var revision = await _db.Revisiones.FindAsync(id);
var evidencias = JsonSerializer.Deserialize<List<string>>(revision.EvidenciasRevisadasJson) ?? new();
```

### Cuando se migre a SQL Server — únicos 3 cambios necesarios
1. Reemplazar paquete: `Microsoft.EntityFrameworkCore.Sqlite` → `Microsoft.EntityFrameworkCore.SqlServer`
2. En `Program.cs`: `options.UseSqlite(...)` → `options.UseSqlServer(...)`
3. Regenerar migraciones: `dotnet ef migrations add InitialSqlServer`

Si el código respeta las reglas anteriores, no hay nada más que cambiar.
