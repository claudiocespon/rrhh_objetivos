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
    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;
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
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string ColorHex { get; set; } = "#000000";
    public bool Activo { get; set; } = true;
    public int Orden { get; set; } = 0;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;
}

public class SoftSkill {
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public bool Activo { get; set; } = true;
    public int Orden { get; set; } = 0;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;
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
    public decimal PorcentajePilar { get; set; } = 0;
    public bool AprobadoPorJefe { get; set; } = false;
    public int? EstadoObjetivoConfigId { get; set; }
    public EstadoObjetivoConfig? EstadoObjetivoConfig { get; set; }
    public int? AreaEspecificaId { get; set; }
    public Area? AreaEspecifica { get; set; }
    public decimal PorcentajeArea { get; set; } = 0;
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
    public int? Puntaje { get; set; }
    public int? EscalaValoracionId { get; set; }
    public EscalaValoracion? EscalaValoracion { get; set; }
    public string ComentarioJefe { get; set; } = "";
    public ResultadoEval? Resultado { get; set; }
    public string EvidenciasRevisadasJson { get; set; } = "[]";
    public bool Completada { get; set; } = false;
    public DateTime? FechaEvaluacion { get; set; }
    public int? EvaluadorId { get; set; }
    public int? EstadoEvaluacionConfigId { get; set; }
    public EstadoEvaluacionConfig? EstadoEvaluacionConfig { get; set; }

    public int? SoftSkill1Puntaje { get; set; }
    public int? SoftSkill1EscalaValoracionId { get; set; }
    public EscalaValoracion? SoftSkill1EscalaValoracion { get; set; }
    public string SoftSkill1Comentario { get; set; } = "";
    public int? SoftSkill2Puntaje { get; set; }
    public int? SoftSkill2EscalaValoracionId { get; set; }
    public EscalaValoracion? SoftSkill2EscalaValoracion { get; set; }
    public string SoftSkill2Comentario { get; set; } = "";
}

public class EvaluacionFinal {
    public int Id { get; set; }
    public int ObjetivoId { get; set; }
    public Objetivo Objetivo { get; set; } = null!;
    public int Anio { get; set; }
    public double PuntajeFinal { get; set; }
    public int? EscalaValoracionIdFinal { get; set; }
    public EscalaValoracion? EscalaValoracionFinal { get; set; }
    public string ComentarioJefe { get; set; } = "";
    public ResultadoEval ResultadoFinal { get; set; }
    public DateTime FechaEvaluacion { get; set; }
    public int EvaluadorId { get; set; }
    public int? EstadoEvaluacionConfigId { get; set; }
    public EstadoEvaluacionConfig? EstadoEvaluacionConfig { get; set; }

    public int? SoftSkill1Puntaje { get; set; }
    public int? SoftSkill1EscalaValoracionId { get; set; }
    public EscalaValoracion? SoftSkill1EscalaValoracion { get; set; }
    public string SoftSkill1Comentario { get; set; } = "";
    public int? SoftSkill2Puntaje { get; set; }
    public int? SoftSkill2EscalaValoracionId { get; set; }
    public EscalaValoracion? SoftSkill2EscalaValoracion { get; set; }
    public string SoftSkill2Comentario { get; set; } = "";
}

public class Autoevaluacion {
    public int Id { get; set; }
    public int ObjetivoId { get; set; }
    public Objetivo Objetivo { get; set; } = null!;
    public int EmpleadoId { get; set; }
    public int Score { get; set; }
    public int? EscalaValoracionIdScore { get; set; }
    public EscalaValoracion? EscalaValoracionScore { get; set; }
    public string Comentario { get; set; } = "";
    public string EvidenciasMencionadasJson { get; set; } = "[]";
    public string ArchivosAdjuntosJson { get; set; } = "[]";
    public int? EstadoEvaluacionConfigId { get; set; }
    public EstadoEvaluacionConfig? EstadoEvaluacionConfig { get; set; }

    public int SoftSkill1Score { get; set; }
    public int? SoftSkill1EscalaValoracionId { get; set; }
    public EscalaValoracion? SoftSkill1EscalaValoracion { get; set; }
    public string SoftSkill1Comentario { get; set; } = "";
    public int SoftSkill2Score { get; set; }
    public int? SoftSkill2EscalaValoracionId { get; set; }
    public EscalaValoracion? SoftSkill2EscalaValoracion { get; set; }
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

    // Nav
    public List<CursoAsignacion> Asignaciones { get; set; } = [];
}

public class CursoAsignacion {
    public int Id { get; set; }
    public int CursoId { get; set; }
    public Curso Curso { get; set; } = null!;
    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaCompletado { get; set; }
    public bool Completado { get; set; } = false;
    public int? AsignadoPorId { get; set; }
    public string? Notas { get; set; }
}

public class EscalaValoracion {
    public int Id { get; set; }
    public string Etiqueta { get; set; } = "";
    public decimal? ValorNumerico { get; set; }
    public int Orden { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;
}

public class EstadoObjetivoConfig {
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Slug { get; set; } = "";
    public string ColorHex { get; set; } = "#000000";
    public int Orden { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}

public class EstadoEvaluacionConfig {
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Slug { get; set; } = "";
    public string ColorHex { get; set; } = "#000000";
    public int Orden { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}

public class ConfiguracionPlataforma {
    public string Clave { get; set; } = "";
    public string Valor { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string Tipo { get; set; } = "string";
    public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;
    public int? ActualizadoPorId { get; set; }
}
