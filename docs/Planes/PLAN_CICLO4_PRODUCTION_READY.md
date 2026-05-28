# PLAN DE REMEDIACIÓN — Ciclo 4 (Production-Ready)
**Fecha:** Mayo 2026
**Proyecto:** `C:\Development\Antigravity\RRHH_Objetivos`
**Auditor:** Arquitecto de Software

---

## OBJETIVO DEL CICLO

Resolver la deuda técnica arquitectónica restante para dejar el proyecto en estado **Production-Ready**:

1. **N-03 (ALTO)**: Unificar todos los servicios al patrón `IDbContextFactory`
2. **N-02 (ALTO)**: Centralizar validación de pesos en `ValidacionObjetivoService`
3. **N-04 (MEDIO)**: Que servicios usen `DataScopeService` en vez de duplicar scope
4. **N-05 (MEDIO)**: Eliminar `RendimientoService` no usado de `ExportService`
5. **N-06 (MEDIO)**: Eliminar las 3 llamadas redundantes en `AutoevaluarDialog`
6. **M-10 (MEDIO)**: Implementar UI de evidencias en `EvaluarDialog`
7. **Limpieza**: Eliminar migración duplicada `150000`

---

## ETAPA 1 — Unificación al patrón IDbContextFactory

Migrar los servicios viejos que inyectan `AppDbContext` directo a `IDbContextFactory<AppDbContext>`.

### Servicios afectados
- `ObjetivoService.cs`
- `RevisionService.cs`
- `RendimientoService.cs`
- `BitacoraService.cs`
- `ChatService.cs`
- `UsuarioService.cs`
- `DashboardService.cs`
- `AuthService.cs`
- `ConfiguracionService.cs`

### Patrón a aplicar
```csharp
// ANTES:
private readonly AppDbContext _db;
public Service(AppDbContext db) { _db = db; }
public async Task<...> Method() {
    return await _db.Entidades.ToListAsync();
}

// DESPUÉS:
private readonly IDbContextFactory<AppDbContext> _dbFactory;
public Service(IDbContextFactory<AppDbContext> dbFactory) { _dbFactory = dbFactory; }
public async Task<...> Method() {
    using var db = await _dbFactory.CreateDbContextAsync();
    return await db.Entidades.ToListAsync();
}
```

### Cuidado especial: transacciones y métodos que comparten estado
- `ObjetivoService.CrearObjetivoAsync` usa `_db.Database.BeginTransactionAsync` — toda la operación va en una sola instancia
- `RevisionService.CompletarRevisionAsync` llama a `_rendimiento.RecalcularProgresoObjetivoAsync` y `_objetivoService.EvaluarEstadoRiesgoAsync` — cada uno crea su propio contexto, lo cual es **correcto y deseado**

### Cleanup en Program.cs
Eliminar la línea:
```csharp
builder.Services.AddScoped(p => p.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());
```

---

## ETAPA 2 — Centralizar validación en ValidacionObjetivoService

Agregar método único que ambos dialogs (Crear y Editar) y el servicio puedan usar:

```csharp
public async Task<(bool Ok, decimal SumaActual)> ValidarSumaPesoAsync(
    int empleadoId, int anio, decimal nuevoPeso, int? objetivoIdEditando = null)
{
    using var db = await _dbFactory.CreateDbContextAsync();
    var sumaActual = await db.Objetivos
        .Where(o => o.EmpleadoId == empleadoId
                 && o.Anio == anio
                 && o.Estado != EstadoObjetivo.CANCELADO
                 && (objetivoIdEditando == null || o.Id != objetivoIdEditando.Value))
        .SumAsync(o => o.PorcentajePilar);
    return (sumaActual + nuevoPeso <= 100.01m, sumaActual);
}
```

Migrar `CrearObjetivoDialog`, `EditarObjetivoDialog` y `ObjetivoService.CrearObjetivoAsync`/`UpdateObjetivoAsync` para usar este método. Quitar la inyección directa de `IDbContextFactory` en los dialogs.

---

## ETAPA 3 — DataScopeService usado en servicios nuevos

Agregar overloads:
```csharp
public IQueryable<RevisionCuatrimestral> AplicarScope(IQueryable<RevisionCuatrimestral> query, ICurrentUserService user)
public IQueryable<Autoevaluacion> AplicarScope(IQueryable<Autoevaluacion> query, ICurrentUserService user)
```

Refactor `EvaluacionService.GetEvaluacionDataAsync` y `AutoevaluacionService.GetAutoevaluacionesAsync` para usar los nuevos overloads.

---

## ETAPA 4 — Quirks menores

### N-05: ExportService
Eliminar `private readonly RendimientoService _rendimiento;` del constructor.

### N-06: AutoevaluarDialog
Cargar escalas una sola vez al inicio de `OnSubmit`.

---

## ETAPA 5 — M-10: Evidencias en EvaluarDialog

Cargar `BitacoraEntradas` del objetivo y mostrarlas como `RadzenCheckBoxList`. Pasarlas como `evidencias` al llamar a `RevisionService.CompletarRevisionAsync`.

---

## ETAPA 6 — Limpieza migraciones

Eliminar archivos:
- `Migrations/20260514150000_RemoveUniqueObjetivoPilar.cs`
- `Migrations/20260514150000_RemoveUniqueObjetivoPilar.Designer.cs`

---

## VERIFICACIÓN FINAL

- `dotnet build` → 0 errores
- Ningún archivo `.cs` de servicio contiene `private readonly AppDbContext`
- `Program.cs` sin `AddScoped(p => ...CreateDbContext())`
- Solo una migración para la corrección del UNIQUE

