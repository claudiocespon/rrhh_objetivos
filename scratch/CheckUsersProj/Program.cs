using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;
using Objetivos.Web.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

var services = new ServiceCollection();
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=c:\\Development\\Antigravity\\RRHH_Objetivos\\Objetivos.Web\\objetivos.db"));

var serviceProvider = services.BuildServiceProvider();
using var scope = serviceProvider.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

// 1. Clean up old transactional data
Console.WriteLine("Cleaning up old transactional data...");
db.EventosCalendario.RemoveRange(await db.EventosCalendario.ToListAsync());
db.CursoAsignaciones.RemoveRange(await db.CursoAsignaciones.ToListAsync());
db.MensajesChat.RemoveRange(await db.MensajesChat.ToListAsync());
db.BitacoraEntradas.RemoveRange(await db.BitacoraEntradas.ToListAsync());
db.Autoevaluaciones.RemoveRange(await db.Autoevaluaciones.ToListAsync());
db.EvaluacionesFinales.RemoveRange(await db.EvaluacionesFinales.ToListAsync());
db.RevisionesCuatrimestrales.RemoveRange(await db.RevisionesCuatrimestrales.ToListAsync());
db.Objetivos.RemoveRange(await db.Objetivos.ToListAsync());
await db.SaveChangesAsync();

// 2. Find Jefe and Empleado
var jefe = await db.Jefes.FirstOrDefaultAsync(j => j.Email == "ptripodi@permaquim.com");
var empleado = await db.Empleados.FirstOrDefaultAsync(e => e.Email == "ncaldiroli@permaquim.com");

if (jefe == null || empleado == null)
{
    Console.WriteLine("CRITICAL ERROR: Pablo Tripodi or Nicolas Caldiroli not found in database!");
    return;
}

// Ensure password change is bypassed for simple testing
jefe.DebeCambiarPassword = false;
empleado.DebeCambiarPassword = false;
await db.SaveChangesAsync();

Console.WriteLine($"Jefe: {jefe.Nombre} {jefe.Apellido} (ID: {jefe.Id}), AreaId: {jefe.AreaId}");
Console.WriteLine($"Empleado: {empleado.Nombre} {empleado.Apellido} (ID: {empleado.Id}), AreaId: {empleado.AreaId}");

// 3. Seed Cursos if missing, and Assign Cursos
var cursoLiderazgo = await db.Cursos.FirstOrDefaultAsync(c => c.Nombre == "Liderazgo Efectivo");
var cursoExcel = await db.Cursos.FirstOrDefaultAsync(c => c.Nombre == "Excel Avanzado");
var cursoGestionTiempo = await db.Cursos.FirstOrDefaultAsync(c => c.Nombre == "Gestión del Tiempo");

Console.WriteLine("Seeding course assignments...");
if (cursoLiderazgo != null)
{
    db.CursoAsignaciones.Add(new CursoAsignacion
    {
        CursoId = cursoLiderazgo.Id,
        EmpleadoId = empleado.Id,
        FechaAsignacion = DateTime.UtcNow.AddDays(-30),
        Completado = false,
        Notas = "Asignado para potenciar habilidades interpersonales y coordinación."
    });
}
if (cursoExcel != null)
{
    db.CursoAsignaciones.Add(new CursoAsignacion
    {
        CursoId = cursoExcel.Id,
        EmpleadoId = empleado.Id,
        FechaAsignacion = DateTime.UtcNow.AddDays(-45),
        FechaCompletado = DateTime.UtcNow.AddDays(-15),
        Completado = true,
        Notas = "Curso obligatorio completado con éxito."
    });
}

// 4. Seed Objetivos
Console.WriteLine("Seeding Objetivos...");

// PILARES: 1 (EXCELENCIA_ORGANIZACIONAL), 2 (INNOVACION_MEJORA), 3 (ORIENTACION_CLIENTE)
// SOFTSKILLS: 21 ("Orientación al cliente"), 28 ("Comunicación efectiva"), 25 ("Pensamiento creativo e innovación"), 26 ("Colaboración y trabajo en equipo"), 27 ("Compromiso con la excelencia"), 24 ("Iniciativa")

