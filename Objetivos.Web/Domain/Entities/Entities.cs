using Objetivos.Web.Domain.Enums;

namespace Objetivos.Web.Domain.Entities;

public class Pais {
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
}

public class Area {
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
}

public class Jefe {
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string Email { get; set; } = "";
    public string Legajo { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public bool DebeCambiarPassword { get; set; } = true;
    public int AreaId { get; set; }
    public Area Area { get; set; } = null!;
    public int PaisId { get; set; }
    public Pais Pais { get; set; } = null!;
    /// <summary>
    /// JEFE, LIDER, GERENTE, DIRECTOR, DIRECTOR_GENERAL, RRHH
    /// </summary>
    public string Rol { get; set; } = "JEFE";
    public bool Activo { get; set; } = true;
    public bool EsSuperusuario { get; set; } = false;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

public class Empleado {
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string Email { get; set; } = "";
    public string Legajo { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public bool DebeCambiarPassword { get; set; } = true;
    public string Puesto { get; set; } = "";
    public int AreaId { get; set; }
    public Area Area { get; set; } = null!;
    public int JefeId { get; set; }
    public Jefe Jefe { get; set; } = null!;
    public int PaisId { get; set; }
    public Pais Pais { get; set; } = null!;
    public bool Activo { get; set; } = true;
    public bool EsSuperusuario { get; set; } = false;
    public DateTime FechaIngreso { get; set; }
    public List<Objetivo> Objetivos { get; set; } = [];
}

public class Pilar {
    public int Id { get; set; }
    public string Nombre { get; set; } = ""; // "EXCELENCIA_ORGANIZACIONAL" | "INNOVACION_MEJORA" | "ORIENTACION_CLIENTE"
    public string Descripcion { get; set; } = "";
    public string ColorHex { get; set; } = "#000000";
}

public class SoftSkill {
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
}

public class Objetivo {
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public int PilarId { get; set; }
    public Pilar Pilar { get; set; } = null!;
    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;
    public int Anio { get; set; }
    public DateTime Deadline { get; set; }
    public int SoftSkill1Id { get; set; }
    public SoftSkill SoftSkill1 { get; set; } = null!;
    public int SoftSkill2Id { get; set; }
    public SoftSkill SoftSkill2 { get; set; } = null!;
    public EstadoObjetivo Estado { get; set; } = EstadoObjetivo.ACTIVO;
    public int Progreso { get; set; } = 0; // 0-100
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public int CreadoPorId { get; set; }
    // Nav
    public List<RevisionCuatrimestral> Revisiones { get; set; } = [];
    public EvaluacionFinal? EvaluacionFinal { get; set; }
    public List<BitacoraEntrada> Bitacora { get; set; } = [];
    public Autoevaluacion? Autoevaluacion { get; set; }
}

public class RevisionCuatrimestral {
    public int Id { get; set; }
    public int ObjetivoId { get; set; }
    public Objetivo Objetivo { get; set; } = null!;
    public PeriodoRevision Periodo { get; set; }
    public int Anio { get; set; }
    public int? Puntaje { get; set; }           // null = pendiente, 1-5 = completada
    public string ComentarioJefe { get; set; } = "";
    public ResultadoEval? Resultado { get; set; }
    public string EvidenciasRevisadasJson { get; set; } = "[]"; // JSON serializado
    public bool Completada { get; set; } = false;
    public DateTime? FechaEvaluacion { get; set; }
    public int? EvaluadorId { get; set; }
}

public class EvaluacionFinal {
    public int Id { get; set; }
    public int ObjetivoId { get; set; }
    public Objetivo Objetivo { get; set; } = null!;
    public int Anio { get; set; }
    public double PuntajeFinal { get; set; }    // resultado del promedio ponderado
    public string ComentarioJefe { get; set; } = "";
    public ResultadoEval ResultadoFinal { get; set; }
    public DateTime FechaEvaluacion { get; set; }
    public int EvaluadorId { get; set; }
}

public class Autoevaluacion {
    public int Id { get; set; }
    public int ObjetivoId { get; set; }
    public Objetivo Objetivo { get; set; } = null!;
    public int EmpleadoId { get; set; }
    public int Score { get; set; }              // 1-5
    public string Comentario { get; set; } = "";
    public string EvidenciasMencionadasJson { get; set; } = "[]";
    public string ArchivosAdjuntosJson { get; set; } = "[]"; // List of relative file paths
    
    // Soft Skills Evaluation
    public int SoftSkill1Score { get; set; }    // 1-5
    public string SoftSkill1Comentario { get; set; } = "";
    public int SoftSkill2Score { get; set; }    // 1-5
    public string SoftSkill2Comentario { get; set; } = "";
    
    public DateTime FechaAutoevaluacion { get; set; }
}

public class BitacoraEntrada {
    public int Id { get; set; }
    public int ObjetivoId { get; set; }
    public Objetivo Objetivo { get; set; } = null!;
    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;
    public DateTime Fecha { get; set; }
    public string Texto { get; set; } = "";
    public string AdjuntosJson { get; set; } = "[]"; // JSON array de strings
    public EstadoBitacora Estado { get; set; } = EstadoBitacora.PENDIENTE_REVISION;
    public string? FeedbackJefe { get; set; }
    public DateTime? FechaFeedback { get; set; }
}

public class MensajeChat {
    public int Id { get; set; }
    public int RemitenteId { get; set; }        // puede ser JefeId o EmpleadoId
    public bool RemitenteEsJefe { get; set; }   // true = remitente es Jefe, false = Empleado
    public int DestinatarioEmpleadoId { get; set; }
    public int JefeId { get; set; }             // siempre presente para filtrar conversaciones
    public string Texto { get; set; } = "";
    public DateTime Fecha { get; set; }
    public bool Leido { get; set; } = false;
}

public class EventoCalendario {
    public int Id { get; set; }
    public string Titulo { get; set; } = "";
    public DateTime Fecha { get; set; }
    public TipoEvento Tipo { get; set; }
    public int? ObjetivoId { get; set; }
    public Objetivo? Objetivo { get; set; }
    public int AreaId { get; set; }
}

public class AuditoriaLog {
    public int Id { get; set; }
    public string Entidad { get; set; } = "";
    public int EntidadId { get; set; }
    public string Accion { get; set; } = "";    // "CREATE" | "UPDATE" | "DELETE"
    public int UsuarioId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string CambiosJson { get; set; } = "{}";
}

public class Notificacion {
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public TipoNotificacion Tipo { get; set; }
    public string Mensaje { get; set; } = "";
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public bool Leida { get; set; } = false;
}

public class Curso {
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string UrlImagen { get; set; } = "";
    public string Categoria { get; set; } = "";
    public int DuracionHoras { get; set; }
    public bool EsObligatorio { get; set; } = false;
}
