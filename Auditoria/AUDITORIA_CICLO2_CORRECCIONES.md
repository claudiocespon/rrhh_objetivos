# AUDITORÍA CICLO 2 — PQ-Talent (RRHH_Objetivos)

**Fecha:** 15 de Abril de 2026  
**Ciclo:** 2 (post-correcciones parciales)  
**Proyecto:** `C:\Development\Antigravity\RRHH_Objetivos`

---

## RESUMEN DE CORRECCIONES APLICADAS

Se verificaron los 39 hallazgos del Ciclo 1 contra el código actual. **Se corrigieron 10 hallazgos, quedan 29 abiertos.**

### ✅ HALLAZGOS RESUELTOS

| ID | Hallazgo | Estado |
|----|----------|--------|
| C-06 | Autoevaluación — EmpleadoId usaba UsuarioId del Jefe | ✅ CORREGIDO — Ahora busca empleado por email |
| C-07 | Chat — Sin refresh, sin marcado de leídos, sin indicadores | ✅ CORREGIDO — Timer 10s, MarcarComoLeidos, auto-scroll, badges en Seguimientos |
| A-03 | ObjetivoService sin validación VAL-01 (duplicados) | ✅ CORREGIDO — Agrega `AnyAsync` antes del INSERT |
| A-04 | ObjetivoService sin validación VAL-03 (SoftSkills iguales) | ✅ CORREGIDO — Valida `SoftSkill1Id == SoftSkill2Id` |
| A-05 | Inconsistencia deadline en EvaluacionFinal | ✅ CORREGIDO — Removido `DateTime.Today < Deadline` con comentario explícito |
| A-06 | RendimientoService — Fórmula sin documentar | ✅ CORREGIDO — XML doc extenso explica comportamiento y diferencia con CONTEXT.md |
| A-11 | AuditoriaLog.Accion "CANCEL" en vez de "DELETE" | ✅ CORREGIDO — Cambiado a "DELETE" con comentario RN-04 |
| M-07 | UpdateObjetivoAsync no llamaba EvaluarEstadoRiesgoAsync | ✅ CORREGIDO — Ahora lo llama post-save |
| M-09 | Chat sin auto-refresh | ✅ CORREGIDO — Timer cada 10s con IDisposable |
| — | ObjetivoService.CrearObjetivoAsync sin validación VAL-04 (deadline) | ✅ CORREGIDO — Valida `Deadline <= DateTime.Today` |

---

## HALLAZGOS QUE PERMANECEN ABIERTOS (29)

### SEVERIDAD CRÍTICA (5 restantes)

#### C-01: Modelo de Revisiones diverge del CONTEXT.md (VIGENTE — decisión documentada)
**Estado:** ACEPTADO — Se mantiene `FEEDBACK_MITAD_ANIO` por decisión explícita. RendimientoService ahora documenta la fórmula simplificada. **No requiere acción adicional** salvo que el negocio pida restaurar Q1/Q2/Q3.

#### C-02: Cursos — Módulo esqueleto sin funcionalidad real
**Archivos:** `Services/CursoService.cs`, `Pages/Cursos/Index.razor`, `Domain/Entities/Entities.cs`  
**Estado:** ❌ SIN CAMBIOS — Solo lista cursos de la BD. Sin asignación a empleados, sin progreso, sin estados, sin evaluaciones. Botón "Ver Curso" sigue sin handler.

#### C-03: Notificaciones — Solo INSERT, sin panel de visualización
**Archivos:** `Components/Layout/MainLayout.razor`  
**Estado:** ❌ SIN CAMBIOS — El click en campanita sigue marcando todo como leído sin mostrar nada. No hay panel ni dropdown de notificaciones.

#### C-04: Calendario de Eventos — Entidad existe, UI no existe
**Estado:** ❌ SIN CAMBIOS — No se creó página de calendario ni se agregó RadzenScheduler. EventoCalendario se crea pero nunca se muestra.

