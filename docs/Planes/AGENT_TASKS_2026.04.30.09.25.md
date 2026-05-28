# PQ Talent — Instrucciones de Implementación para Claude Code

> **Stack:** Blazor Server · EF Core · SQLite · Radzen UI  
> **Proyecto:** `C:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web`  
> **Regla absoluta:** Implementar exactamente lo que se indica. No inferir variantes, no agregar funcionalidad no solicitada, no omitir pasos. Si un archivo no existe, crearlo. Si existe, modificarlo con precisión quirúrgica.

---

## TAREA 1 — Corregir acceso del admin al panel de administración

### Problema
`AdminConfiguracion.razor` tiene `@attribute [Authorize(Roles = "RRHH,SUPERUSUARIO")]` pero el proyecto no tiene middleware de autenticación ASP.NET configurado. Ese atributo bloquea la página para todos los usuarios.

### Paso 1.1 — Agregar propiedad `EsAdmin` a `ICurrentUserService`

**Archivo:** `Services/CurrentUserService.cs`

Agregar en la interfaz `ICurrentUserService`:
```csharp
bool EsAdmin { get; }
```

Agregar en la clase `SessionCurrentUserService`:
```csharp
public bool EsAdmin => Rol == "RRHH" || Rol == "DIRECTOR_GENERAL" || EsSuperusuario;
```

### Paso 1.2 — Reemplazar `[Authorize]` por guard manual en `AdminConfiguracion.razor`

**Archivo:** `Components/Pages/Admin/AdminConfiguracion.razor`

1. Eliminar la línea:
```razor
@attribute [Authorize(Roles = "RRHH,SUPERUSUARIO")]
```

2. Agregar los siguientes injects al inicio del archivo (después de `@page`):
```razor
@inject ICurrentUserService CurrentUser
@inject NavigationManager Nav
```

3. En el bloque `@code`, reemplazar `OnInitializedAsync` para que quede así:
```csharp
protected override async Task OnInitializedAsync()
{
    await CurrentUser.InitializeAsync();
    if (!CurrentUser.EsAdmin)
    {
        Nav.NavigateTo("/", forceLoad: false);
        return;
    }
    await LoadAllData();
}
```

### Paso 1.3 — Corregir `NavMenu.razor`

**Archivo:** `Components/Layout/NavMenu.razor`

Reemplazar el bloque `@code` completo por:
```csharp
@code {
    protected override async Task OnInitializedAsync()
    {
        await CurrentUser.InitializeAsync();
    }
}
```

Reemplazar la condición del menú admin:
```razor
@if (CurrentUser.EsAdmin)
```

---

## TAREA 2 — Implementar CRUDs del panel de administración

Los handlers de todos los CRUDs en `AdminConfiguracion.razor` actualmente solo muestran un toast "pendiente". Implementar los diálogos completos para cada entidad.

### Paso 2.1 — Crear `EditarPilarDialog.razor`

**Archivo a crear:** `Components/Pages/Admin/EditarPilarDialog.razor`

```razor
@inject AdminPilarService AdminPilarService
@inject NotificationService NotificationService
@inject DialogService DialogService

<RadzenTemplateForm TItem="Pilar" Data="@pilar" Submit="@OnSubmit">
    <RadzenStack Gap="1rem">
        <RadzenFormField Text="Nombre" Variant="Variant.Outlined">
            <RadzenTextBox @bind-Value="@pilar.Nombre" Name="Nombre" Style="width:100%" />
            <RadzenRequiredValidator Component="Nombre" Text="Requerido" />
        </RadzenFormField>
        <RadzenFormField Text="Descripción" Variant="Variant.Outlined">
            <RadzenTextArea @bind-Value="@pilar.Descripcion" Name="Descripcion" Rows="4" Style="width:100%" />
        </RadzenFormField>
        <RadzenFormField Text="Color (hex)" Variant="Variant.Outlined">
            <RadzenTextBox @bind-Value="@pilar.ColorHex" Name="ColorHex" Style="width:100%" />
        </RadzenFormField>
        <RadzenFormField Text="Orden" Variant="Variant.Outlined">
            <RadzenNumeric @bind-Value="@pilar.Orden" Min="0" Style="width:100%" />
        </RadzenFormField>
        <RadzenStack Orientation="Orientation.Horizontal" Gap="0.5rem">
            <RadzenCheckBox @bind-Value="@pilar.Activo" Name="Activo" />
            <RadzenLabel Text="Activo" Component="Activo" />
        </RadzenStack>
        <RadzenStack Orientation="Orientation.Horizontal" JustifyContent="JustifyContent.End" Gap="0.5rem">
            <RadzenButton Text="Cancelar" ButtonStyle="ButtonStyle.Light" Click="@(() => DialogService.Close(false))" />
            <RadzenButton ButtonType="ButtonType.Submit" Text="Guardar" ButtonStyle="ButtonStyle.Primary" />
        </RadzenStack>
    </RadzenStack>
</RadzenTemplateForm>

@code {
    [Parameter] public Pilar Pilar { get; set; } = new();
    Pilar pilar = new();

    protected override void OnInitialized()
    {
        pilar = new Pilar
        {
            Id = Pilar.Id, Nombre = Pilar.Nombre, Descripcion = Pilar.Descripcion,
            ColorHex = Pilar.ColorHex, Orden = Pilar.Orden, Activo = Pilar.Activo
        };
    }

    async Task OnSubmit()
    {
        bool ok = pilar.Id == 0
            ? await AdminPilarService.CrearAsync(pilar)
            : await AdminPilarService.ActualizarAsync(pilar);

        if (ok) DialogService.Close(true);
        else NotificationService.Notify(NotificationSeverity.Error, "Error", "No se pudo guardar el pilar.");
    }
}
```

