using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;

namespace Objetivos.Web.Services;

public class AdminEscalaValoracionService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public AdminEscalaValoracionService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<EscalaValoracion>> ObtenerTodosAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.EscalasValoracion.OrderBy(e => e.Orden).ToListAsync();
    }

    public async Task<EscalaValoracion?> ObtenerPorIdAsync(int id)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.EscalasValoracion.FindAsync(id);
    }

    public async Task<bool> CrearAsync(EscalaValoracion escala)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        if (await db.EscalasValoracion.AnyAsync(e => e.Etiqueta.ToLower() == escala.Etiqueta.ToLower()))
            return false;

        escala.CreadoEn = DateTime.UtcNow;
        escala.ActualizadoEn = DateTime.UtcNow;
        db.EscalasValoracion.Add(escala);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActualizarAsync(EscalaValoracion escala)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var existing = await db.EscalasValoracion.FindAsync(escala.Id);
        if (existing == null) return false;

        existing.Etiqueta = escala.Etiqueta;
        existing.ValorNumerico = escala.ValorNumerico;
        existing.Orden = escala.Orden;
        existing.Activo = escala.Activo;
        existing.ActualizadoEn = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var escala = await db.EscalasValoracion.FindAsync(id);
        if (escala == null) return false;

        escala.Activo = false;
        escala.ActualizadoEn = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReordenarAsync(int id, int nuevoOrden)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var escala = await db.EscalasValoracion.FindAsync(id);
        if (escala == null) return false;

        escala.Orden = nuevoOrden;
        escala.ActualizadoEn = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }
}
