# AUDITORÍA TÉCNICA — RRHH_Objetivos
**Firma auditora:** Tier-1 External Software Audit  
**Fecha:** 2026-03-30  
**Alcance:** Código fuente completo — `Objetivos.Web` (Blazor Server + EF Core SQLite)  
**Metodología:** Revisión estática de código, análisis de patrones de diseño, evaluación de seguridad, deuda técnica y correctitud de negocio.

---

## ÍNDICE DE SEVERIDAD

| ID | Área | Severidad | Título |
|----|------|-----------|--------|
| SEC-01 | Seguridad | 🔴 CRÍTICO | Password expuesta en pantalla al recuperar contraseña |
| SEC-02 | Seguridad | 🔴 CRÍTICO | Sin autenticación real en rutas protegidas (client-side guard only) |
| SEC-03 | Seguridad | 🔴 CRÍTICO | SHA-256 sin salt — vulnerable a rainbow tables |
| SEC-04 | Seguridad | 🔴 CRÍTICO | Datos reales de nómina (.xlsx, .csv) versionados en el repo |
| SEC-05 | Seguridad | 🟠 ALTO | Política de contraseña mínima de 4 caracteres |
| SEC-06 | Seguridad | 🟠 ALTO | Emails hardcodeados de superusuarios en SeedData |
| SEC-07 | Seguridad | 🟠 ALTO | Sin CSRF / anti-forgery en formularios Blazor Server |
| SEC-08 | Seguridad | 🟡 MEDIO | Logging level "Debug" en appsettings.json de producción |
| ARQ-01 | Arquitectura | 🔴 CRÍTICO | Inyección directa de `AppDbContext` en páginas Razor (violación lifetime) |
| ARQ-02 | Arquitectura | 🟠 ALTO | Modelo de dos tablas separadas (Jefe/Empleado) — fragmented identity |
| ARQ-03 | Arquitectura | 🟠 ALTO | Lógica de negocio de autorización duplicada en 4+ servicios |
| ARQ-04 | Arquitectura | 🟡 MEDIO | Rol almacenado como `string` libre en entidad `Jefe` |
| ARQ-05 | Arquitectura | 🟡 MEDIO | Mezcla de `AppDbContext` scoped y `IDbContextFactory` en servicios del mismo tier |
| DT-01 | Deuda Técnica | 🟠 ALTO | `CursoService.GetCursosAsync()` inserta seed data en producción (lógica de seed en servicio) |
| DT-02 | Deuda Técnica | 🟠 ALTO | JSON raw strings en entidades de dominio (no tipado) |
| DT-03 | Deuda Técnica | 🟡 MEDIO | `SeedData.FindJefeId()` con fallback silencioso a primer jefe — asignación incorrecta |
| DT-04 | Deuda Técnica | 🟡 MEDIO | `RendimientoService.PromedioGeneralAsync()` con N+1 queries |
| DT-05 | Deuda Técnica | 🟡 MEDIO | `CancelarObjetivoAsync()` registra `Accion = "DELETE"` siendo en realidad un soft-cancel |
| DT-06 | Deuda Técnica | 🟡 MEDIO | `ExportService` no sanitiza todos los campos contra CSV injection |
| DT-07 | Deuda Técnica | 🟡 MEDIO | `RevisionService` tiene dependencia circular implícita (RevisionService → ObjetivoService) |
| DT-08 | Deuda Técnica | 🔵 BAJO | `AuthService.RecuperarPasswordAsync()` retorna la contraseña en texto plano al caller |
| DT-09 | Deuda Técnica | 🔵 BAJO | `objetivos.db` y archivos WAL/SHM en el directorio de la aplicación (sin exclusión .gitignore) |
| DT-10 | Deuda Técnica | 🔵 BAJO | Sin migración EF Core — solo `EnsureCreatedAsync()` |
| PAT-01 | Patrones | 🟠 ALTO | `ICurrentUserService` con estado mutable en servicio Scoped (race condition en Blazor Server) |
| PAT-02 | Patrones | 🟠 ALTO | `DashboardService.GetDashboardDataAsync()` recibe `ICurrentUserService` como parámetro (violación DI) |
| PAT-03 | Patrones | 🟡 MEDIO | Ausencia de repositorios / Unit of Work — acceso directo a DbContext desde todos los servicios |
| PAT-04 | Patrones | 🟡 MEDIO | `AuditoriaLog.CambiosJson` sin estructura tipada — auditabilidad nula |
| PAT-05 | Patrones | 🔵 BAJO | Sin validación en capa de servicio (toda la validación en UI) |

