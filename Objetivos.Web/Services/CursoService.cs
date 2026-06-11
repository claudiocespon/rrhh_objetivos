using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;

namespace Objetivos.Web.Services;

public class CursoService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public CursoService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // ── Catálogo ──────────────────────────────────────────────────────────────

    public async Task<List<Curso>> GetCursosAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Cursos.OrderBy(c => c.Nombre).ToListAsync();
    }

    public async Task<Curso?> GetByIdAsync(int id)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Cursos.FindAsync(id);
    }

    public async Task<bool> CrearCursoAsync(Curso curso)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        try
        {
            db.Cursos.Add(curso);
            return await db.SaveChangesAsync() > 0;
        }
        catch { return false; }
    }

    public async Task<bool> ActualizarCursoAsync(Curso curso)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        try
        {
            db.Entry(curso).State = EntityState.Modified;
            return await db.SaveChangesAsync() > 0;
        }
        catch { return false; }
    }

    public async Task<bool> EliminarCursoAsync(int id)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        try
        {
            var curso = await db.Cursos.FindAsync(id);
            if (curso == null) return false;
            db.Cursos.Remove(curso);
            return await db.SaveChangesAsync() > 0;
        }
        catch { return false; }
    }

    // ── Asignaciones ─────────────────────────────────────────────────────────

    public async Task<List<CursoAsignacion>> GetAsignacionesPorEmailAsync(string email)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var emp = await db.Usuarios.FirstOrDefaultAsync(e => e.Email.ToLower() == email.ToLower() && e.Activo);
        if (emp == null) return [];
        return await db.CursoAsignaciones
            .Include(ca => ca.Curso)
            .Where(ca => ca.UsuarioId == emp.Id)
            .OrderByDescending(ca => ca.FechaAsignacion)
            .ToListAsync();
    }

    public async Task<List<Usuario>> GetEmpleadosActivosAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Usuarios.Where(e => e.Activo).OrderBy(e => e.Apellido).ToListAsync();
    }

    public async Task<List<CursoAsignacion>> GetAsignacionesDeEmpleadoAsync(int usuarioId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.CursoAsignaciones
            .Include(ca => ca.Curso)
            .Where(ca => ca.UsuarioId == usuarioId)
            .OrderByDescending(ca => ca.FechaAsignacion)
            .ToListAsync();
    }

    public async Task<List<CursoAsignacion>> GetAsignacionesDeCursoAsync(int cursoId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.CursoAsignaciones
            .Include(ca => ca.Usuario)
            .Where(ca => ca.CursoId == cursoId)
            .OrderBy(ca => ca.Usuario.Apellido)
            .ToListAsync();
    }

    /// <summary>
    /// Asigna un curso a un usuario. Retorna false si ya estaba asignado (constraint único).
    /// </summary>
    public async Task<(bool Ok, bool Duplicado)> AsignarCursoAsync(int cursoId, int usuarioId, int asignadoPorId, string? notas = null)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        try
        {
            var ya = await db.CursoAsignaciones
                .AnyAsync(ca => ca.CursoId == cursoId && ca.UsuarioId == usuarioId);
            if (ya) return (false, true);

            db.CursoAsignaciones.Add(new CursoAsignacion
            {
                CursoId = cursoId,
                UsuarioId = usuarioId,
                AsignadoPorId = asignadoPorId,
                FechaAsignacion = DateTime.UtcNow,
                Notas = notas
            });
            return (await db.SaveChangesAsync() > 0, false);
        }
        catch { return (false, false); }
    }

    public async Task<bool> MarcarCompletadoAsync(int asignacionId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        try
        {
            return await db.CursoAsignaciones
                .Where(ca => ca.Id == asignacionId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(ca => ca.Completado, true)
                    .SetProperty(ca => ca.FechaCompletado, DateTime.UtcNow)) > 0;
        }
        catch { return false; }
    }

    public async Task<bool> RevocarAsignacionAsync(int asignacionId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        try
        {
            return await db.CursoAsignaciones
                .Where(ca => ca.Id == asignacionId)
                .ExecuteDeleteAsync() > 0;
        }
        catch { return false; }
    }
}
