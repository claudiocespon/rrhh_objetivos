using System.Text;
using Objetivos.Web.Domain.Entities;

namespace Objetivos.Web.Services
{
    public class ExportService
    {
        public byte[] ExportObjetivosToCsv(List<Objetivo> objetivos)
        {
            var sb = new StringBuilder();
            // Header
            sb.AppendLine("ID;Empleado;Email;Area;Pilar;Objetivo;Progreso;Estado;Deadline;Anio;SoftSkill1;SoftSkill2");

            foreach (var o in objetivos)
            {
                var id = o.Id.ToString();
                var empleado = SanitizeCsvField($"{o.Empleado?.Apellido}, {o.Empleado?.Nombre}");
                var email = SanitizeCsvField(o.Empleado?.Email);
                var area = SanitizeCsvField(o.Empleado?.Area?.Nombre);
                var pilar = SanitizeCsvField(o.Pilar?.Nombre.Replace("_", " "));
                var objetivoNombre = SanitizeCsvField(o.Nombre);
                var progreso = o.Progreso.ToString();
                var estado = o.Estado.ToString();
                var deadline = o.Deadline.ToString("dd/MM/yyyy");
                var anio = o.Anio.ToString();
                var ss1 = SanitizeCsvField(o.SoftSkill1?.Nombre);
                var ss2 = SanitizeCsvField(o.SoftSkill2?.Nombre);

                sb.AppendLine($"{id};{empleado};{email};{area};{pilar};{objetivoNombre};{progreso};{estado};{deadline};{anio};{ss1};{ss2}");
            }

            var content = sb.ToString();
            return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(content)).ToArray();
        }

        private string SanitizeCsvField(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            // CSV Injection prevention: if it starts with special chars, prefix with '
            char[] injectionChars = { '=', '+', '-', '@', '\t', '\r' };
            if (injectionChars.Any(c => value.StartsWith(c)))
            {
                value = "'" + value;
            }

            // Remove semicolons (delimiter) and newlines
            return value.Replace(";", ",").Replace("\n", " ").Replace("\r", " ").Trim();
        }
    }
}
