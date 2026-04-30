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
    private readonly ConfiguracionService _configuracion;

    public RevisionService(AppDbContext db, ICurrentUserService currentUser, RendimientoService rendimiento, ObjetivoService objetivoService, ConfiguracionService configuracion)
    {
        _db = db;
        _currentUser = currentUser;
        _rendimiento = rendimiento;
        _objetivoService = objetivoService;
        _configuracion = configuracion;
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
    public async Task<bool> CompletarRevisionAsync(int revisionId, int valoracion, string comentario, ResultadoEval resultado, List<string> evidencias,
        int? ss1Valoracion = null, string? ss1Comentario = null, int? ss2Valoracion = null, string? ss2Comentario = null)
    {
        var revision = await _db.RevisionesCuatrimestrales
            .Include(r => r.Objetivo)
                .ThenInclude(o => o.Revisiones)
            .FirstOrDefaultAsync(r => r.Id == revisionId);

        if (revision == null || revision.Completada) return false;

        revision.Puntaje = valoracion;
        revision.ComentarioJefe = comentario;
        revision.Resultado = resultado;
        revision.EvidenciasRevisadasJson = JsonSerializer.Serialize(evidencias);
        revision.Completada = true;
        revision.FechaEvaluacion = DateTime.UtcNow;
        revision.EvaluadorId = _currentUser.UsuarioId;

        // Soft Skills
        revision.SoftSkill1Puntaje = ss1Valoracion;
        revision.SoftSkill1Comentario = ss1Comentario ?? "";
        revision.SoftSkill2Puntaje = ss2Valoracion;
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

    // RN-03: Evaluación Final (respeta configuración resultado_final_manual)
    public async Task<bool> CompletarEvaluacionFinalAsync(int objetivoId, string comentario, ResultadoEval? resultado,
        int? ss1Valoracion = null, string? ss1Comentario = null, int? ss2Valoracion = null, string? ss2Comentario = null, int? escalaValoracionIdFinal = null)
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
            SoftSkill1Puntaje = ss1Valoracion,
            SoftSkill1Comentario = ss1Comentario ?? "",
            SoftSkill2Puntaje = ss2Valoracion,
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
