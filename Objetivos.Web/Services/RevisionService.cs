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

    // Carga revisión con Objetivo y SoftSkills para el diálogo de evaluación
    public async Task<RevisionCuatrimestral?> GetRevisionDetalleAsync(int revisionId)
    {
        return await _db.RevisionesCuatrimestrales
            .Include(r => r.Objetivo)
                .ThenInclude(o => o.SoftSkill1)
            .Include(r => r.Objetivo)
                .ThenInclude(o => o.SoftSkill2)
            .FirstOrDefaultAsync(r => r.Id == revisionId);
    }

    // RN-02: Completar Revisión Cuatrimestral
    public async Task<bool> CompletarRevisionAsync(int revisionId, int puntaje, string comentario, ResultadoEval resultado, List<string> evidencias,
        int? ss1Puntaje = null, string? ss1Comentario = null, int? ss2Puntaje = null, string? ss2Comentario = null)
    {
        var revision = await _db.RevisionesCuatrimestrales
            .Include(r => r.Objetivo)
                .ThenInclude(o => o.Revisiones)
            .FirstOrDefaultAsync(r => r.Id == revisionId);

        if (revision == null || revision.Completada) return false;

        revision.Puntaje = puntaje;
        revision.ComentarioJefe = comentario;
        revision.Resultado = resultado;
        revision.EvidenciasRevisadasJson = JsonSerializer.Serialize(evidencias);
        revision.Completada = true;
        revision.FechaEvaluacion = DateTime.UtcNow;
        revision.EvaluadorId = _currentUser.UsuarioId;

        // Soft Skills
        revision.SoftSkill1Puntaje = ss1Puntaje;
        revision.SoftSkill1Comentario = ss1Comentario ?? "";
        revision.SoftSkill2Puntaje = ss2Puntaje;
        revision.SoftSkill2Comentario = ss2Comentario ?? "";

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
    public async Task<bool> CompletarEvaluacionFinalAsync(int objetivoId, string comentario, ResultadoEval resultado,
        int? ss1Puntaje = null, string? ss1Comentario = null, int? ss2Puntaje = null, string? ss2Comentario = null)
    {
        var objetivo = await _db.Objetivos
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

        double puntajeFinal = await _rendimiento.CalcularPonderadoAsync(objetivoId);

        var evalFinal = new EvaluacionFinal
        {
            ObjetivoId = objetivoId,
            Anio = objetivo.Anio,
            PuntajeFinal = puntajeFinal,
            ComentarioJefe = comentario,
            ResultadoFinal = resultado,
            FechaEvaluacion = DateTime.UtcNow,
            EvaluadorId = _currentUser.UsuarioId,
            SoftSkill1Puntaje = ss1Puntaje,
            SoftSkill1Comentario = ss1Comentario ?? "",
            SoftSkill2Puntaje = ss2Puntaje,
            SoftSkill2Comentario = ss2Comentario ?? ""
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
