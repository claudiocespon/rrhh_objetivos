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

    public ObjetivoService(AppDbContext db, ICurrentUserService currentUser, DataScopeService dataScope)
    {
        _db = db;
        _currentUser = currentUser;
        _dataScope = dataScope;
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
}

public class RoleObjetivosData
{
    public List<Objetivo>? Personal { get; set; }
    public List<Objetivo>? Equipo { get; set; }
}
