# AUDITORÍA TÉCNICA EXTERNA — TIER 1
**Proyecto:** RRHH_Objetivos  
**Stack:** .NET 10 / Blazor Server / EF Core SQLite / Radzen  
**Fecha:** 2026-03-30  
**Última actualización:** 2026-03-30 — CRIT-03 reclasificado a diferido; agregado FEAT-01 (diseño flujo email)  
**Rol:** Firma de auditoría externa. Cero asunciones sobre intenciones de diseño.

---

## RESUMEN EJECUTIVO

El proyecto es un sistema de gestión de objetivos RRHH (Blazor Server, SQLite, single-project). Se identificaron **27 hallazgos** distribuidos en 5 categorías de severidad, más **1 requerimiento de diseño nuevo** (FEAT-01). Los bloqueantes críticos afectan seguridad de autenticación y privilegios; los de alta severidad comprometen integridad de datos y escalabilidad. El sistema **no está listo para producción** sin resolver al menos los 5 hallazgos CRÍTICOS activos y los 8 de ALTA severidad.

| Severidad | Cantidad | Estado |
|-----------|----------|--------|
| CRÍTICO activo | 5 | Pendiente |
| CRÍTICO diferido | 1 | CRIT-03 — se resuelve en pre-producción con FEAT-01 |
| ALTO | 8 | Pendiente |
| MEDIO | 8 | Pendiente |
| BAJO | 5 | Pendiente |
| Feature de diseño | 1 | FEAT-01 — nuevo |
| **Total** | **28** | |

---

## HALLAZGOS CRÍTICOS

---

### [CRIT-01] Escalada de privilegios: `EsSuperusuario` persiste en sesión sin re-validación

**Archivo:** `CurrentUserService.cs`, `AdminUsuarios.razor`, `MainLayout.razor`  
**Clasificación:** Security — Privilege Escalation

**Descripción:**  
El claim `EsSuperusuario` se almacena en `ProtectedSessionStorage` al momento del login y **nunca se revalida contra la base de datos**. Si un administrador revoca el flag `EsSuperusuario` de un usuario en la pantalla `AdminUsuarios`, ese usuario mantiene acceso total al panel de administración durante toda la vida de su sesión activa. No existe ningún mecanismo de invalidación de sesión.

**Evidencia:**
```csharp
// AdminUsuarios.razor — la guarda se hace solo en memoria de sesión:
if (!CurrentUser.EsSuperusuario) 
{
    Nav.NavigateTo("dashboard");
    return;
}
// ...pero CurrentUser.EsSuperusuario proviene de SessionStorage, nunca de DB.
```

**Impacto:** Un usuario cuyo privilegio fue removido continúa con acceso administrativo indefinidamente.

**Remediación:**  
Implementar un middleware o `CascadingAuthenticationState` real con claims de ASP.NET Core. Alternativamente, en cada operación sensible, revalidar contra DB con `IDbContextFactory`. El `ProtectedSessionStorage` es adecuado para UX pero no para autorización.

---

### [CRIT-02] Ausencia total de autorización server-side en endpoints de servicio

**Archivos:** Todos los `*Service.cs`  
**Clasificación:** Security — Missing Authorization

**Descripción:**  
Ningún método de servicio valida que el `_currentUser.UsuarioId` tenga permiso sobre el recurso que está modificando. La autorización existe únicamente en la **capa de UI** (Razor). En Blazor Server, la UI y el servidor son el mismo proceso, pero cualquier llamada directa a un servicio —ya sea desde otro componente, un test, o una inyección de dependencias alternativa— bypasea completamente los controles de acceso.

**Evidencia:**
```csharp
// ObjetivoService.cs — ninguna validación de ownership:
public async Task<bool> UpdateObjetivoAsync(Objetivo objetivo)
{
    var existing = await _db.Objetivos.FindAsync(objetivo.Id);
    if (existing == null) return false;
    // No verifica que existing.EmpleadoId pertenezca al scope del currentUser
    existing.Nombre = objetivo.Nombre;
    // ...
}

// RevisionService.cs — cualquier jefe puede completar cualquier revisión:
public async Task<bool> CompletarRevisionAsync(int revisionId, ...)
{
    var revision = await _db.RevisionesCuatrimestrales
        .Include(r => r.Objetivo).ThenInclude(o => o.Revisiones)
        .FirstOrDefaultAsync(r => r.Id == revisionId);
    // No valida que revision.Objetivo.Empleado.JefeId == _currentUser.UsuarioId
```

**Impacto:** Cualquier jefe autenticado puede modificar objetivos o completar revisiones de empleados que no le pertenecen mediante manipulación de IDs.

**Remediación:**  
Agregar guards de autorización en cada método mutante de los servicios. Ejemplo:
```csharp
if (!await _dataScope.PuedeOperarSobreObjetivoAsync(objetivoId, _currentUser))
    throw new UnauthorizedAccessException();
```

---

### [CRIT-03] ~~Contraseña inicial = Legajo~~ — DIFERIDO A PRE-PRODUCCIÓN

**Archivos:** `UsuarioService.cs`, `SeedData.cs`  
**Clasificación:** Security — Weak Credential Initialization  
**Estado:** ⏸ **DIFERIDO — Se mantiene la implementación actual (legajo como password inicial) para el entorno de desarrollo y testing. Se resolverá en la fase de pre-producción como parte de FEAT-01 (flujo completo de email con password random). Ver sección FEAT-01.**

**Descripción original:**  
Todos los usuarios se crean con `PasswordHash = AuthService.HashPassword(row.Legajo)`. Si bien `DebeCambiarPassword = true` obliga el cambio en el primer login, la ventana de vulnerabilidad es real en un entorno expuesto: el atacante solo necesita el legajo para autenticarse antes de que el usuario cambie su clave.

**Decisión de equipo:** Aceptable para desarrollo/demo. En pre-producción se implementará FEAT-01 que elimina completamente la dependencia del legajo como credencial inicial.

**Riesgo aceptado hasta pre-producción:** Medio — solo si la aplicación se expone a una red no controlada antes de FEAT-01.

---

### [CRIT-04] `RecuperarPasswordAsync` cambia la contraseña sin notificar al usuario real

**Archivo:** `AuthService.cs`  
**Clasificación:** Security — Account Takeover Vector

**Descripción:**  
El flujo de "recuperación de contraseña" genera una nueva contraseña aleatoria, la escribe en la DB, y retorna `true`. **No hay ningún mecanismo de entrega**: ni email, ni código en pantalla, ni token de un solo uso. El comentario `// TODO: Integrar envío por email` confirma que esto es incompleto. El efecto actual es que cualquiera que conozca el email de un usuario puede **bloquear su acceso** iniciando una recuperación (la contraseña cambia pero el legítimo usuario nunca la recibe).