### Paso 2.2 — Crear `EditarSoftSkillDialog.razor`

**Archivo a crear:** `Components/Pages/Admin/EditarSoftSkillDialog.razor`

Estructura idéntica a `EditarPilarDialog.razor` pero con `SoftSkill` y `AdminSoftSkillService`. Campos: `Nombre`, `Descripcion`, `Orden`, `Activo` (sin `ColorHex`).

### Paso 2.3 — Crear `EditarEscalaDialog.razor`

**Archivo a crear:** `Components/Pages/Admin/EditarEscalaDialog.razor`

Estructura idéntica pero con `EscalaValoracion` y `AdminEscalaValoracionService`. Campos: `Etiqueta`, `ValorNumerico` (RadzenNumeric nullable decimal), `Orden`, `Activo`.

### Paso 2.4 — Crear `EditarEstadoObjetivoDialog.razor`

**Archivo a crear:** `Components/Pages/Admin/EditarEstadoObjetivoDialog.razor`

Con `EstadoObjetivoConfig` y `AdminEstadoService`. Campos: `Nombre`, `ColorHex`, `Orden`, `Activo`. El campo `Slug` se muestra como texto de solo lectura con una advertencia: *"El slug no puede modificarse porque es referenciado por la lógica del sistema."*

### Paso 2.5 — Crear `EditarEstadoEvaluacionDialog.razor`

**Archivo a crear:** `Components/Pages/Admin/EditarEstadoEvaluacionDialog.razor`

Idéntico a `EditarEstadoObjetivoDialog.razor` pero con `EstadoEvaluacionConfig`.

### Paso 2.6 — Crear `EditarAreaDialog.razor`

**Archivo a crear:** `Components/Pages/Admin/EditarAreaDialog.razor`

Con `Area` y `AdminAreaService`. Campos: `Nombre`, `Descripcion`, `Activo`.

### Paso 2.7 — Crear `EditarConfiguracionDialog.razor`

**Archivo a crear:** `Components/Pages/Admin/EditarConfiguracionDialog.razor`

```razor
@inject AdminConfiguracionPlatformaService AdminConfigService
@inject NotificationService NotificationService
@inject DialogService DialogService

<RadzenStack Gap="1rem" Style="padding: 1rem;">
    <RadzenText TextStyle="TextStyle.Subtitle1"><b>@Config.Clave</b></RadzenText>
    <RadzenText TextStyle="TextStyle.Body2" Style="opacity:0.7;">@Config.Descripcion</RadzenText>
    <RadzenFormField Text="Valor" Variant="Variant.Outlined">
        <RadzenTextBox @bind-Value="@nuevoValor" Style="width:100%" />
    </RadzenFormField>
    <RadzenStack Orientation="Orientation.Horizontal" JustifyContent="JustifyContent.End" Gap="0.5rem">
        <RadzenButton Text="Cancelar" ButtonStyle="ButtonStyle.Light" Click="@(() => DialogService.Close(false))" />
        <RadzenButton Text="Guardar" ButtonStyle="ButtonStyle.Primary" Click="@OnGuardar" />
    </RadzenStack>
</RadzenStack>

@code {
    [Parameter] public ConfiguracionPlataforma Config { get; set; } = new();
    string nuevoValor = "";

    protected override void OnInitialized() => nuevoValor = Config.Valor;

    async Task OnGuardar()
    {
        await AdminConfigService.ActualizarAsync(Config.Clave, nuevoValor);
        DialogService.Close(true);
    }
}
```

### Paso 2.8 — Conectar handlers en `AdminConfiguracion.razor`

Reemplazar todos los métodos stub del bloque `@code` por implementaciones reales:

```csharp
async Task OnAgregarPilar()
{
    var r = await DialogService.OpenAsync<EditarPilarDialog>("Nuevo Pilar",
        new Dictionary<string, object?> { { "Pilar", new Pilar { Activo = true } } },
        new DialogOptions { Width = "500px" });
    if (r is true) await LoadAllData();
}

async Task OnEditarPilar(Pilar p)
{
    var r = await DialogService.OpenAsync<EditarPilarDialog>("Editar Pilar",
        new Dictionary<string, object?> { { "Pilar", p } },
        new DialogOptions { Width = "500px" });
    if (r is true) await LoadAllData();
}

async Task OnEliminarPilar(int id)
{
    var confirm = await DialogService.Confirm("¿Desactivar este pilar?", "Confirmar",
        new ConfirmOptions { OkButtonText = "Sí, desactivar", CancelButtonText = "Cancelar" });
    if (confirm == true)
    {
        await AdminPilarService.DesactivarAsync(id);
        await LoadAllData();
    }
}
```

Repetir el mismo patrón para: `OnAgregarSoftSkill`, `OnEditarSoftSkill`, `OnEliminarSoftSkill`, `OnAgregarEscala`, `OnEditarEscala`, `OnEliminarEscala`, `OnEditarEstadoObjetivo`, `OnEditarEstadoEvaluacion`, `OnAgregarArea`, `OnEditarArea`, `OnEliminarArea`, `OnEditarConfiguracion` — cada uno abriendo su dialog correspondiente de los pasos 2.1 a 2.7.

