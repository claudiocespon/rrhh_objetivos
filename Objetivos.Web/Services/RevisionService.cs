using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;
using Objetivos.Web.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Objetivos.Web.Services;

public class RevisionService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly ICurrentUserService _currentUser;
    private readonly RendimientoService _rendimiento;
    private readonly ObjetivoService _objetivoService;
    private readonly ConfiguracionService _configuracion;

    public RevisionService(IDbContextFactory<AppDbContext> dbFactory, ICurrentUserService currentUser, RendimientoService rendimiento, ObjetivoService objetivoService, ConfiguracionService configuracion)
    {
        _dbFactory = dbFactory;
        _currentUser = currentUser;
        _rendimiento = rendimiento;
        _objetivoService = objetivoService;
        _configuracion = configuracion;
    }

    // Carga revisión con Objetivo y SoftSkills para el diálogo de evaluación
    public async Task<RevisionCuatrimestral?> GetRevisionDetalleAsync(int revisionId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.RevisionesCuatrimestrales
            .Include(r => r.Objetivo)
                .ThenInclude(o => o.Pilar)
            .Include(r => r.Objetivo)
                .ThenInclude(o => o.SoftSkill1)
            .Include(r => r.Objetivo)
                .ThenInclude(o => o.SoftSkill2)
            .FirstOrDefaultAsync(r => r.Id == revisionId);
    }

    // M-10: Carga las entradas de bitácora del objetivo para mostrarlas como evidencias seleccionables
    public async Task<List<BitacoraEntrada>> GetBitacoraDelObjetivoAsync(int objetivoId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.BitacoraEntradas
            .Where(b => b.ObjetivoId == objetivoId)
            .OrderByDescending(b => b.Fecha)
            .ToListAsync();
    }

    // RN-02: Completar Revisión Cuatrimestral
    public async Task<bool> CompletarRevisionAsync(int revisionId, int? escalaValoracionId, string comentario, ResultadoEval? resultado = null, List<string> evidencias = null!,
        int? ss1EscalaValoracionId = null, string? ss1Comentario = null, int? ss2EscalaValoracionId = null, string? ss2Comentario = null)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var revision = await db.RevisionesCuatrimestrales
            .Include(r => r.Objetivo)
                .ThenInclude(o => o.Revisiones)
            .FirstOrDefaultAsync(r => r.Id == revisionId);

        if (revision == null || revision.Completada) return false;

        revision.EscalaValoracionId = escalaValoracionId;
        revision.ComentarioJefe = comentario;
        revision.Resultado = resultado;
        revision.EvidenciasRevisadasJson = JsonSerializer.Serialize(evidencias ?? new List<string>());
        revision.Completada = true;
        revision.FechaEvaluacion = DateTime.UtcNow;
        revision.EvaluadorId = _currentUser.UsuarioId;

        // Soft Skills - now using EscalaValoracionId
        revision.SoftSkill1EscalaValoracionId = ss1EscalaValoracionId;
        revision.SoftSkill1Comentario = ss1Comentario ?? "";
        revision.SoftSkill2EscalaValoracionId = ss2EscalaValoracionId;
        revision.SoftSkill2Comentario = ss2Comentario ?? "";

        if (escalaValoracionId.HasValue)
        {
            var escala = await db.EscalasValoracion.FindAsync(escalaValoracionId.Value);
            if (escala != null && escala.ValorNumerico.HasValue)
            {
                revision.Puntaje = (int)Math.Round(escala.ValorNumerico.Value);
            }
        }
        else
        {
            revision.Puntaje = null;
        }

        if (ss1EscalaValoracionId.HasValue)
        {
            var escala = await db.EscalasValoracion.FindAsync(ss1EscalaValoracionId.Value);
            if (escala != null && escala.ValorNumerico.HasValue)
            {
                revision.SoftSkill1Puntaje = (int)Math.Round(escala.ValorNumerico.Value);
            }
        }
        else
        {
            revision.SoftSkill1Puntaje = null;
        }

        if (ss2EscalaValoracionId.HasValue)
        {
            var escala = await db.EscalasValoracion.FindAsync(ss2EscalaValoracionId.Value);
            if (escala != null && escala.ValorNumerico.HasValue)
            {
                revision.SoftSkill2Puntaje = (int)Math.Round(escala.ValorNumerico.Value);
            }
        }
        else
        {
            revision.SoftSkill2Puntaje = null;
        }

        db.AuditoriaLogs.Add(new AuditoriaLog
        {
            Entidad = "RevisionCuatrimestral",
            EntidadId = revision.Id,
            Accion = "UPDATE",
            UsuarioId = _currentUser.UsuarioId,
            Fecha = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        // Recalcular progreso y evaluar riesgo (cada uno crea su propio contexto)
        await _rendimiento.RecalcularProgresoObjetivoAsync(revision.ObjetivoId);
        await _objetivoService.EvaluarEstadoRiesgoAsync(revision.ObjetivoId);

        return true;
    }

    // RN-03: Evaluación Final (respeta configuración resultado_final_manual)
    public async Task<bool> CompletarEvaluacionFinalAsync(int objetivoId, string comentario, ResultadoEval? resultado,
        int? ss1EscalaValoracionId = null, string? ss1Comentario = null, int? ss2EscalaValoracionId = null, string? ss2Comentario = null, int? escalaValoracionIdFinal = null)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var objetivo = await db.Objetivos
            .Include(o => o.Revisiones)
            .Include(o => o.EvaluacionFinal)
            .FirstOrDefaultAsync(o => o.Id == objetivoId);

        if (objetivo == null) return false;

        // Precondiciones
        if (objetivo.Revisiones.Count(r => r.Completada) < 1) return false;
        // A-05: La restricción de deadline fue removida por solicitud explícita del usuario.
        // EvaluacionService ya muestra objetivos para evaluar una vez completado el Feedback,
        // sin verificar si el deadline pasó. Este servicio es consistente con esa decisión.
        if (objetivo.EvaluacionFinal != null) return false;

        // TAREA 6: Verificar configuración resultado_final_manual
        bool resultadoFinalManual = await _configuracion.ObtenerConfiguracionBoolAsync("resultado_final_manual") ?? true;

        double valoracionFinal = 0;
        if (escalaValoracionIdFinal == null)
        {
            // Si resultado_final_manual es true, requerimos ingreso manual
            if (resultadoFinalManual)
                return false; // No se permite cálculo automático

            // Si resultado_final_manual es false, permitimos cálculo automático
            valoracionFinal = await _rendimiento.CalcularPonderadoAsync(objetivoId);
        }
        else
        {
            var escala = await db.EscalasValoracion.FindAsync(escalaValoracionIdFinal.Value);
            if (escala != null && escala.ValorNumerico.HasValue)
            {
                valoracionFinal = (double)escala.ValorNumerico.Value;
            }
        }

        int? ss1Puntaje = null;
        if (ss1EscalaValoracionId.HasValue)
        {
            var escala = await db.EscalasValoracion.FindAsync(ss1EscalaValoracionId.Value);
            if (escala != null && escala.ValorNumerico.HasValue)
            {
                ss1Puntaje = (int)Math.Round(escala.ValorNumerico.Value);
            }
        }

        int? ss2Puntaje = null;
        if (ss2EscalaValoracionId.HasValue)
        {
            var escala = await db.EscalasValoracion.FindAsync(ss2EscalaValoracionId.Value);
            if (escala != null && escala.ValorNumerico.HasValue)
            {
                ss2Puntaje = (int)Math.Round(escala.ValorNumerico.Value);
            }
        }

        var evalFinal = new EvaluacionFinal
        {
            ObjetivoId = objetivoId,
            Anio = objetivo.Anio,
            PuntajeFinal = valoracionFinal,
            EscalaValoracionIdFinal = escalaValoracionIdFinal,
            ComentarioJefe = comentario,
            ResultadoFinal = resultado ?? ResultadoEval.CUMPLIDO,
            FechaEvaluacion = DateTime.UtcNow,
            EvaluadorId = _currentUser.UsuarioId,
            SoftSkill1EscalaValoracionId = ss1EscalaValoracionId,
            SoftSkill1Comentario = ss1Comentario ?? "",
            SoftSkill1Puntaje = ss1Puntaje,
            SoftSkill2EscalaValoracionId = ss2EscalaValoracionId,
            SoftSkill2Comentario = ss2Comentario ?? "",
            SoftSkill2Puntaje = ss2Puntaje
        };
        db.EvaluacionesFinales.Add(evalFinal);

        objetivo.Estado = EstadoObjetivo.COMPLETADO;
        objetivo.Progreso = 100;

        db.AuditoriaLogs.Add(new AuditoriaLog
        {
            Entidad = "EvaluacionFinal",
            EntidadId = objetivoId,
            Accion = "CREATE",
            UsuarioId = _currentUser.UsuarioId,
            Fecha = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return true;
    }
}
