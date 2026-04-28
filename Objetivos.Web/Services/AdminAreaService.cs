using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;

namespace Objetivos.Web.Services;

public class AdminAreaService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public AdminAreaService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<Area>> ObtenerTodosAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Areas.OrderBy(a => a.Nombre).ToListAsync();
    }

    public async Task<Area?> ObtenerPorIdAsync(int id)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Areas.FindAsync(id);
    }

    public async Task<bool> CrearAsync(Area area)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        if (await db.Areas.AnyAsync(a => a.Nombre.ToLower() == area.Nombre.ToLower()))
            return false;

        area.CreadoEn = DateTime.UtcNow;
        area.ActualizadoEn = DateTime.UtcNow;
        db.Areas.Add(area);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActualizarAsync(Area area)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var existing = await db.Areas.FindAsync(area.Id);
        if (existing == null) return false;

        existing.Nombre = area.Nombre;
        existing.Descripcion = area.Descripcion;
        existing.Activo = area.Activo;
        existing.ActualizadoEn = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var area = await db.Areas.FindAsync(id);
        if (area == null) return false;

        area.Activo = false;
        area.ActualizadoEn = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }
}
