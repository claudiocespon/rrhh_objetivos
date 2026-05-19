using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;
using Objetivos.Web.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Objetivos.Web.Services;

public class ObjetivoService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly ICurrentUserService _currentUser;
    private readonly DataScopeService _dataScope;
    private readonly ConfiguracionService _configuracion;
    private readonly ValidacionObjetivoService _validacion;

    public ObjetivoService(IDbContextFactory<AppDbContext> dbFactory, ICurrentUserService currentUser, DataScopeService dataScope, ConfiguracionService configuracion, ValidacionObjetivoService validacion)
    {
        _dbFactory = dbFactory;
        _currentUser = currentUser;
        _dataScope = dataScope;
        _configuracion = configuracion;
        _validacion = validacion;
    }

    public async Task<RoleObjetivosData> GetObjetivosRoleAsync(int? anio)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var result = new RoleObjetivosData();
        var email = _currentUser.Email?.ToLower();

        // 1. Fetch Personal Data
        var empleadoPropio = await db.Empleados.FirstOrDefaultAsync(e => e.Email.ToLower() == email && e.Activo);
        if (empleadoPropio != null)
        {
            result.Personal = await db.Objetivos
                .Include(o => o.Empleado)
                .Include(o => o.Pilar)
                .Where(o => o.EmpleadoId == empleadoPropio.Id && (anio == null || o.Anio == anio))
                .ToListAsync();
        }
        else
        {
            result.Personal = new List<Objetivo>();
        }

        // 2. Fetch Team/Org Data using centralized scope
        var query = db.Objetivos
            .Include(o => o.Empleado)
            .Include(o => o.Pilar)
            .Where(o => anio == null || o.Anio == anio);

        result.Equipo = await _dataScope.AplicarScope(query, _currentUser).ToListAsync();

        return result;
    }

    public async Task<Objetivo?> GetByIdAsync(int id)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Objetivos
            .Include(o => o.Empleado)
            .Include(o => o.Pilar)
            .Include(o => o.SoftSkill1)
            .Include(o => o.SoftSkill2)
            .Include(o => o.Revisiones)
                .ThenInclude(r => r.EscalaValoracion)
            .Include(o => o.Revisiones)
                .ThenInclude(r => r.SoftSkill1EscalaValoracion)
            .Include(o => o.Revisiones)
                .ThenInclude(r => r.SoftSkill2EscalaValoracion)
            .Include(o => o.EvaluacionFinal)
                .ThenInclude(ef => ef!.EscalaValoracionFinal)
            .Include(o => o.EvaluacionFinal)
                .ThenInclude(ef => ef!.SoftSkill1EscalaValoracion)
            .Include(o => o.EvaluacionFinal)
                .ThenInclude(ef => ef!.SoftSkill2EscalaValoracion)
            .Include(o => o.Autoevaluacion)
                .ThenInclude(ae => ae!.EscalaValoracionScore)
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

        // VAL-06: Validar que la suma total no exceda el 100% (delegado a ValidacionObjetivoService)
        var anio = DateTime.Now.Year;
        var (sumaOk, _) = await _validacion.ValidarSumaPesoAsync(nuevo.EmpleadoId, anio, nuevo.PorcentajePilar);
        if (!sumaOk)
            return (false, false);

        using var db = await _dbFactory.CreateDbContextAsync();

        // VAL-01: Unicidad por pilar configurable (default: false = múltiples permitidos)
        bool unPorPilar = await _configuracion.ObtenerConfiguracionBoolAsync("un_objetivo_por_pilar") ?? false;
        if (unPorPilar)
        {
            bool duplicado = await db.Objetivos.AnyAsync(o =>
                o.EmpleadoId == nuevo.EmpleadoId &&
                o.PilarId == nuevo.PilarId &&
                o.Anio == anio &&
                o.Estado != EstadoObjetivo.CANCELADO);
            if (duplicado) return (false, true);
        }

        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            nuevo.Estado = EstadoObjetivo.ACTIVO;
            nuevo.FechaCreacion = DateTime.UtcNow;
            nuevo.CreadoPorId = _currentUser.UsuarioId;
            nuevo.Anio = anio;

            // Set dynamic approval state to "pendiente_aprobacion"
            var estadoPendiente = await db.EstadosObjetivoConfig
                .FirstOrDefaultAsync(e => e.Slug == "pendiente_aprobacion" && e.Activo);
            if (estadoPendiente != null)
                nuevo.EstadoObjetivoConfigId = estadoPendiente.Id;

            db.Objetivos.Add(nuevo);
            await db.SaveChangesAsync(); // Save to get Id

            // Insert Revision (Mitad de Año)
            var revisiones = new List<RevisionCuatrimestral>
            {
                new() { ObjetivoId = nuevo.Id, Periodo = PeriodoRevision.FEEDBACK_MITAD_ANIO, Anio = nuevo.Anio, Completada = false }
            };
            db.RevisionesCuatrimestrales.AddRange(revisiones);

            // Insert EventoCalendario
            db.EventosCalendario.Add(new EventoCalendario
            {
                Titulo = $"Deadline: {nuevo.Nombre}",
                Fecha = nuevo.Deadline,
                Tipo = TipoEvento.DEADLINE_OBJETIVO,
                ObjetivoId = nuevo.Id,
                AreaId = _currentUser.AreaId
            });

            // Audit
            db.AuditoriaLogs.Add(new AuditoriaLog
            {
                Entidad = "Objetivo",
                EntidadId = nuevo.Id,
                Accion = "CREATE",
                UsuarioId = _currentUser.UsuarioId,
                Fecha = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, false);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true)
        {
            await transaction.RollbackAsync();
            return (false, true); // Duplicado detectado a nivel BD
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
        using var db = await _dbFactory.CreateDbContextAsync();
        var objetivo = await db.Objetivos.FindAsync(id);
        if (objetivo != null)
        {
            objetivo.Estado = EstadoObjetivo.CANCELADO;

            db.AuditoriaLogs.Add(new AuditoriaLog
            {
                Entidad = "Objetivo",
                EntidadId = id,
                Accion = "DELETE", // RN-04: semántica DELETE según CONTEXT.md
                UsuarioId = _currentUser.UsuarioId,
                Fecha = DateTime.UtcNow,
                CambiosJson = JsonSerializer.Serialize(new { razon, estadoAnterior = "ACTIVO", estadoNuevo = "CANCELADO" })
            });

            await db.SaveChangesAsync();
        }
    }

    // RN-06: Transición Automática de Estado EN_RIESGO
    public async Task EvaluarEstadoRiesgoAsync(int objetivoId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var objetivo = await db.Objetivos.FindAsync(objetivoId);
        if (objetivo != null && objetivo.Estado == EstadoObjetivo.ACTIVO)
        {
            var diasRestantes = (objetivo.Deadline - DateTime.Today).TotalDays;
            if (objetivo.Progreso < 50 && diasRestantes < 60)
            {
                objetivo.Estado = EstadoObjetivo.EN_RIESGO;
                await db.SaveChangesAsync();
            }
        }
    }

    public async Task<bool> UpdateObjetivoAsync(Objetivo objetivo)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var existing = await db.Objetivos.FindAsync(objetivo.Id);
        if (existing == null) return false;

        existing.Nombre = objetivo.Nombre;
        existing.Descripcion = objetivo.Descripcion;
        existing.Deadline = objetivo.Deadline;
        existing.PilarId = objetivo.PilarId;
        existing.SoftSkill1Id = objetivo.SoftSkill1Id;
        existing.SoftSkill2Id = objetivo.SoftSkill2Id;
        existing.Estado = objetivo.Estado;
        existing.Progreso = objetivo.Progreso;

        // VAL-06: Validar que la suma total no exceda el 100% en UPDATE (delegado a ValidacionObjetivoService)
        var (sumaOk, _) = await _validacion.ValidarSumaPesoAsync(existing.EmpleadoId, existing.Anio, objetivo.PorcentajePilar, existing.Id);
        if (!sumaOk)
            return false;

        existing.PorcentajePilar = objetivo.PorcentajePilar;

        db.AuditoriaLogs.Add(new AuditoriaLog
        {
            Entidad = "Objetivo",
            EntidadId = objetivo.Id,
            Accion = "UPDATE",
            UsuarioId = _currentUser.UsuarioId,
            Fecha = DateTime.UtcNow
        });

        var saved = await db.SaveChangesAsync() > 0;

        // M-07: Re-evaluar riesgo al actualizar progreso o deadline (nuevo contexto)
        if (saved)
            await EvaluarEstadoRiesgoAsync(objetivo.Id);

        return saved;
    }

    // RN-01A: Aprobar Objetivo (flujo de aprobación por jefe)
    public async Task<bool> AprobarObjetivoAsync(int objetivoId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var objetivo = await db.Objetivos
            .Include(o => o.Empleado)
            .FirstOrDefaultAsync(o => o.Id == objetivoId);

        if (objetivo == null) return false;

        // Solo el jefe del empleado o superusuario puede aprobar
        if (!_currentUser.EsSuperusuario && objetivo.Empleado.JefeId != _currentUser.UsuarioId)
            return false;

        // Obtener estado "aprobado"
        var estadoAprobado = await db.EstadosObjetivoConfig
            .FirstOrDefaultAsync(e => e.Slug == "aprobado" && e.Activo);
        if (estadoAprobado == null) return false;

        objetivo.EstadoObjetivoConfigId = estadoAprobado.Id;
        objetivo.AprobadoPorJefe = true;

        db.AuditoriaLogs.Add(new AuditoriaLog
        {
            Entidad = "Objetivo",
            EntidadId = objetivoId,
            Accion = "UPDATE",
            UsuarioId = _currentUser.UsuarioId,
            Fecha = DateTime.UtcNow,
            CambiosJson = JsonSerializer.Serialize(new { accion = "APROBACIÓN", estado = "aprobado" })
        });

        await db.SaveChangesAsync();
        return true;
    }

    // RN-01B: Rechazar Objetivo (vuelve a "pendiente_aprobacion")
    public async Task<bool> RechazarObjetivoAsync(int objetivoId, string comentario)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var objetivo = await db.Objetivos
            .Include(o => o.Empleado)
            .FirstOrDefaultAsync(o => o.Id == objetivoId);

        if (objetivo == null) return false;

        // Solo el jefe del empleado o superusuario puede rechazar
        if (!_currentUser.EsSuperusuario && objetivo.Empleado.JefeId != _currentUser.UsuarioId)
            return false;

        // Vuelve a "pendiente_aprobacion" sin cambiar AprobadoPorJefe (permanece false)
        var estadoPendiente = await db.EstadosObjetivoConfig
            .FirstOrDefaultAsync(e => e.Slug == "pendiente_aprobacion" && e.Activo);
        if (estadoPendiente == null) return false;

        objetivo.EstadoObjetivoConfigId = estadoPendiente.Id;

        db.AuditoriaLogs.Add(new AuditoriaLog
        {
            Entidad = "Objetivo",
            EntidadId = objetivoId,
            Accion = "UPDATE",
            UsuarioId = _currentUser.UsuarioId,
            Fecha = DateTime.UtcNow,
            CambiosJson = JsonSerializer.Serialize(new { accion = "RECHAZO", estado = "pendiente_aprobacion", comentario })
        });

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<Objetivo>> GetObjetivosPendientesAprobacionAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var empleadosDelJefe = await db.Empleados
            .Where(e => e.JefeId == _currentUser.UsuarioId && e.Activo)
            .Select(e => e.Id)
            .ToListAsync();

        var estadoPendiente = await db.EstadosObjetivoConfig
            .FirstOrDefaultAsync(e => e.Slug == "pendiente_aprobacion" && e.Activo);
        if (estadoPendiente == null) return new();

        return await db.Objetivos
            .Include(o => o.Empleado)
            .Include(o => o.Pilar)
            .Include(o => o.EstadoObjetivoConfig)
            .Where(o => empleadosDelJefe.Contains(o.EmpleadoId)
                    && o.EstadoObjetivoConfigId == estadoPendiente.Id
                    && o.Estado != EstadoObjetivo.CANCELADO)
            .OrderBy(o => o.FechaCreacion)
            .ToListAsync();
    }

    public async Task<List<PilarConConfig>> GetPilaresDisponiblesAsync(int empleadoId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var empleado = await db.Empleados.FindAsync(empleadoId);
        if (empleado == null) return new();

        var pilares = await db.Pilares
            .Where(p => p.Activo && (p.AreaId == null || p.AreaId == empleado.AreaId))
            .OrderBy(p => p.Orden)
            .Select(p => new PilarConConfig {
                Pilar = p,
                EsObligatorio = p.EsObligatorio || p.AreaId == null, // Globales son obligatorios por defecto
                PesoSugerido = 0
            })
            .ToListAsync();

        return pilares;
    }
}

public class PilarConConfig
{
    public Pilar Pilar { get; set; } = null!;
    public bool EsObligatorio { get; set; }
    public decimal PesoSugerido { get; set; }
}

public class RoleObjetivosData
{
    public List<Objetivo>? Personal { get; set; }
    public List<Objetivo>? Equipo { get; set; }
}
