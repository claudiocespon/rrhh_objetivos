import subprocess

images_with_durations = [
    ('Capturas/00 Login.png', 24.8),
    ('docs/screenshots/Administrador/dashboard.png', 42.7 - 24.8),
    # Objetivos (42.7 -> 61.3 = 18.6s, 3 screens = 6.2s each)
    ('docs/screenshots/Administrador/objetivos_competencias_pendientes_aprobacion.png', 6.2),
    ('docs/screenshots/Administrador/objetivos_competencias_equipo.png', 6.2),
    ('docs/screenshots/Administrador/objetivos_competencias_mis_objetivos.png', 6.2),
    # Seguimientos (61.3 -> 78.0 = 16.7s, 2 screens = 8.35s each)
    ('docs/screenshots/Administrador/seguimientos_toda_organizacion.png', 8.35),
    ('docs/screenshots/Administrador/seguimientos_mi_rendimiento.png', 8.35),
    # Autoevaluacion (78.0 -> 104.3 = 26.3s, 2 screens = 13.15s each)
    ('docs/screenshots/Administrador/autoevaluacion_equipo.png', 13.15),
    ('docs/screenshots/Administrador/autoevaluacion_mis_autoevaluaciones.png', 13.15),
    # Evaluacion (104.3 -> 129.2 = 24.9s, 5 screens = 4.98s each)
    ('docs/screenshots/Administrador/evaluacion_feedback_mitad_anio_equipo.png', 4.98),
    ('docs/screenshots/Administrador/evaluacion_feedback_mitad_anio_realizados_equipo.png', 4.98),
    ('docs/screenshots/Administrador/evaluaciones_finales_pendientes.png', 4.98),
    ('docs/screenshots/Administrador/evaluaciones_finales_realizadas.png', 4.98),
    ('docs/screenshots/Administrador/evaluacion_mis_evaluaciones_recibidas.png', 4.98),
    # Cursos (129.2 -> 144.8)
    ('docs/screenshots/Administrador/cursos_catalogo.png', 144.8 - 129.2),
    # Calendario (144.8 -> 179.0)
    ('docs/screenshots/Administrador/calendario.png', 179.0 - 144.8),
    # Guia (179.0 -> 211.2 = 32.2s, 3 screens = 10.73s each)
    ('docs/screenshots/Administrador/guia_pilares_estrategicos.png', 10.73),
    ('docs/screenshots/Administrador/guia_competencias.png', 10.73),
    ('docs/screenshots/Administrador/guia_escala_valoracion.png', 10.74),
    # Usuarios (211.2 -> 228.3)
    ('docs/screenshots/Administrador/usuarios.png', 228.3 - 211.2),
    # Organigrama (228.3 -> 243.9)
    ('Capturas/60_Admin_Organigrama.png', 243.9 - 228.3),
    # Config: Pilares (243.9 -> 256.9)
    ('docs/screenshots/Administrador/configuracion_pilares_estrategicos.png', 256.9 - 243.9),
    # Config: Competencias (256.9 -> 269.4)
    ('docs/screenshots/Administrador/configuracion_competencias.png', 269.4 - 256.9),
    # Config: Escala (269.4 -> 281.6)
    ('docs/screenshots/Administrador/configuracion_escala_valoracion.png', 281.6 - 269.4),
    # Config: Estados Obj (281.6 -> 294.1)
    ('docs/screenshots/Administrador/configuracion_estados_objetivo.png', 294.1 - 281.6),
    # Config: Estados Eval (294.1 -> 314.7)
    ('docs/screenshots/Administrador/configuracion_estados_evaluacion.png', 314.7 - 294.1),
    # Config: Areas, Puestos, Configuraciones (314.7 -> 335.74 = 21.04s, 3 screens = 7.01s each)
    ('docs/screenshots/Administrador/configuracion_areas.png', 7.01),
    ('docs/screenshots/Administrador/configuracion_puestos.png', 7.01),
    ('docs/screenshots/Administrador/configuracion_configuraciones.png', 7.02)
]

with open('concat.txt', 'w', encoding='utf-8') as f:
    for img, dur in images_with_durations:
        f.write(f"file '{img}'\n")
        f.write(f"duration {dur:.2f}\n")
    
    # FFmpeg concat quirk requires the last file to be repeated without duration
    f.write(f"file '{images_with_durations[-1][0]}'\n")

print("Generado concat.txt exitosamente. Ejecutando ffmpeg...")

subprocess.run([
    "ffmpeg", "-y", "-f", "concat", "-safe", "0", "-i", "concat.txt",
    "-i", "audio.wav", "-vf", "scale=trunc(iw/2)*2:trunc(ih/2)*2",
    "-c:v", "libx264", "-pix_fmt", "yuv420p", "-c:a", "aac", "-b:a", "192k",
    "-shortest", "video_admin_final.mp4"
])
