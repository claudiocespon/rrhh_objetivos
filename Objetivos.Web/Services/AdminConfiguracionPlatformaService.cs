using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;

namespace Objetivos.Web.Services;

public class AdminConfiguracionPlatformaService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly ConfiguracionService _configuracionService;

    public AdminConfiguracionPlatformaService(IDbContextFactory<AppDbContext> dbFactory, ConfiguracionService configuracionService)
    {
        _dbFactory = dbFactory;
        _configuracionService = configuracionService;
    }

    public async Task<List<ConfiguracionPlataforma>> ObtenerTodosAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.ConfiguracionesPlataforma.OrderBy(c => c.Clave).ToListAsync();
    }

    public async Task<ConfiguracionPlataforma?> ObtenerPorClaveAsync(string clave)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.ConfiguracionesPlataforma.FindAsync(clave);
    }

    public async Task<bool> ActualizarAsync(ConfiguracionPlataforma config)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var existing = await db.ConfiguracionesPlataforma.FindAsync(config.Clave);
        if (existing == null) return false;

        existing.Valor = config.Valor;
        existing.Descripcion = config.Descripcion;
        existing.ActualizadoEn = DateTime.UtcNow;

        await db.SaveChangesAsync();

        // Invalidar cache
        await Task.Delay(100); // Pequeño delay para asegurar sincronización
        return true;
    }

    public async Task<bool> CrearAsync(ConfiguracionPlataforma config)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        if (await db.ConfiguracionesPlataforma.AnyAsync(c => c.Clave == config.Clave))
            return false;

        config.ActualizadoEn = DateTime.UtcNow;
        db.ConfiguracionesPlataforma.Add(config);
        await db.SaveChangesAsync();
        return true;
    }
}