### Paso 2.9 — Verificar que los Admin Services tienen los métodos necesarios

Cada `AdminXxxService` debe exponer: `ObtenerTodosAsync()`, `CrearAsync(entidad)`, `ActualizarAsync(entidad)`, `DesactivarAsync(id)`. Si algún método falta, implementarlo con EF Core (mismo patrón del servicio existente).

---

## TAREA 3 — Corregir creación de objetivos: solo empleados pueden crear

### Problema
En `MisObjetivos/Index.razor` el botón "Nuevo Objetivo" tiene `Visible="@(CurrentUser.EsJefe)"` — muestra el botón solo para jefes, exactamente al revés de lo requerido.

En `CrearObjetivoDialog.razor` la condición de validación tiene un bug de precedencia de operadores.

### Paso 3.1 — Corregir visibilidad del botón en `MisObjetivos/Index.razor`

1. Agregar inject y campo al bloque `@code`:
```csharp
@inject ConfiguracionService ConfigService

bool puedeCrearObjetivo;

protected override async Task OnInitializedAsync()
{
    bool jefePermitido = await ConfigService.ObtenerConfiguracionBoolAsync("jefe_puede_crear_objetivos") ?? false;
    puedeCrearObjetivo = CurrentUser.EsJefe
        ? (jefePermitido || CurrentUser.Rol == "RRHH" || CurrentUser.Rol == "DIRECTOR_GENERAL")
        : true;
    await LoadData();
}
```

2. Reemplazar la línea del botón:
```razor
@* ANTES *@
<RadzenButton Icon="add" Text="Nuevo Objetivo" Click="@OnCreateClick" ButtonStyle="ButtonStyle.Primary" Visible="@(CurrentUser.EsJefe)" />

@* DESPUÉS *@
<RadzenButton Icon="add" Text="Nuevo Objetivo" Click="@OnCreateClick" ButtonStyle="ButtonStyle.Primary" Visible="@puedeCrearObjetivo" />
```

### Paso 3.2 — Corregir bug de precedencia en `CrearObjetivoDialog.razor`

En `OnInitializedAsync`, reemplazar:
```csharp
// ELIMINAR esta condición bugueada:
if (CurrentUser.EsJefe && !jefePermitido && CurrentUser.Rol == "DIRECTOR" || (CurrentUser.Rol != "RRHH" && CurrentUser.Rol != "DIRECTOR_GENERAL"))
```
Por:
```csharp
if (CurrentUser.EsJefe && !jefePermitido
    && CurrentUser.Rol != "RRHH"
    && CurrentUser.Rol != "DIRECTOR_GENERAL")
{
    errorMsg = "Su rol no tiene permiso para crear objetivos. Contacte al área de RRHH.";
    return;
}
```

### Paso 3.3 — Verificar backend en `ObjetivoService.CrearObjetivoAsync`

La validación backend ya existe y es correcta. Verificar que esté presente y no haya sido eliminada:
```csharp
bool jefePermitido = await _configuracion.ObtenerConfiguracionBoolAsync("jefe_puede_crear_objetivos") ?? false;
if (_currentUser.EsJefe && !jefePermitido && _currentUser.Rol != "RRHH" && _currentUser.Rol != "DIRECTOR_GENERAL")
    return (false, false);
```

---

## TAREA 4 — Seed de datos iniciales (Migración)

Crear o actualizar `Data/SeedData.cs` para que en `InitializeAsync` se inserten los siguientes registros **solo si no existen** (verificar por clave/slug/nombre antes de insertar).

### 4.1 — Seed de `Pilares`

```csharp
if (!await db.Pilares.AnyAsync())
{
    db.Pilares.AddRange(
        new Pilar { Nombre = "Crecimiento de Ventas", ColorHex = "#4CAF50", Orden = 1, Activo = true,
            Descripcion = "Este objetivo se centra en incrementar los ingresos a través de la expansión de la base de clientes, la mejora de la oferta de productos o servicios, y la optimización de las estrategias de ventas. Implica explorar nuevos mercados, nuevos productos y nuevos segmentos, fortalecer relaciones con los clientes actuales y desarrollar tácticas innovadoras para aumentar las ventas en el corto, mediano y largo plazo." },
        new Pilar { Nombre = "Orientación al Cliente", ColorHex = "#2196F3", Orden = 2, Activo = true,
            Descripcion = "Optimización y Experiencia Integral (Interna y Externa). Este pilar busca optimizar la totalidad del ciclo posterior a la venta, entendiendo que el servicio de calidad hacia afuera es el resultado de una operación interna eficiente y coordinada.\n\n1. Compromiso con el Cliente Externo: Buscamos asegurar un servicio ágil que supere las expectativas, reduciendo los tiempos de respuesta y resolviendo cualquier incidencia con precisión. El objetivo es que cada contacto postventa fortalezca la confianza en la marca, transformando una transacción en una relación de fidelidad a largo plazo.\n\n2. Fortalecimiento del Cliente Interno: Para lograrlo, optimizamos nuestros procesos internos proporcionando a nuestros colaboradores las herramientas, la información y el soporte necesarios para actuar con autonomía y eficacia. Un flujo de trabajo interno fluido y una comunicación interdepartamental clara son los motores que permiten al equipo brindar soluciones rápidas y de alto valor." },
        new Pilar { Nombre = "Eficiencia Organizacional", ColorHex = "#F9A825", Orden = 3, Activo = true,
            Descripcion = "Este objetivo busca mejorar el rendimiento interno de la compañía a través de la optimización de sus recursos, la eliminación de ineficiencias y la implementación de mejores prácticas en la gestión de los procesos. La eficiencia organizacional se traduce en un entorno de trabajo más ágil, con procesos más simplificados y una significativa reducción de costos operativos. Al fomentar una cultura de mejora continua y ahorro estratégico, logramos maximizar tanto los resultados financieros como la calidad en todas las áreas, asegurando la sostenibilidad del negocio a largo plazo." }
    );
    await db.SaveChangesAsync();
}
```

