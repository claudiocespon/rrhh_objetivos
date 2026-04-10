using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;
using Objetivos.Web.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Objetivos.Web.Services;

public class RevisionService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly RendimientoService _rendimiento;
    private readonly ObjetivoService _objetivoService;

    public RevisionService(AppDbContext db, ICurrentUserService currentUser, RendimientoService rendimiento, ObjetivoService objetivoService)
    {
        _db = db;
        _currentUser = currentUser;
        _rendimiento = rendimiento;
        _objetivoService = objetivoService;
    }

    // RN-02: Completar Revisión Cuatrimestral
    public async Task<bool> CompletarRevisionAsync(int revisionId, int puntaje, string comentario, ResultadoEval resultado, List<string> evidencias)
    {
        var revision = await _db.RevisionesCuatrimestrales
            .Include(r => r.Objetivo)
                .ThenInclude(o => o.Revisiones)
            .FirstOrDefaultAsync(r => r.Id == revisionId);

        if (revision == null || revision.Completada) return false;

        // RN-02: Completar Revisión de Feedback
        // No longer need to validate sequence as there is only one period

        revision.Puntaje = puntaje;
        revision.ComentarioJefe = comentario;
        revision.Resultado = resultado;
        revision.EvidenciasRevisadasJson = JsonSerializer.Serialize(evidencias);
        revision.Completada = true;
        revision.FechaEvaluacion = DateTime.UtcNow;
        revision.EvaluadorId = _currentUser.UsuarioId;

        _db.AuditoriaLogs.Add(new AuditoriaLog
        {
            Entidad = "RevisionCuatrimestral",
            EntidadId = revision.Id,
            Accion = "UPDATE",
            UsuarioId = _currentUser.UsuarioId,
            Fecha = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        // Recalcular progreso y evaluar riesgo
        await _rendimiento.RecalcularProgresoObjetivoAsync(revision.ObjetivoId);
        await _objetivoService.EvaluarEstadoRiesgoAsync(revision.ObjetivoId);

        return true;
    }

    // RN-03: Evaluación Final
    public async Task<bool> CompletarEvaluacionFinalAsync(int objetivoId, string comentario, ResultadoEval resultado)
    {
        var objetivo = await _db.Objetivos
            .Include(o => o.Revisiones)
            .Include(o => o.EvaluacionFinal)
            .FirstOrDefaultAsync(o => o.Id == objetivoId);

        if (objetivo == null) return false;

        // Precondiciones
        if (objetivo.Revisiones.Count(r => r.Completada) < 1) return false;
        if (DateTime.Today < objetivo.Deadline) return false;
        if (objetivo.EvaluacionFinal != null) return false;

        double puntajeFinal = await _rendimiento.CalcularPonderadoAsync(objetivoId);

        var evalFinal = new EvaluacionFinal
        {
            ObjetivoId = objetivoId,
            Anio = objetivo.Anio,
            PuntajeFinal = puntajeFinal,
            ComentarioJefe = comentario,
            ResultadoFinal = resultado,
            FechaEvaluacion = DateTime.UtcNow,
            EvaluadorId = _currentUser.UsuarioId
        };
        _db.EvaluacionesFinales.Add(evalFinal);

        objetivo.Estado = EstadoObjetivo.COMPLETADO;

        _db.AuditoriaLogs.Add(new AuditoriaLog
        {
            Entidad = "EvaluacionFinal",
            EntidadId = objetivoId,
            Accion = "CREATE",
            UsuarioId = _currentUser.UsuarioId,
            Fecha = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return true;
    }
}