---

## HALLAZGOS DETALLADOS

---

### 🔴 SEC-01 — Password expuesta en pantalla al recuperar contraseña
**Archivo:** `Components/Pages/Login.razor`, `Services/AuthService.cs`

**Descripción:**  
Cuando un usuario solicita recuperación de contraseña, `RecuperarPasswordAsync()` retorna la nueva contraseña temporal en texto plano. `Login.razor` la renderiza directamente en el HTML:

```razor
Su nueva contraseña provisional es: <strong>@passwordRecuperado</strong>
```

Cualquier persona con acceso físico a la pantalla, capturas de pantalla, o que inyecte JS puede leerla. Peor aún: el comentario en `AuthService.cs` dice explícitamente:

```csharp
// In production, send email. For now, return the password for display/log.
return randomPassword;
```

Este "for now" llegó a producción.

**Impacto:** Compromiso total de cuenta de cualquier usuario cuya pantalla sea observada o capturada.

**Remediación:**
- Integrar envío por email (SMTP/SendGrid) como único canal de entrega.
- Si no hay email disponible temporalmente, mostrar solo los últimos 4 caracteres enmascarados.
- Eliminar el retorno del password plano del método; retornar `bool` solamente.

---

### 🔴 SEC-02 — Sin autenticación real en rutas protegidas
**Archivo:** `Components/Pages/Admin/AdminUsuarios.razor`, todos los componentes protegidos

**Descripción:**  
La "autorización" se implementa únicamente con checks en `OnInitializedAsync()` del componente:

```csharp
protected override async Task OnInitializedAsync()
{
    if (!CurrentUser.EsSuperusuario) 
    {
        Nav.NavigateTo("dashboard");
        return;
    }
```

Y en el markup:
```razor
@if (CurrentUser.EstaAutenticado && CurrentUser.EsSuperusuario)
```

Esto es una guarda en el cliente (UI), no una protección real del endpoint. En Blazor Server, si `InitializeAsync()` no se llama correctamente (prerendering, reconexión del circuit), el estado de `ICurrentUserService` puede estar vacío, permitiendo acceso. No hay middleware de autorización en el pipeline HTTP.

**Impacto:** Posibilidad de acceder a rutas administrativas con circuit manipulation o simplemente accediendo directamente a la URL.

**Remediación:**
- Implementar ASP.NET Core Authentication con `CookieAuthentication` o `ProtectedBrowserStorage`-backed claims.
- Agregar `[Authorize]` attributes o `AuthorizeRouteView` en `App.razor`.
- Agregar `app.UseAuthentication()` y `app.UseAuthorization()` en el pipeline.

---

### 🔴 SEC-03 — SHA-256 sin salt — vulnerable a rainbow tables
**Archivo:** `Services/AuthService.cs`

**Descripción:**
```csharp
public static string HashPassword(string password)
{
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
    return Convert.ToHexStringLower(bytes);
}
```

SHA-256 sin salt es inseguro para passwords. Dos usuarios con el mismo legajo/password tendrán exactamente el mismo hash. Esto permite:
1. Ataques de rainbow tables precomputadas.
2. Si alguien obtiene la DB, puede identificar instantáneamente todos los usuarios con el mismo password.

Agravante: el password inicial es el **Legajo** del empleado, un valor conocido/predecible. La mayoría de empleados nunca cambiarán su password.

**Impacto:** Compromiso masivo de credenciales con acceso mínimo a la base de datos.

**Remediación:**
- Reemplazar por `Microsoft.AspNetCore.Identity.PasswordHasher<T>` o `BCrypt.Net`.
- Mínimo aceptable: `PBKDF2` con sal aleatoria de 128 bits (disponible nativamente en .NET como `Rfc2898DeriveBytes`).

---

### 🔴 SEC-04 — Datos reales de nómina en el repositorio
**Archivo:** `Data/Nomina.csv`, `Data/Planilla Final Nomina Regional Finalizada.xlsx`

**Descripción:**  
Archivos con nómina real de empleados (nombres, emails, legajos, centros de costos) están en el directorio de la aplicación y presumiblemente versionados en Git. El `.gitignore` revisado no los excluye explícitamente.

**Impacto:** PII (Personally Identifiable Information) expuesta en el historial de versiones de Git. Viola GDPR/LPDP Argentina. Cualquier persona con acceso al repo tiene acceso a la nómina completa.

