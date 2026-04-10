namespace Objetivos.Web.Domain.Enums;

public enum EstadoObjetivo    { BORRADOR, ACTIVO, EN_RIESGO, COMPLETADO, CANCELADO }
public enum PeriodoRevision   { FEEDBACK_MITAD_ANIO }
public enum ResultadoEval     { CUMPLIDO, PARCIAL, NO_CUMPLIDO, EN_RIESGO }
public enum EstadoBitacora    { PENDIENTE_REVISION, COMENTADO_JEFE, REQUIERE_AJUSTE, CERRADO }
public enum TipoEvento        { DEADLINE_OBJETIVO, FEEDBACK_MITAD_ANIO, EVALUACION_FINAL }
public enum TipoNotificacion  { SOLICITUD_ACTUALIZACION, NUEVA_EVALUACION, DEADLINE_PROXIMO }
