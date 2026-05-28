# Revisión de Implementación – Segunda Pasada
> **Fecha:** 30/04/2026 · **Veredicto del agente:** "Listo para producción" ❌

---

## LO QUE SÍ MEJORÓ RESPECTO A LA REVISIÓN ANTERIOR

| Ítem | Estado anterior | Estado actual |
|------|----------------|---------------|
| EscalaSelector binding bug | 🔴 Valor nunca llegaba al padre | ✅ Corregido con `OnValueChanged` + `InvokeAsync` |
| EvaluarDialog: estrellas | 🔴 `RadzenRating` | ✅ `EscalaSelector` |
| EvaluarFinalDialog: soft skills | 🔴 `RadzenRating` | ✅ `EscalaSelector` |
| EvaluarDialog: bloqueo silencioso de submit | 🔴 `RequiereEvidencia()` bloqueaba | ✅ Lógica de evidencias eliminada del submit |
| Autoevaluaciones/Index: estrellas | 🔴 `RadzenRating` | ✅ Etiqueta de texto (`EscalaValoracionScore?.Etiqueta`) |
| Autoevaluación en sección correcta | 🔴 Solo lanzable desde Objetivos | ✅ Tab "Pendientes de Autoevaluar" agregada |
| Botón Autoevaluar en Objetivos | 🔴 Presente | ✅ Eliminado de `MisObjetivos/Index.razor` |
| Seed de datos | 🔴 No implementado | ✅ Implementado en `SeedData.cs`, invocado desde `Program.cs` |
| RevisionService: escala dinámica | 🔴 Puntaje int 1-5 | ✅ `EscalaValoracionId` FK |

---

## PROBLEMAS QUE IMPIDEN IR A PRODUCCIÓN

---

### 🔴 BUG-01 — Error de compilación: método inexistente en `AutoevaluacionService`

**Archivo:** `Components/Pages/Autoevaluaciones/Index.razor` línea:
```csharp
objetivosPendientes = await AutoevaluacionService.GetObjetivosPendientesAutoevAsync(CurrentUser);
```

`GetObjetivosPendientesAutoevAsync` **no existe** en `AutoevaluacionService.cs`. El servicio solo tiene: `GetAutoevaluacionesAsync`, `GetObjetivoParaAutoevAsync`, `GetExistingAsync`, `GetEmpleadoIdByEmailAsync`, `GuardarAutoevaluacionAsync`.

**El proyecto no compila. No puede ir a producción.**

**Corrección — agregar el método en `AutoevaluacionService.cs`:**
```csharp
public async Task<List<Objetivo>> GetObjetivosPendientesAutoevAsync(ICurrentUserService user)
{
    using var db = await _dbFactory.CreateDbContextAsync();

    var empleado = await db.Empleados
        .FirstOrDefaultAsync(e => e.Email.ToLower() == user.Email.ToLower() && e.Activo);

    if (empleado == null) return new();

    var idsConAutoev = await db.Autoevaluaciones
        .Where(ae => ae.Objetivo.EmpleadoId == empleado.Id)
        .Select(ae => ae.ObjetivoId)
        .ToHashSetAsync();

    return await db.Objetivos
        .Include(o => o.Pilar)
        .Where(o => o.EmpleadoId == empleado.Id
                 && o.Estado != EstadoObjetivo.CANCELADO
                 && !idsConAutoev.Contains(o.Id))
        .OrderBy(o => o.Deadline)
        .ToListAsync();
}
```

---

### 🔴 BUG-02 — Seed no corre en bases de datos existentes

**Archivo:** `Data/SeedData.cs` línea 1:
```csharp
if (await db.Paises.AnyAsync()) return;  // ← salta TODO si ya hay países
```

Esta condición hace que en cualquier BD que ya tenga datos (por ejemplo, la BD actual en producción con la nómina cargada), **todo el seed se saltea**. Los registros de `ConfiguracionPlataforma`, `EscalaValoracion`, `EstadosObjetivoConfig`, `EstadosEvaluacionConfig` y `Pilares` nunca se insertarán.

Consecuencias concretas:
- `ConfiguracionService` no encuentra `jefe_puede_crear_objetivos` → devuelve `null` → el jefe puede crear objetivos.
- `ObjetivoService.CrearObjetivoAsync` no encuentra el estado `pendiente_aprobacion` → `EstadoObjetivoConfigId` queda en `null`.
- Los selectores de `EscalaSelector` quedan vacíos (sin opciones).

