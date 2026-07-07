import os
import subprocess
import glob

base_dir = r"c:\Development\Antigravity\RRHH_Objetivos"
capturas_dir = os.path.join(base_dir, "Capturas_Admin_Pablo")
out_dir = os.path.join(base_dir, "scratch", "video_admin_pablo_build")
edge_tts = r"c:\users\ccespon\appdata\roaming\python\python314\Scripts\edge-tts.exe"
final_video = os.path.join(base_dir, "video_admin_pablo_final.mp4")

os.makedirs(out_dir, exist_ok=True)

# Clean previous build files if any
for f in glob.glob(os.path.join(out_dir, "*.mp4")) + glob.glob(os.path.join(out_dir, "*.mp3")):
    try: os.remove(f)
    except: pass

data = [
    ("01.Pantalla de Login.png", "Hola. Bienvenidos al portal de la plataforma de gestión de talento PQ-Talent. En este video aprenderemos a utilizar la plataforma para gestionar nuestras metas, realizar autoevaluaciones y, si somos líderes de equipo, hacer seguimiento y evaluación a nuestros colaboradores. Ingresamos con nuestro usuario y contraseña."),
    ("02.Dashboard.png", "La pantalla inicial es nuestro dashboard. Aquí encontraremos un resumen rápido de nuestro progreso en el periodo actual. Podemos ver de un vistazo cómo vamos con nuestras metas anuales y qué tareas requieren nuestra atención inmediata."),
    ("3.1.Objetivos y Competencias(Pendientes de Aprobación).png", "Dirigiéndonos al menú izquierdo, la primera sección es Objetivos y Competencias. Si tienes personal a cargo, en la pestaña Pendientes de Aprobación podrás revisar el detalle de las metas pendientes de aprobación para tu equipo."),
    ("3.2.Objetivos y Competencias(Objetivos de mi equipo).png", "Todos los objetivos asignados a tus colaboradores directos, se encuentran en la pestaña Objetivos de mi Equipo."),
    ("3.3Objetivos y Competencias(Mis Objetivos).png", "Pasando a la pestaña Mis objetivos, encontraremos el detalle de las metas que hemos acordado con nuestro líder. Podemos revisar la descripción, el peso de cada objetivo y ver nuestro nivel de avance actual."),
    ("3.4.Crear_Objetivo_Modal.png", "Para registrar una meta en el sistema, hacemos clic en Nuevo objetivo. Deberemos completar el formulario detallando el pilar estratégico, la fecha límite y las competencias asociadas. Recuerda que la carga de este objetivo refleja lo previamente acordado de palabra con tu línea directa, y una vez que lo guardes, será enviado a tu líder para su aprobación formal."),
    ("04.Seguimientos(Toda la Organizacion).png", "En la sección de Seguimientos, solapa Toda la Organización, podremos ver el detalle general de los objetivos y el avance global."),
    ("05.Seguimientos(Mi Rendimiento).png", "En la sección de Mi rendimiento tenemos una vista rápida a nuestro rendimiento general basado en las revisiones periódicas o reuniones uno a uno que mantenemos durante el año."),
    ("06.Autevaluaciones (Autoevaluaciones de Mi Equipo).png", "Llegado el momento de cierre de ciclo, ingresaremos a la sección Autoevaluación. En la solapa Autoevaluaciones de Mi Equipo podremos ver el detalle de las autoevaluaciones de nuestros colaboradores."),
    ("07.Autevaluaciones (Mis Autoevaluaciones).png", "En Mis Autoevaluaciones queda el registro de nuestras reflexiones ya enviadas. Es importante ser honestos y detallar nuestros logros, ya que esto servirá de base para la reunión de feedback."),
    ("08.Evaluaciones (Feedback Mitad de Año Equipo).png", "La etapa de Evaluación nos muestra las devoluciones formales que recibimos. Empezando por la pestaña de Feedback de mitad de año equipo, podremos evaluar el progreso semestral de nuestro equipo."),
    ("09.Evaluaciones (Feedback Mitad de Año Realizados( Equipo)).png", "En la pestaña de Feedback de mitad de año realizados, veremos el historial de evaluaciones ya enviadas."),
    ("10.Evaluaciones (Evaluaciones finales pendientes).png", "La tercera solapa es crítica: Evaluaciones Finales Pendientes. Aquí calificaremos el desempeño anual definitivo de cada colaborador, asignando notas a cada objetivo y competencia."),
    ("11.Evaluaciones (Evaluaciones finales realizadas).png", "Una vez enviadas, pasarán a la solapa Evaluaciones Finales Realizadas, donde podremos consultar las calificaciones emitidas en procesos anteriores."),
    ("12.Evaluaciones (mis evaluaciones recibidas).png", "Por último, en Mis Evaluaciones Recibidas tendremos acceso a leer la devolución y calificación final que nuestro propio líder nos ha otorgado."),
    ("13.Cursos y capacitaciones (Catalogo de Cursos).png", "La sección de Cursos nos provee acceso directo al catálogo de formaciones disponibles y recomendadas para el desarrollo de nuevas competencias."),
    ("14.Cursos y capacitaciones (Mis Cursos asignados).png", "En Mis Cursos Asignados podremos llevar un control de las capacitaciones que estamos realizando en la actualidad."),
    ("15.Calendario de Eventos.png", "En el Calendario encontraremos resaltadas todas las fechas de corte y los hitos clave del proceso de desempeño para no perder ningún plazo."),
    ("16.Guia de Uso - PQ Talent (Pilares Estrategicos).png", "La Guía de Uso es tu manual de consulta constante. Comenzamos con los Pilares Estratégicos que rigen nuestra metodología."),
    ("17.Guia de Uso - PQ Talent (Competencias).png", "También podemos consultar el detalle de las Competencias definidas y esperadas por la organización."),
    ("18.Guia de Uso - PQ Talent (Manual de uso).png", "El Manual de Uso detalla paso a paso el funcionamiento de esta plataforma para resolver cualquier duda operativa."),
    ("19.Guia de Uso - PQ Talent (Escala de Valoracion).png", "Y la Escala de Valoración nos explica los criterios y puntuaciones utilizados durante los períodos de evaluaciones."),
    ("20.Administracion de Configuracion (Pilares Estratégicos).png", "Ahora sí, ingresando al apartado exclusivo de Administración, encontraremos primero la opción de Configuración. Aquí definimos los parámetros globales del sistema, establecemos los pilares estratégicos, las competencias activas y los periodos habilitados."),
    ("21.Administracion de usuarios.png", "Finalmente, en Usuarios podemos dar de alta, desactivar o modificar perfiles. Es crucial mantener actualizados los roles y quién es el jefe directo de cada empleado para asegurar que la cadena de reporte funcione."),
    # FINAL: 5 seconds with login screen
    ("01.Pantalla de Login.png", "Gracias por ver este tutorial. Hasta la próxima.")
]

