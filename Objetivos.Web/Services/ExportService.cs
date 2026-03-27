using System.Text;
using Objetivos.Web.Domain.Entities;

namespace Objetivos.Web.Services
{
    public class ExportService
    {
        public byte[] ExportObjetivosToCsv(List<Objetivo> objetivos)
        {
            var sb = new StringBuilder();
            // Header with BOM for Excel compatibility with UTF-8
            sb.AppendLine("ID;Empleado;Email;Area;Pilar;Objetivo;Progreso;Estado;Deadline;Anio;SoftSkill1;SoftSkill2");

            foreach (var o in objetivos)
            {
                var cleanNombre = (o.Nombre ?? "").Replace(";", ",").Replace("\n", " ").Replace("\r", "");
                sb.AppendLine($"{o.Id};" +
                              $"{o.Empleado?.Apellido}, {o.Empleado?.Nombre};" +
                              $"{o.Empleado?.Email};" +
                              $"{o.Empleado?.Area?.Nombre};" +
                              $"{o.Pilar?.Nombre.Replace("_", " ")};" +
                              $"{cleanNombre};" +
                              $"{o.Progreso};" +
                              $"{o.Estado};" +
                              $"{o.Deadline:dd/MM/yyyy};" +
                              $"{o.Anio};" +
                              $"{o.SoftSkill1?.Nombre};" +
                              $"{o.SoftSkill2?.Nombre}");
            }

            var content = sb.ToString();
            return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(content)).ToArray();
        }
    }
}
