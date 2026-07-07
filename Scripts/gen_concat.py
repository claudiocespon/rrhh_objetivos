times = [
    23.00, 24.84, 33.17, 42.68, 51.01, 69.68, 78.02, 86.35, 104.36, 
    112.69, 129.20, 137.53, 144.87, 153.20, 160.54, 168.87, 179.04, 187.38, 
    195.04, 210.00, 230.00, 240.00, 250.00, 260.00, 270.00, 280.00, 290.00, 
    334.72
]

images = [
    'Capturas/00 Login.png',
    'docs/screenshots/Administrador/dashboard.png',
    'docs/screenshots/Administrador/objetivos_competencias_pendientes_aprobacion.png',
    'docs/screenshots/Administrador/objetivos_competencias_equipo.png',
    'docs/screenshots/Administrador/objetivos_competencias_mis_objetivos.png',
    'docs/screenshots/Administrador/seguimientos_toda_organizacion.png',
    'docs/screenshots/Administrador/seguimientos_mi_rendimiento.png',
    'docs/screenshots/Administrador/autoevaluacion_equipo.png',
    'docs/screenshots/Administrador/autoevaluacion_mis_autoevaluaciones.png',
    'docs/screenshots/Administrador/evaluacion_feedback_mitad_anio_equipo.png',
    'docs/screenshots/Administrador/evaluacion_feedback_mitad_anio_realizados_equipo.png',
    'docs/screenshots/Administrador/evaluaciones_finales_pendientes.png',
    'docs/screenshots/Administrador/evaluaciones_finales_realizadas.png',
    'docs/screenshots/Administrador/evaluacion_mis_evaluaciones_recibidas.png',
    'docs/screenshots/Administrador/cursos_catalogo.png',
    'docs/screenshots/Administrador/calendario.png',
    'docs/screenshots/Administrador/guia_pilares_estrategicos.png',
    'docs/screenshots/Administrador/guia_competencias.png',
    'docs/screenshots/Administrador/guia_escala_valoracion.png',
    'docs/screenshots/Administrador/configuracion_pilares_estrategicos.png',
    'docs/screenshots/Administrador/configuracion_competencias.png',
    'docs/screenshots/Administrador/configuracion_escala_valoracion.png',
    'docs/screenshots/Administrador/configuracion_estados_objetivo.png',
    'docs/screenshots/Administrador/configuracion_estados_evaluacion.png',
    'docs/screenshots/Administrador/configuracion_areas.png',
    'docs/screenshots/Administrador/configuracion_puestos.png',
    'docs/screenshots/Administrador/configuracion_configuraciones.png',
    'docs/screenshots/Administrador/usuarios.png'
]

with open('concat.txt', 'w') as f:
    last_t = 0.0
    for img, t in zip(images, times):
        dur = t - last_t
        f.write(f"file '{img}'\n")
        f.write(f"duration {dur:.2f}\n")
        last_t = t
    
    # FFmpeg concat quirk requires the last file to be repeated without duration
    f.write(f"file '{images[-1]}'\n")
