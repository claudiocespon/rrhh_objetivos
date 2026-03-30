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

var app = builder.Build();

// Database Initialization and Seeding
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // H-10: Estrategia de inicialización robusta (Garantiza creación de esquema si no existe)
    await db.Database.EnsureCreatedAsync();
    await SeedData.InitializeAsync(db, app.Environment, app.Configuration);
}




// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