**Evidencia:**
```csharp
public async Task<bool> RecuperarPasswordAsync(string email)
{
    // ...
    jefe.PasswordHash = hash;          // Password changed!
    jefe.DebeCambiarPassword = true;
    await _db.SaveChangesAsync();
    // TODO: Integrar envío por email — NUNCA IMPLEMENTADO
    return true;
}
```

**Impacto:** Denial-of-service sobre cuentas individuales. Potencial account takeover si el atacante logra acceso al sistema antes que la víctima.

**Remediación (se consolida con FEAT-01):**  
Implementar flujo completo con token de tiempo limitado almacenado en DB y entrega vía email. Ver FEAT-01 para el diseño completo. Hasta entonces, el botón "Olvidaste tu contraseña" debe estar deshabilitado en producción.

---

### [CRIT-05] `AppDbContext` registrado como Scoped y como Singleton-Factory simultáneamente

**Archivo:** `Program.cs`  
**Clasificación:** Architecture — DbContext Lifetime Conflict

**Descripción:**  
```csharp
// Program.cs:
builder.Services.AddDbContextFactory<AppDbContext>(...);  // IDbContextFactory: Singleton
builder.Services.AddScoped(p => p.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());
```
Registrar `AppDbContext` como Scoped derivado de una factory Singleton crea **instancias de DbContext que no están bajo el control del DI container para disposal**. El DbContext retornado por `factory.CreateDbContext()` no es tracked por el scope: si el scope se destruye, el DbContext queda sin hacer `Dispose()` hasta que el GC lo recolecte. En Blazor Server (donde los scopes son equivalentes a circuitos de larga vida), esto genera **acumulación de conexiones abiertas**.

Adicionalmente, mezclar servicios que usan `AppDbContext` (directo) con servicios que usan `IDbContextFactory` (ej. `EvaluacionService`, `UsuarioService`) crea inconsistencias en el patrón: algunos servicios comparten contexto, otros crean el propio.

**Impacto:** Memory leaks, conexiones no liberadas, comportamiento no determinístico en operaciones concurrentes.

**Remediación:**  
Eliminar el registro `AddScoped` del DbContext derivado de factory. Migrar **todos** los servicios a `IDbContextFactory<AppDbContext>` con `using var db = await _dbFactory.CreateDbContextAsync()`. Esto garantiza disposal correcto y es el patrón recomendado por Microsoft para Blazor Server.

---

### [CRIT-06] Emails de superusuarios en `appsettings.json` (comiteado en repositorio)

**Archivo:** `appsettings.json`, `appsettings.Development.json`  
**Clasificación:** Security — Sensitive Data in Source Control

**Descripción:**  
```json
"SuperUsers": {
    "Emails": [
        "ccespon@permaquim.com",
        "ptripodi@permaquim.com",
        "scrosio@permaquim.com"
    ]
}
```
Emails corporativos reales de superusuarios están hardcodeados en archivos de configuración que forman parte del repositorio Git. El `.gitignore` no los excluye.

**Impacto:** Cualquier persona con acceso al repositorio conoce exactamente qué cuentas tienen privilegios máximos en el sistema, facilitando ataques dirigidos.

**Remediación:**  
Mover a variables de entorno o User Secrets (desarrollo) / Azure Key Vault o equivalente (producción). Usar `dotnet user-secrets` para desarrollo. Nunca comprometer emails de usuarios reales.

---

## HALLAZGOS DE ALTA SEVERIDAD

---

### [HIGH-01] `DataScopeService` retorna `query.Where(o => false)` para colaboradores

**Archivo:** `DataScopeService.cs`  
**Clasificación:** Architecture — Silent Data Denial

**Descripción:**  
```csharp
// Default to no access or just personal (handled in services usually by ID)
return query.Where(o => false);
```
Para usuarios que no son Jefes ni tienen roles especiales (colaboradores estándar), el scope retorna un predicado `false`, efectivamente devolviendo cero resultados silenciosamente. El comentario "handled in services usually by ID" implica que hay otro mecanismo, pero **ningún servicio que llama a `AplicarScope` para datos de equipo valida que el caller sea jefe**. Esto puede ocultar bugs donde un colaborador ve una página vacía cuando debería ver sus propios datos, o donde la lógica asume que "scope vacío = sin equipo" cuando podría significar "scope mal configurado".

**Remediación:**  
Cambiar el caso default a `throw new UnauthorizedAccessException("Rol sin scope definido")` o a un scope explícito `query.Where(o => o.EmpleadoId == userId)` para colaboradores. El fail-silencioso es un antipatrón de seguridad.

---

### [HIGH-02] `EvaluacionService` duplica lógica de scope de `DataScopeService`

**Archivos:** `EvaluacionService.cs`, `DataScopeService.cs`  
**Clasificación:** Design — DRY Violation, Authorization Inconsistency

**Descripción:**  
`EvaluacionService.GetEvaluacionDataAsync` implementa su propia lógica de filtrado por rol (DIRECTOR_GENERAL, RRHH, EsSuperusuario, DIRECTOR, JefeId) en lugar de delegar a `DataScopeService`. Esto crea **dos fuentes de verdad** para las reglas de acceso. Si se modifica una, la otra queda desincronizada.

**Evidencia:**
```csharp
// EvaluacionService.cs — duplica exactamente la lógica de DataScopeService:
bool canSeeAll = currentUser.Rol == "DIRECTOR_GENERAL" || currentUser.Rol == "RRHH" || currentUser.EsSuperusuario;
if (!canSeeAll) {
    if (currentUser.Rol == "DIRECTOR") { ... }
    else { queryRev = queryRev.Where(r => r.Objetivo.Empleado.JefeId == currentUser.UsuarioId); }
}
```

**Remediación:**  
`EvaluacionService` debe recibir `DataScopeService` por DI y delegar el filtrado. Extender `DataScopeService` para soportar `IQueryable<RevisionCuatrimestral>` (ya tiene un overload, pero no se usa en EvaluacionService).

---

### [HIGH-03] Sin validación de puntaje en `CompletarRevisionAsync`

**Archivo:** `RevisionService.cs`  
**Clasificación:** Data Integrity — Missing Input Validation