#### C-05: ExportService limitado
**Archivo:** `Services/ExportService.cs`  
**Estado:** ❌ SIN CAMBIOS — Solo CSV básico sin métricas de rendimiento.

---

### SEVERIDAD ALTA (7 restantes)

#### A-01/A-02: Ruta `/misobjetivos` en vez de `/objetivos`
**Estado:** ❌ SIN CAMBIOS — NavMenu sigue con `Path="misobjetivos"`, la página sigue con `@page "/misobjetivos"`.

#### A-07: Inyección directa de AppDbContext en páginas Razor
**Estado:** ❌ SIN CAMBIOS — Las siguientes páginas siguen inyectando `@inject AppDbContext Db` directamente:
- `Seguimientos/Index.razor` — queries de empleados y promedios
- `Seguimientos/EmpleadoDetalle.razor` — query complejo con Includes
- `Autoevaluaciones/Index.razor` — queries de autoevaluaciones
- `Autoevaluaciones/AutoevaluarDialog.razor` — carga y guardado directo
- `Evaluacion/EvaluarDialog.razor` — carga de revisión
- `Evaluacion/EvaluarFinalDialog.razor` — carga de objetivo
- `Layout/MainLayout.razor` — queries de notificaciones

#### A-08: DataScopeService no cubre Empleados ni Autoevaluaciones
**Estado:** ❌ SIN CAMBIOS — Solo tiene overloads para `Objetivo` y `RevisionCuatrimestral`. La lógica de scope sigue duplicada en páginas.

#### A-09: Logo sin clase CSS en Login.razor
**Estado:** ❌ SIN CAMBIOS — `.login-logo-img` sigue sin definir.

#### A-10: No hay migración inicial (InitialCreate)
**Estado:** ❌ SIN CAMBIOS — Solo las 2 migraciones parciales.

#### A-12: Upload sin validación de seguridad
**Archivo:** `Controllers/UploadController.cs`  
**Estado:** ❌ SIN CAMBIOS — Sin validación de tipo MIME, tamaño, auth, ni path traversal.

---

### SEVERIDAD MEDIA (10 restantes)

| ID | Hallazgo | Estado |
|----|----------|--------|
| M-01 | Dashboard Donut muestra datos engañosos | ❌ Sin cambios |
| M-02 | EmpleadoDetalle N+1 en RendimientoPorPilarAsync | ❌ Sin cambios |
| M-03 | Seguimientos/Index N+1 en PromedioGeneralAsync | ❌ Sin cambios |
| M-04 | Entidad Pais sin uso en lógica de scope | ❌ Sin cambios |
| M-05 | EstadoBadge sin documentación de uso | ❌ Sin cambios |
| M-06 | SeedData no verificada | ❌ Sin cambios |
| M-08 | MainLayout usa ExecuteUpdateAsync sin servicio | ❌ Sin cambios |
| M-10 | EvaluarDialog evidencias hardcodeadas | ❌ Sin cambios |
| M-11 | CrearObjetivoDialog/EditarObjetivoDialog no auditados | ❌ Sin cambios |
| NUEVO-M-12 | ObjetivoDetalle.razor sigue inyectando `@inject AppDbContext Db` | ❌ No se usa directamente pero está inyectado |

---

### SEVERIDAD BAJA (7 restantes)

| ID | Hallazgo |
|----|----------|
| B-01 | CSS del login embebido en componente |
| B-02 | Directorio `Shared/` vacío en raíz de Objetivos.Web |
| B-03 | Pages legacy (_Host.cshtml, _Layout.cshtml) — verificar uso |
| B-04 | `antigravityDownloadFile` JS no verificada en wwwroot |
| B-05 | Favicon genérico |
| B-07 | Auditorías previas en raíz del repo |
| B-08 | Curso sin relación con SoftSkills |

---

## NUEVOS HALLAZGOS DETECTADOS EN CICLO 2