**Remediación:**
- Agregar inmediatamente a `.gitignore`: `*.csv`, `*.xlsx`, `Data/*.csv`, `Data/*.xlsx`.
- Usar `git filter-repo` para purgar el historial.
- Mover los archivos a almacenamiento externo seguro (Azure Blob Storage con acceso restringido).
- El seed data solo debe cargarse desde fuera del repositorio en deploy time.

---

### 🟠 SEC-05 — Política de contraseña mínima de 4 caracteres
**Archivo:** `Services/AuthService.cs`, `Components/Pages/Login.razor`

```csharp
if (string.IsNullOrWhiteSpace(nuevaPassword) || nuevaPassword.Length < 4)
    return false;
```

4 caracteres es el mínimo más bajo posible, debajo incluso de estándares de los 90s. NIST SP 800-63B exige mínimo 8 caracteres para memorized secrets.

**Remediación:** Mínimo 8 caracteres. Considerar validación de complejidad básica (al menos un número o símbolo).

---

### 🟠 SEC-06 — Emails de superusuarios hardcodeados en SeedData
**Archivo:** `Data/SeedData.cs`

```csharp
var superUserEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "ccespon@permaquim.com",
    "ptripodi@permaquim.com",
    "scrosio@permaquim.com"
};
```

Emails corporativos reales hardcodeados en código fuente. Identifica quiénes son superusuarios del sistema a cualquiera que lea el repo.

**Remediación:** Mover a configuración en `appsettings.json` (excluido de source control) o a variable de entorno.

---

### 🟠 SEC-08 — Log level Debug en appsettings de producción
**Archivo:** `appsettings.json`

```json
"Default": "Debug",
"Microsoft.EntityFrameworkCore.Database.Command": "Information"
```

El archivo base `appsettings.json` (usado en producción) tiene `Default: Debug`. Esto loguea información sensible incluyendo queries SQL completas con parámetros, valores de variables, y stack traces detallados.

**Remediación:** `appsettings.json` base debe tener `Warning` o `Error`. Solo `appsettings.Development.json` debe tener `Debug`.

---

### 🔴 ARQ-01 — Inyección directa de AppDbContext en páginas Razor
**Archivo:** `Components/Pages/Evaluacion/Index.razor`

```razor
@inject AppDbContext Db
```

`AppDbContext` está registrado como Scoped. En Blazor Server, el "scope" es el circuit (conexión WebSocket), que puede durar horas o días. Inyectar `AppDbContext` directamente en componentes causa:

1. **Context tracking acumulado:** EF Core trackea entidades indefinidamente en memoria.
2. **Stale data:** El contexto cachea datos obsoletos entre requests.
3. **Concurrencia:** El mismo contexto se usa en múltiples operaciones sin coordinación.

El equipo ya detectó el problema parcialmente (comentario en Program.cs: *"to avoid lifetime conflicts"*) pero lo resolvió solo en algunos servicios.

**Remediación:**  
- Eliminar `@inject AppDbContext Db` de todos los componentes.
- Mover la lógica a un servicio que use `IDbContextFactory<AppDbContext>` con `using var db = await _factory.CreateDbContextAsync()`.
- O usar `OwningComponentBase` para crear un scope de componente.

---

### 🟠 ARQ-02 — Modelo de identidad fragmentado: dos tablas Jefe/Empleado
**Archivo:** `Domain/Entities/Entities.cs`, múltiples servicios

Toda la lógica de autenticación y autorización debe manejar dos ramas separadas:
```csharp
// En AuthService, UsuarioService, SeedData, etc:
var jefe = await _db.Jefes.FirstOrDefaultAsync(...);
if (jefe != null) { ... }
var empleado = await _db.Empleados.FirstOrDefaultAsync(...);
if (empleado != null) { ... }
```

Este patrón se repite en `AuthService` (2 veces), `SeedData` (3 veces), `UsuarioService` (3 veces), `ObjetivoService`, `DashboardService`. Cada nuevo servicio que necesite conocer al usuario debe duplicar esta lógica.

**Consecuencias:**
- Un Jefe que también tiene objetivos (es también colaborador de otro jefe superior) no puede ser modelado.
- Bug latente: si alguien crea un Empleado y un Jefe con el mismo email, el login siempre retornará el Jefe.

**Remediación:**  
Unificar en una tabla `Usuario` con campo `Rol` (ya existe el enum conceptualmente). Usar herencia TPH en EF Core si se necesitan propiedades específicas por rol.

