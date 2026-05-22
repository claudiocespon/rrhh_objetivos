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
    private readonly DataScopeService _dataScope;

    public EvaluacionService(IDbContextFactory<AppDbContext> dbFactory, DataScopeService dataScope)
    {
        _dbFactory = dbFactory;
        _dataScope = dataScope;
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
                .Include(r => r.Objetivo)
                    .ThenInclude(o => o.Pilar)
                .Include(r => r.EscalaValoracion)
                .Where(r => r.Completada && r.Objetivo.EmpleadoId == empleadoPropio.Id)
                .OrderByDescending(r => r.FechaEvaluacion)
                .ToListAsync();
        }

        // 2. Fetch Pendientes (Solo Jefes) — usa DataScopeService centralizado
        if (currentUser.EsJefe)
        {
            // Revisiones periódicas pendientes
            var queryRev = db.RevisionesCuatrimestrales
                .Include(r => r.Objetivo)
                    .ThenInclude(o => o.Empleado)
                .Include(r => r.Objetivo)
                    .ThenInclude(o => o.Pilar)
                .Where(r => !r.Completada
                         && r.Objetivo.Empleado.Activo
                         && r.Objetivo.Estado != EstadoObjetivo.CANCELADO);

            // Objetivos para Evaluación Final
            var queryFinal = db.Objetivos
                .Include(o => o.Empleado)
                .Include(o => o.Pilar)
                .Include(o => o.Revisiones)
                .Include(o => o.EvaluacionFinal)
                .Where(o => o.Estado != EstadoObjetivo.CANCELADO
                         && o.Empleado.Activo
                         && o.EvaluacionFinal == null
                         && o.Revisiones.Any(r => r.Completada));

            // Aplicar scope centralizado
            queryRev = _dataScope.AplicarScope(queryRev, currentUser);
            queryFinal = _dataScope.AplicarScope(queryFinal, currentUser);

            data.Pendientes = await queryRev.OrderBy(r => r.Periodo).ToListAsync();
            data.FinalesPendientes = await queryFinal.ToListAsync();
        }

        return data;
    }
}
