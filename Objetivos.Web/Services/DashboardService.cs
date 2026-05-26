using Objetivos.Web.Data;
using Objetivos.Web.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Domain.Entities;

namespace Objetivos.Web.Services;

public class DashboardService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly ICurrentUserService _currentUser;
    private readonly DataScopeService _dataScope;
    private readonly ConfiguracionService _configuracion;

    public DashboardService(IDbContextFactory<AppDbContext> dbFactory, ICurrentUserService currentUser, DataScopeService dataScope, ConfiguracionService configuracion)
    {
        _dbFactory = dbFactory;
        _currentUser = currentUser;
        _dataScope = dataScope;
        _configuracion = configuracion;
    }

    public async Task<RoleDashboardData> GetDashboardDataAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var result = new RoleDashboardData();
        var hoy = DateTime.Today;

        // Obtener días próximo vencimiento de configuración (default 7 si no está definido)
        var diasProximoVencimiento = await _configuracion.ObtenerConfiguracionIntAsync("dias_proximo_vencimiento") ?? 7;
        var proximaFecha = hoy.AddDays(diasProximoVencimiento);

        // 1. Fetch Personal Data (for Colaborador, or for Jefe/Gerente's own objectives)
        var empleadoPropio = await db.Empleados.FirstOrDefaultAsync(e => e.Email.ToLower() == _currentUser.Email.ToLower() && e.Activo);
        if (empleadoPropio != null)
        {
            var misObjetivos = await db.Objetivos
                .Include(o => o.Revisiones)
                .Include(o => o.EvaluacionFinal)
                .Where(o => o.EmpleadoId == empleadoPropio.Id && o.Anio == hoy.Year)
                .ToListAsync();

            result.Personal = new DashboardData
            {
                TotalObjetivos = misObjetivos.Count(o => o.Estado != EstadoObjetivo.CANCELADO),
                EnCurso = misObjetivos.Count(o => o.Estado == EstadoObjetivo.ACTIVO),
                EnRiesgo = misObjetivos.Count(o => o.Estado == EstadoObjetivo.EN_RIESGO),
                Completados = misObjetivos.Count(o => o.Estado == EstadoObjetivo.COMPLETADO),
                 VencenPronto = misObjetivos.Count(o => (o.Estado == EstadoObjetivo.ACTIVO || o.Estado == EstadoObjetivo.EN_RIESGO) && o.Deadline <= proximaFecha),
                PendientesRevision = misObjetivos.Count(o => o.Estado != EstadoObjetivo.CANCELADO && (o.Revisiones.Any(r => !r.Completada) || (o.Revisiones.Any(r => r.Completada) && o.EvaluacionFinal == null))),
                EvaluacionesFinalizadas = misObjetivos.Count(o => o.EvaluacionFinal != null)
            };
        }

        // 2. Fetch Team/Org Data using centralized scope
        if (_currentUser.EsJefe || _dataScope.PuedeVerTodo(_currentUser))
        {
            IQueryable<Objetivo> query = db.Objetivos
                .Include(o => o.Empleado)
                .Include(o => o.Revisiones)
                .Include(o => o.EvaluacionFinal)
                .Where(o => o.Anio == hoy.Year && o.Empleado.Activo);

            query = _dataScope.AplicarScope(query, _currentUser);

            var teamObjetivos = await query.ToListAsync();

            result.Equipo = new DashboardData
            {
                TotalObjetivos = teamObjetivos.Count(o => o.Estado != EstadoObjetivo.CANCELADO),
                EnCurso = teamObjetivos.Count(o => o.Estado == EstadoObjetivo.ACTIVO),
                EnRiesgo = teamObjetivos.Count(o => o.Estado == EstadoObjetivo.EN_RIESGO),
                Completados = teamObjetivos.Count(o => o.Estado == EstadoObjetivo.COMPLETADO),
                VencenPronto = teamObjetivos.Count(o => (o.Estado == EstadoObjetivo.ACTIVO || o.Estado == EstadoObjetivo.EN_RIESGO) && o.Deadline <= proximaFecha),
                PendientesRevision = teamObjetivos.Count(o => o.Estado != EstadoObjetivo.CANCELADO && (o.Revisiones.Any(r => !r.Completada) || (o.Revisiones.Any(r => r.Completada) && o.EvaluacionFinal == null))),
                EvaluacionesFinalizadas = teamObjetivos.Count(o => o.EvaluacionFinal != null)
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
    public int EvaluacionesFinalizadas { get; set; }
}
