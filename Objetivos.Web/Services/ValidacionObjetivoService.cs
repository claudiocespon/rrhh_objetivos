using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;
using Objetivos.Web.Domain.Enums;

namespace Objetivos.Web.Services;

public class ValidacionObjetivoService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public ValidacionObjetivoService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // VAL-14: Validar que la suma de porcentajes = 100%
    // Usado en frontend para validación en tiempo real y backend para persistencia
    public bool ValidarPorcentajesPilares(decimal? porcentajePilar)
    {
        if (porcentajePilar == null)
            return false;

        // Por ahora, soportamos un solo objetivo por pilar y usuario/año
        // La validación es que el porcentaje sea entre 0 y 100
        return porcentajePilar >= 0 && porcentajePilar <= 100;
    }

    // VAL-15: Validar suma de porcentajes en múltiples objetivos
    // Si el usuario tiene varios objetivos (múltiples pilares), la suma debe ser 100%
    public bool ValidarSumaPorcentajesPilares(List<Objetivo> objetivosEmpleado, decimal tolerancia = 0.01m)
    {
        if (!objetivosEmpleado.Any())
            return false; // Sin objetivos no llega a 100

        var sumaPorcentajes = objetivosEmpleado.Sum(o => o.PorcentajePilar);

        // Permitir pequeña desviación por redondeos decimales (0.01%)
        return Math.Abs(sumaPorcentajes - 100) <= tolerancia;
    }

    public bool ValidarTotalExacto(decimal suma, decimal tolerancia = 0.01m)
    {
        return Math.Abs(suma - 100) <= tolerancia;
    }

    // Validar que el nuevo peso no exceda el 100% total (en memoria, sin consultar BD)
    public bool ValidarNuevoPeso(List<Objetivo> objetivosExistentes, decimal nuevoPeso, int? objetivoIdEditando = null)
    {
        var sumaExistente = objetivosExistentes
            .Where(o => o.Id != objetivoIdEditando)
            .Sum(o => o.PorcentajePilar);

        return (sumaExistente + nuevoPeso) <= 100.01m; // Tolerancia por decimales
    }

    /// <summary>
    /// Centraliza VAL-06: consulta la BD para validar que la suma de pesos no exceda 100%.
    /// Único punto de verdad — usado por ObjetivoService, CrearObjetivoDialog y EditarObjetivoDialog.
    /// </summary>
    /// <param name="usuarioId">Usuario dueño de los objetivos.</param>
    /// <param name="anio">Año del ejercicio.</param>
    /// <param name="nuevoPeso">Peso porcentual del nuevo/editado objetivo.</param>
    /// <param name="objetivoIdEditando">Id a excluir de la suma (caso edición). Null en creación.</param>
    /// <returns>Tupla (Ok, SumaActual) con suma actual sin contar el editado.</returns>
    public async Task<(bool Ok, decimal SumaActual)> ValidarSumaPesoAsync(
        int usuarioId, int anio, decimal nuevoPeso, int? objetivoIdEditando = null)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        var sumaActual = await db.Objetivos
            .Where(o => o.UsuarioId == usuarioId
                     && o.Anio == anio
                     && o.Estado != EstadoObjetivo.CANCELADO
                     && (objetivoIdEditando == null || o.Id != objetivoIdEditando.Value))
            .SumAsync(o => o.PorcentajePilar);

        return (sumaActual + nuevoPeso <= 100.01m, sumaActual);
    }

    // Obtener mensaje de error para porcentajes
    public string ObtenerMensajeErrorPorcentaje(List<Objetivo> objetivosEmpleado)
    {
        var sumaPorcentajes = objetivosEmpleado.Sum(o => o.PorcentajePilar);
        var diferencia = 100 - sumaPorcentajes;

        if (diferencia > 0)
            return $"Los porcentajes suman {sumaPorcentajes}%. Faltan {diferencia}% para llegar a 100%";
        else if (diferencia < 0)
            return $"Los porcentajes suman {sumaPorcentajes}%. Exceden en {Math.Abs(diferencia)}% el límite de 100%";
        else
            return "Los porcentajes suman correctamente 100%";
    }
}
