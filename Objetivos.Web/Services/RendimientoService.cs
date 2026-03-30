using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;
using Objetivos.Web.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Objetivos.Web.Services;

public class RendimientoService
{
    private readonly AppDbContext _db;

    public RendimientoService(AppDbContext db)
    {
        _db = db;
    }

    private static double CalcularPonderadoInterno(Objetivo? objetivo)
    {
        if (objetivo == null) return 0;

        double q1 = objetivo.Revisiones.FirstOrDefault(r => r.Periodo == PeriodoRevision.Q1_ABRIL)?.Puntaje ?? 0;
        double q2 = objetivo.Revisiones.FirstOrDefault(r => r.Periodo == PeriodoRevision.Q2_AGOSTO)?.Puntaje ?? 0;
        double q3 = objetivo.Revisiones.FirstOrDefault(r => r.Periodo == PeriodoRevision.Q3_NOVIEMBRE)?.Puntaje ?? 0;
        double fin = objetivo.EvaluacionFinal?.PuntajeFinal ?? 0;

        return (q1 * 0.2) + (q2 * 0.3) + (q3 * 0.3) + (fin * 0.2);
    }

    // Puntaje ponderado de un objetivo (1-5) - RN-07
    public async Task<double> CalcularPonderadoAsync(int objetivoId)
    {
        var objetivo = await _db.Objetivos
            .Include(o => o.Revisiones)
            .Include(o => o.EvaluacionFinal)
            .FirstOrDefaultAsync(o => o.Id == objetivoId);

        return CalcularPonderadoInterno(objetivo);
    }

    // Rendimiento de empleado por pilar (0-5) - RN-07
    public async Task<double> RendimientoPorPilarAsync(int empleadoId, int pilarId, int anio)
    {
        var objetivo = await _db.Objetivos
            .Include(o => o.Revisiones)
            .Include(o => o.EvaluacionFinal)
            .FirstOrDefaultAsync(o => o.EmpleadoId == empleadoId 
                                   && o.PilarId == pilarId 
                                   && o.Anio == anio);
        
        if (objetivo == null) return 0;
        
        return CalcularPonderadoInterno(objetivo);
    }

    // Promedio general del empleado (0-5) - RN-07
    public async Task<double> PromedioGeneralAsync(int empleadoId, int anio)
    {
        var objetivos = await _db.Objetivos
            .Include(o => o.Revisiones)
            .Include(o => o.EvaluacionFinal)
            .Where(o => o.EmpleadoId == empleadoId && o.Anio == anio && o.Estado != EstadoObjetivo.CANCELADO)
            .ToListAsync();

        if (!objetivos.Any()) return 0;

        var scores = objetivos.Select(CalcularPonderadoInterno).ToList();

        var objetivosConDatos = scores.Where(v => v > 0).ToList();
        if (!objetivosConDatos.Any()) return 0;
        
        return objetivosConDatos.Average();
    }

    // Semáforo (usar para badges y cards) - RN-07
    public string GetSemaforoColor(double promedio)
    {
        if (promedio >= 4.0) return "var(--rz-success)";   // Verde
        if (promedio >= 3.0) return "var(--rz-warning)";   // Amarillo
        return "var(--rz-danger)";                         // Rojo
    }

    // Display sobre 100 - RN-07
    public int DisplayScore(double promedio)
    {
        return (int)Math.Round(promedio * 20);
    }

    // Recalcular progreso objetivo - RN-07
    public async Task RecalcularProgresoObjetivoAsync(int objetivoId)
    {
        var objetivo = await _db.Objetivos
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

        await _db.SaveChangesAsync();
    }
}