> Nota: el color del tercer pilar se cambia de `#FFEB3B` (amarillo claro, ilegible sobre fondo blanco) a `#F9A825` (ámbar oscuro, legible).

### 4.2 — Seed de `EscalaValoracion`

```csharp
if (!await db.EscalasValoracion.AnyAsync())
{
    db.EscalasValoracion.AddRange(
        new EscalaValoracion { Etiqueta = "Excelente",  ValorNumerico = 5, Orden = 1, Activo = true },
        new EscalaValoracion { Etiqueta = "Muy bueno",  ValorNumerico = 4, Orden = 2, Activo = true },
        new EscalaValoracion { Etiqueta = "Bueno",      ValorNumerico = 3, Orden = 3, Activo = true },
        new EscalaValoracion { Etiqueta = "Regular",    ValorNumerico = 2, Orden = 4, Activo = true },
        new EscalaValoracion { Etiqueta = "Malo",       ValorNumerico = 1, Orden = 5, Activo = true }
    );
    await db.SaveChangesAsync();
}
```

### 4.3 — Seed de `EstadosObjetivoConfig`

```csharp
if (!await db.EstadosObjetivoConfig.AnyAsync())
{
    db.EstadosObjetivoConfig.AddRange(
        new EstadoObjetivoConfig { Slug = "borrador",             Nombre = "Borrador",                ColorHex = "#9E9E9E", Orden = 1, Activo = true },
        new EstadoObjetivoConfig { Slug = "pendiente_aprobacion", Nombre = "Pendiente de aprobación", ColorHex = "#FF9800", Orden = 2, Activo = true },
        new EstadoObjetivoConfig { Slug = "aprobado",             Nombre = "Aprobado",                ColorHex = "#4CAF50", Orden = 3, Activo = true },
        new EstadoObjetivoConfig { Slug = "en_curso",             Nombre = "En curso",                ColorHex = "#2196F3", Orden = 4, Activo = true },
        new EstadoObjetivoConfig { Slug = "completado",           Nombre = "Completado",              ColorHex = "#8BC34A", Orden = 5, Activo = true },
        new EstadoObjetivoConfig { Slug = "en_riesgo",            Nombre = "En riesgo",               ColorHex = "#FF5722", Orden = 6, Activo = true },
        new EstadoObjetivoConfig { Slug = "vencido",              Nombre = "Vencido",                 ColorHex = "#F44336", Orden = 7, Activo = true },
        new EstadoObjetivoConfig { Slug = "cancelado",            Nombre = "Cancelado",               ColorHex = "#757575", Orden = 8, Activo = false }
    );
    await db.SaveChangesAsync();
}
```

### 4.4 — Seed de `EstadosEvaluacionConfig`

```csharp
if (!await db.EstadosEvaluacionConfig.AnyAsync())
{
    db.EstadosEvaluacionConfig.AddRange(
        new EstadoEvaluacionConfig { Slug = "pendiente",         Nombre = "Pendiente",         ColorHex = "#FF9800", Orden = 1, Activo = true },
        new EstadoEvaluacionConfig { Slug = "en_progreso",       Nombre = "En progreso",       ColorHex = "#2196F3", Orden = 2, Activo = true },
        new EstadoEvaluacionConfig { Slug = "completada",        Nombre = "Completada",        ColorHex = "#4CAF50", Orden = 3, Activo = true },
        new EstadoEvaluacionConfig { Slug = "proxima_a_vencer",  Nombre = "Próxima a vencer",  ColorHex = "#FF5722", Orden = 4, Activo = true }
    );
    await db.SaveChangesAsync();
}
```

### 4.5 — Seed de `ConfiguracionPlataforma`

```csharp
var configsRequeridas = new[]
{
    new ConfiguracionPlataforma { Clave = "email_soporte",                   Valor = "rrhh@permaquim.com", Tipo = "email",   Descripcion = "Email de contacto para soporte e inconvenientes." },
    new ConfiguracionPlataforma { Clave = "dias_proximo_vencimiento",        Valor = "7",                  Tipo = "integer", Descripcion = "Días previos al vencimiento para mostrar alerta 'Próximo a vencer'." },
    new ConfiguracionPlataforma { Clave = "objetivo_area_habilitado",        Valor = "true",               Tipo = "boolean", Descripcion = "Habilita la sección de objetivo específico por área en el formulario de evaluación." },
    new ConfiguracionPlataforma { Clave = "calculos_comerciales_habilitados",Valor = "false",              Tipo = "boolean", Descripcion = "Habilita cálculos específicos del área comercial. Mantener en false hasta nuevo aviso." },
    new ConfiguracionPlataforma { Clave = "resultado_final_manual",          Valor = "true",               Tipo = "boolean", Descripcion = "Si true, el resultado final de la evaluación lo ingresa manualmente el jefe. Si false, se calcula automáticamente." },
    new ConfiguracionPlataforma { Clave = "texto_guia_plataforma",           Valor = "",                   Tipo = "text",    Descripcion = "Texto del manual de uso que aparece en la sección Guía." },
    new ConfiguracionPlataforma { Clave = "jefe_puede_crear_objetivos",      Valor = "false",              Tipo = "boolean", Descripcion = "Si false, solo los empleados/colaboradores pueden crear objetivos. Si true, los jefes también pueden." }
};

foreach (var config in configsRequeridas)
{
    if (!await db.ConfiguracionesPlataforma.AnyAsync(c => c.Clave == config.Clave))
        db.ConfiguracionesPlataforma.Add(config);
}
await db.SaveChangesAsync();
```

