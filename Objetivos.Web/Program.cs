using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Radzen;
using Objetivos.Web.Data;
using Objetivos.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();

// Radzen Services
builder.Services.AddRadzenComponents();

// EF Core SQLite with Factory
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite($"Data Source={Path.Combine(builder.Environment.ContentRootPath, "objetivos.db")}"));

// Register AppDbContext as Scoped using the factory to avoid lifetime conflicts
builder.Services.AddScoped(p => p.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

// Business Services
builder.Services.AddScoped<ICurrentUserService, SessionCurrentUserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RendimientoService>();
builder.Services.AddScoped<ObjetivoService>();
builder.Services.AddScoped<RevisionService>();
builder.Services.AddScoped<BitacoraService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<UsuarioService>();
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
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

var app = builder.Build();

// Database Initialization and Seeding
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // H-10: Estrategia de inicialización robusta (Usa migraciones)
    await db.Database.MigrateAsync();
    await SeedData.InitializeAsync(db, app.Environment, app.Configuration);
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

// HTTPS redirect deshabilitado: el proxy inverso maneja TLS externamente.
// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