---

### 🟠 ARQ-03 — Lógica de autorización por rol duplicada en múltiples servicios
**Archivos:** `ObjetivoService.cs`, `DashboardService.cs`, `Evaluacion/Index.razor`

La misma lógica de "quién puede ver qué" está replicada:

```csharp
// Aparece en ObjetivoService, DashboardService, Index.razor de Evaluacion:
bool canSeeAll = currentUser.Rol == "DIRECTOR_GENERAL" || currentUser.Rol == "RRHH" || currentUser.EsSuperusuario;
if (canSeeAll) { /* todo */ }
else if (currentUser.Rol == "DIRECTOR") { /* su área */ }
else if (currentUser.EsJefe) { /* sus reportes */ }
```

Si se agrega un nuevo rol (ej. "GERENTE_REGIONAL"), hay que modificar 4+ archivos sin garantía de consistencia.

**Remediación:**  
Crear un servicio `AuthorizationPolicyService` o `DataScopeService` con métodos:
```csharp
IQueryable<Objetivo> ApplyScope(IQueryable<Objetivo> query, ICurrentUserService user);
```
Un único lugar para mantener la lógica de visibilidad.

---

### 🟠 ARQ-04 — Rol almacenado como string libre en entidad Jefe
**Archivo:** `Domain/Entities/Entities.cs`

```csharp
public string Rol { get; set; } = "JEFE";
// comentario: "JEFE, LIDER, GERENTE, DIRECTOR, DIRECTOR_GENERAL, RRHH"
```

El rol es un string libre en la DB. La comparación en todos los servicios usa strings literales:
```csharp
currentUser.Rol == "DIRECTOR_GENERAL"
```

Un typo en cualquier asignación de rol produce escalada silenciosa de privilegios (downgrade) o acceso denegado inesperado.

**Remediación:** Usar el enum `string RolUsuario` ya existente conceptualmente, o crear uno y aplicarlo como `[Column(TypeName = "TEXT")]` en EF Core con conversión.

---

### 🟠 ARQ-05 — Mezcla de AppDbContext scoped y IDbContextFactory en servicios
**Archivo:** `Program.cs`, `Services/`

```csharp
// Program.cs registra ambos:
builder.Services.AddDbContextFactory<AppDbContext>(...);
builder.Services.AddScoped(p => p.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

// Algunos servicios usan AppDbContext inyectado:
public AuthService(AppDbContext db) { _db = db; }

// Otros usan IDbContextFactory:
public UsuarioService(IDbContextFactory<AppDbContext> dbFactory) { _dbFactory = dbFactory; }
```

Esta inconsistencia significa que `AuthService` y `UsuarioService` en el mismo request HTTP usan contextos **diferentes**, con tracking states **diferentes**. Una entidad guardada por `UsuarioService` no es visible en el contexto de `AuthService` hasta el próximo request.

**Remediación:** Elegir una estrategia y aplicarla consistentemente. Para Blazor Server, `IDbContextFactory` con `using var db = ...` es la correcta para todos los servicios.

---

### 🟠 DT-01 — Seed data embebida en CursoService de producción
**Archivo:** `Services/CursoService.cs`

```csharp
public async Task<List<Curso>> GetCursosAsync()
{
    var cursos = await _db.Cursos.ToListAsync();
    if (!cursos.Any())  // <-- Seed en producción
    {
        cursos = new List<Curso> { new Curso { Nombre = "Liderazgo Efectivo", ... } };
        _db.Cursos.AddRange(cursos);
        await _db.SaveChangesAsync();
    }
    return cursos;
}
```

Un método de lectura (`Get`) tiene efectos secundarios de escritura. Esto viola el principio CQS (Command Query Separation). Además, es thread-unsafe en Blazor Server: si dos usuarios acceden simultáneamente con la tabla vacía, se pueden insertar datos duplicados.

**Remediación:** Mover el seed de cursos a `SeedData.InitializeAsync()`. El servicio solo lee.

---

### 🟠 DT-02 — JSON raw strings en entidades de dominio
**Archivo:** `Domain/Entities/Entities.cs`

```csharp
public string EvidenciasRevisadasJson { get; set; } = "[]";
public string AdjuntosJson { get; set; } = "[]";
public string EvidenciasMencionadasJson { get; set; } = "[]";
public string CambiosJson { get; set; } = "{}";
```

