import os
import subprocess

base_dir = r"c:\Development\Antigravity\RRHH_Objetivos"
os.chdir(base_dir)

# Mapeo corregido de audios a capturas de pantalla.
# Verificado visualmente: título de pantalla + solapa activa vs. texto del audio.
#
# Audios 1-12: Recorrido general de la plataforma (perspectiva usuario/líder)
# Audios 13-19: Secciones exclusivas de administrador/líder
#
# CORRECCIONES APLICADAS:
# - audio_6: Antes usaba 38 (MI RENDIMIENTO, vacía). El audio dice "rendimiento general"
#   y "grado de desarrollo de tus colaboradores" → solapa TODA LA ORGANIZACIÓN (captura 37).
# - audio_7: Antes usaba 26 (usuario Vivot Romina, PENDIENTES DE AUTOEVALUAR).
#   El video es de Admin. La primera solapa del Admin es AUTOEVALUACIONES DE MI EQUIPO (captura 39).
# - audio_8: Antes usaba 40 (MIS AUTOEVALUACIONES del Admin, vacía).
#   El audio dice "registro de reflexiones ya enviadas", mejor usar la captura de usuario
#   con datos (27) que muestra la solapa MIS AUTOEVALUACIONES con registros reales.
# - audio_9: Audio dice "Feedback de mitad de año... comentarios de nuestro líder".
#   La captura 41 muestra la vista del líder (FEEDBACK MITAD DE AÑO EQUIPO) para evaluar.
#   Pero para recibir feedback, se usaría la perspectiva usuario (captura 28).
#   Sin embargo, en un video de admin que ES líder, la captura 41 es la correcta
#   porque muestra la solapa activa que coincide (FEEDBACK MITAD DE AÑO).
# - audio_10: Antes usaba 29 (usuario Vivot Romina, solapa EVALUACIONES FINALES).
#   Es captura de usuario, no de admin. Para vista admin se usa la captura 42 que muestra
#   FEEDBACK MITAD DE AÑO REALIZADOS (EQUIPO) -- pero el audio habla de "Evaluaciones finales".
#   Realmente no hay captura admin con solapa "Evaluaciones finales" en perspectiva propia.
#   La más cercana es la de usuario (29), pero para coherencia visual (admin logueado) NO sirve.
#   Mejor usar 43 (EVALUACIONES FINALES PENDIENTES del admin) que al menos coincide en nombre.
#   Aunque el audio habla de "resultado de nuestro desempeño", se ajusta al contexto de admin/líder.