clips = []

for i, (img_filename, text) in enumerate(data, 1):
    print(f"--- Procesando Paso {i}: {img_filename} ---")
    
    img_path = os.path.join(capturas_dir, img_filename)
    if not os.path.exists(img_path):
        raise FileNotFoundError(f"No se encontro la imagen: {img_path}")
    
    # 1. Generar Audio
    audio_path = os.path.join(out_dir, f"audio_{i}.mp3")
    tts_cmd = [
        edge_tts,
        "-v", "es-MX-JorgeNeural",
        "-t", text,
        "--write-media", audio_path
    ]
    subprocess.run(tts_cmd, check=True)
    
    # 2. Obtener Duracion Audio
    ffprobe_cmd = [
        "ffprobe", "-v", "error", "-show_entries", "format=duration", 
        "-of", "default=noprint_wrappers=1:nokey=1", audio_path
    ]
    res = subprocess.run(ffprobe_cmd, stdout=subprocess.PIPE, text=True, check=True)
    audio_duration = float(res.stdout.strip())
    
    # Duración final: en el ultimo clip forzar al menos 5.0 segundos.
    # En los demás sumamos 2.0s de padding general para que se alcance a leer/ver bien.
    if i == len(data):
        final_duration = max(5.0, audio_duration + 0.5)
    else:
        final_duration = audio_duration + 2.0
    
    print(f"Paso {i}: Audio {audio_duration:.2f}s, Video {final_duration:.2f}s")
    
    # 3. Crear Clip Video
    video_path = os.path.join(out_dir, f"clip_{i}.mp4")
    
    vf = (
        "scale=1920:1080:force_original_aspect_ratio=decrease,"
        "pad=1920:1080:(ow-iw)/2:(oh-ih)/2,"
        "format=yuv420p"
    )
    
    ffmpeg_cmd = [
        "ffmpeg", "-y",
        "-loop", "1", "-framerate", "25", "-i", img_path,
        "-i", audio_path,
        "-vf", vf,
        "-af", "apad",
        "-c:v", "libx264", "-preset", "fast",
        "-c:a", "aac", "-b:a", "192k",
        "-t", str(final_duration),
        video_path
    ]
    
    subprocess.run(ffmpeg_cmd, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    clips.append(video_path)

# 4. Concatenar
print("\n--- Concatenando Clips ---")
concat_file = os.path.join(out_dir, "concat.txt")
with open(concat_file, "w", encoding='utf-8') as f:
    for c in clips:
        f.write(f"file '{os.path.basename(c)}'\n")

concat_cmd = [
    "ffmpeg", "-y",
    "-f", "concat",
    "-safe", "0",
    "-i", concat_file,
    "-c", "copy",
    final_video
]
subprocess.run(concat_cmd, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

print(f"\n¡Video generado exitosamente en: {final_video}!")