Cuatro campos de entidades críticas almacenan JSON serializado manualmente. No hay validación de estructura, no hay tipado, no hay soporte para queries sobre el contenido. EF Core con SQLite soporta conversión de valores con `List<string>` → JSON nativamente desde EF 7.

**Remediación:**
```csharp
// En AppDbContext.OnModelCreating:
modelBuilder.Entity<BitacoraEntrada>()
    .Property(b => b.Adjuntos)
    .HasConversion(
        v => JsonSerializer.Serialize(v, null),
        v => JsonSerializer.Deserialize<List<string>>(v, null)!);
```

---

### 🟡 DT-03 — FindJefeId() con fallback silencioso a primer jefe
**Archivo:** `Data/SeedData.cs`

```csharp
int FindJefeId(string responsable)
{
    // ... matching logic ...
    return jefeMap.Values.FirstOrDefault()?.Id ?? 1; // <-- FALLBACK SILENCIOSO
}
```

Cuando no se puede resolver el responsable de evaluación, se asigna el primer jefe del diccionario (orden no determinístico en Dictionary). Esto significa que múltiples empleados terminan con un jefe incorrecto en la base de datos, sin log de advertencia alguno.

**Remediación:**
- Loguear un warning cuando el fallback se active.
- Usar un valor sentinela (JefeId = null o un "Sin Asignar") en lugar de asignar uno arbitrario.
- Hacer el seed más robusto o separar la resolución de responsables como paso manual post-seed.

---

### 🟡 DT-04 — N+1 queries en RendimientoService.PromedioGeneralAsync()
**Archivo:** `Services/RendimientoService.cs`

```csharp
var objetivos = await _db.Objetivos.Where(...).ToListAsync();
foreach(var obj in objetivos)
{
    scores.Add(await CalcularPonderadoAsync(obj.Id)); // <-- Query por cada objetivo
}
```

Para un empleado con 3 objetivos, esto emite 1 + 3 = 4 queries. `CalcularPonderadoAsync()` internamente hace un `FirstOrDefaultAsync` con `Include`. En un dashboard con múltiples empleados, esto se multiplica.

**Remediación:** Cargar todos los datos en una sola query con `Include` y calcular el promedio en memoria.

---

### 🟡 DT-05 — Auditoría incorrecta: CANCELAR registrado como DELETE
**Archivo:** `Services/ObjetivoService.cs`

```csharp
public async Task CancelarObjetivoAsync(int id, string razon)
{
    objetivo.Estado = EstadoObjetivo.CANCELADO;
    _db.AuditoriaLogs.Add(new AuditoriaLog
    {
        Accion = "DELETE",  // ← Incorrecto: es un UPDATE de estado
```

El `AuditoriaLog` registra `"DELETE"` para una operación que es en realidad un soft-delete (cambio de estado). Cualquier sistema de auditoría o reporte que filtre por acción obtendrá resultados incorrectos.

**Remediación:** Usar `"CANCEL"` o `"UPDATE"` con el campo `CambiosJson` poblado.

---

### 🟡 DT-06 — CSV Injection en ExportService
**Archivo:** `Services/ExportService.cs`

```csharp
var cleanNombre = (o.Nombre ?? "").Replace(";", ",").Replace("\n", " ").Replace("\r", "");
```

Solo se sanitiza el campo `Nombre`. Los campos `Empleado.Email`, `Area.Nombre`, `SoftSkill.Nombre`, y `Pilar.Nombre` se escriben directamente sin sanitización. Un dato como `=CMD|' /C calc'!A0` en el nombre de un área puede ejecutarse en Excel al abrir el CSV.

**Remediación:** Sanitizar **todos** los campos: si el valor empieza con `=`, `+`, `-`, o `@`, prefijarlo con `'` (comilla simple).

---

### 🟠 PAT-01 — ICurrentUserService con estado mutable en Scoped service
**Archivo:** `Services/CurrentUserService.cs`

En Blazor Server, múltiples renders del mismo componente pueden ocurrir concurrentemente en el mismo circuit. `SessionCurrentUserService` tiene propiedades mutables con setters privados que son modificadas por `InitializeAsync()`, `SetUsuarioAsync()` y `CerrarSesionAsync()`. Si dos renders ocurren simultáneamente, puede haber race conditions en el estado del usuario.

Adicionalmente, el servicio tiene estado en memoria (`UsuarioId`, `EsJefe`, etc.) que se usa **antes** de que `InitializeAsync()` sea llamado, retornando valores por defecto (0, false, "") que pueden pasar guards de autorización incorrectamente.