var obj1 = new Objetivo
{
    Nombre = "Optimizar soporte técnico de mesa de ayuda",
    Descripcion = "Reducir el tiempo promedio de resolución de incidentes críticos a menos de 4 horas, logrando un 95% de satisfacción en el cliente interno.",
    PilarId = 3, // ORIENTACION_CLIENTE
    EmpleadoId = empleado.Id,
    Anio = 2026,
    Deadline = new DateTime(2026, 11, 30),
    SoftSkill1Id = 21, // Orientación al cliente
    SoftSkill2Id = 28, // Comunicación efectiva
    Estado = EstadoObjetivo.ACTIVO,
    Progreso = 75,
    FechaCreacion = DateTime.UtcNow.AddDays(-60),
    CreadoPorId = jefe.Id,
    PorcentajePilar = 40,
    PorcentajeArea = 40,
    AprobadoPorJefe = true,
    EstadoObjetivoConfigId = 4 // En curso
};

var obj2 = new Objetivo
{
    Nombre = "Desplegar portal web PQ-Talent (RRHH)",
    Descripcion = "Desarrollar y desplegar el portal web PQ-Talent en Blazor Server para automatizar solicitudes y gestión de objetivos, reduciendo en un 30% la carga administrativa.",
    PilarId = 1, // EXCELENCIA_ORGANIZACIONAL
    EmpleadoId = empleado.Id,
    Anio = 2026,
    Deadline = new DateTime(2026, 12, 15),
    SoftSkill1Id = 25, // Pensamiento creativo e innovación
    SoftSkill2Id = 26, // Colaboración y trabajo en equipo
    Estado = EstadoObjetivo.EN_RIESGO,
    Progreso = 45,
    FechaCreacion = DateTime.UtcNow.AddDays(-60),
    CreadoPorId = empleado.Id,
    PorcentajePilar = 30,
    PorcentajeArea = 30,
    AprobadoPorJefe = true,
    EstadoObjetivoConfigId = 4 // En curso
};

var obj3 = new Objetivo
{
    Nombre = "Automatizar reportes de nómina mensual",
    Descripcion = "Desarrollar scripts de integración de base de datos para la generación automática de reportes consolidados de nómina.",
    PilarId = 2, // INNOVACION_MEJORA
    EmpleadoId = empleado.Id,
    Anio = 2026,
    Deadline = new DateTime(2026, 8, 30),
    SoftSkill1Id = 27, // Compromiso con la excelencia
    SoftSkill2Id = 24, // Iniciativa
    Estado = EstadoObjetivo.ACTIVO,
    Progreso = 90,
    FechaCreacion = DateTime.UtcNow.AddDays(-60),
    CreadoPorId = jefe.Id,
    PorcentajePilar = 30,
    PorcentajeArea = 30,
    AprobadoPorJefe = true,
    EstadoObjetivoConfigId = 4 // En curso
};

db.Objetivos.AddRange(obj1, obj2, obj3);
await db.SaveChangesAsync();

// 5. Seed Bitacora
Console.WriteLine("Seeding Bitacora...");
db.BitacoraEntradas.Add(new BitacoraEntrada
{
    ObjetivoId = obj1.Id,
    EmpleadoId = empleado.Id,
    Fecha = DateTime.UtcNow.AddDays(-15),
    Texto = "Se reestructuró el flujo de priorización de tickets en Jira. Los incidentes críticos ahora se asignan automáticamente al equipo de soporte de guardia, reduciendo el tiempo de espera inicial de 45 a 15 minutos.",
    Estado = EstadoBitacora.CERRADO,
    FeedbackJefe = "Excelente iniciativa, Nicolas. El impacto se notó inmediatamente en las encuestas semanales de soporte. ¡Seguí así!",
    FechaFeedback = DateTime.UtcNow.AddDays(-13)
});

db.BitacoraEntradas.Add(new BitacoraEntrada
{
    ObjetivoId = obj2.Id,
    EmpleadoId = empleado.Id,
    Fecha = DateTime.UtcNow.AddDays(-2),
    Texto = "Finalicé la maqueta interactiva del módulo de autoevaluaciones en Blazor. Quedó lista para revisión técnica con el equipo de infraestructura. Adjunto los diagramas de flujo.",
    Estado = EstadoBitacora.PENDIENTE_REVISION
});

