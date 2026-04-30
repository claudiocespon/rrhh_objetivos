using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;
using System.Text.Json;

namespace Objetivos.Web.Services;

public class AutoevaluacionPageData
{
    public List<Autoevaluacion> Personal { get; set; } = [];
    public List<Autoevaluacion> Equipo { get; set; } = [];
}

public class AutoevaluacionService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public AutoevaluacionService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<AutoevaluacionPageData> GetAutoevaluacionesAsync(ICurrentUserService user)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var data = new AutoevaluacionPageData();

        var empleadoPropio = await db.Empleados
            .FirstOrDefaultAsync(e => e.Email.ToLower() == user.Email.ToLower() && e.Activo);

        // Personal
        if (empleadoPropio != null)
        {
            data.Personal = await db.Autoevaluaciones
                .Include(ae => ae.Objetivo)
                    .ThenInclude(o => o.Empleado)
                .Include(ae => ae.Objetivo)
                    .ThenInclude(o => o.SoftSkill1)
                .Include(ae => ae.Objetivo)
                    .ThenInclude(o => o.SoftSkill2)
                .Include(ae => ae.EscalaValoracionScore)
                .Include(ae => ae.SoftSkill1EscalaValoracion)
                .Include(ae => ae.SoftSkill2EscalaValoracion)
                .Where(ae => ae.Objetivo.EmpleadoId == empleadoPropio.Id)
                .OrderByDescending(ae => ae.FechaAutoevaluacion)
                .ToListAsync();
        }

        // Equipo (solo jefes)
        if (user.EsJefe)
        {
            var query = db.Autoevaluaciones
                .Include(ae => ae.Objetivo)
                    .ThenInclude(o => o.Empleado)
                .Include(ae => ae.Objetivo)
                    .ThenInclude(o => o.SoftSkill1)
                .Include(ae => ae.Objetivo)
                    .ThenInclude(o => o.SoftSkill2)
                .Include(ae => ae.EscalaValoracionScore)
                .Include(ae => ae.SoftSkill1EscalaValoracion)
                .Include(ae => ae.SoftSkill2EscalaValoracion)
                .AsQueryable();

            if (!user.EsSuperusuario && user.Rol != "RRHH" && user.Rol != "DIRECTOR_GENERAL")
            {
                query = user.Rol == "DIRECTOR"
                    ? query.Where(ae => ae.Objetivo.Empleado.AreaId == user.AreaId)
                    : query.Where(ae => ae.Objetivo.Empleado.JefeId == user.UsuarioId);
            }

            data.Equipo = await query
                .OrderByDescending(ae => ae.FechaAutoevaluacion)
                .ToListAsync();
        }

        return data;
    }

    public async Task<Objetivo?> GetObjetivoParaAutoevAsync(int objetivoId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Objetivos
            .Include(o => o.SoftSkill1)
            .Include(o => o.SoftSkill2)
            .FirstOrDefaultAsync(o => o.Id == objetivoId);
    }

    public async Task<Autoevaluacion?> GetExistingAsync(int objetivoId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Autoevaluaciones.FirstOrDefaultAsync(ae => ae.ObjetivoId == objetivoId);
    }

    public async Task<int?> GetEmpleadoIdByEmailAsync(string email)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var emp = await db.Empleados.FirstOrDefaultAsync(e => e.Email.ToLower() == email.ToLower() && e.Activo);
        return emp?.Id;
    }

    public async Task<bool> GuardarAutoevaluacionAsync(Autoevaluacion ae, List<string> evidencias, List<string> adjuntos)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        try
        {
            ae.EvidenciasMencionadasJson = JsonSerializer.Serialize(evidencias);
            ae.ArchivosAdjuntosJson = JsonSerializer.Serialize(adjuntos);
            ae.FechaAutoevaluacion = DateTime.UtcNow;

            if (ae.Id == 0)
                db.Autoevaluaciones.Add(ae);
            else
                db.Entry(ae).State = EntityState.Modified;

            return await db.SaveChangesAsync() > 0;
        }
        catch { return false; }
    }
}
