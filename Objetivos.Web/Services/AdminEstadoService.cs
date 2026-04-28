using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;

namespace Objetivos.Web.Services;

public class AdminEstadoService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public AdminEstadoService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<EstadoObjetivoConfig>> ObtenerEstadosObjetivoAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.EstadosObjetivoConfig.OrderBy(e => e.Orden).ToListAsync();
    }

    public async Task<List<EstadoEvaluacionConfig>> ObtenerEstadosEvaluacionAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.EstadosEvaluacionConfig.OrderBy(e => e.Orden).ToListAsync();
    }

    public async Task<EstadoObjetivoConfig?> ObtenerEstadoObjetivoPorIdAsync(int id)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.EstadosObjetivoConfig.FindAsync(id);
    }

    public async Task<EstadoEvaluacionConfig?> ObtenerEstadoEvaluacionPorIdAsync(int id)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.EstadosEvaluacionConfig.FindAsync(id);
    }

    public async Task<bool> CrearEstadoObjetivoAsync(EstadoObjetivoConfig estado)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        if (await db.EstadosObjetivoConfig.AnyAsync(e => e.Slug.ToLower() == estado.Slug.ToLower()))
            return false;

        estado.CreadoEn = DateTime.UtcNow;
        db.EstadosObjetivoConfig.Add(estado);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CrearEstadoEvaluacionAsync(EstadoEvaluacionConfig estado)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        if (await db.EstadosEvaluacionConfig.AnyAsync(e => e.Slug.ToLower() == estado.Slug.ToLower()))
            return false;

        estado.CreadoEn = DateTime.UtcNow;
        db.EstadosEvaluacionConfig.Add(estado);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActualizarEstadoObjetivoAsync(EstadoObjetivoConfig estado)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var existing = await db.EstadosObjetivoConfig.FindAsync(estado.Id);
        if (existing == null) return false;

        existing.Nombre = estado.Nombre;
        existing.Slug = estado.Slug;
        existing.ColorHex = estado.ColorHex;
        existing.Orden = estado.Orden;
        existing.Activo = estado.Activo;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActualizarEstadoEvaluacionAsync(EstadoEvaluacionConfig estado)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var existing = await db.EstadosEvaluacionConfig.FindAsync(estado.Id);
        if (existing == null) return false;

        existing.Nombre = estado.Nombre;
        existing.Slug = estado.Slug;
        existing.ColorHex = estado.ColorHex;
        existing.Orden = estado.Orden;
        existing.Activo = estado.Activo;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarEstadoObjetivoAsync(int id)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var estado = await db.EstadosObjetivoConfig.FindAsync(id);
        if (estado == null) return false;

        estado.Activo = false;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarEstadoEvaluacionAsync(int id)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var estado = await db.EstadosEvaluacionConfig.FindAsync(id);
        if (estado == null) return false;

        estado.Activo = false;
        await db.SaveChangesAsync();
        return true;
    }
}
