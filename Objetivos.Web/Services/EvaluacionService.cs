using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;
using Objetivos.Web.Domain.Enums;

namespace Objetivos.Web.Services;

public class EvaluacionPageData
{
    public List<RevisionCuatrimestral> Pendientes { get; set; } = new();
    public List<Objetivo> FinalesPendientes { get; set; } = new();
    public List<RevisionCuatrimestral> Recibidas { get; set; } = new();
}

public class EvaluacionService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public EvaluacionService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<EvaluacionPageData> GetEvaluacionDataAsync(ICurrentUserService currentUser)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var data = new EvaluacionPageData();

        var empleadoPropio = await db.Empleados.FirstOrDefaultAsync(e => e.Email.ToLower() == currentUser.Email.ToLower() && e.Activo);

        // 1. Fetch Mis Evaluaciones Recibidas (Completadas)
        if (empleadoPropio != null)
        {
            data.Recibidas = await db.RevisionesCuatrimestrales
                .Include(r => r.Objetivo)
                    .ThenInclude(o => o.Empleado)
                .Where(r => r.Completada && r.Objetivo.EmpleadoId == empleadoPropio.Id)
                .OrderByDescending(r => r.FechaEvaluacion)
                .ToListAsync();
        }

        // 2. Fetch Pendientes (Solo Jefes)
        if (currentUser.EsJefe)
        {
            // Revisiones Cuatrimestrales
            var queryRev = db.RevisionesCuatrimestrales
                .Include(r => r.Objetivo)
                    .ThenInclude(o => o.Empleado)
                .Where(r => !r.Completada && r.Objetivo.Empleado.Activo);

            // Objetivos para Evaluación Final (RN-03)
            var queryFinal = db.Objetivos
                .Include(o => o.Empleado)
                .Include(o => o.Revisiones)
                .Include(o => o.EvaluacionFinal)
                .Where(o => o.Estado == EstadoObjetivo.ACTIVO 
                         && o.Empleado.Activo 
                         && o.EvaluacionFinal == null
                         && o.Revisiones.Count(r => r.Completada) >= 3
                         && DateTime.Today >= o.Deadline);

            bool canSeeAll = currentUser.Rol == "DIRECTOR_GENERAL" || currentUser.Rol == "RRHH" || currentUser.EsSuperusuario;
            
            if (!canSeeAll)
            {
                if (currentUser.Rol == "DIRECTOR")
                {
                    queryRev = queryRev.Where(r => r.Objetivo.Empleado.AreaId == currentUser.AreaId);
                    queryFinal = queryFinal.Where(o => o.Empleado.AreaId == currentUser.AreaId);
                }
                else
                {
                    queryRev = queryRev.Where(r => r.Objetivo.Empleado.JefeId == currentUser.UsuarioId);
                    queryFinal = queryFinal.Where(o => o.Empleado.JefeId == currentUser.UsuarioId);
                }
            }

            data.Pendientes = await queryRev.OrderBy(r => r.Periodo).ToListAsync();
            data.FinalesPendientes = await queryFinal.ToListAsync();
        }

        return data;
    }
}
