using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;
using Objetivos.Web.Domain.Enums;

namespace Objetivos.Web.Services;

public class ProximoEvento
{
    public string Titulo { get; set; } = "";
    public string? Subtitulo { get; set; }
    public DateTime Fecha { get; set; }
    public string Tipo { get; set; } = ""; // "DEADLINE" | "REVISION_PENDIENTE" | "EVALUACION_FINAL"
    public int DiasFaltantes => (Fecha.Date - DateTime.Today).Days;
    public bool EsVencido => Fecha.Date < DateTime.Today;
}

public class CalendarioService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly DataScopeService _dataScope;

    public CalendarioService(IDbContextFactory<AppDbContext> dbFactory, DataScopeService dataScope)
    {
        _dbFactory = dbFactory;
        _dataScope = dataScope;
    }

    /// <summary>
    /// Retorna los próximos eventos relevantes para el usuario (deadlines + revisiones pendientes).
    /// Los eventos se ordenan por fecha ascendente y se limitan a un horizonte de 60 días.
    /// </summary>
    public async Task<List<ProximoEvento>> GetProximosEventosAsync(ICurrentUserService user, int diasHorizonte = 60)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        var hoy = DateTime.Today;
        var horizonte = hoy.AddDays(diasHorizonte);
        var anio = hoy.Year;

        IQueryable<Domain.Entities.Objetivo> query = db.Objetivos
            .Include(o => o.Usuario)
            .Where(o => o.Anio == anio && o.Estado != EstadoObjetivo.CANCELADO && o.Estado != EstadoObjetivo.COMPLETADO);

        if (user.EsJefe || _dataScope.PuedeVerTodo(user))
        {
            query = _dataScope.AplicarScope(query, user);
        }
        else
        {
            // Usuario regular: solo sus propios objetivos
            var emp = await db.Usuarios.FirstOrDefaultAsync(e => e.Email.ToLower() == user.Email.ToLower() && e.Activo);
            if (emp == null) return [];
            query = query.Where(o => o.UsuarioId == emp.Id);
        }

        var objetivos = await query.ToListAsync();

        var eventos = new List<ProximoEvento>();

        foreach (var obj in objetivos)
        {
            // Evento de deadline próximo (dentro del horizonte)
            if (obj.Deadline.Date <= horizonte)
            {
                eventos.Add(new ProximoEvento
                {
                    Titulo = obj.Nombre,
                    Subtitulo = $"{obj.Usuario.Nombre} {obj.Usuario.Apellido}",
                    Fecha = obj.Deadline.Date,
                    Tipo = "DEADLINE"
                });
            }
        }

        // Revisiones pendientes no completadas (si el período ya comenzó)
        var revisionesPendientes = await db.RevisionesCuatrimestrales
            .Include(r => r.Objetivo)
                .ThenInclude(o => o.Usuario)
            .Where(r => !r.Completada && r.Anio == anio)
            .ToListAsync();

        // Solo incluir revisiones de objetivos en el scope del usuario
        var objetivoIds = objetivos.Select(o => o.Id).ToHashSet();
        foreach (var rev in revisionesPendientes.Where(r => objetivoIds.Contains(r.ObjetivoId)))
        {
            // Estimar fecha de revisión: Feedback de mitad de año → 30 de junio del año en curso
            var fechaRevision = new DateTime(anio, 6, 30);
            if (fechaRevision.Date <= horizonte)
            {
                eventos.Add(new ProximoEvento
                {
                    Titulo = $"Revisión: {rev.Objetivo.Nombre}",
                    Subtitulo = $"{rev.Objetivo.Usuario.Nombre} {rev.Objetivo.Usuario.Apellido}",
                    Fecha = fechaRevision,
                    Tipo = "REVISION_PENDIENTE"
                });
            }
        }

        return eventos
            .OrderBy(e => e.Fecha)
            .ThenBy(e => e.Titulo)
            .Take(10)
            .ToList();
    }

    /// <summary>
    /// Retorna EventoCalendario desde la BD, filtrado por scope del usuario.
    /// </summary>
    public async Task<List<EventoCalendario>> GetEventosAsync(ICurrentUserService user, int anio)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        IQueryable<EventoCalendario> query = db.EventosCalendario
            .Include(e => e.Objetivo)
                .ThenInclude(o => o!.Usuario)
            .Where(e => e.Fecha.Year == anio);

        if (!_dataScope.PuedeVerTodo(user) && !user.EsJefe)
        {
            var emp = await db.Usuarios.FirstOrDefaultAsync(e => e.Email.ToLower() == user.Email.ToLower() && e.Activo);
            if (emp != null)
                query = query.Where(e => e.Objetivo != null && e.Objetivo.UsuarioId == emp.Id);
        }
        else if (user.EsJefe && !_dataScope.PuedeVerTodo(user))
        {
            query = query.Where(e => e.AreaId == user.AreaId);
        }

        return await query.OrderBy(e => e.Fecha).ToListAsync();
    }
}
