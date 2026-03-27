using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;
using Objetivos.Web.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Objetivos.Web.Services;

public class ObjetivoService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ObjetivoService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<RoleObjetivosData> GetObjetivosRoleAsync(int anio)
    {
        var result = new RoleObjetivosData();
        var email = _currentUser.Email?.ToLower();

        // 1. Fetch Personal Data
        var empleadoPropio = await _db.Empleados.FirstOrDefaultAsync(e => e.Email.ToLower() == email && e.Activo);
        if (empleadoPropio != null)
        {
            result.Personal = await _db.Objetivos
                .Include(o => o.Empleado)
                .Include(o => o.Pilar)
                .Where(o => o.EmpleadoId == empleadoPropio.Id && o.Anio == anio)
                .ToListAsync();
        }

        // 2. Fetch Team/Org Data
        bool canSeeAll = _currentUser.Rol == "DIRECTOR_GENERAL" || _currentUser.Rol == "RRHH" || _currentUser.EsSuperusuario;
        
        if (canSeeAll)
        {
            result.Equipo = await _db.Objetivos
                .Include(o => o.Empleado)
                .Include(o => o.Pilar)
                .Where(o => o.Anio == anio)
                .ToListAsync();
        }
        else if (_currentUser.Rol == "DIRECTOR")
        {
            // Directors see everyone in their area (full center of cost)
            result.Equipo = await _db.Objetivos
                .Include(o => o.Empleado)
                .Include(o => o.Pilar)
                .Where(o => o.Empleado.AreaId == _currentUser.AreaId && o.Anio == anio)
                .ToListAsync();
        }
        else if (_currentUser.EsJefe)
        {
            // Standard bosses see their direct reports
            result.Equipo = await _db.Objetivos
                .Include(o => o.Empleado)
                .Include(o => o.Pilar)
                .Where(o => o.Empleado.JefeId == _currentUser.UsuarioId && o.Anio == anio)
                .ToListAsync();
        }

        return result;
    }

    public async Task<Objetivo?> GetByIdAsync(int id)
    {
        return await _db.Objetivos
            .Include(o => o.Empleado)
            .Include(o => o.Pilar)
            .Include(o => o.SoftSkill1)
            .Include(o => o.SoftSkill2)
            .Include(o => o.Revisiones)
            .Include(o => o.EvaluacionFinal)
            .Include(o => o.Autoevaluacion)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    // RN-01: Crear Objetivo (transacción atómica)
    public async Task<bool> CrearObjetivoAsync(Objetivo nuevo)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            nuevo.Estado = EstadoObjetivo.ACTIVO;
            nuevo.FechaCreacion = DateTime.UtcNow;
            nuevo.CreadoPorId = _currentUser.UsuarioId;
            nuevo.Anio = DateTime.Now.Year;

            _db.Objetivos.Add(nuevo);
            await _db.SaveChangesAsync(); // Save to get Id

            // Insert RevisionCuatrimestral x3
            var revisiones = new List<RevisionCuatrimestral>
            {
                new() { ObjetivoId = nuevo.Id, Periodo = PeriodoRevision.Q1_ABRIL,    Anio = nuevo.Anio, Completada = false },
                new() { ObjetivoId = nuevo.Id, Periodo = PeriodoRevision.Q2_AGOSTO,   Anio = nuevo.Anio, Completada = false },
                new() { ObjetivoId = nuevo.Id, Periodo = PeriodoRevision.Q3_NOVIEMBRE, Anio = nuevo.Anio, Completada = false }
            };
            _db.RevisionesCuatrimestrales.AddRange(revisiones);

            // Insert EventoCalendario
            _db.EventosCalendario.Add(new EventoCalendario
            {
                Titulo = $"Deadline: {nuevo.Nombre}",
                Fecha = nuevo.Deadline,
                Tipo = TipoEvento.DEADLINE_OBJETIVO,
                ObjetivoId = nuevo.Id,
                AreaId = _currentUser.AreaId
            });

            // Audit
            _db.AuditoriaLogs.Add(new AuditoriaLog
            {
                Entidad = "Objetivo",
                EntidadId = nuevo.Id,
                Accion = "CREATE",
                UsuarioId = _currentUser.UsuarioId,
                Fecha = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    // RN-04: Cancelar Objetivo (soft delete)
    public async Task CancelarObjetivoAsync(int id, string razon)
    {
        var objetivo = await _db.Objetivos.FindAsync(id);
        if (objetivo != null)
        {
            objetivo.Estado = EstadoObjetivo.CANCELADO;
            
            _db.AuditoriaLogs.Add(new AuditoriaLog
            {
                Entidad = "Objetivo",
                EntidadId = id,
                Accion = "DELETE", 
                UsuarioId = _currentUser.UsuarioId,
                Fecha = DateTime.UtcNow,
                CambiosJson = $"{{\"razon\": \"{razon}\"}}"
            });

            await _db.SaveChangesAsync();
        }
    }

    // RN-06: Transición Automática de Estado EN_RIESGO
    public async Task EvaluarEstadoRiesgoAsync(int objetivoId)
    {
        var objetivo = await _db.Objetivos.FindAsync(objetivoId);
        if (objetivo != null && objetivo.Estado == EstadoObjetivo.ACTIVO)
        {
            var diasRestantes = (objetivo.Deadline - DateTime.Today).TotalDays;
            if (objetivo.Progreso < 50 && diasRestantes < 60)
            {
                objetivo.Estado = EstadoObjetivo.EN_RIESGO;
                await _db.SaveChangesAsync();
            }
        }
    }

    public async Task<bool> UpdateObjetivoAsync(Objetivo objetivo)
    {
        var existing = await _db.Objetivos.FindAsync(objetivo.Id);
        if (existing == null) return false;

        existing.Nombre = objetivo.Nombre;
        existing.Descripcion = objetivo.Descripcion;
        existing.Deadline = objetivo.Deadline;
        existing.PilarId = objetivo.PilarId;
        existing.SoftSkill1Id = objetivo.SoftSkill1Id;
        existing.SoftSkill2Id = objetivo.SoftSkill2Id;
        existing.Estado = objetivo.Estado;
        existing.Progreso = objetivo.Progreso;

        _db.AuditoriaLogs.Add(new AuditoriaLog
        {
            Entidad = "Objetivo",
            EntidadId = objetivo.Id,
            Accion = "UPDATE",
            UsuarioId = _currentUser.UsuarioId,
            Fecha = DateTime.UtcNow
        });

        return await _db.SaveChangesAsync() > 0;
    }
}

public class RoleObjetivosData
{
    public List<Objetivo>? Personal { get; set; }
    public List<Objetivo>? Equipo { get; set; }
}
