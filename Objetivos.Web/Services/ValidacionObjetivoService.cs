using Objetivos.Web.Domain.Entities;

namespace Objetivos.Web.Services;

public class ValidacionObjetivoService
{
    // VAL-14: Validar que la suma de porcentajes = 100%
    // Usado en frontend para validación en tiempo real y backend para persistencia
    public bool ValidarPorcentajesPilares(decimal? porcentajePilar)
    {
        if (porcentajePilar == null)
            return false;

        // Por ahora, soportamos un solo objetivo por pilar y empleado/año
        // La validación es que el porcentaje sea entre 0 y 100
        return porcentajePilar >= 0 && porcentajePilar <= 100;
    }

    // VAL-15: Validar suma de porcentajes en múltiples objetivos
    // Si el empleado tiene varios objetivos (múltiples pilares), la suma debe ser 100%
    public bool ValidarSumaPorcentajesPilares(List<Objetivo> objetivosEmpleado, decimal tolerancia = 0.01m)
    {
        if (!objetivosEmpleado.Any())
            return true; // Sin objetivos es válido

        var sumaPorcentajes = objetivosEmpleado.Sum(o => o.PorcentajePilar);

        // Permitir pequeña desviación por redondeos decimales (0.01%)
        return Math.Abs(sumaPorcentajes - 100) <= tolerancia;
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