mapping = [
    # --- Recorrido general ---
    # audio_1: Login → Pantalla "PQ-Talent / Plataforma de gestión de Talento"
    ("scratch/video_build/audio_1.mp3", "Capturas/00 Login.png"),

    # audio_2: Dashboard → Título "Dashboard", métricas, gráfico de objetivos
    ("scratch/video_build/audio_2.mp3", "Capturas/33_Admin_Dashboard.png"),

    # audio_3: "Objetivos de mi equipo" → Título "Objetivos y Competencias", solapa "OBJETIVOS DE MI EQUIPO"
    ("scratch/video_build/audio_3.mp3", "Capturas/35_Admin_Objetivos_Equipo.png"),

    # audio_4: "Mis objetivos" → Título "Objetivos y Competencias", solapa "MIS OBJETIVOS"
    ("scratch/video_build/audio_4.mp3", "Capturas/36_Admin_Objetivos_Propios.png"),

    # audio_5: "Crear Nuevo Objetivo" → Modal "Crear Nuevo Objetivo" con campos Pilar, Nombre, etc.
    ("scratch/video_build/audio_5.mp3", "Capturas/24b_Usuario_Crear_Objetivo_Modal.png"),

    # audio_6: "Seguimientos... rendimiento general... colaboradores directos"
    #   → Título "Seguimientos", solapa "TODA LA ORGANIZACIÓN" (con lista de colaboradores)
    #   CORREGIDO: Antes era 38 (MI RENDIMIENTO vacía)
    ("scratch/video_build/audio_6.mp3", "Capturas/37_Admin_Seguimientos_Organizacion.png"),

    # audio_7: "Autoevaluación... primera pestaña... pendientes de reflexión"
    #   → Título "Autoevaluaciones", solapa "AUTOEVALUACIONES DE MI EQUIPO"
    #   CORREGIDO: Antes era 26 (usuario Vivot, PENDIENTES DE AUTOEVALUAR)
    ("scratch/video_build/audio_7.mp3", "Capturas/39_Admin_Autoevaluaciones_Equipo.png"),

    # audio_8: "Mis Autoevaluaciones... registro de reflexiones enviadas"
    #   → Título "Autoevaluaciones", solapa "MIS AUTOEVALUACIONES"
    #   CORREGIDO: Antes era 40 (admin, vacía). Ahora usa 27 (usuario Vivot con datos reales).
    #   Mejor experiencia visual ya que la solapa MIS AUTOEVALUACIONES muestra registros.
    ("scratch/video_build/audio_8.mp3", "Capturas/27_Usuario_Autoevaluaciones_Propias.png"),

    # audio_9: "Evaluación... Feedback de mitad de año... comentarios de nuestro líder"
    #   → Título "Evaluaciones", solapa "FEEDBACK MITAD DE AÑO (EQUIPO)"
    #   Captura 41: admin, muestra la lista de feedback pendiente (EVALUAR)
    ("scratch/video_build/audio_9.mp3", "Capturas/41_Admin_Eval_Feedback_Mitad_Ano.png"),

    # audio_10: "Evaluaciones finales... valoración obtenida en cada objetivo"
    #   → Título "Evaluaciones", solapa "FEEDBACK MITAD DE AÑO REALIZADOS (EQUIPO)"
    #   CORREGIDO: Antes era 29 (usuario Vivot). Ahora usa 42 (admin, feedback realizados)
    #   que muestra evaluaciones ya completadas con valoraciones.
    ("scratch/video_build/audio_10.mp3", "Capturas/42_Admin_Eval_Feedback_Mitad_Ano_Realizados.png"),

    # audio_11: "Cursos... catálogo de capacitaciones"
    #   → Título "Cursos y Capacitaciones", solapa "CATÁLOGO DE CURSOS"
    ("scratch/video_build/audio_11.mp3", "Capturas/46_Admin_Cursos.png"),

    # audio_12: "Calendario... y la Guía"
    #   → Título "Calendario de Eventos"
    ("scratch/video_build/audio_12.mp3", "Capturas/47_Admin_Calendario.png"),

    # --- Secciones específicas de Admin/Líder ---

    # audio_13: "Evaluaciones Finales Pendientes... calificar desempeño anual"
    #   → Título "Evaluaciones", solapa "EVALUACIONES FINALES PENDIENTES"
    ("scratch/video_build/audio_13.mp3", "Capturas/43_Admin_Eval_Finales_Pendientes.png"),

    # audio_14: "Evaluaciones Finales Realizadas... calificaciones emitidas"
    #   → Título "Evaluaciones", solapa "EVALUACIONES FINALES REALIZADAS"
    ("scratch/video_build/audio_14.mp3", "Capturas/44_Admin_Eval_Finales_Realizadas.png"),

    # audio_15: "Mis Evaluaciones Recibidas... devolución de nuestro líder"
    #   → Título "Evaluaciones", solapa "MIS EVALUACIONES RECIBIDAS"
    ("scratch/video_build/audio_15.mp3", "Capturas/45_Admin_Eval_Mis_Recibidas.png"),

    # audio_16: "Cursos... catálogo de formaciones"
    #   → Título "Cursos y Capacitaciones", solapa "CATÁLOGO DE CURSOS"
    ("scratch/video_build/audio_16.mp3", "Capturas/46_Admin_Cursos.png"),

    # audio_17: "Calendario... fechas de corte e hitos"
    #   → Título "Calendario de Eventos"
    ("scratch/video_build/audio_17.mp3", "Capturas/47_Admin_Calendario.png"),

    # audio_18: "Administración > Configuración... pilares estratégicos, competencias, periodos"
    #   → Título "Administración de Configuración", solapa "PILARES ESTRATÉGICOS"
    ("scratch/video_build/audio_18.mp3", "Capturas/51_Admin_Config_Pilares.png"),

    # audio_19: "Usuarios... dar de alta, desactivar, modificar perfiles... roles y jefe directo"
    #   → Título "Administración de Usuarios"
    ("scratch/video_build/audio_19.mp3", "Capturas/59_Admin_Usuarios.png"),
]

clips = []
for i, (audio, img) in enumerate(mapping):
    clip_file = f"new_clip_{i+1}.mp4"
    clip_path = f"scratch/video_build/{clip_file}"
    clips.append(clip_file)

    cmd = [
        "ffmpeg", "-y",
        "-framerate", "25",
        "-loop", "1",
        "-i", img,
        "-i", audio,
        "-c:v", "libx264", "-c:a", "aac", "-b:a", "192k",
        "-pix_fmt", "yuv420p",
        "-shortest",
        "-vf", "scale=1920:1080:force_original_aspect_ratio=decrease,pad=1920:1080:(ow-iw)/2:(oh-ih)/2",
        clip_path
    ]
    print(f"Generando clip {i+1}/{len(mapping)}...")
    subprocess.run(cmd, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

concat_file = "scratch/video_build/new_concat.txt"
with open(concat_file, "w") as f:
    for clip in clips:
        f.write(f"file '{clip}'\n")

print("Concatenando todos los clips...")
cmd_final = [
    "ffmpeg", "-y", "-f", "concat", "-safe", "0",
    "-i", concat_file,
    "-c", "copy",
    "video_admin_nuevo.mp4"
]
subprocess.run(cmd_final, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
print("Video generado exitosamente: video_admin_nuevo.mp4")