**Descripción:**  
```csharp
public async Task<bool> CompletarRevisionAsync(int revisionId, int puntaje, ...)
{
    // ...
    revision.Puntaje = puntaje;  // puntaje podría ser -999 o 9999
```
No hay validación de que `puntaje` esté en el rango 1-5. Un puntaje fuera de rango corrompe silenciosamente todos los cálculos posteriores: `RendimientoService.CalcularPonderadoInterno`, `DisplayScore`, `GetSemaforoColor`.

**Remediación:**  
```csharp
if (puntaje < 1 || puntaje > 5) return false;
```
Además, agregar constraint a nivel de modelo con `[Range(1, 5)]` y considerar FK a una tabla de puntajes válidos.

---

### [HIGH-04] `Objetivo.CreadoPorId` sin navegación ni FK definida en EF Core

**Archivos:** `Entities.cs`, `AppDbContext.cs`  
**Clasificación:** Data Integrity — Orphan Foreign Key

**Descripción:**  
```csharp
public class Objetivo {
    public int CreadoPorId { get; set; }
    // No hay: public Empleado? CreadoPor { get; set; }
    // No hay: public Jefe? CreadoPorJefe { get; set; }
}
```
`CreadoPorId` puede referenciar tanto un `Jefe.Id` como un `Empleado.Id` (ver `ObjetivoService`: `nuevo.CreadoPorId = _currentUser.UsuarioId`). No hay FK configurada en `AppDbContext.OnModelCreating`, no hay propiedad de navegación, y el tipo de usuario no se guarda. Esto hace la referencia integralmente inútil: no se puede resolver `CreadoPorId` a un nombre sin saber si es Jefe o Empleado.

**Remediación:**  
Agregar `bool CreadoPorEsJefe` al modelo, o unificar `Jefe` y `Empleado` en una tabla `Usuario` con discriminador de rol (refactoring mayor). Como mínimo, agregar la FK y la propiedad de navegación correspondiente.

---

### [HIGH-05] `AuditoriaLog` sin FK real, no audita cambios de datos

**Archivo:** `Entities.cs`, todos los servicios  
**Clasificación:** Audit — Incomplete Audit Trail

**Descripción:**  
El `AuditoriaLog` tiene `EntidadId` (int) y `Entidad` (string) como referencia polimórfica sin FK real. El campo `CambiosJson` en la mayoría de los casos se registra como `"{}"` (vacío), sin capturar el estado anterior ni posterior. En `ObjetivoService.UpdateObjetivoAsync`:
```csharp
_db.AuditoriaLogs.Add(new AuditoriaLog
{
    Entidad = "Objetivo", EntidadId = objetivo.Id, Accion = "UPDATE",
    // CambiosJson = "{}" — no hay registro de qué cambió
});
```
Un log de auditoría que no registra los cambios es funcionalmente equivalente a no tener log.

**Remediación:**  
Usar `_db.Entry(existing).OriginalValues` antes y `CurrentValues` después para serializar el diff. Considerar Interceptors de EF Core para auditoría centralizada.

---

### [HIGH-06] `MainLayout` inyecta `AppDbContext` directamente en un Layout de larga vida

**Archivo:** `MainLayout.razor`  
**Clasificación:** Architecture — DbContext in Long-Lived Component

**Descripción:**  
```csharp
@inject AppDbContext Db
```
`MainLayout` es un componente que vive durante toda la sesión del circuito Blazor. Inyectar `AppDbContext` directamente en él mantiene el contexto abierto indefinidamente. EF Core DbContext no está diseñado para uso de larga vida: acumula el change tracker, no refleja cambios hechos por otras instancias, y puede presentar comportamiento stale.

**Remediación:**  
Inyectar `IDbContextFactory<AppDbContext>` y crear contextos de corta vida para cada operación (`LoadNotifications`, `OnNotificationsClick`).

---

### [HIGH-07] `ObjetivoService` usa `DateTime.Now.Year` para asignar año al crear objetivo

**Archivo:** `ObjetivoService.cs`  
**Clasificación:** Logic Bug — Timezone Inconsistency

**Descripción:**  
```csharp
nuevo.Anio = DateTime.Now.Year;
```
El resto del sistema usa `DateTime.UtcNow` para timestamps. Si el servidor está en UTC y se ejecuta `CrearObjetivoAsync` el 31 de diciembre a las 23:30 UTC pero el usuario está en UTC-3, `DateTime.Now.Year` podría retornar el año correcto o el siguiente dependiendo de la configuración del servidor. Más grave: si el servidor migra a zona horaria diferente, los objetivos creados cerca de año nuevo tendrán año incorrecto.

**Remediación:**  
Decidir una estrategia de tiempo consistente. Recomendado: usar `DateTimeOffset.UtcNow.Year` o derivarlo del campo `Deadline` que viene del usuario.

---

### [HIGH-08] `SeedData` carga CSV completo en memoria con fallback silencioso de JefeId

**Archivo:** `SeedData.cs`  
**Clasificación:** Reliability — Silent Data Corruption on Seed

**Descripción:**  
Cuando `FindJefeId` no puede resolver el responsable de un empleado, asigna silenciosamente el primer jefe de la base:
```csharp
var fallbackJefeId = jefeMap.Values.FirstOrDefault()?.Id ?? 1;
Console.WriteLine($"[SeedData WARNING] No se pudo resolver jefe...");
return fallbackJefeId;
```
El warning va a `Console.WriteLine` (no al logger de ASP.NET Core), no es visible en producción, y el empleado queda con un jefe incorrecto. Dado que toda la lógica de evaluación y scope depende de `JefeId`, esto puede resultar en empleados completamente invisibles para su jefe real.

**Remediación:**  
Usar `ILogger` en lugar de `Console.WriteLine`. Agregar una lista de "filas con error" y lanzar excepción o escribir un archivo de reporte post-seed. Considerar hacer el seed transaccional.

---

## HALLAZGOS DE SEVERIDAD MEDIA

---

### [MED-01] Roles implementados como `string` libre sin enum ni constantes

**Archivos:** `Entities.cs`, `DataScopeService.cs`, `EvaluacionService.cs`, múltiples Razor  
**Clasificación:** Design — Magic Strings, Type Safety

**Descripción:**  
`Jefe.Rol` es un `string` con valores como `"JEFE"`, `"GERENTE"`, `"DIRECTOR"`, `"DIRECTOR_GENERAL"`, `"RRHH"`. Estos valores se comparan literalmente en al menos 8 lugares distintos sin ninguna constante centralizada. Existe `Domain/Enums/Enums.cs` pero el enum de roles no está ahí. 