### 4.6 — Seed de `SoftSkills`

El seed de soft skills se deja vacío intencionalmente. Las soft skills se cargan a través del CRUD de Admin (`/admin/configuracion` → tab "Soft Skills") una vez que RRHH provea el listado. No hardcodear ningún valor.

---

## TAREA 5 — Escala de valoración dinámica: reemplazar estrellas y enums

### Objetivo
Reemplazar en toda la plataforma:
- `RadzenRating` (estrellas) para soft skills → selector dropdown desde `EscalaValoracion`
- Campos `Puntaje` (integer 1-5) → `EscalaValoracionId` (FK a tabla dinámica)
- Texto "puntaje" → "valoración" en toda la UI

### Paso 5.1 — Find & Replace global en todos los archivos `.razor` y `.cs`

Ejecutar en el directorio `Objetivos.Web`:
- Reemplazar `"puntaje"` → `"valoración"` (case-insensitive, en strings de UI)
- Reemplazar `"Puntaje"` → `"Valoración"` (en títulos y labels)
- NO reemplazar nombres de propiedades de C# como `Puntaje` en entidades ni columnas de BD (para no romper EF Core).

### Paso 5.2 — Componente `EscalaSelector.razor`

**Archivo a crear:** `Components/Shared/EscalaSelector.razor`

```razor
@inject ConfiguracionService ConfigService

<RadzenDropDown @bind-Value="@Value" Data="@opciones"
                TextProperty="Etiqueta" ValueProperty="Id"
                Style="width:100%" Placeholder="Seleccionar valoración..."
                Change="@(() => ValueChanged.InvokeAsync(Value))" />

@code {
    [Parameter] public int? Value { get; set; }
    [Parameter] public EventCallback<int?> ValueChanged { get; set; }

    List<EscalaValoracion> opciones = new();

    protected override async Task OnInitializedAsync()
    {
        opciones = await ConfigService.ObtenerEscalasActivasAsync();
    }
}
```

### Paso 5.3 — Reemplazar estrellas en `AutoevaluarDialog.razor`

En `Components/Pages/Autoevaluaciones/AutoevaluarDialog.razor`:

1. Localizar todos los `RadzenRating` usados para score de soft skills.
2. Reemplazarlos por `<EscalaSelector @bind-Value="@ae.SoftSkill1EscalaValoracionId" />`.
3. Hacer lo mismo para `SoftSkill2EscalaValoracionId` y el score principal del objetivo (`EscalaValoracionIdScore`).
4. Eliminar los campos `Score`, `SoftSkill1Score`, `SoftSkill2Score` de la vista (mantenerlos en BD pero no mostrarlos en la UI nueva).

### Paso 5.4 — Reemplazar estrellas en la vista de autoevaluaciones `Autoevaluaciones/Index.razor`

Localizar la columna que usa `RadzenRating` para mostrar el score. Reemplazar por texto que muestre `ae.EscalaValoracionScore?.Etiqueta ?? "-"`. Hacer lo mismo para las soft skills (`SoftSkill1EscalaValoracion?.Etiqueta`, `SoftSkill2EscalaValoracion?.Etiqueta`).

Asegurarse de que las queries en `AutoevaluacionService` incluyen los `.Include()` necesarios para cargar esas navegaciones.

### Paso 5.5 — Reemplazar en `EvaluarDialog.razor` y `EvaluarFinalDialog.razor`

En `Components/Pages/Evaluacion/`:
- `EvaluarDialog.razor`: reemplazar cualquier `RadzenRating` o selector de puntaje por `<EscalaSelector>` vinculado a `EscalaValoracionId` de la revisión.
- `EvaluarFinalDialog.razor`: reemplazar el selector de resultado final por `<EscalaSelector @bind-Value="@evaluacion.EscalaValoracionIdFinal" />`. Eliminar cualquier cálculo automático de promedio ponderado. Eliminar el cartel/mensaje que indica que el resultado se calculará automáticamente. Mostrar únicamente el selector manual.

---

## TAREA 6 — Resultado final de evaluación: modo manual obligatorio

### Paso 6.1 — Modificar `EvaluarFinalDialog.razor`

**Archivo:** `Components/Pages/Evaluacion/EvaluarFinalDialog.razor`

1. Eliminar completamente cualquier bloque de código que calcule `PuntajeFinal` como promedio de las revisiones.
2. Eliminar cualquier `RadzenAlert` o texto que diga "el resultado se calculará automáticamente".
3. Mostrar los valores ponderados de cada revisión como texto de solo lectura (referencia visual, no editable).
4. El único campo editable para el resultado final es el `<EscalaSelector @bind-Value="@evaluacion.EscalaValoracionIdFinal" />`.
5. En el servicio de evaluación final (`EvaluacionService`), el campo `PuntajeFinal` se calcula como el `ValorNumerico` de la `EscalaValoracion` seleccionada al momento de guardar. Si `ValorNumerico` es null, guardar `0`.

