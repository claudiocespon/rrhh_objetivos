using System.Text;
using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;
using Objetivos.Web.Domain.Enums;

namespace Objetivos.Web.Services
{
    public class ExportService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public ExportService(IDbContextFactory<AppDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // ── Exportación genérica (usada desde MisObjetivos) ──────────────────

        public byte[] ExportObjetivosToCsv(List<Objetivo> objetivos)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ID;Empleado;Email;Area;Pilar;Objetivo;Progreso;Estado;Deadline;Año;Competencia1;Competencia2");

            foreach (var o in objetivos)
            {
                sb.AppendLine(string.Join(";",
                    o.Id,
                    SanitizeCsvField($"{o.Empleado?.Apellido}, {o.Empleado?.Nombre}"),
                    SanitizeCsvField(o.Empleado?.Email),
                    SanitizeCsvField(o.Empleado?.Area?.Nombre),
                    SanitizeCsvField(o.Pilar?.Nombre),
                    SanitizeCsvField(o.Nombre),
                    o.Progreso,
                    o.Estado,
                    o.Deadline.ToString("dd/MM/yyyy"),
                    o.Anio,
                    SanitizeCsvField(o.SoftSkill1?.Nombre),
                    SanitizeCsvField(o.SoftSkill2?.Nombre)
                ));
            }

            return ToUtf8Bom(sb.ToString());
        }

        // ── Etapa 8.1: Reporte por empleado (para jefes) ─────────────────────

        public async Task<byte[]> ExportarReporteEmpleadoAsync(int empleadoId, int anio)
        {
            using var db = await _dbFactory.CreateDbContextAsync();

            var empleado = await db.Empleados
                .Include(e => e.Area)
                .FirstOrDefaultAsync(e => e.Id == empleadoId);

            if (empleado == null) return ToUtf8Bom("");

            var objetivos = await db.Objetivos
                .Include(o => o.Pilar)
                .Include(o => o.SoftSkill1)
                .Include(o => o.SoftSkill2)
                .Include(o => o.Revisiones).ThenInclude(r => r.EscalaValoracion)
                .Include(o => o.EvaluacionFinal).ThenInclude(e => e!.EscalaValoracionFinal)
                .Include(o => o.Autoevaluacion)
                .Where(o => o.EmpleadoId == empleadoId && o.Anio == anio)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine($"Reporte de Empleado — {empleado.Apellido}, {empleado.Nombre} — Año {anio}");
            sb.AppendLine($"Área: {empleado.Area?.Nombre}");
            sb.AppendLine();
            sb.AppendLine("Pilar;Objetivo;Progreso;Estado;Feedback Score;Resultado Final;Puntaje Ponderado;Semáforo;Competencia1;Competencia2");

            foreach (var o in objetivos)
            {
                var feedback = o.Revisiones.FirstOrDefault(r => r.Periodo == PeriodoRevision.FEEDBACK_MITAD_ANIO);
                var ponderado = RendimientoService.CalcularPonderadoStatic(o);
                var semaforo = ponderado >= 4.0 ? "Verde" : ponderado >= 3.0 ? "Amarillo" : ponderado > 0 ? "Rojo" : "Sin datos";

                sb.AppendLine(string.Join(";",
                    SanitizeCsvField(o.Pilar?.Nombre),
                    SanitizeCsvField(o.Nombre),
                    o.Progreso + "%",
                    o.Estado,
                    feedback?.EscalaValoracion?.Etiqueta ?? "-",
                    o.EvaluacionFinal?.EscalaValoracionFinal?.Etiqueta ?? "-",
                    ponderado > 0 ? ponderado.ToString("F2") : "-",
                    semaforo,
                    SanitizeCsvField(o.SoftSkill1?.Nombre),
                    SanitizeCsvField(o.SoftSkill2?.Nombre)
                ));
            }

            return ToUtf8Bom(sb.ToString());
        }

        // ── Etapa 8.1: Reporte de área (para RRHH/DG) ────────────────────────

        public async Task<byte[]> ExportarReporteAreaAsync(int areaId, int anio)
        {
            using var db = await _dbFactory.CreateDbContextAsync();

            var empleados = await db.Empleados
                .Include(e => e.Area)
                .Include(e => e.Objetivos.Where(o => o.Anio == anio))
                    .ThenInclude(o => o.Revisiones)
                .Include(e => e.Objetivos.Where(o => o.Anio == anio))
                    .ThenInclude(o => o.EvaluacionFinal)
                .Where(e => e.AreaId == areaId && e.Activo)
                .OrderBy(e => e.Apellido)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine($"Reporte de Área — Año {anio}");
            sb.AppendLine();
            sb.AppendLine("Empleado;Email;Objetivos Activos;Objetivos Completados;En Riesgo;Promedio Ponderado;Semáforo");

            foreach (var emp in empleados)
            {
                var activos = emp.Objetivos.Count(o => o.Estado == EstadoObjetivo.ACTIVO);
                var completados = emp.Objetivos.Count(o => o.Estado == EstadoObjetivo.COMPLETADO);
                var enRiesgo = emp.Objetivos.Count(o => o.Estado == EstadoObjetivo.EN_RIESGO);
                var promedios = emp.Objetivos
                    .Where(o => o.Estado != EstadoObjetivo.CANCELADO)
                    .Select(o => RendimientoService.CalcularPonderadoStatic(o))
                    .Where(v => v > 0)
                    .ToList();
                var promedio = promedios.Count > 0 ? promedios.Average() : 0;
                var semaforo = promedio >= 4.0 ? "Verde" : promedio >= 3.0 ? "Amarillo" : promedio > 0 ? "Rojo" : "Sin datos";

                sb.AppendLine(string.Join(";",
                    SanitizeCsvField($"{emp.Apellido}, {emp.Nombre}"),
                    SanitizeCsvField(emp.Email),
                    activos,
                    completados,
                    enRiesgo,
                    promedio > 0 ? promedio.ToString("F2") : "-",
                    semaforo
                ));
            }

            return ToUtf8Bom(sb.ToString());
        }

        // ── Etapa 8.1: Mis datos (para empleado) ─────────────────────────────

        public async Task<byte[]> ExportarMisDatosAsync(int empleadoId, int anio)
        {
            return await ExportarReporteEmpleadoAsync(empleadoId, anio);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static byte[] ToUtf8Bom(string content)
            => Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(content)).ToArray();

        private string SanitizeCsvField(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            char[] injectionChars = { '=', '+', '-', '@', '\t', '\r' };
            if (injectionChars.Any(c => value.StartsWith(c)))
                value = "'" + value;

            return value.Replace(";", ",").Replace("\n", " ").Replace("\r", " ").Trim();
        }
    }
}