### NUEVO-A-13: Chat timer puede causar ObjectDisposedException en Blazor Server

**Archivo:** `ObjetivoDetalle.razor`  
**Problema:** El `Timer` callback hace `await InvokeAsync(...)` pero si el circuito Blazor se desconecta antes del Dispose, `InvokeAsync` lanzará `ObjectDisposedException`. El try/catch en `OnAfterRenderAsync` solo cubre el scroll JS, no el timer callback.

**Corrección:** Envolver el callback del timer en try/catch:
```csharp
_chatTimer = new Timer(async _ =>
{
    try
    {
        await InvokeAsync(async () => { ... });
    }
    catch (ObjectDisposedException) { }
}, null, 10_000, 10_000);
```

### NUEVO-A-14: Chat — GetConversacionesConNoLeidosAsync usa `DestinatarioEmpleadoId` como key pero la agrupación es incorrecta

**Archivo:** `Services/ChatService.cs`  
**Problema:** La query agrupa por `DestinatarioEmpleadoId`, pero en el modelo de `MensajeChat`, `DestinatarioEmpleadoId` es el empleado en la conversación (no necesariamente el remitente). Cuando el empleado envía un mensaje (`RemitenteEsJefe=false`), `DestinatarioEmpleadoId` sigue siendo el ID del empleado. Esto funciona correctamente por coincidencia del modelo, pero el nombre del campo es confuso. Verificar con datos reales que los counts sean correctos.

### NUEVO-M-13: ObjetivoService.CrearObjetivoAsync rechaza silenciosamente duplicados

**Archivo:** `Services/ObjetivoService.cs`  
**Problema:** La validación VAL-01 ahora retorna `false` si existe un duplicado, pero el CONTEXT.md especifica que debería mostrar "¿Desea reemplazarlo?" al usuario. Actualmente el servicio solo rechaza — no hay mecanismo para que el UI ofrezca reemplazo. La corrección implementada es correcta como "red de seguridad" pero incompleta funcionalmente: el UI debería preguntar primero y enviar un flag de "reemplazo confirmado".

---

## PLAN DE CORRECCIÓN CICLO 2 — ETAPAS PARA CLAUDE CODE

Las etapas se reorganizan priorizando los hallazgos abiertos. Se omiten los ya resueltos.

---

### ETAPA 1 — REFACTORIZAR DBCONTEXT FUERA DE PÁGINAS (A-07, A-08, M-08)

Esta es la deuda técnica más extendida. Impacta 7 archivos y bloquea optimizaciones N+1.

