using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Domain.Entities;
using Objetivos.Web.Domain.Enums;
using Objetivos.Web.Services;

namespace Objetivos.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext db, IWebHostEnvironment env)
    {
        if (await db.Paises.AnyAsync()) return;

        // ─── Países ───
        var paisArgentina = new Pais { Id = 1, Nombre = "Argentina" };
        var paisChile = new Pais { Id = 2, Nombre = "Chile" };
        var paisUruguay = new Pais { Id = 3, Nombre = "Uruguay" };
        db.Paises.AddRange(paisArgentina, paisChile, paisUruguay);

        // ─── Read CSV ───
        var csvPath = Path.Combine(env.ContentRootPath, "Data", "Nomina.csv");
        
        var lines = new List<string>();
        using (var fs = new FileStream(csvPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var sr = new StreamReader(fs, Encoding.UTF8))
        {
            string? line;
            while ((line = await sr.ReadLineAsync()) != null)
            {
                lines.Add(line);
            }
        }
        // Skip header
        var dataLines = lines.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

        // ─── Parse rows ───
        var rows = new List<NominaRow>();
        foreach (var line in dataLines)
        {
            var parts = line.Split(';');
            if (parts.Length < 9) continue;
            rows.Add(new NominaRow
            {
                CentroCostos = parts[0].Trim(),
                DescripcionCC = parts[1].Trim(),
                Sector = parts[2].Trim(),
                ApellidoYNombre = parts[3].Trim(),
                Legajo = parts[4].Trim(),
                ResponsableEvaluacion = parts[5].Trim(),
                Mail = parts[6].Trim(),
                Pais = parts[7].Trim(),
                Rol = parts[8].Trim()
            });
        }

        // ─── Areas (from DescripcionCC, deduplicated) ───
        var areaNames = rows.Select(r => r.DescripcionCC)
            .Where(d => !string.IsNullOrWhiteSpace(d) && !d.Equals("No aplica", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        // Add "No aplica" as fallback area
        areaNames.Insert(0, "Sin Asignar");
        var areaMap = new Dictionary<string, Area>(StringComparer.OrdinalIgnoreCase);
        int areaId = 1;
        foreach (var name in areaNames)
        {
            var area = new Area { Id = areaId++, Nombre = name, Descripcion = name };
            areaMap[name] = area;
        }
        db.Areas.AddRange(areaMap.Values);

        // ─── Pilares (keep original) ───
        var pilares = new List<Pilar>
        {
            new Pilar { Id = 1, Nombre = "EXCELENCIA_ORGANIZACIONAL", ColorHex = "#2563EB" },
            new Pilar { Id = 2, Nombre = "INNOVACION_MEJORA",         ColorHex = "#7C3AED" },
            new Pilar { Id = 3, Nombre = "ORIENTACION_CLIENTE",       ColorHex = "#059669" }
        };
        db.Pilares.AddRange(pilares);

        // ─── SoftSkills ───
        var skillNames = new string[]
        {
            "Comunicación efectiva", "Liderazgo", "Trabajo en equipo",
            "Resolución de problemas", "Pensamiento crítico", "Adaptabilidad",
            "Gestión del tiempo", "Creatividad", "Inteligencia emocional",
            "Negociación", "Toma de decisiones", "Empatía",
            "Proactividad", "Resiliencia", "Orientación a resultados",
            "Planificación estratégica", "Delegación efectiva", "Escucha activa",
            "Gestión de conflictos", "Mentoría"
        };
        for (int i = 0; i < skillNames.Length; i++)
        {
            db.SoftSkills.Add(new SoftSkill { Id = i + 1, Nombre = $"SS{i + 1:D2}-{skillNames[i]}" });
        }

        await db.SaveChangesAsync();

        // ─── Classify people: Jefes vs Empleados ───
        // Roles that imply management: "Jefe", "Gerente", "Director", "Director General"
        var jefeRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "Jefe", "Gerente", "Director", "Director General", "RRHH" };

        var superUserEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ccespon@permaquim.com",
            "ptripodi@permaquim.com",
            "scrosio@permaquim.com"
        };

        // First pass: create Jefes
        var jefeMap = new Dictionary<string, Jefe>(StringComparer.OrdinalIgnoreCase);
        int jefeIdCounter = 1;
        foreach (var row in rows)
        {
            if (!jefeRoles.Contains(row.Rol)) continue;
            if (jefeMap.ContainsKey(row.Mail.ToLower())) continue;
            if (row.Mail.Equals("No aplica", StringComparison.OrdinalIgnoreCase)) continue;

            var (nombre, apellido) = ParseNombre(row.ApellidoYNombre);
            var areaName = row.DescripcionCC.Equals("No aplica", StringComparison.OrdinalIgnoreCase)
                ? "Sin Asignar" : row.DescripcionCC;
            var paisId = GetPaisId(row.Pais);

            var jefe = new Jefe
            {
                Id = jefeIdCounter++,
                Nombre = nombre,
                Apellido = apellido,
                Email = row.Mail.ToLower(),
                Legajo = row.Legajo,
                PasswordHash = AuthService.HashPassword(row.Legajo),
                DebeCambiarPassword = true,
                AreaId = areaMap.ContainsKey(areaName) ? areaMap[areaName].Id : areaMap["Sin Asignar"].Id,
                PaisId = paisId,
                Rol = MapRol(row.Rol),
                Activo = true,
                EsSuperusuario = superUserEmails.Contains(row.Mail.ToLower())
            };
            jefeMap[row.Mail.ToLower()] = jefe;
        }
        db.Jefes.AddRange(jefeMap.Values);
        await db.SaveChangesAsync();

        // ─── Build a lookup of "ResponsableEvaluacion" name → JefeId ───
        var jefeByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var j in jefeMap.Values)
        {
            var fullName1 = $"{j.Apellido}, {j.Nombre}";
            var fullName2 = $"{j.Nombre} {j.Apellido}";
            var fullName3 = $"{j.Apellido} {j.Nombre}";
            if (!jefeByName.ContainsKey(fullName1)) jefeByName[fullName1] = j.Id;
            if (!jefeByName.ContainsKey(fullName2)) jefeByName[fullName2] = j.Id;
            if (!jefeByName.ContainsKey(fullName3)) jefeByName[fullName3] = j.Id;
        }

        // Also map from original CSV name format (in case of variations)
        foreach (var row in rows)
        {
            if (!jefeRoles.Contains(row.Rol)) continue;
            var email = row.Mail.ToLower();
            if (jefeMap.ContainsKey(email) && !jefeByName.ContainsKey(row.ApellidoYNombre))
            {
                jefeByName[row.ApellidoYNombre] = jefeMap[email].Id;
            }
        }

        int FindJefeId(string responsable)
        {
            if (string.IsNullOrWhiteSpace(responsable) || responsable.Equals("No aplica", StringComparison.OrdinalIgnoreCase)
                || responsable == "-")
                return jefeMap.Values.FirstOrDefault()?.Id ?? 1;

            // Direct match
            if (jefeByName.TryGetValue(responsable, out var id)) return id;

            // Try partial match
            foreach (var kvp in jefeByName)
            {
                if (kvp.Key.Contains(responsable, StringComparison.OrdinalIgnoreCase) ||
                    responsable.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                    return kvp.Value;
            }

            // Try matching by last name
            var parts = responsable.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                var lastName = parts[0].Trim();
                foreach (var kvp in jefeByName)
                {
                    if (kvp.Key.StartsWith(lastName, StringComparison.OrdinalIgnoreCase))
                        return kvp.Value;
                }
            }

            return jefeMap.Values.FirstOrDefault()?.Id ?? 1;
        }

        // Second pass: create Empleados (Colaboradores)
        int empIdCounter = 1;
        var empleadoEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in rows)
        {
            // Skip Jefes already added (unless they are "Colaborador")
            if (jefeRoles.Contains(row.Rol)) continue;
            // Skip "No Aplica" role entries without email
            if (row.Mail.Equals("No aplica", StringComparison.OrdinalIgnoreCase)) continue;
            if (string.IsNullOrWhiteSpace(row.Mail)) continue;
            // Skip duplicate emails
            if (empleadoEmails.Contains(row.Mail.ToLower())) continue;
            empleadoEmails.Add(row.Mail.ToLower());
            // Skip if email already used by a Jefe
            if (jefeMap.ContainsKey(row.Mail.ToLower())) continue;

            var (nombre, apellido) = ParseNombre(row.ApellidoYNombre);
            var areaName = row.DescripcionCC.Equals("No aplica", StringComparison.OrdinalIgnoreCase)
                ? "Sin Asignar" : row.DescripcionCC;
            var paisId = GetPaisId(row.Pais);
            var jefeId = FindJefeId(row.ResponsableEvaluacion);

            var empleado = new Empleado
            {
                Id = empIdCounter++,
                Nombre = nombre,
                Apellido = apellido,
                Email = row.Mail.ToLower(),
                Legajo = row.Legajo,
                PasswordHash = AuthService.HashPassword(row.Legajo),
                DebeCambiarPassword = true,
                Puesto = row.Sector,
                AreaId = areaMap.ContainsKey(areaName) ? areaMap[areaName].Id : areaMap["Sin Asignar"].Id,
                JefeId = jefeId,
                PaisId = paisId,
                Activo = true,
                EsSuperusuario = superUserEmails.Contains(row.Mail.ToLower()),
                FechaIngreso = DateTime.UtcNow
            };
            db.Empleados.Add(empleado);
        }
        await db.SaveChangesAsync();
    }

    private static (string Nombre, string Apellido) ParseNombre(string apellidoYNombre)
    {
        // Format: "Apellido, Nombre" or "Apellido Nombre" or "Nombre Apellido"
        if (string.IsNullOrWhiteSpace(apellidoYNombre))
            return ("", "");

        // Try comma-separated first
        var commaIdx = apellidoYNombre.IndexOf(',');
        if (commaIdx > 0)
        {
            var apellido = apellidoYNombre[..commaIdx].Trim();
            var nombre = apellidoYNombre[(commaIdx + 1)..].Trim();
            return (nombre, apellido);
        }

        // Otherwise split by space; first word = apellido, rest = nombre
        var parts = apellidoYNombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return (parts[0], "");
        if (parts.Length == 2) return (parts[1], parts[0]);

        // 3+ words: first word = apellido, rest = nombre
        return (string.Join(" ", parts.Skip(1)), parts[0]);
    }

    private static int GetPaisId(string pais)
    {
        if (pais.Contains("Argentina", StringComparison.OrdinalIgnoreCase) &&
            pais.Contains("Chile", StringComparison.OrdinalIgnoreCase))
            return 1; // Default to Argentina for dual-country

        if (pais.Contains("Chile", StringComparison.OrdinalIgnoreCase)) return 2;
        if (pais.Contains("Uruguay", StringComparison.OrdinalIgnoreCase)) return 3;
        return 1; // Default Argentina
    }

    private static string MapRol(string rol) => rol.ToUpper() switch
    {
        "JEFE" => "JEFE",
        "GERENTE" => "GERENTE",
        "DIRECTOR" => "DIRECTOR",
        "DIRECTOR GENERAL" => "DIRECTOR_GENERAL",
        "RRHH" => "RRHH",
        _ => "JEFE"
    };

    private class NominaRow
    {
        public string CentroCostos { get; set; } = "";
        public string DescripcionCC { get; set; } = "";
        public string Sector { get; set; } = "";
        public string ApellidoYNombre { get; set; } = "";
        public string Legajo { get; set; } = "";
        public string ResponsableEvaluacion { get; set; } = "";
        public string Mail { get; set; } = "";
        public string Pais { get; set; } = "";
        public string Rol { get; set; } = "";
    }
}
