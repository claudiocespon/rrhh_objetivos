using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;
using Objetivos.Web.Domain.Enums;

namespace Objetivos.Web.Services;

public class EmpleadoConPromedio
{
    public Usuario Usuario { get; set; } = null!;
    public double Promedio { get; set; }
}

public class EmpleadoDetalleData
{
    public Usuario Usuario { get; set; } = null!;
    public List<RadarPilarItem> RadarData { get; set; } = [];
    public Dictionary<int, double> Ponderados { get; set; } = [];
}

public class RadarPilarItem
{
    public string Pilar { get; set; } = "";
    public double Score { get; set; }
}

public class SeguimientoService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly DataScopeService _dataScope;

    public SeguimientoService(IDbContextFactory<AppDbContext> dbFactory, DataScopeService dataScope)
    {
        _dbFactory = dbFactory;
        _dataScope = dataScope;
    }

    /// <summary>
    /// Carga el usuario propio del usuario con su promedio calculado en memoria (sin N+1).
    /// </summary>
    public async Task<EmpleadoConPromedio?> GetEmpleadoPersonalAsync(string email, int anio)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var emp = await db.Usuarios
            .Include(e => e.Area)
            .Include(e => e.Puesto)
            .FirstOrDefaultAsync(e => e.Email.ToLower() == email.ToLower() && e.Activo);
        if (emp == null) return null;

        var objetivos = await db.Objetivos
            .Include(o => o.Revisiones)
            .Include(o => o.EvaluacionFinal)
            .Where(o => o.UsuarioId == emp.Id && o.Anio == anio && o.Estado != EstadoObjetivo.CANCELADO)
            .ToListAsync();

        var scores = objetivos.Select(RendimientoService.CalcularPonderadoStatic).Where(v => v > 0).ToList();
        double promedio = scores.Any() ? scores.Average() : 0;

        return new EmpleadoConPromedio { Usuario = emp, Promedio = promedio };
    }

    /// <summary>
    /// M-03 / Etapa 6: Carga usuarios del equipo y calcula promedios en batch (una sola query).
    /// Elimina el N+1 de PromedioGeneralAsync por usuario.
    /// </summary>
    public async Task<List<EmpleadoConPromedio>> GetEmpleadosEquipoConPromediosAsync(ICurrentUserService user, int anio)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        IQueryable<Usuario> query = db.Usuarios
            .Include(e => e.Area)
            .Include(e => e.Puesto)
            .Where(e => e.Activo);
        query = _dataScope.AplicarScope(query, user);

        var usuarios = await query.ToListAsync();

        if (!usuarios.Any())
            return [];

        var ids = usuarios.Select(e => e.Id).ToList();

        // Batch: cargar todos los objetivos del equipo con revisiones y evaluaciones en UNA query
        var objetivos = await db.Objetivos
            .Include(o => o.Revisiones)
            .Include(o => o.EvaluacionFinal)
            .Where(o => ids.Contains(o.UsuarioId) && o.Anio == anio && o.Estado != EstadoObjetivo.CANCELADO)
            .ToListAsync();

        var objetivosPorEmpleado = objetivos.GroupBy(o => o.UsuarioId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return usuarios.Select(emp =>
        {
            double promedio = 0;
            if (objetivosPorEmpleado.TryGetValue(emp.Id, out var objs) && objs.Any())
            {
                var scores = objs.Select(RendimientoService.CalcularPonderadoStatic).Where(v => v > 0).ToList();
                promedio = scores.Any() ? scores.Average() : 0;
            }
            return new EmpleadoConPromedio { Usuario = emp, Promedio = promedio };
        }).ToList();
    }

    /// <summary>
    /// M-02 / Etapa 6: Carga el detalle completo del usuario con radar y ponderados en memoria.
    /// Elimina el N+1 de RendimientoPorPilarAsync y CalcularPonderadoAsync por objetivo.
    /// </summary>
    public async Task<EmpleadoDetalleData?> GetEmpleadoDetalleCompletoAsync(int usuarioId, int anio)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        var usuario = await db.Usuarios
            .Include(e => e.Area)
            .Include(e => e.Puesto)
            .Include(e => e.Objetivos.Where(o => o.Anio == anio))
                .ThenInclude(o => o.Pilar)
            .Include(e => e.Objetivos.Where(o => o.Anio == anio))
                .ThenInclude(o => o.Revisiones)
            .Include(e => e.Objetivos.Where(o => o.Anio == anio))
                .ThenInclude(o => o.EvaluacionFinal)
            .FirstOrDefaultAsync(e => e.Id == usuarioId);

        if (usuario == null) return null;

        var pilares = await db.Pilares.ToListAsync();

        // Calcular radar en memoria (sin N+1)
        var radarData = pilares.Select(pilar =>
        {
            var obj = usuario.Objetivos.FirstOrDefault(o => o.PilarId == pilar.Id && o.Estado != EstadoObjetivo.CANCELADO);
            return new RadarPilarItem
            {
                Pilar = pilar.Nombre.Replace("_", " "),
                Score = obj != null ? RendimientoService.CalcularPonderadoStatic(obj) : 0
            };
        }).ToList();

        // Calcular ponderados en memoria (sin N+1)
        var ponderados = usuario.Objetivos
            .ToDictionary(o => o.Id, o => o.Estado == EstadoObjetivo.CANCELADO ? 0.0 : RendimientoService.CalcularPonderadoStatic(o));

        return new EmpleadoDetalleData
        {
            Usuario = usuario,
            RadarData = radarData,
            Ponderados = ponderados
        };
    }
}
