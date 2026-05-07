using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Domain.Entities;
using Objetivos.Web.Domain.Enums;
using Objetivos.Web.Services;

namespace Objetivos.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext db, IWebHostEnvironment env, IConfiguration config)
    {
        // Separar: BD nueva (nómina) vs tablas de configuración (siempre completar)
        bool esBdNueva = !await db.Paises.AnyAsync();

        // Declarar fuera del if para que sean accesibles más abajo
        var rows = new List<NominaRow>();
        var areaMap = new Dictionary<string, Area>(StringComparer.OrdinalIgnoreCase);
        var superUserEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // SOLO si es BD NUEVA: cargar nómina y áreas
        if (esBdNueva)
        {
            // ─── Países ───
            var paisArgentina = new Pais { Id = 1, Nombre = "Argentina" };
            var paisChile = new Pais { Id = 2, Nombre = "Chile" };
            var paisUruguay = new Pais { Id = 3, Nombre = "Uruguay" };
            db.Paises.AddRange(paisArgentina, paisChile, paisUruguay);

            // ─── SuperUsers from Config ───
            var superUserEmailsConfig = config.GetSection("SuperUsers:Emails").Get<string[]>() ?? Array.Empty<string>();
            superUserEmails = new HashSet<string>(superUserEmailsConfig, StringComparer.OrdinalIgnoreCase);

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
            int areaId = 1;
            foreach (var name in areaNames)
            {
                var area = new Area { Id = areaId++, Nombre = name, Descripcion = name };
                areaMap[name] = area;
            }
            db.Areas.AddRange(areaMap.Values);

            // Guardar nómina y áreas
            await db.SaveChangesAsync();
        }

        // ─── SIEMPRE: Completar tablas de configuración (no dependen de BD nueva/existente) ───

        // Pilares: agregar solo si faltan
        if (!await db.Pilares.AnyAsync())
        {
            var pilares = new List<Pilar>
            {
                new Pilar {
                    Id = 1, Nombre = "Crecimiento de Ventas", ColorHex = "#4CAF50", Orden = 1, Activo = true,
                    Descripcion = "Este objetivo se centra en incrementar los ingresos a través de la expansión de la base de clientes, la mejora de la oferta de productos o servicios, y la optimización de las estrategias de ventas. Implica explorar nuevos mercados, nuevos productos y nuevos segmentos, fortalecer relaciones con los clientes actuales y desarrollar tácticas innovadoras para aumentar las ventas en el corto, mediano y largo plazo."
                },
                new Pilar {
                    Id = 2, Nombre = "Orientación al Cliente", ColorHex = "#2196F3", Orden = 2, Activo = true,
                    Descripcion = "Optimización y Experiencia Integral (Interna y Externa). Este pilar busca optimizar la totalidad del ciclo posterior a la venta, entendiendo que el servicio de calidad hacia afuera es el resultado de una operación interna eficiente y coordinada.\n\n1. Compromiso con el Cliente Externo: Buscamos asegurar un servicio ágil que supere las expectativas, reduciendo los tiempos de respuesta y resolviendo cualquier incidencia con precisión. El objetivo es que cada contacto postventa fortalezca la confianza en la marca, transformando una transacción en una relación de fidelidad a largo plazo.\n\n2. Fortalecimiento del Cliente Interno: Para lograrlo, optimizamos nuestros procesos internos proporcionando a nuestros colaboradores las herramientas, la información y el soporte necesarios para actuar con autonomía y eficacia."
                },
                new Pilar {
                    Id = 3, Nombre = "Eficiencia Organizacional", ColorHex = "#F9A825", Orden = 3, Activo = true,
                    Descripcion = "Este objetivo busca mejorar el rendimiento interno de la compañía a través de la optimización de sus recursos, la eliminación de ineficiencias y la implementación de mejores prácticas en la gestión de los procesos. La eficiencia organizacional se traduce en un entorno de trabajo más ágil, con procesos más simplificados y una significativa reducción de costos operativos. Al fomentar una cultura de mejora continua y ahorro estratégico, logramos maximizar tanto los resultados financieros como la calidad en todas las áreas, asegurando la sostenibilidad del negocio a largo plazo."
                }
            };
            db.Pilares.AddRange(pilares);
        }

        // ─── SoftSkills ───
        if (!await db.SoftSkills.AnyAsync())
        {
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
                db.SoftSkills.Add(new SoftSkill
                {
                    Id = i + 1,
                    Nombre = skillNames[i],
                    Descripcion = $"Definición pendiente de carga por RRHH para: {skillNames[i]}.",
                    Activo = true,
                    Orden = i + 1
                });
            }
        }

        // ─── Cursos ───
        if (!await db.Cursos.AnyAsync())
        {
            var cursos = new List<Curso>
            {
                new Curso { Nombre = "Liderazgo Efectivo", Categoria = "Soft Skills", DuracionHoras = 20, EsObligatorio = true, UrlImagen = "https://images.unsplash.com/photo-1542744173-8e7e53415bb0?q=80&w=400", Descripcion = "Desarrolla habilidades para guiar equipos de alto rendimiento." },
                new Curso { Nombre = "Gestión del Tiempo", Categoria = "Productividad", DuracionHoras = 10, EsObligatorio = false, UrlImagen = "https://images.unsplash.com/photo-1506784983877-45594efa4cbe?q=80&w=400", Descripcion = "Optimiza tu jornada laboral con técnicas avanzadas." },
                new Curso { Nombre = "Excel Avanzado", Categoria = "Técnica", DuracionHoras = 30, EsObligatorio = true, UrlImagen = "https://images.unsplash.com/photo-1596495573105-d14658ce6091?q=80&w=400", Descripcion = "Domina tablas dinámicas y macros." }
            };
            db.Cursos.AddRange(cursos);
        }

        // ─── Escala de Valoración ───
        if (!await db.EscalasValoracion.AnyAsync())
        {
            var escalas = new List<EscalaValoracion>
            {
                new EscalaValoracion { Etiqueta = "Excelente", ValorNumerico = 5, Orden = 1, Activo = true },
                new EscalaValoracion { Etiqueta = "Muy bueno", ValorNumerico = 4, Orden = 2, Activo = true },
                new EscalaValoracion { Etiqueta = "Bueno", ValorNumerico = 3, Orden = 3, Activo = true },
                new EscalaValoracion { Etiqueta = "Regular", ValorNumerico = 2, Orden = 4, Activo = true },
                new EscalaValoracion { Etiqueta = "Malo", ValorNumerico = 1, Orden = 5, Activo = true }
            };
            db.EscalasValoracion.AddRange(escalas);
        }

        // ─── Estados Objetivo ───
        if (!await db.EstadosObjetivoConfig.AnyAsync())
        {
            var estadosObjetivo = new List<EstadoObjetivoConfig>
            {
                new EstadoObjetivoConfig { Nombre = "Borrador", Slug = "borrador", ColorHex = "#9E9E9E", Orden = 1, Activo = true },
                new EstadoObjetivoConfig { Nombre = "Pendiente de aprobación", Slug = "pendiente_aprobacion", ColorHex = "#FF9800", Orden = 2, Activo = true },
                new EstadoObjetivoConfig { Nombre = "Aprobado", Slug = "aprobado", ColorHex = "#4CAF50", Orden = 3, Activo = true },
                new EstadoObjetivoConfig { Nombre = "En curso", Slug = "en_curso", ColorHex = "#2196F3", Orden = 4, Activo = true },
                new EstadoObjetivoConfig { Nombre = "Completado", Slug = "completado", ColorHex = "#8BC34A", Orden = 5, Activo = true },
                new EstadoObjetivoConfig { Nombre = "Vencido", Slug = "vencido", ColorHex = "#F44336", Orden = 6, Activo = true }
            };
            db.EstadosObjetivoConfig.AddRange(estadosObjetivo);
        }

        // ─── Estados Evaluación ───
        if (!await db.EstadosEvaluacionConfig.AnyAsync())
        {
            var estadosEvaluacion = new List<EstadoEvaluacionConfig>
            {
                new EstadoEvaluacionConfig { Nombre = "Pendiente", Slug = "pendiente", ColorHex = "#FF9800", Orden = 1, Activo = true },
                new EstadoEvaluacionConfig { Nombre = "En progreso", Slug = "en_progreso", ColorHex = "#2196F3", Orden = 2, Activo = true },
                new EstadoEvaluacionConfig { Nombre = "Completada", Slug = "completada", ColorHex = "#4CAF50", Orden = 3, Activo = true },
                new EstadoEvaluacionConfig { Nombre = "Próxima a vencer", Slug = "proxima_a_vencer", ColorHex = "#FF5722", Orden = 4, Activo = true }
            };
            db.EstadosEvaluacionConfig.AddRange(estadosEvaluacion);
        }

        // ─── Configuración Plataforma (upsert por clave) ───
        var configsRequeridas = new List<ConfiguracionPlataforma>
        {
            new ConfiguracionPlataforma { Clave = "email_soporte", Valor = "rrhh@permaquim.com", Descripcion = "Email de ayuda e inconvenientes", Tipo = "email" },
            new ConfiguracionPlataforma { Clave = "dias_proximo_vencimiento", Valor = "7", Descripcion = "Días antes del vencimiento para marcar como 'Próximo a vencer'", Tipo = "integer" },
            new ConfiguracionPlataforma { Clave = "objetivo_area_habilitado", Valor = "true", Descripcion = "Habilita objetivo específico por área en evaluaciones", Tipo = "boolean" },
            new ConfiguracionPlataforma { Clave = "calculos_comerciales_habilitados", Valor = "false", Descripcion = "Habilita cálculos del área comercial (diferido)", Tipo = "boolean" },
            new ConfiguracionPlataforma { Clave = "resultado_final_manual", Valor = "true", Descripcion = "Resultado final de evaluación ingresado manualmente por el jefe", Tipo = "boolean" },
            new ConfiguracionPlataforma { Clave = "jefe_puede_crear_objetivos", Valor = "false", Descripcion = "Si es true, el rol jefe puede crear objetivos. Si es false, solo empleados pueden crearlos", Tipo = "boolean" },
            new ConfiguracionPlataforma { Clave = "texto_guia_plataforma", Valor = "", Descripcion = "Contenido del manual de uso en la sección Guía", Tipo = "text" }
        };
        foreach (var conf in configsRequeridas)
        {
            if (!await db.ConfiguracionesPlataforma.AnyAsync(c => c.Clave == conf.Clave))
            {
                db.ConfiguracionesPlataforma.Add(conf);
            }
        }

        await db.SaveChangesAsync();

        // Si NO es BD nueva, no procesar jefes/empleados (ya existen)
        if (!esBdNueva) return;

        // ─── Classify people: Jefes vs Empleados ───
        // Roles that imply management: "Jefe", "Gerente", "Director", "Director General"
        var jefeRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "Jefe", "Gerente", "Director", "Director General", "RRHH" };

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

            var fallbackJefeId = jefeMap.Values.FirstOrDefault()?.Id ?? 1;
            Console.WriteLine($"[SeedData WARNING] No se pudo resolver jefe para: '{responsable}'. Se asignó fallback ID={fallbackJefeId}");
            return fallbackJefeId;
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
        else if (commaIdx == 0)
        {
            // Case ", Nombre"
            var nombre = apellidoYNombre[1..].Trim();
            return (nombre, "");
        }

        // Otherwise split by space; first word = apellido, rest = nombre
        var parts = apellidoYNombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return ("", "");
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
