using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;

namespace Objetivos.Web.Services;

public class AdminSoftSkillService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public AdminSoftSkillService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<SoftSkill>> ObtenerTodosAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.SoftSkills.OrderBy(s => s.Orden).ToListAsync();
    }

    public async Task<SoftSkill?> ObtenerPorIdAsync(int id)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.SoftSkills.FindAsync(id);
    }

    public async Task<bool> CrearAsync(SoftSkill skill)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        if (await db.SoftSkills.AnyAsync(s => s.Nombre.ToLower() == skill.Nombre.ToLower()))
            return false;

        skill.CreadoEn = DateTime.UtcNow;
        skill.ActualizadoEn = DateTime.UtcNow;
        db.SoftSkills.Add(skill);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActualizarAsync(SoftSkill skill)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var existing = await db.SoftSkills.FindAsync(skill.Id);
        if (existing == null) return false;

        existing.Nombre = skill.Nombre;
        existing.Descripcion = skill.Descripcion;
        existing.Activo = skill.Activo;
        existing.Orden = skill.Orden;
        existing.ActualizadoEn = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var skill = await db.SoftSkills.FindAsync(id);
        if (skill == null) return false;

        skill.Activo = false;
        skill.ActualizadoEn = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReordenarAsync(int id, int nuevoOrden)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var skill = await db.SoftSkills.FindAsync(id);
        if (skill == null) return false;

        skill.Orden = nuevoOrden;
        skill.ActualizadoEn = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }
}