**Evidencia:** `user.Rol == "DIRECTOR_GENERAL"` aparece en `DataScopeService`, `EvaluacionService`, `DashboardService`, `AdminUsuarios.razor`.

**Remediación:**  
Crear `public enum RolUsuario { JEFE, LIDER, GERENTE, DIRECTOR, DIRECTOR_GENERAL, RRHH }` y usar enum en todas partes. Almacenar como string en DB mediante `.HasConversion<string>()`.

---

### [MED-02] `GetObjetivosRoleAsync` busca empleado por email en cada llamada

**Archivo:** `ObjetivoService.cs`, `DashboardService.cs`, `EvaluacionService.cs`  
**Clasificación:** Performance — N+1 / Redundant DB Lookup

**Descripción:**  
```csharp
var empleadoPropio = await _db.Empleados.FirstOrDefaultAsync(e => e.Email.ToLower() == email && e.Activo);
```
Este patrón aparece en al menos 3 servicios diferentes. Cada carga de página hace una query adicional para resolver `email → EmpleadoId`, información que **ya está disponible en `ICurrentUserService`**. El `UsuarioId` en sesión es el ID del usuario autenticado; si el usuario es un empleado, ese es su `EmpleadoId`. No hay razón para buscarlo por email.

**Remediación:**  
Agregar `bool EsEmpleado => !EsJefe` y usar `UsuarioId` directamente cuando `!EsJefe`. Esto elimina 3 queries redundantes en cada carga de pantalla principal.

---

### [MED-03] `Pilar.Nombre` es un string libre que duplica la semántica de un enum

**Archivos:** `Entities.cs`, `SeedData.cs`, múltiples Razor  
**Clasificación:** Design — Type Safety, Data Integrity

**Descripción:**  
`Pilar.Nombre` almacena `"EXCELENCIA_ORGANIZACIONAL"`, `"INNOVACION_MEJORA"`, `"ORIENTACION_CLIENTE"` como strings. Los Razor hacen `.Replace("_", " ")` para display. No hay constraint que impida crear un Pilar con nombre arbitrario.

**Remediación:**  
Agregar enum `TipoPilar` o al menos un check constraint en EF. Agregar propiedad `NombreDisplay` calculada.

---

### [MED-04] Todas las entidades están en un único archivo `Entities.cs`

**Archivo:** `Domain/Entities/Entities.cs`  
**Clasificación:** Design — Single Responsibility, Maintainability

**Descripción:**  
16 clases de dominio en un solo archivo de 200+ líneas. Viola el principio de un archivo por tipo. Dificulta code reviews, merge conflicts y navegación.

**Remediación:**  
Separar en archivos individuales: `Pais.cs`, `Area.cs`, `Jefe.cs`, `Empleado.cs`, etc.

---

### [MED-05] `ChatService` no valida que el remitente pertenezca a la conversación

**Archivo:** `ChatService.cs`  
**Clasificación:** Security — IDOR (Insecure Direct Object Reference)

**Descripción:**  
```csharp
public async Task EnviarMensajeAsync(MensajeChat mensaje)
{
    mensaje.Fecha = DateTime.UtcNow;
    mensaje.Leido = false;
    _db.MensajesChat.Add(mensaje);  // No valida que mensaje.RemitenteId == currentUser
```
Cualquier usuario autenticado puede enviar mensajes haciéndose pasar por otro usuario simplemente construyendo un `MensajeChat` con `RemitenteId` arbitrario.

**Remediación:**  
```csharp
mensaje.RemitenteId = _currentUser.UsuarioId;
mensaje.RemitenteEsJefe = _currentUser.EsJefe;
```
Nunca confiar en los IDs que vienen del cliente para identificar al remitente.

---

### [MED-06] `EvaluacionFinal.ResultadoFinal` puede ser inconsistente con `PuntajeFinal`

**Archivos:** `RevisionService.cs`, `EvaluacionService.cs`  
**Clasificación:** Logic — Business Rule Inconsistency

**Descripción:**  
`CompletarEvaluacionFinalAsync` recibe `ResultadoEval resultado` como parámetro del jefe, independientemente del `puntajeFinal` calculado automáticamente. El jefe puede asignar `ResultadoEval.CUMPLIDO` con `PuntajeFinal = 1.2`. No hay validación de coherencia entre el score calculado y el resultado declarado.

**Remediación:**  
Derivar `ResultadoFinal` automáticamente del `PuntajeFinal` usando `RendimientoService.GetSemaforoColor` o un método equivalente, o al menos validar coherencia.

---

### [MED-07] `ExportService` no verifica el scope de datos al exportar

**Archivo:** `ExportService.cs`, `MisObjetivos/Index.razor`  
**Clasificación:** Security — Data Exposure

**Descripción:**  
El CSV se genera a partir de `roleData.Equipo` que viene del scope aplicado en `ObjetivoService`. Sin embargo, el método `ExportObjetivosToCsv` acepta cualquier `List<Objetivo>` sin validación de scope. Si en el futuro se llama desde otro lugar sin scope, exportará todos los datos.

**Remediación:**  
El `ExportService` debería recibir `ICurrentUserService` y aplicar su propio scope, o al menos registrar quién exportó qué en `AuditoriaLogs`.

---

### [MED-08] `RendimientoService.CalcularPonderadoInterno` asigna peso 0 a revisiones nulas

**Archivo:** `RendimientoService.cs`  
**Clasificación:** Logic — Silent Score Distortion

**Descripción:**  
```csharp
double q1 = objetivo.Revisiones.FirstOrDefault(r => r.Periodo == PeriodoRevision.Q1_ABRIL)?.Puntaje ?? 0;
// pesos: q1*0.2 + q2*0.3 + q3*0.3 + fin*0.2
```
Si Q1 no está completado (puntaje null → 0), el cálculo ponderado no excluye ese componente, sino que lo cuenta como 0. Un objetivo con solo Q3 completado con puntaje 5 dará `0*0.2 + 0*0.3 + 5*0.3 + 0*0.2 = 1.5` en lugar de `5`. Los pesos no se redistribuyen cuando faltan componentes.

**Remediación:**  
Calcular la ponderación solo sobre revisiones completadas, redistribuyendo los pesos proporcionalmente, o documentar explícitamente que el score es acumulativo (0 = aún sin evaluar).

---

## HALLAZGOS DE SEVERIDAD BAJA

---

### [LOW-01] `BitacoraService` y `CursoService` usan primary constructors pero el patrón no es consistente

