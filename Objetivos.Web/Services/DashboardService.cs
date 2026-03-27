using Objetivos.Web.Data;
using Objetivos.Web.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Domain.Entities;

namespace Objetivos.Web.Services;

public class DashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<RoleDashboardData> GetDashboardDataAsync(ICurrentUserService currentUser)
    {
        var result = new RoleDashboardData();
        var hoy = DateTime.Today;
        var en30Dias = hoy.AddDays(30);

        // 1. Fetch Personal Data (for Colaborador, or for Jefe/Gerente's own objectives)
        var empleadoPropio = await _db.Empleados.FirstOrDefaultAsync(e => e.Email.ToLower() == currentUser.Email.ToLower() && e.Activo);
        if (empleadoPropio != null)
        {
            var misObjetivos = await _db.Objetivos
                .Where(o => o.EmpleadoId == empleadoPropio.Id && o.Anio == hoy.Year)
                .ToListAsync();

            result.Personal = new DashboardData
            {
                TotalObjetivos = misObjetivos.Count(o => o.Estado != EstadoObjetivo.CANCELADO),
                EnCurso = misObjetivos.Count(o => o.Estado == EstadoObjetivo.ACTIVO),
                VencenPronto = misObjetivos.Count(o => o.Estado == EstadoObjetivo.ACTIVO && o.Deadline <= en30Dias),
                PendientesRevision = await _db.RevisionesCuatrimestrales
                    .CountAsync(r => !r.Completada && r.Objetivo.EmpleadoId == empleadoPropio.Id && r.Anio == hoy.Year && r.Objetivo.Estado != EstadoObjetivo.CANCELADO)
            };
        }

        // 2. Fetch Team/Org Data
        bool canSeeAll = currentUser.Rol == "DIRECTOR_GENERAL" || currentUser.Rol == "RRHH" || currentUser.EsSuperusuario;
        
        if (canSeeAll || currentUser.Rol == "DIRECTOR" || currentUser.EsJefe)
        {
            IQueryable<Objetivo> query = _db.Objetivos.Include(o => o.Empleado).Where(o => o.Anio == hoy.Year);
            IQueryable<RevisionCuatrimestral> revisionQuery = _db.RevisionesCuatrimestrales.Where(r => r.Anio == hoy.Year && r.Objetivo.Estado != EstadoObjetivo.CANCELADO && !r.Completada);

            if (canSeeAll)
            {
                // Sees everything
            }
            else if (currentUser.Rol == "DIRECTOR")
            {
                query = query.Where(o => o.Empleado.AreaId == currentUser.AreaId);
                revisionQuery = revisionQuery.Where(r => r.Objetivo.Empleado.AreaId == currentUser.AreaId);
            }
            else if (currentUser.EsJefe)
            {
                query = query.Where(o => o.Empleado.JefeId == currentUser.UsuarioId);
                revisionQuery = revisionQuery.Where(r => r.Objetivo.Empleado.JefeId == currentUser.UsuarioId);
            }

            var teamObjetivos = await query.ToListAsync();
            var pendingTeamRevision = await revisionQuery.CountAsync();

            result.Equipo = new DashboardData
            {
                TotalObjetivos = teamObjetivos.Count(o => o.Estado != EstadoObjetivo.CANCELADO),
                EnCurso = teamObjetivos.Count(o => o.Estado == EstadoObjetivo.ACTIVO),
                VencenPronto = teamObjetivos.Count(o => o.Estado == EstadoObjetivo.ACTIVO && o.Deadline <= en30Dias),
                PendientesRevision = pendingTeamRevision
            };
        }

        return result;
    }
}

public class RoleDashboardData
{
    public DashboardData? Personal { get; set; }
    public DashboardData? Equipo { get; set; }
}

public class DashboardData
{
    public int TotalObjetivos { get; set; }
    public int EnCurso { get; set; }
    public int VencenPronto { get; set; }
    public int PendientesRevision { get; set; }
}