**Corrección — verificar cada tabla de forma independiente:**
```csharp
public static async Task InitializeAsync(AppDbContext db, IWebHostEnvironment env, IConfiguration config)
{
    // Nómina: solo si es BD nueva
    bool esBdNueva = !await db.Paises.AnyAsync();
    if (esBdNueva) { /* ... seed de países, jefes, empleados ... */ }

    // Tablas de configuración: siempre verificar y completar si faltan
    if (!await db.EscalasValoracion.AnyAsync()) { /* seed escalas */ }
    if (!await db.EstadosObjetivoConfig.AnyAsync()) { /* seed estados objetivo */ }
    if (!await db.EstadosEvaluacionConfig.AnyAsync()) { /* seed estados evaluacion */ }
    if (!await db.Pilares.AnyAsync()) { /* seed pilares */ }

    // Configuraciones: insertar solo las que no existen (upsert por clave)
    var clavesRequeridas = new[] { "email_soporte", "dias_proximo_vencimiento", 
        "objetivo_area_habilitado", "resultado_final_manual", 
        "jefe_puede_crear_objetivos", "texto_guia_plataforma" };
    foreach (var clave in clavesRequeridas)
    {
        if (!await db.ConfiguracionesPlataforma.AnyAsync(c => c.Clave == clave))
        {
            // insertar según clave
        }
    }

    await db.SaveChangesAsync();
}
```

---

### 🟠 BUG-03 — Pilares en seed con definiciones incorrectas (no las de Pablo Cirac)

**Archivo:** `Data/SeedData.cs`

El seed insertó descripciones genéricas placeholder:
```csharp
// LO QUE ESTÁ EN EL CÓDIGO (incorrecto):
new Pilar { Nombre = "Crecimiento de Ventas", 
    Descripcion = "Objetivo enfocado en el incremento de ingresos y participación de mercado" }
```

Las definiciones oficiales están documentadas en `especificaciones_pqtalent.md` y deben ser:

```csharp
// CORRECCIÓN:
new Pilar { Nombre = "Crecimiento de Ventas", ColorHex = "#4CAF50", Orden = 1, Activo = true,
    Descripcion = "Este objetivo se centra en incrementar los ingresos a través de la expansión de la base de clientes, la mejora de la oferta de productos o servicios, y la optimización de las estrategias de ventas. Implica explorar nuevos mercados, nuevos productos y nuevos segmentos, fortalecer relaciones con los clientes actuales y desarrollar tácticas innovadoras para aumentar las ventas en el corto, mediano y largo plazo." },

new Pilar { Nombre = "Orientación al Cliente", ColorHex = "#2196F3", Orden = 2, Activo = true,
    Descripcion = "Optimización y Experiencia Integral (Interna y Externa). Este pilar busca optimizar la totalidad del ciclo posterior a la venta, entendiendo que el servicio de calidad hacia afuera es el resultado de una operación interna eficiente y coordinada.\n\n1. Compromiso con el Cliente Externo: Buscamos asegurar un servicio ágil que supere las expectativas, reduciendo los tiempos de respuesta y resolviendo cualquier incidencia con precisión. El objetivo es que cada contacto postventa fortalezca la confianza en la marca, transformando una transacción en una relación de fidelidad a largo plazo.\n\n2. Fortalecimiento del Cliente Interno: Para lograrlo, optimizamos nuestros procesos internos proporcionando a nuestros colaboradores las herramientas, la información y el soporte necesarios para actuar con autonomía y eficacia." },

new Pilar { Nombre = "Eficiencia Organizacional", ColorHex = "#F9A825", Orden = 3, Activo = true,
    Descripcion = "Este objetivo busca mejorar el rendimiento interno de la compañía a través de la optimización de sus recursos, la eliminación de ineficiencias y la implementación de mejores prácticas en la gestión de los procesos. La eficiencia organizacional se traduce en un entorno de trabajo más ágil, con procesos más simplificados y una significativa reducción de costos operativos. Al fomentar una cultura de mejora continua y ahorro estratégico, logramos maximizar tanto los resultados financieros como la calidad en todas las áreas, asegurando la sostenibilidad del negocio a largo plazo." }
```

---

### 🟠 BUG-04 — `EvaluarDialog` todavía muestra `ResultadoEval` enum (CUMPLIDO/PARCIAL/NO_CUMPLIDO/EN_RIESGO)

**Archivo:** `Components/Pages/Evaluacion/EvaluarDialog.razor`

Las estrellas fueron correctamente reemplazadas por `EscalaSelector`. Sin embargo, el campo "Resultado" sigue siendo un dropdown del enum deprecado:
```razor
@* AÚN EN EL CÓDIGO — debe eliminarse *@
<RadzenDropDown @bind-Value="@model.Resultado" Data="@resultados" .../>
@code {
    List<ResultadoEval> resultados = Enum.GetValues<ResultadoEval>().ToList();
    // Resultado: CUMPLIDO | PARCIAL | NO_CUMPLIDO | EN_RIESGO
}
```

Adicionalmente, `RevisionService.CompletarRevisionAsync` recibe `ResultadoEval resultado` como parámetro **no nullable**, forzando que la UI siempre envíe un valor del enum.

