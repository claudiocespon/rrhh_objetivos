using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;

namespace Objetivos.Web.Services;

public class ConfiguracionService(IDbContextFactory<AppDbContext> dbFactory)
{
    private Dictionary<string, string> _cache = new();
    private DateTime _cacheExpiry = DateTime.MinValue;
    private const int CACHE_MINUTES = 5;

    public async Task<string?> ObtenerConfiguracionAsync(string clave)
    {
        if (DateTime.UtcNow >= _cacheExpiry)
            await CargarCacheAsync();

        return _cache.TryGetValue(clave, out var valor) ? valor : null;
    }

    public async Task<int?> ObtenerConfiguracionIntAsync(string clave)
    {
        var valor = await ObtenerConfiguracionAsync(clave);
        if (valor == null || !int.TryParse(valor, out var result))
            return null;
        return result;
    }

    public async Task<bool?> ObtenerConfiguracionBoolAsync(string clave)
    {
        var valor = await ObtenerConfiguracionAsync(clave);
        if (valor == null || !bool.TryParse(valor, out var result))
            return null;
        return result;
    }

    public async Task ActualizarConfiguracionAsync(string clave, string valor)
    {
        using var db = await dbFactory.CreateDbContextAsync();
        var config = await db.ConfiguracionesPlataforma.FindAsync(clave);
        if (config != null)
        {
            config.Valor = valor;
            config.ActualizadoEn = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
        LimpiarCache();
    }

    public async Task<List<EscalaValoracion>> ObtenerEscalasActivasAsync()
    {
        using var db = await dbFactory.CreateDbContextAsync();
        return await db.EscalasValoracion
            .Where(e => e.Activo)
            .OrderBy(e => e.Orden)
            .ToListAsync();
    }

    public async Task<List<EstadoObjetivoConfig>> ObtenerEstadosObjetivoActivosAsync()
    {
        using var db = await dbFactory.CreateDbContextAsync();
        return await db.EstadosObjetivoConfig
            .Where(e => e.Activo)
            .OrderBy(e => e.Orden)
            .ToListAsync();
    }

    public async Task<List<EstadoEvaluacionConfig>> ObtenerEstadosEvaluacionActivosAsync()
    {
        using var db = await dbFactory.CreateDbContextAsync();
        return await db.EstadosEvaluacionConfig
            .Where(e => e.Activo)
            .OrderBy(e => e.Orden)
            .ToListAsync();
    }

    public async Task<EstadoObjetivoConfig?> ObtenerEstadoObjetivoBySlugAsync(string slug)
    {
        using var db = await dbFactory.CreateDbContextAsync();
        return await db.EstadosObjetivoConfig
            .FirstOrDefaultAsync(e => e.Slug == slug && e.Activo);
    }

    public async Task<EstadoEvaluacionConfig?> ObtenerEstadoEvaluacionBySlugAsync(string slug)
    {
        using var db = await dbFactory.CreateDbContextAsync();
        return await db.EstadosEvaluacionConfig
            .FirstOrDefaultAsync(e => e.Slug == slug && e.Activo);
    }

    private async Task CargarCacheAsync()
    {
        using var db = await dbFactory.CreateDbContextAsync();
        var configs = await db.ConfiguracionesPlataforma.ToListAsync();
        _cache = configs.ToDictionary(c => c.Clave, c => c.Valor);
        _cacheExpiry = DateTime.UtcNow.AddMinutes(CACHE_MINUTES);
    }

    private void LimpiarCache()
    {
        _cache.Clear();
        _cacheExpiry = DateTime.MinValue;
    }
}