**Prompt para Claude Code:**
```
Lee CONTEXT.md. Refactoriza para que NINGUNA página Razor inyecte AppDbContext directamente.
Toda lógica de queries debe ir en Services/. Usar IDbContextFactory<AppDbContext> en servicios nuevos.

1. Crear Services/NotificacionService.cs:
   - GetNotificacionesAsync(int usuarioId, int take = 20) → List<Notificacion>
   - GetNoLeidasCountAsync(int usuarioId) → int
   - MarcarTodasComoLeidasAsync(int usuarioId)
   - MarcarComoLeidaAsync(int notificacionId)

2. Crear Services/SeguimientoService.cs (o ampliar RendimientoService):
   - GetEmpleadosEquipoConPromediosAsync(ICurrentUserService user, int anio) 
     → retornar lista con promedios pre-calculados en UNA query batch
   - GetEmpleadoDetalleCompletoAsync(int empleadoId, int anio) 
     → empleado + objetivos + radar data + ponderados, todo en memoria

3. Crear Services/AutoevaluacionService.cs:
   - GetAutoevaluacionesAsync(ICurrentUserService user) → data personal + equipo
   - GuardarAutoevaluacionAsync(Autoevaluacion ae) → lógica de save actualmente en AutoevaluarDialog
   - GetObjetivoParaAutoevAsync(int objetivoId) → carga de objetivo con SoftSkills

4. Ampliar RevisionService:
   - GetRevisionDetalleAsync(int revisionId) → carga la revisión con Objetivo y SoftSkills
   - (Ya tiene CompletarRevisionAsync)

5. Ampliar DataScopeService con overloads:
   - AplicarScope(IQueryable<Empleado>, ICurrentUserService)
   - AplicarScope(IQueryable<Autoevaluacion>, ICurrentUserService)

6. Actualizar las páginas para usar servicios en vez de @inject AppDbContext Db:
   - Seguimientos/Index.razor → usar SeguimientoService
   - Seguimientos/EmpleadoDetalle.razor → usar SeguimientoService
   - Autoevaluaciones/Index.razor → usar AutoevaluacionService
   - Autoevaluaciones/AutoevaluarDialog.razor → usar AutoevaluacionService
   - Evaluacion/EvaluarDialog.razor → usar RevisionService
   - Evaluacion/EvaluarFinalDialog.razor → usar ObjetivoService.GetByIdAsync
   - Layout/MainLayout.razor → usar NotificacionService

7. Registrar servicios nuevos en Program.cs como AddScoped.

Archivos a crear: NotificacionService.cs, SeguimientoService.cs, AutoevaluacionService.cs
Archivos a modificar: todas las páginas listadas + DataScopeService.cs + RevisionService.cs + Program.cs
```

---

### ETAPA 2 — PANEL DE NOTIFICACIONES (C-03)

**Prompt para Claude Code:**
```
Usando el NotificacionService creado en Etapa 1, implementa un panel de notificaciones:

1. Crear Components/Shared/NotificacionPanel.razor:
   - Componente que se renderiza como dropdown/sidebar al click de la campanita
   - Lista las últimas 20 notificaciones del usuario
   - Cada notificación muestra: icono según TipoNotificacion, mensaje, fecha relativa
   - Notificaciones no leídas destacadas con fondo diferente
   - Botón "Marcar todas como leídas" en el header del panel

2. Modificar MainLayout.razor:
   - Reemplazar OnNotificationsClick actual por apertura del panel
   - Usar RadzenDialog o panel posicionado absolutamente bajo el ícono
   - Mantener el badge de conteo

3. Buscar el emoji correcto:
   - SOLICITUD_ACTUALIZACION → icono "update"
   - NUEVA_EVALUACION → icono "grading"  
   - DEADLINE_PROXIMO → icono "schedule"
```

---

### ETAPA 3 — CALENDARIO DE EVENTOS (C-04)

**Prompt para Claude Code:**
```
Implementa la vista de calendario:

1. Crear Services/CalendarioService.cs:
   - GetEventosAsync(ICurrentUserService user, int anio) → List<EventoCalendario>
   - Aplicar scope según rol del usuario

2. Opción A — Página dedicada:
   - Crear Components/Pages/Calendario/Index.razor con @page "/calendario"
   - Usar RadzenScheduler con datos del servicio
   - Colores por tipo: DEADLINE_OBJETIVO=rojo, FEEDBACK_MITAD_ANIO=azul, EVALUACION_FINAL=verde

3. Opción B — Integrar en Dashboard (preferida para MVP):
   - Agregar sección "Próximos Eventos" debajo del Donut chart
   - Mostrar los próximos 5 eventos como lista con fecha e ícono

4. Agregar al NavMenu si se elige Opción A:
   - RadzenPanelMenuItem Text="Calendario" Icon="event" Path="calendario"

5. Registrar CalendarioService en Program.cs
```

---

### ETAPA 4 — MÓDULO DE CURSOS COMPLETO (C-02)