**Remediación:**  
- Usar `IHttpContextAccessor` o Claims-based identity que está natively thread-safe.
- Si se mantiene el patrón, asegurarse que todos los componentes llamen `InitializeAsync()` antes de cualquier check de autorización.

---

### 🟠 PAT-02 — DashboardService recibe ICurrentUserService como parámetro
**Archivo:** `Services/DashboardService.cs`

```csharp
public async Task<RoleDashboardData> GetDashboardDataAsync(ICurrentUserService currentUser)
```

El servicio está registrado como Scoped y podría recibir `ICurrentUserService` por inyección en el constructor (como hacen todos los demás servicios). Pasarlo como parámetro en el método viola la consistencia del diseño y hace que el servicio sea más difícil de testear (mockear). Es probable que esto sea un workaround de un bug de lifetime anterior.

**Remediación:** Inyectar `ICurrentUserService` en el constructor del servicio.

---

### 🟡 PAT-03 — Sin capa de repositorio / Unit of Work
**Observación general**

Todos los servicios acceden directamente a `AppDbContext`. Esto no es necesariamente incorrecto para un proyecto de esta escala, pero significa que:
1. No hay forma de hacer unit testing de los servicios sin una DB real o in-memory.
2. Si se migrara de SQLite a PostgreSQL u otro motor, los cambios son en toda la codebase.

**Recomendación:** Para el tamaño actual del proyecto, mantener el patrón directo es aceptable, pero agregar al menos un interface por servicio para permitir testing.

---

### 🟡 PAT-04 — AuditoriaLog.CambiosJson sin estructura
**Archivo:** `Domain/Entities/Entities.cs`, múltiples servicios

```csharp
public string CambiosJson { get; set; } = "{}";
```

La mayoría de los logs de auditoría se crean con `CambiosJson = "{}"` (vacío). Solo `CancelarObjetivoAsync` popula este campo, con string interpolation inline:
```csharp
CambiosJson = $"{{\"razon\": \"{razon}\"}}"
```

Una auditoría sin cambios registrados no sirve para reconstruir qué ocurrió. La tabla `AuditoriaLog` existe pero no audita.

**Remediación:** Crear un `AuditEntry` tipado o al menos serializar con `JsonSerializer.Serialize(new { before = ..., after = ... })`.

---

### 🔵 DT-09 — Archivos de base de datos SQLite en directorio de aplicación
**Archivos:** `objetivos.db`, `objetivos.db-shm`, `objetivos.db-wal`

Los archivos de base de datos están en el directorio raíz de la aplicación y presumiblemente están (o estuvieron) versionados en Git. El WAL file (`-wal`) puede contener transacciones incompletas. Estos archivos no deben estar en source control bajo ninguna circunstancia.

**Remediación:** Agregar al `.gitignore`:
```
*.db
*.db-shm
*.db-wal
```
Verificar que no estén en el historial de Git con `git log --all -- "*.db"`.

---

### 🔵 DT-10 — Sin migraciones EF Core
**Archivo:** `Program.cs`

```csharp
await db.Database.EnsureCreatedAsync();
```

`EnsureCreatedAsync()` crea el esquema desde cero si no existe, pero **no puede actualizar un esquema existente**. Cualquier cambio al modelo (agregar columna, renombrar campo) en producción requiere recrear la base de datos o migraciones manuales.

**Remediación:** Implementar EF Core Migrations (`dotnet ef migrations add`). En producción usar `MigrateAsync()` en lugar de `EnsureCreatedAsync()`.

---

## RESUMEN EJECUTIVO

El proyecto `RRHH_Objetivos` es funcionalmente coherente para un MVP interno, pero presenta **vulnerabilidades de seguridad críticas** que lo hacen inapropiado para uso con datos reales de empleados en su estado actual.

### Prioridades de acción inmediata (antes de cualquier uso en producción):

1. **[SEC-04]** Purgar datos de nómina del repositorio Git — acción en < 1 hora.
2. **[SEC-03]** Reemplazar SHA-256 por BCrypt/PBKDF2 con salt — requiere migración de passwords existentes.
3. **[SEC-01]** Eliminar la devolución de passwords en texto plano — requiere integrar envío por email.
4. **[SEC-02]** Implementar autenticación real con ASP.NET Core Identity o Cookie Authentication.
5. **[ARQ-01]** Eliminar inyección directa de `AppDbContext` en componentes Razor.