**Clasificación:** Style — Inconsistency  
Algunos servicios usan primary constructors (C# 12), otros usan constructores tradicionales. No es un bug pero reduce legibilidad al mezclar estilos.

---

### [LOW-02] `Objetivo.Progreso` se calcula y almacena en DB pero también se calcula on-the-fly

**Clasificación:** Design — Redundancy  
`RendimientoService.RecalcularProgresoObjetivoAsync` escribe `Progreso` en DB. `RendimientoService.DisplayScore` lo recalcula desde revisiones. Dos fuentes de verdad para el mismo dato.

---

### [LOW-03] `SoftSkill.Nombre` tiene prefijo `SS01-` hardcodeado en seed

**Archivo:** `SeedData.cs`  
**Clasificación:** Data Quality  
```csharp
Nombre = $"SS{i + 1:D2}-{skillNames[i]}"
```
El prefijo artificial `SS01-` se mezcla con el nombre de la habilidad y aparece en la UI. Es ruido visual.

---

### [LOW-04] No hay manejo de errores en las páginas Razor (sin `try/catch` en `OnInitializedAsync`)

**Clasificación:** UX — Error Handling  
Si un servicio lanza excepción durante la carga inicial, el usuario ve un crash en lugar de un mensaje amigable. Considerar `ErrorBoundary` de Blazor.

---

### [LOW-05] `objetivos.db` está en el directorio de contenido y en `.gitignore` implícito, pero no explícito

**Clasificación:** Operations  
El archivo SQLite de 228KB está en el directorio del proyecto. Debería estar excluido explícitamente en `.gitignore` y nunca comiteado en producción.

---

## DEUDA TÉCNICA ESTRUCTURAL

### [DEBT-01] Modelo de dominio anémico — sin lógica de negocio en entidades

Las entidades son pure DTOs. Toda la lógica de negocio está dispersa en servicios. Para un sistema de esta complejidad (reglas de validación de secuencia de revisiones, cálculo de puntajes, transiciones de estado) sería apropiado usar Domain Objects con métodos como `objetivo.Completar()`, `revision.ValidarSecuencia()`.

### [DEBT-02] Sin capa de Application Services ni CQRS — servicios hacen query + mutation en el mismo método

`GetObjetivosRoleAsync` combina lectura de datos personales y de equipo. `GetDashboardDataAsync` combina múltiples queries en un único método. A medida que el sistema crezca, estos métodos se volverán imposibles de testear y mantener.

### [DEBT-03] Sin tests — 0% cobertura verificable

No existe ningún proyecto de tests en la solución. La lógica de `CalcularPonderadoInterno`, `EvaluarEstadoRiesgoAsync`, y las reglas de secuencia de revisiones son críticas para el negocio y no tienen ninguna cobertura.

### [DEBT-04] Single-project architecture limita escalabilidad

Todo el código (Domain, Data, Services, UI) en un único proyecto `Objetivos.Web`. Cuando se requiera separar en microservicios, agregar API REST, o implementar tests de integración, el costo de refactoring será alto.

---

## FEAT-01 — DISEÑO: Flujo de email transaccional con password random y cambio obligatorio

**Relacionado con:** CRIT-03 (diferido), CRIT-04  
**Fase de implementación:** Pre-producción  
**Prioridad:** Bloqueante para go-live

### Descripción del requerimiento

El sistema necesita un mecanismo completo para la entrega de credenciales iniciales y recuperación de contraseña, que elimine la dependencia del legajo como credencial y garantice que el usuario real recibe la clave generada. El flujo debe forzar el cambio de contraseña en el siguiente login, patrón ya estructurado en el modelo (`DebeCambiarPassword`).

### Casos de uso cubiertos

1. **Creación de nuevo usuario** — el administrador crea el usuario; el sistema genera una contraseña aleatoria, la hashea y la envía al email del usuario.
2. **Reset de contraseña por administrador** — el administrador hace reset; el sistema genera nueva contraseña aleatoria y la envía al email.
3. **Recuperación de contraseña por el propio usuario** — el usuario ingresa su email; el sistema genera una contraseña aleatoria y la envía al email registrado.

En los tres casos: `DebeCambiarPassword = true`, lo que activa el flujo de cambio obligatorio en el próximo login (ya implementado en `Login.razor`).

### Diseño de la interfaz `IEmailService`

```csharp
// Services/IEmailService.cs
namespace Objetivos.Web.Services;

public interface IEmailService
{
    /// <summary>
    /// Envía las credenciales iniciales a un usuario recién creado.
    /// </summary>
    Task EnviarCredencialesInicialesAsync(string destinatario, string nombreCompleto, string passwordTemporal);

    /// <summary>
    /// Envía una contraseña de reset (por admin o por recuperación propia).
    /// </summary>
    Task EnviarPasswordResetAsync(string destinatario, string nombreCompleto, string passwordTemporal);
}
```

### Implementación con SmtpClient / SendGrid (patrón Strategy)

El servicio se registra en DI. Se proveen dos implementaciones intercambiables:

**Opción A — SMTP nativo (MailKit recomendado sobre SmtpClient legacy):**

```csharp
// Services/SmtpEmailService.cs
using MailKit.Net.Smtp;
using MimeKit;

public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;

    public SmtpEmailService(IOptions<SmtpSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task EnviarCredencialesInicialesAsync(string destinatario, string nombreCompleto, string passwordTemporal)
    {
        var body = $"""
            Hola {nombreCompleto},
            
            Tu cuenta en RRHH Objetivos ha sido creada.
            
            Tu contraseña temporal es: {passwordTemporal}
            
            Deberás cambiarla en tu próximo inicio de sesión.
            Esta contraseña expira en 72 horas si no es utilizada.
            
            Accedé al sistema en: {_settings.AppUrl}
            """;

        await EnviarAsync(destinatario, "Bienvenido a RRHH Objetivos — Credenciales de acceso", body);
    }

    public async Task EnviarPasswordResetAsync(string destinatario, string nombreCompleto, string passwordTemporal)
    {
        var body = $"""
            Hola {nombreCompleto},
            
            Se ha generado una nueva contraseña temporal para tu cuenta.
            
            Tu contraseña temporal es: {passwordTemporal}
            
            Deberás cambiarla en tu próximo inicio de sesión.
            
            Si no solicitaste este cambio, contactá a RRHH inmediatamente.
            """;

        await EnviarAsync(destinatario, "RRHH Objetivos — Restablecimiento de contraseña", body);
    }

    private async Task EnviarAsync(string destinatario, string asunto, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(destinatario));
        message.Subject = asunto;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Host, _settings.Port, _settings.UseSsl);
        if (!string.IsNullOrEmpty(_settings.Username))
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
```

**Opción B — SendGrid (recomendado para producción cloud):**

```csharp
// Services/SendGridEmailService.cs
using SendGrid;
using SendGrid.Helpers.Mail;

public class SendGridEmailService : IEmailService
{
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public SendGridEmailService(IConfiguration config)
    {
        _apiKey   = config["SendGrid:ApiKey"]   ?? throw new InvalidOperationException("SendGrid:ApiKey no configurado");
        _fromEmail = config["SendGrid:FromEmail"] ?? throw new InvalidOperationException("SendGrid:FromEmail no configurado");
        _fromName  = config["SendGrid:FromName"]  ?? "RRHH Objetivos";
    }

    public async Task EnviarCredencialesInicialesAsync(string destinatario, string nombreCompleto, string passwordTemporal)
        => await EnviarAsync(destinatario, nombreCompleto, passwordTemporal,
            "Bienvenido a RRHH Objetivos — Credenciales de acceso", esReset: false);

    public async Task EnviarPasswordResetAsync(string destinatario, string nombreCompleto, string passwordTemporal)
        => await EnviarAsync(destinatario, nombreCompleto, passwordTemporal,
            "RRHH Objetivos — Restablecimiento de contraseña", esReset: true);

    private async Task EnviarAsync(string dest, string nombre, string pwd, string asunto, bool esReset)
    {
        var client = new SendGridClient(_apiKey);
        var from   = new EmailAddress(_fromEmail, _fromName);
        var to     = new EmailAddress(dest);
        var plain  = esReset
            ? $"Hola {nombre}, tu nueva contraseña temporal es: {pwd}. Debés cambiarla al ingresar."
            : $"Hola {nombre}, tu cuenta fue creada. Tu contraseña temporal es: {pwd}. Debés cambiarla al ingresar.";
        var msg = MailHelper.CreateSingleEmail(from, to, asunto, plain, plain);
        var response = await client.SendEmailAsync(msg);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"SendGrid error: {response.StatusCode}");
    }
}
```

**Implementación Null (para desarrollo/testing sin SMTP real):**

```csharp
// Services/NullEmailService.cs
using Microsoft.Extensions.Logging;

public class NullEmailService : IEmailService
{
    private readonly ILogger<NullEmailService> _logger;
    public NullEmailService(ILogger<NullEmailService> logger) { _logger = logger; }

    public Task EnviarCredencialesInicialesAsync(string dest, string nombre, string pwd)
    {
        _logger.LogWarning("[NullEmailService] Credenciales para {Nombre} <{Email}>: {Pwd}", nombre, dest, pwd);
        return Task.CompletedTask;
    }

    public Task EnviarPasswordResetAsync(string dest, string nombre, string pwd)
    {
        _logger.LogWarning("[NullEmailService] Reset para {Nombre} <{Email}>: {Pwd}", nombre, dest, pwd);
        return Task.CompletedTask;
    }
}
```

### Configuración en `appsettings.json`

```json
"Email": {
    "Provider": "Smtp",  // "Smtp" | "SendGrid" | "Null"
    "Smtp": {
        "Host": "",
        "Port": 587,
        "UseSsl": true,
        "Username": "",
        "Password": "",
        "FromAddress": "noreply@permaquim.com",
        "FromName": "RRHH Objetivos",
        "AppUrl": "https://rrhh.permaquim.com"
    }
},
"SendGrid": {
    "ApiKey": "",
    "FromEmail": "noreply@permaquim.com",
    "FromName": "RRHH Objetivos"
}
```

> ⚠️ `ApiKey`, `Password` y credenciales nunca en repositorio. Usar User Secrets en desarrollo y variables de entorno en producción.

### Registro en `Program.cs`

```csharp
var emailProvider = builder.Configuration["Email:Provider"] ?? "Null";
switch (emailProvider)
{
    case "SendGrid":
        builder.Services.AddScoped<IEmailService, SendGridEmailService>();
        break;
    case "Smtp":
        builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Email:Smtp"));
        builder.Services.AddScoped<IEmailService, SmtpEmailService>();
        break;
    default:
        builder.Services.AddScoped<IEmailService, NullEmailService>(); // desarrollo
        break;
}
```

### Cambios en `AuthService` — integrar `IEmailService`

```csharp
// AuthService.cs — nueva firma del constructor:
public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;

    public AuthService(AppDbContext db, IEmailService email)
    {
        _db = db;
        _email = email;
    }

    // RecuperarPasswordAsync actualizado:
    public async Task<bool> RecuperarPasswordAsync(string email)
    {
        email = email.Trim().ToLowerInvariant();
        var randomPassword = GenerarPasswordAleatorio();
        var hash = HashPassword(randomPassword);

        var jefe = await _db.Jefes.FirstOrDefaultAsync(j => j.Email.ToLower() == email && j.Activo);
        if (jefe != null)
        {
            jefe.PasswordHash = hash;
            jefe.DebeCambiarPassword = true;
            await _db.SaveChangesAsync();
            await _email.EnviarPasswordResetAsync(jefe.Email, $"{jefe.Nombre} {jefe.Apellido}", randomPassword);
            return true;
        }

        var empleado = await _db.Empleados.FirstOrDefaultAsync(e => e.Email.ToLower() == email && e.Activo);
        if (empleado != null)
        {
            empleado.PasswordHash = hash;
            empleado.DebeCambiarPassword = true;
            await _db.SaveChangesAsync();
            await _email.EnviarPasswordResetAsync(empleado.Email, $"{empleado.Nombre} {empleado.Apellido}", randomPassword);
            return true;
        }

        return false;
    }

    // GenerarPasswordAleatorio: cambiar de private a internal static
    internal static string GenerarPasswordAleatorio()
    {
        // ... implementación actual sin cambios
    }
}
```

### Cambios en `UsuarioService` — enviar credenciales al crear

```csharp
// UsuarioService.cs — CreateUsuarioAsync actualizado:
public class UsuarioService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IEmailService _email;

    public UsuarioService(IDbContextFactory<AppDbContext> dbFactory, IEmailService email)
    {
        _dbFactory = dbFactory;
        _email = email;
    }

    public async Task<bool> CreateUsuarioAsync(UsuarioDto dto)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var passwordTemporal = AuthService.GenerarPasswordAleatorio();

        if (dto.EsJefe)
        {
            var jefe = new Jefe
            {
                // ...campos existentes...
                PasswordHash = AuthService.HashPassword(passwordTemporal),
                DebeCambiarPassword = true,
            };
            db.Jefes.Add(jefe);
            await db.SaveChangesAsync();
            await _email.EnviarCredencialesInicialesAsync(jefe.Email, $"{jefe.Nombre} {jefe.Apellido}", passwordTemporal);
        }
        else
        {
            var emp = new Empleado
            {
                // ...campos existentes...
                PasswordHash = AuthService.HashPassword(passwordTemporal),
                DebeCambiarPassword = true,
            };
            db.Empleados.Add(emp);
            await db.SaveChangesAsync();
            await _email.EnviarCredencialesInicialesAsync(emp.Email, $"{emp.Nombre} {emp.Apellido}", passwordTemporal);
        }

        return true;
    }

    // ResetPasswordAsync actualizado:
    public async Task<bool> ResetPasswordAsync(int id, bool esJefe)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var passwordTemporal = AuthService.GenerarPasswordAleatorio();

        if (esJefe)
        {
            var jefe = await db.Jefes.FindAsync(id);
            if (jefe == null) return false;
            jefe.PasswordHash = AuthService.HashPassword(passwordTemporal);
            jefe.DebeCambiarPassword = true;
            await db.SaveChangesAsync();
            await _email.EnviarPasswordResetAsync(jefe.Email, $"{jefe.Nombre} {jefe.Apellido}", passwordTemporal);
        }
        else
        {
            var emp = await db.Empleados.FindAsync(id);
            if (emp == null) return false;
            emp.PasswordHash = AuthService.HashPassword(passwordTemporal);
            emp.DebeCambiarPassword = true;
            await db.SaveChangesAsync();
            await _email.EnviarPasswordResetAsync(emp.Email, $"{emp.Nombre} {emp.Apellido}", passwordTemporal);
        }

        return true;
    }
}
```

### Dependencias NuGet a agregar

```xml
<!-- Para SmtpEmailService -->
<PackageReference Include="MailKit" Version="4.*" />

<!-- Para SendGridEmailService -->
<PackageReference Include="SendGrid" Version="9.*" />
```

Solo agregar el paquete correspondiente al provider elegido. `NullEmailService` no requiere dependencias externas.

### Comportamiento de `DebeCambiarPassword` — ya implementado

El flujo de cambio obligatorio en el primer login **ya está correctamente implementado** en `Login.razor` (`OnLogin` detecta `result.DebeCambiarPassword == true` y muestra el formulario de cambio antes de navegar al dashboard). No se requieren cambios en el componente de login para FEAT-01.

### Criterios de aceptación para pre-producción

- [ ] `IEmailService` registrada en DI con el provider correcto según entorno
- [ ] `NullEmailService` activa en `appsettings.Development.json` (logea la contraseña en el logger)
- [ ] `SmtpEmailService` o `SendGridEmailService` activa en producción con credenciales vía variables de entorno
- [ ] `CreateUsuarioAsync` envía email con contraseña aleatoria (no legajo)
- [ ] `ResetPasswordAsync` envía email con contraseña aleatoria (no legajo)
- [ ] `RecuperarPasswordAsync` envía email con contraseña aleatoria
- [ ] Todos los flujos setean `DebeCambiarPassword = true`
- [ ] Test de integración: crear usuario → verificar `DebeCambiarPassword == true` y `PasswordHash != HashPassword(legajo)`
- [ ] CRIT-03 marcado como ✅ RESUELTO en esta auditoría una vez validados los criterios

---

## PROMPT PARA EL AGENTE DE REMEDIACIÓN

```
Eres un agente de refactoring para el proyecto C:\Development\Antigravity\RRHH_Objetivos.
Lee primero este archivo completo: C:\Development\Antigravity\RRHH_Objetivos\AUDITORIA_EXTERNA_TIER1.md

Ejecuta el siguiente plan en orden estricto. Después de cada paso, compila el proyecto y confirma 0 errores antes de continuar.

FASE 1 — SEGURIDAD CRÍTICA (no negociable antes de cualquier deploy)

PASO 1 [CRIT-05]: Eliminar el registro `AddScoped(p => p.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext())` de Program.cs. Migrar TODOS los servicios que usan `AppDbContext` directamente a `IDbContextFactory<AppDbContext>`. Los servicios afectados son: AuthService, ObjetivoService, RevisionService, BitacoraService, ChatService, CursoService, RendimientoService, DashboardService. Cada método debe usar `using var db = await _dbFactory.CreateDbContextAsync()`. Actualizar MainLayout.razor para usar IDbContextFactory en lugar de AppDbContext directo.

PASO 2 [CRIT-04 + FEAT-01 parcial]: Crear el archivo Services/IEmailService.cs con la interfaz IEmailService tal como está especificada en la sección FEAT-01 de este documento. Crear Services/NullEmailService.cs con la implementación NullEmailService. Registrar NullEmailService como implementación por defecto en Program.cs. Modificar AuthService para recibir IEmailService por DI y actualizar RecuperarPasswordAsync para llamar a _email.EnviarPasswordResetAsync tras el SaveChanges. Cambiar GenerarPasswordAleatorio() de private a internal static. El botón de recuperación en Login.razor puede quedar activo ahora que el flujo está completo (con NullEmailService en dev).

PASO 3 [CRIT-03 — diferido, implementación dev]: CRIT-03 se mantiene con legajo como password inicial en SeedData.cs exclusivamente para el seed de desarrollo. NO modificar SeedData.cs. Actualizar UsuarioService.CreateUsuarioAsync y ResetPasswordAsync para usar AuthService.GenerarPasswordAleatorio() en lugar del legajo, y llamar a _email.EnviarCredencialesInicialesAsync / _email.EnviarPasswordResetAsync tras el SaveChanges. Agregar IEmailService al constructor de UsuarioService.

PASO 4 [CRIT-06]: Eliminar los emails reales de appsettings.json y appsettings.Development.json. Reemplazar con array vacío `[]` y agregar comentario "// Configurar vía user-secrets o variables de entorno en producción". Crear archivo README-CONFIG.md en la raíz del proyecto con instrucciones de configuración.

PASO 5 [CRIT-02]: En ObjetivoService.UpdateObjetivoAsync, CancelarObjetivoAsync, CrearObjetivoAsync: verificar que el objetivo pertenece al scope del currentUser antes de ejecutar la operación. En RevisionService.CompletarRevisionAsync y CompletarEvaluacionFinalAsync: verificar que revision.Objetivo.Empleado.JefeId == _currentUser.UsuarioId OR _dataScope.PuedeVerTodo(_currentUser). Retornar false si la verificación falla.

PASO 6 [HIGH-03]: En RevisionService.CompletarRevisionAsync, agregar al inicio: `if (puntaje < 1 || puntaje > 5) return false;`

FASE 2 — INTEGRIDAD Y CORRECCIÓN

PASO 7 [MED-01]: Crear archivo Domain/Enums/RolUsuario.cs con enum RolUsuario { JEFE, LIDER, GERENTE, DIRECTOR, DIRECTOR_GENERAL, RRHH, COLABORADOR }. Reemplazar todas las comparaciones de string de rol (mínimo en DataScopeService, EvaluacionService, DashboardService) con el enum. Usar .HasConversion<string>() en AppDbContext para Jefe.Rol.

PASO 8 [HIGH-01]: En DataScopeService.AplicarScope, reemplazar `return query.Where(o => false)` por `throw new UnauthorizedAccessException($"Rol '{user.Rol}' no tiene scope definido para esta operación.")`.

PASO 9 [HIGH-02]: Modificar EvaluacionService para inyectar DataScopeService y reemplazar la lógica de filtrado duplicada con llamadas a _dataScope.AplicarScope.

PASO 10 [MED-05]: En ChatService, agregar ICurrentUserService por DI. En EnviarMensajeAsync, sobreescribir mensaje.RemitenteId = _currentUser.UsuarioId y mensaje.RemitenteEsJefe = _currentUser.EsJefe sin importar lo que venga del caller.

PASO 11 [MED-02]: En ICurrentUserService, agregar propiedad `int? EmpleadoId { get; }` (nullable, null si EsJefe). En SessionCurrentUserService, setearla durante SetUsuarioAsync solo cuando !esJefe. Reemplazar las 3 ocurrencias del patrón `FirstOrDefaultAsync(e => e.Email.ToLower() == email && e.Activo)` en ObjetivoService, DashboardService y EvaluacionService por uso directo de UsuarioId cuando !EsJefe.

FASE 3 — CALIDAD Y DEUDA

PASO 12 [MED-04]: Separar Entities.cs en archivos individuales: Pais.cs, Area.cs, Jefe.cs, Empleado.cs, Pilar.cs, SoftSkill.cs, Objetivo.cs, RevisionCuatrimestral.cs, EvaluacionFinal.cs, Autoevaluacion.cs, BitacoraEntrada.cs, MensajeChat.cs, EventoCalendario.cs, AuditoriaLog.cs, Notificacion.cs, Curso.cs — todos en la carpeta Domain/Entities/.

PASO 13 [HIGH-07]: Reemplazar `DateTime.Now.Year` por `DateTime.UtcNow.Year` en ObjetivoService.CrearObjetivoAsync.

PASO 14 [LOW-03]: En SeedData.cs, cambiar `Nombre = $"SS{i + 1:D2}-{skillNames[i]}"` por `Nombre = skillNames[i]`.

PASO 15 [DEBT-03]: Crear proyecto de tests xUnit: Objetivos.Tests. Agregar tests unitarios para: RendimientoService.CalcularPonderadoInterno (casos: sin revisiones, todas completas, solo algunas), AuthService.VerifyPassword, RevisionService (validación de secuencia).

PASO 16 [LOW-05]: Agregar a .gitignore: `*.db`, `*.db-shm`, `*.db-wal`.

FASE 4 — PRE-PRODUCCIÓN (ejecutar antes del deploy a producción)

PASO 17 [FEAT-01 completo]: Crear Services/SmtpEmailService.cs y/o Services/SendGridEmailService.cs según el provider elegido por el equipo, siguiendo exactamente el diseño en la sección FEAT-01 de este documento. Agregar el paquete NuGet correspondiente (MailKit o SendGrid). Actualizar Program.cs con el switch de provider basado en configuración. Agregar las secciones "Email" y "SendGrid" a appsettings.json con valores vacíos. Documentar en README-CONFIG.md cómo configurar las credenciales SMTP/SendGrid vía user-secrets o variables de entorno. Verificar que NullEmailService sigue activa en Development.

PASO 18 [CRIT-03 cierre]: Verificar que CreateUsuarioAsync y ResetPasswordAsync ya no usan el legajo como password (hecho en PASO 3). Ejecutar los criterios de aceptación listados en FEAT-01. Marcar CRIT-03 como RESUELTO en el documento de auditoría.

Al finalizar cada FASE, compilar la solución completa y reportar el estado. Si algún paso genera un error de compilación, detenerlo, reportarlo con el mensaje de error completo, y esperar instrucciones antes de continuar.
```

---

## MATRIZ DE RIESGO CONSOLIDADA

| ID | Severidad | Estado | Categoría | Esfuerzo Fix | Impacto Negocio |
|----|-----------|--------|-----------|-------------|-----------------|
| CRIT-01 | CRÍTICO | Pendiente | Security | Medio | Muy Alto |
| CRIT-02 | CRÍTICO | Pendiente | Security | Alto | Muy Alto |
| CRIT-03 | CRÍTICO | ⏸ Diferido | Security | Bajo | Alto |
| CRIT-04 | CRÍTICO | Pendiente | Security | Bajo | Alto |
| CRIT-05 | CRÍTICO | Pendiente | Architecture | Medio | Alto |
| CRIT-06 | CRÍTICO | Pendiente | Security | Bajo | Alto |
| FEAT-01 | Feature | Diseñado | Security+UX | Medio | Alto |
| HIGH-01 | ALTO | Pendiente | Architecture | Bajo | Medio |
| HIGH-02 | ALTO | Pendiente | Design | Bajo | Medio |
| HIGH-03 | ALTO | Pendiente | Data Integrity | Bajo | Alto |
| HIGH-04 | ALTO | Pendiente | Data Integrity | Medio | Medio |
| HIGH-05 | ALTO | Pendiente | Audit | Alto | Alto |
| HIGH-06 | ALTO | Pendiente | Architecture | Bajo | Medio |
| HIGH-07 | ALTO | Pendiente | Logic | Bajo | Bajo |
| HIGH-08 | ALTO | Pendiente | Reliability | Medio | Medio |
| MED-01..08 | MEDIO | Pendiente | Varios | Bajo-Medio | Bajo-Medio |
| LOW-01..05 | BAJO | Pendiente | Varios | Bajo | Bajo |

---

*Auditoría realizada el 2026-03-30. Revisión de código estático completa sobre 19 archivos .cs y 27 archivos .razor. Sin ejecución de código ni acceso a datos de producción.*  
*Actualización 2026-03-30: CRIT-03 reclasificado como diferido a pre-producción. FEAT-01 agregado con diseño completo de flujo de email.*
