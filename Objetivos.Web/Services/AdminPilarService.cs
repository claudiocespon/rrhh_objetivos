using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;

namespace Objetivos.Web.Services;

public class AdminPilarService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public AdminPilarService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<Pilar>> ObtenerTodosAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Pilares.Include(p => p.Area).OrderBy(p => p.Orden).ToListAsync();
    }

    public async Task<Pilar?> ObtenerPorIdAsync(int id)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Pilares.FindAsync(id);
    }

    public async Task<bool> CrearAsync(Pilar pilar)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        if (await db.Pilares.AnyAsync(p => p.Nombre.ToLower() == pilar.Nombre.ToLower()))
            return false;

        pilar.CreadoEn = DateTime.UtcNow;
        pilar.ActualizadoEn = DateTime.UtcNow;
        pilar.AreaId = pilar.AreaId;
        pilar.EsObligatorio = pilar.EsObligatorio;
        db.Pilares.Add(pilar);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActualizarAsync(Pilar pilar)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var existing = await db.Pilares.FindAsync(pilar.Id);
        if (existing == null) return false;

        existing.Nombre = pilar.Nombre;
        existing.Descripcion = pilar.Descripcion;
        existing.ColorHex = pilar.ColorHex;
        existing.Activo = pilar.Activo;
        existing.Orden = pilar.Orden;
        existing.AreaId = pilar.AreaId;
        existing.EsObligatorio = pilar.EsObligatorio;
        existing.ActualizadoEn = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var pilar = await db.Pilares.FindAsync(id);
        if (pilar == null) return false;

        pilar.Activo = false;
        pilar.ActualizadoEn = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReordenarAsync(int id, int nuevoOrden)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var pilar = await db.Pilares.FindAsync(id);
        if (pilar == null) return false;

        pilar.Orden = nuevoOrden;
        pilar.ActualizadoEn = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }
}
