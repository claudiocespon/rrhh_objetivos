namespace Objetivos.Web.Domain.Enums;

public enum EstadoObjetivo    { BORRADOR, ACTIVO, EN_RIESGO, COMPLETADO, CANCELADO }
public enum PeriodoRevision   { Q1_ABRIL, Q2_AGOSTO, Q3_NOVIEMBRE }
public enum ResultadoEval     { CUMPLIDO, PARCIAL, NO_CUMPLIDO, EN_RIESGO }
public enum EstadoBitacora    { PENDIENTE_REVISION, COMENTADO_JEFE, REQUIERE_AJUSTE, CERRADO }
public enum TipoEvento        { DEADLINE_OBJETIVO, REVISION_Q1, REVISION_Q2, REVISION_Q3, EVALUACION_FINAL }
public enum TipoNotificacion  { SOLICITUD_ACTUALIZACION, NUEVA_EVALUACION, DEADLINE_PROXIMO }