**Prompt para Claude Code:**
```
Implementa el módulo de Cursos funcional:

1. Crear entidad Domain/Entities — agregar en Entities.cs:
   public class CursoAsignacion {
       public int Id { get; set; }
       public int CursoId { get; set; }
       public Curso Curso { get; set; } = null!;
       public int EmpleadoId { get; set; }
       public Empleado Empleado { get; set; } = null!;
       public int Progreso { get; set; } = 0; // 0-100
       public string Estado { get; set; } = "NO_INICIADO"; // NO_INICIADO, EN_CURSO, COMPLETADO
       public double? Calificacion { get; set; }
       public DateTime? FechaInicio { get; set; }
       public DateTime? FechaCompletado { get; set; }
   }

2. Agregar DbSet<CursoAsignacion> en AppDbContext + constraint único CursoId+EmpleadoId

3. Ampliar CursoService.cs:
   - GetCursosConAsignacionAsync(int empleadoId) → cursos con estado de asignación del empleado
   - GetAsignacionesEquipoAsync(ICurrentUserService user) → para vista de jefe
   - InscribirAsync(int cursoId, int empleadoId)
   - ActualizarProgresoAsync(int asignacionId, int progreso)

4. Rediseñar Pages/Cursos/Index.razor:
   - Tab "Catálogo" → lista con botón Inscribirse (si no está inscrito)
   - Tab "Mis Cursos" → cursos del empleado con barra de progreso y estado
   - Tab "Equipo" (solo jefes) → grid con empleado, curso, progreso

5. Crear migración: dotnet ef migrations add AddCursoAsignaciones

6. Registrar cambios en Program.cs si es necesario.
```

---

### ETAPA 5 — SEGURIDAD UPLOAD + RUTA OBJETIVOS (A-01, A-12)

**Prompt para Claude Code:**
```
1. En Controllers/UploadController.cs:
   - Agregar validación de tipos MIME: solo .pdf, .docx, .xlsx, .jpg, .jpeg, .png
   - Limitar tamaño: 10MB por archivo (verificar file.Length)
   - Sanitizar nombre: reemplazar caracteres peligrosos, prevenir path traversal
   - Retornar 400 Bad Request con mensaje descriptivo si falla validación
   
2. Corregir ruta de Objetivos:
   - En Components/Pages/MisObjetivos/Index.razor: agregar @page "/objetivos" 
     (mantener @page "/misobjetivos" como alias para no romper bookmarks)
   - En NavMenu.razor: cambiar Path="misobjetivos" a Path="objetivos"
```

---

### ETAPA 6 — OPTIMIZACIÓN N+1 (M-02, M-03)

**Prompt para Claude Code:**
```
Usando el SeguimientoService creado en Etapa 1, optimizar queries N+1:

1. SeguimientoService.GetEmpleadosEquipoConPromediosAsync:
   - Cargar todos los objetivos del equipo en UNA query con Include
   - Calcular promedios en memoria con CalcularPonderadoInterno (hacer método estático público)
   - Retornar Dictionary<int, double> o un DTO con empleado + promedio

2. SeguimientoService.GetEmpleadoDetalleCompletoAsync:
   - La query de Include ya está correcta en EmpleadoDetalle
   - Mover los cálculos de radar y ponderados a in-memory usando los datos cargados
   - Exponer CalcularPonderadoInterno como método estático público en RendimientoService

3. Verificar que el timer del chat NO haga queries N+1 adicionales.
```

---

### ETAPA 7 — DASHBOARD MEJORADO (M-01)

**Prompt para Claude Code:**
```
Corregir el gráfico Donut del Dashboard:

1. En DashboardService.cs agregar conteo de estados:
   - EnRiesgo = count donde Estado == EN_RIESGO
   - Completados = count donde Estado == COMPLETADO

2. En Dashboard.razor, cambiar donutData para mostrar la distribución real:
   var donutData = new List<DonutItem>
   {
       new() { Estado = "En Curso", Cantidad = data.EnCurso },
       new() { Estado = "En Riesgo", Cantidad = data.EnRiesgo },
       new() { Estado = "Completados", Cantidad = data.Completados },
   };
   // "Vencen Pronto" ya tiene su KPI card, no incluirlo en el donut

3. Actualizar DashboardData con las propiedades nuevas (EnRiesgo, Completados).
```