**Corrección:**
1. En `EvaluarDialog.razor`: eliminar el campo "Resultado" y el dropdown de `ResultadoEval`. El resultado de una revisión cuatrimestral queda expresado únicamente por el `EscalaValoracionId` ya implementado.
2. En `RevisionService.CompletarRevisionAsync`: cambiar la firma para que `resultado` sea `ResultadoEval? resultado = null` y guardarlo como nullable.
3. En `RevisionCuatrimestral` (entidad): el campo `Resultado` ya es `ResultadoEval?` (nullable) — no requiere cambio de modelo.

---

### 🟡 BUG-05 — Jefes sin entrada en tabla `Empleados` no ven el "Cuerpo de Objetivos"

**Archivo:** `Components/Pages/MisObjetivos/Index.razor`

```razor
@if (roleData.Equipo != null && roleData.Personal != null)
{
    @* ÚNICO lugar donde aparece "Cuerpo de Objetivos" y los tabs *@
}
else if (roleData.Equipo != null)
{
    @* Solo grilla plana — sin accordion *@
    @RenderObjetivos(roleData.Equipo, true)
}
```

`ObjetivoService.GetObjetivosRoleAsync` deja `result.Personal = null` (no lista vacía) si el jefe no tiene registro en la tabla `Empleados`. En ese caso el jefe cae en la rama `else if`, ve solo la grilla plana y no tiene acceso al Cuerpo de Objetivos.

**Corrección en `ObjetivoService.GetObjetivosRoleAsync`:**
```csharp
// Garantizar que Personal nunca sea null, solo lista vacía si no aplica
result.Personal ??= new List<Objetivo>();
```
Agregar esta línea al final del método, antes del `return result`.

---

### 🟡 BUG-06 — Soft skills en seed con descripción vacía (nombre repetido)

**Archivo:** `Data/SeedData.cs`

```csharp
db.SoftSkills.Add(new SoftSkill {
    Nombre = skillNames[i],
    Descripcion = skillNames[i],  // ← nombre repetido como descripción
    ...
});
```

La sección "Guía" mostrará cada soft skill con su nombre repetido como definición. Ej: "Comunicación efectiva: Comunicación efectiva". Esto no es un error de compilación pero sí un problema de contenido visible para el usuario.

**Corrección:** Las definiciones reales deben obtenerse de RRHH. Mientras tanto, cambiar la descripción a un texto placeholder diferenciable:
```csharp
Descripcion = $"Definición pendiente de carga por RRHH para: {skillNames[i]}."
```

---

## CHECKLIST ACTUALIZADO

| # | Check | Estado |
|---|-------|--------|
| 1 | El proyecto compila sin errores | ❌ `GetObjetivosPendientesAutoevAsync` inexistente |
| 2 | El seed corre en BD existente sin saltar | ❌ `if (AnyAsync()) return` salta todo |
| 3 | Pilares tienen definiciones oficiales | ❌ Descripciones placeholder |
| 4 | EvaluarDialog: sin estrellas, sin ResultadoEval enum | ⚠️ Sin estrellas ✅ / ResultadoEval sigue ❌ |
| 5 | Jefes ven el Cuerpo de Objetivos | ⚠️ Solo si tienen registro en Empleados |
| 6 | Admin ve menú y CRUDs | ✅ |
| 7 | Jefe no puede crear objetivos (param false) | ✅ (si el seed corre — ver BUG-02) |
| 8 | Autoevaluación lanzable desde sección Autoevaluación | ✅ |
| 9 | Botón Autoevaluar eliminado de Objetivos | ✅ |
| 10 | Valoraciones con escala dinámica (sin estrellas) | ✅ en Autoevaluar y EvaluarFinal; ⚠️ parcial en Evaluar |
| 11 | Seed de escalas, estados, configuraciones | ✅ (si BUG-02 se corrige) |
| 12 | Guía muestra pilares con definiciones reales | ❌ Descripciones placeholder en seed |
| 13 | EscalaSelector notifica al padre correctamente | ✅ |

---

## ORDEN DE CORRECCIÓN (de mayor a menor bloqueo)

| Prioridad | Bug | Acción |
|-----------|-----|--------|
| 🔴 1 | BUG-01 compile error | Agregar `GetObjetivosPendientesAutoevAsync` a `AutoevaluacionService` |
| 🔴 2 | BUG-02 seed skip-all | Separar condición de nómina vs tablas de configuración |
| 🟠 3 | BUG-03 pilares con texto incorrecto | Reemplazar descripciones placeholder por las de Pablo Cirac |
| 🟠 4 | BUG-04 ResultadoEval en EvaluarDialog | Eliminar dropdown y parámetro en RevisionService |
| 🟡 5 | BUG-05 Personal null para jefes | Agregar `result.Personal ??= new List<Objetivo>()` en ObjetivoService |
| 🟡 6 | BUG-06 soft skills sin descripción | Cambiar placeholder de descripción en seed |
