using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;
using Objetivos.Web.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Objetivos.Web.Services;

public class RendimientoService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public RendimientoService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    /// <summary>
    /// Calcula la valoracion ponderada de un objetivo según el modelo simplificado (1 revisión).
    ///
    /// Fórmula: (feedback * 0.5) + (evaluaciónFinal * 0.5)
    ///
    /// NOTA — comportamiento durante el flujo:
    ///   1. Al completar el Feedback de Mitad de Año: EvaluacionFinal aún no existe → fin = 0.
    ///      El resultado en ese momento es feedback * 0.5 (la mitad de la valoracion máxima posible).
    ///   2. Al completar la Evaluación Final: este método se llama ANTES de guardar EvaluacionFinal,
    ///      por lo que fin sigue siendo 0. La ValoracionFinal almacenada en EvaluacionFinal es por tanto
    ///      feedback * 0.5. Esto es by design: el jefe puede ajustar ResultadoFinal manualmente.
    ///
    /// Diferencia con CONTEXT.md RN-07 (Q1*0.2 + Q2*0.3 + Q3*0.3 + fin*0.2):
    ///   Se decidió mantener el modelo simplificado (1 feedback) por solicitud explícita.
    ///   Si en el futuro se restauran Q1/Q2/Q3, actualizar este método y el enum PeriodoRevision.
    /// </summary>
    /// <summary>Versión estática pública para uso en batch (SeguimientoService) sin N+1.</summary>
    public static double CalcularPonderadoStatic(Objetivo? objetivo) => CalcularPonderadoInterno(objetivo);

    private static double CalcularPonderadoInterno(Objetivo? objetivo)
    {
        if (objetivo == null) return 0;

        double feedback = objetivo.Revisiones.FirstOrDefault(r => r.Periodo == PeriodoRevision.FEEDBACK_MITAD_ANIO)?.Puntaje ?? 0;
        double fin = objetivo.EvaluacionFinal?.PuntajeFinal ?? 0;

        return (feedback * 0.5) + (fin * 0.5);
    }

    // Valoracion ponderada de un objetivo (1-5) - RN-07
    public async Task<double> CalcularPonderadoAsync(int objetivoId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var objetivo = await db.Objetivos
            .Include(o => o.Revisiones)
            .Include(o => o.EvaluacionFinal)
            .FirstOrDefaultAsync(o => o.Id == objetivoId);

        return CalcularPonderadoInterno(objetivo);
    }

    // Rendimiento de empleado por pilar (0-5) - RN-07
    public async Task<double> RendimientoPorPilarAsync(int empleadoId, int pilarId, int anio)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var objetivo = await db.Objetivos
            .Include(o => o.Revisiones)
            .Include(o => o.EvaluacionFinal)
            .FirstOrDefaultAsync(o => o.EmpleadoId == empleadoId
                                   && o.PilarId == pilarId
                                   && o.Anio == anio);

        if (objetivo == null) return 0;

        return CalcularPonderadoInterno(objetivo);
    }

    // Promedio general del empleado (0-5) - RN-07 - AHORA PONDERADO POR PORCENTAJE DE OBJETIVO
    public async Task<double> PromedioGeneralAsync(int empleadoId, int anio)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var objetivos = await db.Objetivos
            .Include(o => o.Revisiones)
            .Include(o => o.EvaluacionFinal)
            .Where(o => o.EmpleadoId == empleadoId && o.Anio == anio && o.Estado != EstadoObjetivo.CANCELADO)
            .ToListAsync();
        if (!objetivos.Any()) return 0;

        double sumaPonderada = 0;
        decimal sumaPesos = 0;

        foreach (var obj in objetivos)
        {
            double score = CalcularPonderadoInterno(obj);
            if (score > 0)
            {
                sumaPonderada += score * (double)obj.PorcentajePilar;
                sumaPesos += obj.PorcentajePilar;
            }
        }

        if (sumaPesos == 0) return 0;

        // Retornar promedio ponderado (normalizado a escala 1-5 si los pesos suman 100)
        return sumaPonderada / (double)sumaPesos;
    }

    // Semáforo (usar para badges y cards) - RN-07
    public string GetSemaforoColor(double promedio)
    {
        if (promedio >= 4.0) return "var(--rz-success)";   // Verde
        if (promedio >= 3.0) return "var(--rz-warning)";   // Amarillo
        return "var(--rz-danger)";                         // Rojo
    }

    // Display sobre 100 - RN-07
    public int DisplayValoracion(double promedio)
    {
        return (int)Math.Round(promedio * 20);
    }

    // Recalcular progreso objetivo - RN-07
    public async Task RecalcularProgresoObjetivoAsync(int objetivoId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var objetivo = await db.Objetivos
            .Include(o => o.Revisiones)
            .FirstOrDefaultAsync(o => o.Id == objetivoId);

        if (objetivo == null) return;

        var revisionesCompletadas = objetivo.Revisiones.Where(r => r.Completada && r.Puntaje.HasValue);
        if (!revisionesCompletadas.Any())
        {
            objetivo.Progreso = 0;
        }
        else
        {
            double promedio = revisionesCompletadas.Average(r => r.Puntaje!.Value);
            objetivo.Progreso = (int)Math.Round(promedio * 20);
        }

        await db.SaveChangesAsync();
    }
}