### Deuda técnica prioritaria (sprint siguiente):

6. **[ARQ-03]** Centralizar lógica de autorización por rol.
7. **[DT-01]** Mover seed de cursos fuera del servicio de lectura.
8. **[PAT-01]** Hacer `ICurrentUserService` thread-safe o reemplazar por Claims.
9. **[DT-04]** Resolver N+1 en `RendimientoService`.
10. **[DT-09 + DT-10]** Gitignore para .db files + migrar a EF Core Migrations.

---

## PROMPT PARA AGENTE DE EJECUCIÓN

```
Eres un agente de desarrollo senior trabajando en C:\Development\Antigravity\RRHH_Objetivos.
Lee el archivo AUDITORIA_TECNICA.md en la raíz del proyecto para entender el contexto completo.

PLAN DE EJECUCIÓN — ejecutar en orden estricto:

## FASE 1 — Seguridad Crítica (no negociable)

### TASK 1.1 — Gitignore: excluir datos sensibles y archivos de DB
Editar C:\Development\Antigravity\RRHH_Objetivos\.gitignore y agregar:
- *.db
- *.db-shm  
- *.db-wal
- Objetivos.Web/Data/*.csv
- Objetivos.Web/Data/*.xlsx

### TASK 1.2 — Reemplazar SHA-256 por PBKDF2 con salt
En Services/AuthService.cs:
- Reemplazar HashPassword() por una implementación con Rfc2898DeriveBytes (PBKDF2, SHA256, 100_000 iteraciones, salt aleatorio de 16 bytes).
- El formato del hash almacenado debe ser: "iteraciones.saltBase64.hashBase64"
- Actualizar CambiarPasswordAsync() y RecuperarPasswordAsync() para usar el nuevo método.
- Actualizar SeedData.cs que llama a AuthService.HashPassword() — verificar que siga funcionando.
- IMPORTANTE: Los passwords en SeedData se generan en seed time, no hay datos previos que migrar.

### TASK 1.3 — Eliminar password en texto plano de la pantalla de recuperación
En Services/AuthService.cs:
- RecuperarPasswordAsync() debe cambiar la firma a Task<bool> — solo retorna éxito/fallo.
- Agregar un TODO comment bien visible: "// TODO: Integrar envío por email (SendGrid/SMTP) — ver SEC-01 en AUDITORIA_TECNICA.md"
- En Components/Pages/Login.razor: eliminar el bloque que muestra @passwordRecuperado.
- Reemplazar con un mensaje genérico: "Si el email existe en el sistema, recibirá instrucciones para restablecer su contraseña."

### TASK 1.4 — Eliminar emails hardcodeados de SeedData
En Data/SeedData.cs:
- Crear en appsettings.json una sección "SuperUsers": { "Emails": ["ccespon@permaquim.com", ...] }
- Modificar SeedData.InitializeAsync() para recibir IConfiguration y leer los emails desde configuración.
- Actualizar Program.cs para pasar IConfiguration a SeedData.InitializeAsync().
- Nota: Los emails actuales quedan en appsettings.json que debe ser agregado al .gitignore si contiene datos sensibles, o moverse a appsettings.Production.json (fuera de source control).

### TASK 1.5 — Corregir log level en appsettings.json
En appsettings.json cambiar:
  "Default": "Debug"  →  "Default": "Warning"
  "Microsoft.EntityFrameworkCore.Database.Command": "Information"  →  eliminar esta línea
En appsettings.Development.json agregar/mantener:
  "Default": "Debug"
  "Microsoft.EntityFrameworkCore.Database.Command": "Information"

### TASK 1.6 — Política de password mínima
En Services/AuthService.cs, en CambiarPasswordAsync():
- Cambiar la validación de Length < 4 a Length < 8
- Agregar validación: debe contener al menos una letra y al menos un número.
En Components/Pages/Login.razor, en OnCambiarPassword():
- Actualizar el mensaje de error para reflejar el nuevo mínimo.

## FASE 2 — Arquitectura (alta prioridad)

### TASK 2.1 — Eliminar inyección directa de AppDbContext en componentes Razor
Problema: Components/Pages/Evaluacion/Index.razor tiene @inject AppDbContext Db y usa _db directamente.
- Crear Services/EvaluacionService.cs con un método:
  Task<EvaluacionPageData> GetEvaluacionDataAsync(ICurrentUserService currentUser)
  que encapsule toda la lógica de LoadData() que hoy está en el componente.
  El servicio debe usar IDbContextFactory<AppDbContext>.
- Registrar EvaluacionService como Scoped en Program.cs.
- Refactorizar Components/Pages/Evaluacion/Index.razor para usar @inject EvaluacionService EvaluacionService
  y eliminar el @inject AppDbContext Db.
- Crear el DTO EvaluacionPageData con: List<RevisionCuatrimestral> Pendientes, List<Objetivo> FinalesPendientes, List<RevisionCuatrimestral> Recibidas.

### TASK 2.2 — Centralizar lógica de autorización de scope de datos
Crear Services/DataScopeService.cs con métodos:
  IQueryable<Objetivo> AplicarScope(IQueryable<Objetivo> query, ICurrentUserService user)
  IQueryable<RevisionCuatrimestral> AplicarScope(IQueryable<RevisionCuatrimestral> query, ICurrentUserService user)
  bool PuedeVerTodo(ICurrentUserService user)  // DIRECTOR_GENERAL, RRHH, Superusuario
  
Refactorizar ObjetivoService.GetObjetivosRoleAsync() y DashboardService.GetDashboardDataAsync()
para usar DataScopeService en lugar de replicar la lógica de roles.

### TASK 2.3 — Corregir DashboardService para inyectar ICurrentUserService por constructor
En Services/DashboardService.cs:
- Agregar ICurrentUserService al constructor en lugar de recibirlo como parámetro de método.
- Cambiar la firma de GetDashboardDataAsync(ICurrentUserService) a GetDashboardDataAsync().
- Actualizar el componente que llama a este servicio para no pasar el parámetro.

## FASE 3 — Deuda Técnica

### TASK 3.1 — Resolver N+1 en RendimientoService.PromedioGeneralAsync()
En Services/RendimientoService.cs, reescribir PromedioGeneralAsync():
- Cargar todos los objetivos con sus revisiones y evaluación final en una sola query.
- Calcular el ponderado en memoria sin queries adicionales.
- Extraer el cálculo ponderado en un método privado estático que opere sobre datos ya cargados.

### TASK 3.2 — Mover seed de cursos fuera de CursoService
En Services/CursoService.cs:
- Eliminar el bloque if (!cursos.Any()) { ... } de GetCursosAsync().
- Mover esos cursos de ejemplo a SeedData.InitializeAsync().

### TASK 3.3 — Corregir AuditoriaLog en CancelarObjetivoAsync
En Services/ObjetivoService.cs, CancelarObjetivoAsync():
- Cambiar Accion = "DELETE" por Accion = "CANCEL".
- Poblar CambiosJson con JsonSerializer.Serialize(new { razon, estadoAnterior = "ACTIVO", estadoNuevo = "CANCELADO" }).

### TASK 3.4 — CSV Injection: sanitizar todos los campos en ExportService
En Services/ExportService.cs:
- Crear un método privado string SanitizeCsvField(string? value).
- Si el valor empieza con '=', '+', '-', '@', '\t', '\r', prefijar con comilla simple.
- Reemplazar punto y coma, newlines.
- Aplicar SanitizeCsvField() a TODOS los campos del CSV, no solo a Nombre.

### TASK 3.5 — Agregar .gitignore para archivos de SQLite
(si no fue cubierto en TASK 1.1, verificar y complementar)

### TASK 3.6 — Agregar WARNING en FindJefeId cuando usa fallback
En Data/SeedData.cs, en el método FindJefeId():
- Antes del return de fallback, agregar: Console.WriteLine($"[SeedData WARNING] No se pudo resolver jefe para: '{responsable}'. Se asignó fallback ID={jefeMap.Values.FirstOrDefault()?.Id}");
- Esto es un fix temporal; el WARNING documenta el problema para revisión manual post-seed.

## REGLAS DEL AGENTE

1. Para cada task, leer el archivo antes de editarlo.
2. No romper funcionalidad existente — si una refactorización implica un cambio en cascada, completarlo.
3. Después de cada FASE, listar los archivos modificados y un resumen de cambios.
4. Si encontrás algo durante la ejecución que contradice este plan, documentarlo como BLOCKER antes de continuar.
5. No modificar archivos de datos (*.csv, *.xlsx) ni la base de datos SQLite.
6. Al finalizar, crear un archivo AUDITORIA_EJECUTADA.md en la raíz con: lista de tasks completadas, archivos modificados, y cualquier deuda remanente.
```

---

*Fin del informe de auditoría. Generado por revisión estática completa del código fuente.*