---

## TAREA 7 — Autoevaluación: mover al lugar correcto

### Problema
El formulario de autoevaluación (`AutoevaluarDialog`) se lanza desde `MisObjetivos/Index.razor`. El usuario lo ve en "Objetivos y Competencias" antes de haberlo completado. Una vez completado, también aparece en "Autoevaluación". El flujo correcto es: el formulario se lanza **solo** desde la sección "Autoevaluación".

### Paso 7.1 — Eliminar el botón de autoevaluación de `MisObjetivos/Index.razor`

Eliminar el botón y su handler:
```razor
@* ELIMINAR este bloque completo *@
@if (!showEmployee && obj.Estado == EstadoObjetivo.ACTIVO)
{
    <RadzenButton Icon="rate_review" Text="Autoevaluar" ... Click="@(() => OnAutoevaluarClick(obj))" />
}
```

Eliminar también el método `OnAutoevaluarClick` del bloque `@code`.

### Paso 7.2 — Agregar botón de autoevaluación en `Autoevaluaciones/Index.razor`

En la sección "Mis Autoevaluaciones" (vista de empleado), agregar una grilla de objetivos propios pendientes de autoevaluar:

1. En `OnInitializedAsync`, cargar también los objetivos del empleado actual que no tienen autoevaluación:
```csharp
List<Objetivo> objetivosPendientesAutoev = new();

// Agregar en OnInitializedAsync:
var empleadoId = await AutoevaluacionService.GetEmpleadoIdByEmailAsync(CurrentUser.Email);
if (empleadoId.HasValue)
{
    var idsConAutoev = data.Personal.Select(ae => ae.ObjetivoId).ToHashSet();
    using var db = DbFactory.CreateDbContext(); // inyectar IDbContextFactory
    objetivosPendientesAutoev = await db.Objetivos
        .Include(o => o.Pilar)
        .Where(o => o.EmpleadoId == empleadoId.Value
                 && o.Estado != EstadoObjetivo.CANCELADO
                 && !idsConAutoev.Contains(o.Id))
        .ToListAsync();
}
```

2. Renderizar esa lista con un botón "Autoevaluar" por fila que abra `AutoevaluarDialog`.

---

## TAREA 8 — Cuerpo de objetivos: vista agrupada para el jefe

### Problema
En `MisObjetivos/Index.razor`, la pestaña "Objetivos de mi Equipo" muestra una fila por cada objetivo. Debe mostrar un bloque colapsable por empleado.

### Paso 8.1 — Reemplazar la grilla plana por acordeón en la tab del equipo

En `MisObjetivos/Index.razor`, reemplazar el `RenderObjetivos` que se usa en la tab "Objetivos de mi Equipo" por un nuevo render fragment `RenderObjetivosAgrupados`:

```csharp
private RenderFragment RenderObjetivosAgrupados(List<Objetivo> lista) => __builder =>
{
    var grupos = lista.GroupBy(o => o.EmpleadoId).ToList();

    if (!grupos.Any())
    {
        <EmptyState Message="No hay objetivos cargados por el equipo aún." />
        return;
    }

    foreach (var grupo in grupos)
    {
        var empleado = grupo.First().Empleado;
        var aprobados = grupo.Count(o => o.AprobadoPorJefe);
        var total = grupo.Count();

        <RadzenPanel AllowCollapse="true" Style="margin-bottom: 0.75rem;">
            <HeaderTemplate>
                <RadzenStack Orientation="Orientation.Horizontal" AlignItems="AlignItems.Center" Gap="1rem">
                    <RadzenText TextStyle="TextStyle.Subtitle1" Style="margin:0; font-weight:600;">
                        @empleado.Nombre @empleado.Apellido
                    </RadzenText>
                    <RadzenBadge Text="@($"{aprobados} de {total} aprobados")"
                                 BadgeStyle="@(aprobados == total ? BadgeStyle.Success : BadgeStyle.Warning)" />
                </RadzenStack>
            </HeaderTemplate>
            <ChildContent>
                <RadzenDataGrid Data="@grupo.ToList()" TItem="Objetivo" AllowPaging="false" Responsive="true" Style="margin-top:0.5rem;">
                    <Columns>
                        <RadzenDataGridColumn TItem="Objetivo" Property="Nombre" Title="Objetivo" />
                        <RadzenDataGridColumn TItem="Objetivo" Property="Pilar.Nombre" Title="Pilar">
                            <Template Context="obj">
                                <RadzenBadge Text="@obj.Pilar.Nombre" Style="@($"background-color:{obj.Pilar.ColorHex};color:white;")" />
                            </Template>
                        </RadzenDataGridColumn>
                        <RadzenDataGridColumn TItem="Objetivo" Property="Deadline" Title="Vencimiento" FormatString="{0:dd/MM/yyyy}" Width="130px" />
                        <RadzenDataGridColumn TItem="Objetivo" Title="Aprobado" Width="100px">
                            <Template Context="obj">
                                <RadzenIcon Icon="@(obj.AprobadoPorJefe ? "check_circle" : "schedule")"
                                            Style="@($"color:{(obj.AprobadoPorJefe ? "#4CAF50" : "#FF9800")}")" />
                            </Template>
                        </RadzenDataGridColumn>
                        <RadzenDataGridColumn TItem="Objetivo" Filterable="false" Sortable="false" Width="100px">
                            <Template Context="obj">
                                <RadzenButton Icon="visibility" Size="ButtonSize.Small" ButtonStyle="ButtonStyle.Light"
                                              Click="@(() => OnViewClick(obj.Id, obj.EmpleadoId))" />
                            </Template>
                        </RadzenDataGridColumn>
                    </Columns>
                </RadzenDataGrid>
            </ChildContent>
        </RadzenPanel>
    }
};
```

