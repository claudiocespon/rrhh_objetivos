using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;
using Objetivos.Web.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Objetivos.Web.Services;

public class ObjetivoService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly DataScopeService _dataScope;
    private readonly ConfiguracionService _configuracion;

    public ObjetivoService(AppDbContext db, ICurrentUserService currentUser, DataScopeService dataScope, ConfiguracionService configuracion)
    {
        _db = db;
        _currentUser = currentUser;
        _dataScope = dataScope;
        _configuracion = configuracion;
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

        // 2. Fetch Team/Org Data using centralized scope
        var query = _db.Objetivos
            .Include(o => o.Empleado)
            .Include(o => o.Pilar)
            .Where(o => o.Anio == anio);

        result.Equipo = await _dataScope.AplicarScope(query, _currentUser).ToListAsync();

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
    // Retorna (Ok=true, Duplicado=false) en éxito.
    // Retorna (Ok=false, Duplicado=true) si ya existe pilar+empleado+año y reemplazar=false.
    // Con reemplazar=true cancela el existente y crea el nuevo (VAL-01 completo).
    public async Task<(bool Ok, bool Duplicado)> CrearObjetivoAsync(Objetivo nuevo, bool reemplazar = false)
    {
        // VAL-JEFE: Verificar si jefe está autorizado a crear objetivos
        bool jefePermitido = await _configuracion.ObtenerConfiguracionBoolAsync("jefe_puede_crear_objetivos") ?? false;
        if (_currentUser.EsJefe && !jefePermitido && _currentUser.Rol != "RRHH" && _currentUser.Rol != "DIRECTOR_GENERAL")
        {
            return (false, false); // 403 Forbidden equivalent
        }

        // VAL-03: Soft skills deben ser diferentes
        if (nuevo.SoftSkill1Id == nuevo.SoftSkill2Id)
            return (false, false);

        // VAL-04: Deadline debe ser posterior a hoy
        if (nuevo.Deadline <= DateTime.Today)
            return (false, false);

        // VAL-01: Verificar duplicado pilar+empleado+año
        var existente = await _db.Objetivos.FirstOrDefaultAsync(o =>
            o.PilarId == nuevo.PilarId &&
            o.EmpleadoId == nuevo.EmpleadoId &&
            o.Anio == DateTime.Now.Year &&
            o.Estado != EstadoObjetivo.CANCELADO);

        if (existente != null)
        {
            if (!reemplazar)
                return (false, true);

            // Cancelar el existente antes de crear el nuevo
            await CancelarObjetivoAsync(existente.Id, "Reemplazado por nuevo objetivo");
        }

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            nuevo.Estado = EstadoObjetivo.ACTIVO;
            nuevo.FechaCreacion = DateTime.UtcNow;
            nuevo.CreadoPorId = _currentUser.UsuarioId;
            nuevo.Anio = DateTime.Now.Year;

            // Set dynamic approval state to "pendiente_aprobacion"
            var estadoPendiente = await _db.EstadosObjetivoConfig
                .FirstOrDefaultAsync(e => e.Slug == "pendiente_aprobacion" && e.Activo);
            if (estadoPendiente != null)
                nuevo.EstadoObjetivoConfigId = estadoPendiente.Id;

            _db.Objetivos.Add(nuevo);
            await _db.SaveChangesAsync(); // Save to get Id

            // Insert Revision (Mitad de Año)
            var revisiones = new List<RevisionCuatrimestral>
            {
                new() { ObjetivoId = nuevo.Id, Periodo = PeriodoRevision.FEEDBACK_MITAD_ANIO, Anio = nuevo.Anio, Completada = false }
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
            return (true, false);
        }
        catch
        {
            await transaction.RollbackAsync();
            return (false, false);
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
                Accion = "DELETE", // RN-04: semántica DELETE según CONTEXT.md
                UsuarioId = _currentUser.UsuarioId,
                Fecha = DateTime.UtcNow,
                CambiosJson = JsonSerializer.Serialize(new { razon, estadoAnterior = "ACTIVO", estadoNuevo = "CANCELADO" })
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

        var saved = await _db.SaveChangesAsync() > 0;

        // M-07: Re-evaluar riesgo al actualizar progreso o deadline
        if (saved)
            await EvaluarEstadoRiesgoAsync(objetivo.Id);

        return saved;
    }

    // RN-01A: Aprobar Objetivo (flujo de aprobación por jefe)
    public async Task<bool> AprobarObjetivoAsync(int objetivoId)
    {
        var objetivo = await _db.Objetivos
            .Include(o => o.Empleado)
            .FirstOrDefaultAsync(o => o.Id == objetivoId);

        if (objetivo == null) return false;

        // Solo el jefe del empleado o superusuario puede aprobar
        if (!_currentUser.EsSuperusuario && objetivo.Empleado.JefeId != _currentUser.UsuarioId)
            return false;

        // Obtener estado "aprobado"
        var estadoAprobado = await _db.EstadosObjetivoConfig
            .FirstOrDefaultAsync(e => e.Slug == "aprobado" && e.Activo);
        if (estadoAprobado == null) return false;

        objetivo.EstadoObjetivoConfigId = estadoAprobado.Id;
        objetivo.AprobadoPorJefe = true;

        _db.AuditoriaLogs.Add(new AuditoriaLog
        {
            Entidad = "Objetivo",
            EntidadId = objetivoId,
            Accion = "UPDATE",
            UsuarioId = _currentUser.UsuarioId,
            Fecha = DateTime.UtcNow,
            CambiosJson = JsonSerializer.Serialize(new { accion = "APROBACIÓN", estado = "aprobado" })
        });

        await _db.SaveChangesAsync();
        return true;
    }

    // RN-01B: Rechazar Objetivo (vuelve a "pendiente_aprobacion")
    public async Task<bool> RechazarObjetivoAsync(int objetivoId, string comentario)
    {
        var objetivo = await _db.Objetivos
            .Include(o => o.Empleado)
            .FirstOrDefaultAsync(o => o.Id == objetivoId);

        if (objetivo == null) return false;

        // Solo el jefe del empleado o superusuario puede rechazar
        if (!_currentUser.EsSuperusuario && objetivo.Empleado.JefeId != _currentUser.UsuarioId)
            return false;

        // Vuelve a "pendiente_aprobacion" sin cambiar AprobadoPorJefe (permanece false)
        var estadoPendiente = await _db.EstadosObjetivoConfig
            .FirstOrDefaultAsync(e => e.Slug == "pendiente_aprobacion" && e.Activo);
        if (estadoPendiente == null) return false;

        objetivo.EstadoObjetivoConfigId = estadoPendiente.Id;

        _db.AuditoriaLogs.Add(new AuditoriaLog
        {
            Entidad = "Objetivo",
            EntidadId = objetivoId,
            Accion = "UPDATE",
            UsuarioId = _currentUser.UsuarioId,
            Fecha = DateTime.UtcNow,
            CambiosJson = JsonSerializer.Serialize(new { accion = "RECHAZO", estado = "pendiente_aprobacion", comentario })
        });

        await _db.SaveChangesAsync();
        return true;
    }

    // RN-01C: Obtener objetivos pendientes de aprobación para un jefe
    public async Task<List<Objetivo>> GetObjetivosPendientesAprobacionAsync()
    {
        var empleadosDelJefe = await _db.Empleados
            .Where(e => e.JefeId == _currentUser.UsuarioId && e.Activo)
            .Select(e => e.Id)
            .ToListAsync();

        var estadoPendiente = await _db.EstadosObjetivoConfig
            .FirstOrDefaultAsync(e => e.Slug == "pendiente_aprobacion" && e.Activo);
        if (estadoPendiente == null) return new();

        return await _db.Objetivos
            .Include(o => o.Empleado)
            .Include(o => o.Pilar)
            .Include(o => o.EstadoObjetivoConfig)
            .Where(o => empleadosDelJefe.Contains(o.EmpleadoId)
                    && o.EstadoObjetivoConfigId == estadoPendiente.Id
                    && o.Estado != EstadoObjetivo.CANCELADO)
            .OrderBy(o => o.FechaCreacion)
            .ToListAsync();
    }
}

public class RoleObjetivosData
{
    public List<Objetivo>? Personal { get; set; }
    public List<Objetivo>? Equipo { get; set; }
}
