using Objetivos.Web.Data;
using Objetivos.Web.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Domain.Entities;

namespace Objetivos.Web.Services;

public class DashboardService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly DataScopeService _dataScope;

    public DashboardService(AppDbContext db, ICurrentUserService currentUser, DataScopeService dataScope)
    {
        _db = db;
        _currentUser = currentUser;
        _dataScope = dataScope;
    }

    public async Task<RoleDashboardData> GetDashboardDataAsync()
    {
        var result = new RoleDashboardData();
        var hoy = DateTime.Today;
        var en30Dias = hoy.AddDays(30);

        // 1. Fetch Personal Data (for Colaborador, or for Jefe/Gerente's own objectives)
        var empleadoPropio = await _db.Empleados.FirstOrDefaultAsync(e => e.Email.ToLower() == _currentUser.Email.ToLower() && e.Activo);
        if (empleadoPropio != null)
        {
            var misObjetivos = await _db.Objetivos
                .Where(o => o.EmpleadoId == empleadoPropio.Id && o.Anio == hoy.Year)
                .ToListAsync();

            result.Personal = new DashboardData
            {
                TotalObjetivos = misObjetivos.Count(o => o.Estado != EstadoObjetivo.CANCELADO),
                EnCurso = misObjetivos.Count(o => o.Estado == EstadoObjetivo.ACTIVO),
                EnRiesgo = misObjetivos.Count(o => o.Estado == EstadoObjetivo.EN_RIESGO),
                Completados = misObjetivos.Count(o => o.Estado == EstadoObjetivo.COMPLETADO),
                VencenPronto = misObjetivos.Count(o => o.Estado == EstadoObjetivo.ACTIVO && o.Deadline <= en30Dias),
                PendientesRevision = await _db.RevisionesCuatrimestrales
                    .CountAsync(r => !r.Completada && r.Objetivo.EmpleadoId == empleadoPropio.Id && r.Anio == hoy.Year && r.Objetivo.Estado != EstadoObjetivo.CANCELADO)
            };
        }

        // 2. Fetch Team/Org Data using centralized scope
        if (_currentUser.EsJefe || _dataScope.PuedeVerTodo(_currentUser))
        {
            IQueryable<Objetivo> query = _db.Objetivos.Include(o => o.Empleado).Where(o => o.Anio == hoy.Year);
            IQueryable<RevisionCuatrimestral> revisionQuery = _db.RevisionesCuatrimestrales.Where(r => r.Anio == hoy.Year && r.Objetivo.Estado != EstadoObjetivo.CANCELADO && !r.Completada);

            query = _dataScope.AplicarScope(query, _currentUser);
            revisionQuery = _dataScope.AplicarScope(revisionQuery, _currentUser);

            var teamObjetivos = await query.ToListAsync();
            var pendingTeamRevision = await revisionQuery.CountAsync();

            result.Equipo = new DashboardData
            {
                TotalObjetivos = teamObjetivos.Count(o => o.Estado != EstadoObjetivo.CANCELADO),
                EnCurso = teamObjetivos.Count(o => o.Estado == EstadoObjetivo.ACTIVO),
                EnRiesgo = teamObjetivos.Count(o => o.Estado == EstadoObjetivo.EN_RIESGO),
                Completados = teamObjetivos.Count(o => o.Estado == EstadoObjetivo.COMPLETADO),
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
    public int EnRiesgo { get; set; }
    public int Completados { get; set; }
    public int VencenPronto { get; set; }
    public int PendientesRevision { get; set; }
}