Llamar a `RenderObjetivosAgrupados(roleData.Equipo)` en la tab "Objetivos de mi Equipo".

---

## TAREA 9 — Eliminar "Evidencias Verificadas" de la vista de Evaluación

### Paso 9.1
En `EvaluarDialog.razor` y en cualquier otro componente de la sección "Evaluación" (`Components/Pages/Evaluacion/`):

1. Buscar y eliminar cualquier campo, label, columna de grilla o sección que muestre el texto "evidencias verificadas", "evidencias" o que esté vinculado a la propiedad `EvidenciasRevisadasJson`.
2. No eliminar la columna `EvidenciasRevisadasJson` de la BD ni de la entidad `RevisionCuatrimestral` — dejar en el modelo pero no renderizar en UI.
3. No limpiar datos históricos existentes en esa columna.

---

## TAREA 10 — Sección Guía

### Paso 10.1 — Actualizar `Guia.razor`

**Archivo:** `Components/Pages/Guia.razor`

Reemplazar el contenido completo del archivo por una implementación que cargue dinámicamente desde las tablas:

```razor
@page "/guia"
@inject AppDbContext Db
@inject ConfiguracionService ConfigService

<RadzenText TextStyle="TextStyle.H4" Style="margin-bottom:1.5rem;">Guía de la Plataforma</RadzenText>

@if (loading)
{
    <RadzenProgressBar Value="100" ShowValue="false" Mode="ProgressBarMode.Indeterminate" />
}
else
{
    <RadzenTabs>
        <Tabs>
            <RadzenTabsItem Text="Pilares Estratégicos" Icon="flag">
                @foreach (var pilar in pilares)
                {
                    <RadzenCard Style="margin-bottom:1rem;">
                        <RadzenStack Gap="0.5rem">
                            <RadzenStack Orientation="Orientation.Horizontal" AlignItems="AlignItems.Center" Gap="0.75rem">
                                <div style="@($"width:16px;height:16px;border-radius:50%;background-color:{pilar.ColorHex};flex-shrink:0;")"></div>
                                <RadzenText TextStyle="TextStyle.H6" Style="margin:0;">@pilar.Nombre</RadzenText>
                            </RadzenStack>
                            <RadzenText TextStyle="TextStyle.Body1" Style="white-space:pre-line;">@pilar.Descripcion</RadzenText>
                        </RadzenStack>
                    </RadzenCard>
                }
            </RadzenTabsItem>
            <RadzenTabsItem Text="Soft Skills" Icon="psychology">
                @if (!softSkills.Any())
                {
                    <EmptyState Message="Las definiciones de soft skills serán publicadas próximamente." />
                }
                else
                {
                    @foreach (var ss in softSkills)
                    {
                        <RadzenCard Style="margin-bottom:1rem;">
                            <RadzenText TextStyle="TextStyle.H6">@ss.Nombre</RadzenText>
                            <RadzenText TextStyle="TextStyle.Body1">@ss.Descripcion</RadzenText>
                        </RadzenCard>
                    }
                }
            </RadzenTabsItem>
            <RadzenTabsItem Text="Manual de Uso" Icon="help_outline">
                @if (string.IsNullOrWhiteSpace(textoGuia))
                {
                    <EmptyState Message="El manual de uso será publicado próximamente." />
                }
                else
                {
                    <RadzenCard>
                        <RadzenText TextStyle="TextStyle.Body1" Style="white-space:pre-line;">@textoGuia</RadzenText>
                    </RadzenCard>
                }
            </RadzenTabsItem>
        </Tabs>
    </RadzenTabs>
}

@code {
    List<Pilar> pilares = new();
    List<SoftSkill> softSkills = new();
    string textoGuia = "";
    bool loading = true;

    protected override async Task OnInitializedAsync()
    {
        pilares = await Db.Pilares.Where(p => p.Activo).OrderBy(p => p.Orden).ToListAsync();
        softSkills = await Db.SoftSkills.Where(s => s.Activo).OrderBy(s => s.Orden).ToListAsync();
        textoGuia = await ConfigService.ObtenerConfiguracionAsync("texto_guia_plataforma") ?? "";
        loading = false;
    }
}
```

### Paso 10.2 — Mostrar descripción de pilares y soft skills en formularios de evaluación

En `CrearObjetivoDialog.razor`, junto al dropdown de selección de pilar, agregar un tooltip o texto informativo que muestre la descripción del pilar seleccionado:

```razor
@if (pilarSeleccionado != null && !string.IsNullOrEmpty(pilarSeleccionado.Descripcion))
{
    <RadzenAlert AlertStyle="AlertStyle.Info" Variant="Variant.Flat" Shade="Shade.Lighter" Style="margin-top:0.5rem;">
        @pilarSeleccionado.Descripcion
    </RadzenAlert>
}
```

En el bloque `@code`, agregar:
```csharp
Pilar? pilarSeleccionado;

// En el evento de cambio del dropdown de pilar:
void OnPilarChanged(int id)
{
    objetivo.PilarId = id;
    pilarSeleccionado = pilares.FirstOrDefault(p => p.Id == id);
}
```