// 6. Seed Chat
Console.WriteLine("Seeding Chat messages...");
db.MensajesChat.AddRange(
    new MensajeChat
    {
        RemitenteId = empleado.Id,
        RemitenteEsJefe = false,
        DestinatarioEmpleadoId = empleado.Id,
        JefeId = jefe.Id,
        Texto = "Hola Pablo, ¿cómo estás? Quería consultarte si pudiste revisar la propuesta de pilares y competencias para el portal de RRHH que subí ayer.",
        Fecha = DateTime.UtcNow.AddDays(-2).AddHours(9),
        Leido = true
    },
    new MensajeChat
    {
        RemitenteId = jefe.Id,
        RemitenteEsJefe = true,
        DestinatarioEmpleadoId = empleado.Id,
        JefeId = jefe.Id,
        Texto = "Hola Nicolas, ¡buen día! Sí, lo estuve revisando. La estructura general está muy bien, me gusta el enfoque. Solo agregaría una aclaración sobre los flujos de aprobación.",
        Fecha = DateTime.UtcNow.AddDays(-2).AddHours(10),
        Leido = true
    },
    new MensajeChat
    {
        RemitenteId = empleado.Id,
        RemitenteEsJefe = false,
        DestinatarioEmpleadoId = empleado.Id,
        JefeId = jefe.Id,
        Texto = "Buenísimo, le sumo esa aclaración hoy mismo. ¿Te parece si coordinamos una breve videollamada mañana para cerrar los detalles?",
        Fecha = DateTime.UtcNow.AddDays(-2).AddHours(11),
        Leido = false // Unread!
    }
);

// 7. Seed Autoevaluacion for Objetivo 1
Console.WriteLine("Seeding Autoevaluación...");
db.Autoevaluaciones.Add(new Autoevaluacion
{
    ObjetivoId = obj1.Id,
    EmpleadoId = empleado.Id,
    Score = 4, // Muy bueno
    EscalaValoracionIdScore = 2,
    Comentario = "He logrado reducir significativamente los tiempos de atención y hemos mantenido un alto nivel de respuesta, superando la meta inicial de satisfacción del cliente interno.",
    SoftSkill1Score = 4,
    SoftSkill1EscalaValoracionId = 2,
    SoftSkill1Comentario = "Logré una comunicación más fluida con los usuarios de soporte, minimizando los malentendidos en los reportes de incidentes.",
    SoftSkill2Score = 3,
    SoftSkill2EscalaValoracionId = 3,
    SoftSkill2Comentario = "Coordiné de manera oportuna con los distintos miembros de la mesa de soporte para asegurar la cobertura diaria.",
    FechaAutoevaluacion = DateTime.UtcNow.AddDays(-1),
    EstadoEvaluacionConfigId = 3 // Completada
});

// 8. Seed Revision Cuatrimestral for Objetivo 1
Console.WriteLine("Seeding Revisión Cuatrimestral...");
db.RevisionesCuatrimestrales.Add(new RevisionCuatrimestral
{
    ObjetivoId = obj1.Id,
    Periodo = PeriodoRevision.FEEDBACK_MITAD_ANIO,
    Anio = 2026,
    Puntaje = 4, // Muy bueno
    EscalaValoracionId = 2,
    ComentarioJefe = "Nicolás ha demostrado un gran compromiso con la mejora de la atención al cliente. Ha liderado los cambios con efectividad.",
    Completada = true,
    FechaEvaluacion = DateTime.UtcNow,
    EvaluadorId = jefe.Id,
    EstadoEvaluacionConfigId = 3, // Completada
    SoftSkill1Puntaje = 4,
    SoftSkill1EscalaValoracionId = 2,
    SoftSkill1Comentario = "Excelente comunicación y empatía con los usuarios de soporte.",
    SoftSkill2Puntaje = 4,
    SoftSkill2EscalaValoracionId = 2,
    SoftSkill2Comentario = "Ha tomado la iniciativa en la coordinación diaria del equipo de soporte."
});

// 9. Seed Calendar Events
Console.WriteLine("Seeding Calendar Events...");
db.EventosCalendario.AddRange(
    new EventoCalendario
    {
        Titulo = "Revisión de Mitad de Año - Nicolás Caldiroli",
        Fecha = DateTime.UtcNow.AddDays(15),
        Tipo = TipoEvento.FEEDBACK_MITAD_ANIO,
        AreaId = empleado.AreaId
    },
    new EventoCalendario
    {
        Titulo = "Fecha Límite - Objetivos Q3",
        Fecha = DateTime.UtcNow.AddDays(45),
        Tipo = TipoEvento.DEADLINE_OBJETIVO,
        AreaId = empleado.AreaId
    },
    new EventoCalendario
    {
        Titulo = "Evaluación Final de Desempeño 2026",
        Fecha = DateTime.UtcNow.AddDays(120),
        Tipo = TipoEvento.EVALUACION_FINAL,
        AreaId = empleado.AreaId
    }
);

await db.SaveChangesAsync();
Console.WriteLine("Database seeding completed successfully!");
