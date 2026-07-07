using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Radzen;
using Objetivos.Web.Data;
using Objetivos.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options =>
    {
        if (builder.Environment.IsDevelopment())
        {
            options.DetailedErrors = true;
        }
    });
builder.Services.AddControllers();

// Radzen Services
builder.Services.AddRadzenComponents();

// EF Core SQLite with Factory.
// Ciclo 4: TODOS los servicios usan IDbContextFactory para evitar conflictos de lifetime.
// El registro Scoped<AppDbContext> fue ELIMINADO porque generaba dos contextos en paralelo
// (uno scoped, uno del factory) durante la misma request, con riesgo de inconsistencias.
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite($"Data Source={Path.Combine(builder.Environment.ContentRootPath, "objetivos.db")}"));

// Business Services
builder.Services.AddScoped<ICurrentUserService, SessionCurrentUserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RendimientoService>();
builder.Services.AddScoped<ObjetivoService>();
builder.Services.AddScoped<RevisionService>();
builder.Services.AddScoped<BitacoraService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<AdminPuestoService>();
builder.Services.AddScoped<ExportService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<CursoService>();
builder.Services.AddScoped<EvaluacionService>();
builder.Services.AddScoped<DataScopeService>();
builder.Services.AddScoped<NotificacionService>();
builder.Services.AddScoped<SeguimientoService>();
builder.Services.AddScoped<AutoevaluacionService>();
builder.Services.AddScoped<CalendarioService>();
builder.Services.AddScoped<ConfiguracionService>();
builder.Services.AddScoped<ValidacionObjetivoService>();
builder.Services.AddScoped<AdminPilarService>();
builder.Services.AddScoped<AdminSoftSkillService>();
builder.Services.AddScoped<AdminAreaService>();
builder.Services.AddScoped<AdminEscalaValoracionService>();
builder.Services.AddScoped<AdminEstadoService>();
builder.Services.AddScoped<AdminConfiguracionPlatformaService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

var app = builder.Build();

// Database Initialization and Seeding
using (var scope = app.Services.CreateScope())
{
    // Para inicialización usamos el factory una vez (no hay Scoped<AppDbContext> ya)
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var db = await factory.CreateDbContextAsync();
    
    var dbPath = Path.Combine(app.Environment.ContentRootPath, "objetivos.db");
    
    if (File.Exists(dbPath))
    {
        // Backup de contingencia si el archivo ya existe
        var backupPath = Path.Combine(app.Environment.ContentRootPath, $"objetivos_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db");
        File.Copy(dbPath, backupPath, true);
    }
    else
    {
        // Solo se genera la base de datos (y datos semilla) si el archivo de DB no existe
        await db.Database.MigrateAsync();
        await SeedData.InitializeAsync(db, app.Environment, app.Configuration);
    }
}

// PathBase: soporta despliegue bajo subruta (ej: /objetivos) vía proxy inverso.
// Configurar en appsettings.json: "PathBase": "/objetivos"
var pathBase = app.Configuration["PathBase"];
if (!string.IsNullOrEmpty(pathBase))
{
    app.UsePathBase(pathBase);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
else
{
    app.UseDeveloperExceptionPage();
}

// HTTPS redirect deshabilitado: el proxy inverso maneja TLS externamente.
// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