Vincular el dropdown de pilar al evento: `Change="@((args) => OnPilarChanged((int)args))"`.

Aplicar el mismo patrón para las soft skills en `AutoevaluarDialog.razor`.

---

## TAREA 11 — Corregir cache estática en `ConfiguracionService`

**Archivo:** `Services/ConfiguracionService.cs`

Reemplazar los campos estáticos por instancia con expiración:

```csharp
private Dictionary<string, string> _cache = new();
private DateTime _cacheExpiry = DateTime.MinValue;
private const int CACHE_MINUTES = 5;

private async Task CargarCacheAsync()
{
    using var db = await dbFactory.CreateDbContextAsync();
    var configs = await db.ConfiguracionesPlataforma.ToListAsync();
    _cache = configs.ToDictionary(c => c.Clave, c => c.Valor);
    _cacheExpiry = DateTime.UtcNow.AddMinutes(CACHE_MINUTES);
}

public async Task<string?> ObtenerConfiguracionAsync(string clave)
{
    if (DateTime.UtcNow >= _cacheExpiry)
        await CargarCacheAsync();
    return _cache.TryGetValue(clave, out var valor) ? valor : null;
}

private void LimpiarCache()
{
    _cache.Clear();
    _cacheExpiry = DateTime.MinValue;
}
```

---

## TAREA 12 — Corregir `GetObjetivosRoleAsync`: jefes ven sus propios objetivos

**Archivo:** `Services/ObjetivoService.cs` — método `GetObjetivosRoleAsync`

Reemplazar el bloque que busca `empleadoPropio` por uno que busca en ambas tablas:

```csharp
// Primero buscar en Empleados
var empleadoPropio = await _db.Empleados
    .FirstOrDefaultAsync(e => e.Email.ToLower() == email && e.Activo);

int? empleadoPropioId = empleadoPropio?.Id;

// Si no se encontró en Empleados, buscar si el jefe también tiene objetivos cargados como empleado
// (caso edge: un jefe también puede tener objetivos asignados a su propio legajo de empleado)
// Si empleadoPropioId es null, result.Personal queda como lista vacía, no null.
if (empleadoPropioId.HasValue)
{
    result.Personal = await _db.Objetivos
        .Include(o => o.Empleado)
        .Include(o => o.Pilar)
        .Where(o => o.EmpleadoId == empleadoPropioId.Value && o.Anio == anio)
        .ToListAsync();
}
else
{
    result.Personal = new List<Objetivo>();
}
```

En `MisObjetivos/Index.razor`, ajustar la condición de renderizado:

```razor
@* ANTES *@
@if (roleData.Equipo != null && roleData.Personal != null)

@* DESPUÉS *@
@if (roleData.Equipo != null)
```

---

## TAREA 13 — Objetivo específico por área en el formulario

El formulario `CrearObjetivoDialog.razor` ya tiene la sección condicional de área específica controlada por `objetivo_area_habilitado`. Verificar que:

1. Si `objetivo_area_habilitado = true`, la sección aparece y el dropdown de áreas carga desde `db.Areas.Where(a => a.Activo)`.
2. El campo `PorcentajeArea` suma al `PorcentajePilar`. Agregar validación en `OnSubmit`:
```csharp
decimal totalPorcentaje = objetivo.PorcentajePilar + (objetivo.AreaEspecificaId.HasValue ? objetivo.PorcentajeArea : 0);
if (totalPorcentaje != 100)
{
    errorMsg = $"La suma de porcentajes debe ser exactamente 100%. Actualmente: {totalPorcentaje}%";
    return;
}
```
3. Si `objetivo.AreaEspecificaId` es null, `PorcentajeArea` se ignora y el 100% lo debe cubrir `PorcentajePilar` solo.

---

## VERIFICACIÓN FINAL

Después de implementar todas las tareas, verificar:

| Check | Descripción |
|-------|-------------|
| ✅ | Usuario RRHH ve el menú "Administración" con sub-ítems al iniciar sesión |
| ✅ | `/admin/configuracion` carga correctamente para RRHH y DIRECTOR_GENERAL |
| ✅ | Los botones Agregar/Editar/Eliminar del admin abren diálogos funcionales |
| ✅ | El seed se ejecuta sin errores en una BD vacía |
| ✅ | El empleado ve el botón "Nuevo Objetivo" en la sección Objetivos |
| ✅ | El jefe NO ve el botón "Nuevo Objetivo" (parámetro `jefe_puede_crear_objetivos = false`) |
| ✅ | El jefe SÍ puede aprobar/rechazar objetivos de sus reportes |
| ✅ | El formulario de autoevaluación aparece en la sección "Autoevaluación", no en "Objetivos y Competencias" |
| ✅ | Los objetivos del equipo aparecen agrupados por empleado con acordeón colapsable |
| ✅ | Los selectores de valoración son dropdowns desde `EscalaValoracion` (no estrellas) |
| ✅ | No aparece ningún texto "puntaje" en la UI |
| ✅ | La evaluación final no tiene cálculo automático; solo el selector de escala manual |
| ✅ | La sección "Guía" muestra los 3 pilares con sus definiciones completas |
| ✅ | No hay texto "evidencias verificadas" en ninguna pantalla de evaluación |
| ✅ | La suma de porcentajes pilar + área se valida en 100% |