---

### ETAPA 8 — FIX CHAT TIMER + VAL-01 COMPLETO (NUEVO-A-13, NUEVO-M-13)

**Prompt para Claude Code:**
```
1. En ObjetivoDetalle.razor — Proteger el timer callback:
   _chatTimer = new Timer(async _ =>
   {
       try
       {
           await InvokeAsync(async () =>
           {
               if (objetivo != null)
               {
                   mensajes = await ChatService.GetConversacionAsync(objetivo.Empleado.JefeId, EmpleadoId);
                   _scrollPending = true;
                   StateHasChanged();
               }
           });
       }
       catch (ObjectDisposedException) { /* Circuit disconnected, timer will be disposed */ }
   }, null, 10_000, 10_000);

2. En ObjetivoService.CrearObjetivoAsync — Soportar reemplazo de duplicados:
   - Cambiar firma: public async Task<(bool Ok, bool Duplicado)> CrearObjetivoAsync(Objetivo nuevo, bool reemplazar = false)
   - Si duplicado y reemplazar=false → retornar (false, true)
   - Si duplicado y reemplazar=true → cancelar el existente y crear el nuevo
   - Actualizar CrearObjetivoDialog.razor para manejar el caso Duplicado:
     mostrar DialogService.Confirm con mensaje VAL-01 y reintentar con reemplazar=true
```

---

### ETAPA 9 — LIMPIEZA Y CONSISTENCIA (Bajas)

**Prompt para Claude Code:**
```
Limpieza general:

1. Eliminar directorio vacío Objetivos.Web/Shared/
2. En Login.razor: mover estilos CSS a wwwroot/css/site.css
3. Agregar .login-logo-img { width: 80px; height: auto; } en site.css
4. Verificar que wwwroot tiene el JS para antigravityDownloadFile 
   (si no existe, crear wwwroot/js/app.js con la función e incluirlo en _Host.cshtml)
5. Verificar que .gitignore excluya bin/, obj/, *.db, .vs/
6. Verificar migración inicial — si solo hay 2 parciales, considerar crear InitialCreate
7. En Login.razor: agregar class="login-logo-img" al <img> tag
```

---

## DIAGRAMA DE DEPENDENCIAS CICLO 2

```
ETAPA 1 (Refactor DbContext) ←── PRIORIDAD MÁXIMA, desbloquea todo
   │
   ├──→ ETAPA 2 (Panel Notificaciones)
   │
   ├──→ ETAPA 6 (Optimización N+1)
   │
   └──→ ETAPA 7 (Dashboard mejorado)

ETAPA 3 (Calendario) ←── Independiente
ETAPA 4 (Cursos) ←── Independiente  
ETAPA 5 (Seguridad + Ruta) ←── Independiente
ETAPA 8 (Fix Timer + VAL-01) ←── Independiente, ejecutar cuanto antes
ETAPA 9 (Limpieza) ←── Ejecutar al final
```

---

## NOTAS PARA CLAUDE CODE

1. **Siempre leer CONTEXT.md primero** — es la fuente de verdad del proyecto.
2. **Usar `IDbContextFactory<AppDbContext>`** en todos los servicios nuevos.
3. **No agregar paquetes NuGet** que no estén en CONTEXT.md.
4. **Ejecutar `dotnet build`** después de cada etapa.
5. **Generar migraciones** si se modifica el modelo: `dotnet ef migrations add NombreDescriptivo`.
6. **La Etapa 1 es la más grande** — considerar dividirla en sub-etapas si Claude Code tiene límite de contexto.
7. **Mantener portabilidad SQLite ↔ SQL Server** — solo LINQ, nunca SQL raw.

---

*Fin de Auditoría Ciclo 2 — Documento generado para uso con Claude Code*
