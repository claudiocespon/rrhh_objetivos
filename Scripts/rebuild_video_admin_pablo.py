import os
import subprocess
import glob

base_dir = r"c:\Development\Antigravity\RRHH_Objetivos"
capturas_dir = os.path.join(base_dir, "Capturas_Admin_Pablo")
out_dir = os.path.join(base_dir, "scratch", "video_admin_pablo_build")
final_video = os.path.join(base_dir, "video_admin_pablo_final.mp4")

# Same image list as the original build
images = [
    "01.Pantalla de Login.png",
    "02.Dashboard.png",
    "3.1.Objetivos y Competencias(Pendientes de Aprobación).png",
    "3.2.Objetivos y Competencias(Objetivos de mi equipo).png",
    "3.3Objetivos y Competencias(Mis Objetivos).png",
    "3.4.Crear_Objetivo_Modal.png",
    "04.Seguimientos(Toda la Organizacion).png",
    "05.Seguimientos(Mi Rendimiento).png",
    "06.Autevaluaciones (Autoevaluaciones de Mi Equipo).png",
    "07.Autevaluaciones (Mis Autoevaluaciones).png",
    "08.Evaluaciones (Feedback Mitad de Año Equipo).png",
    "09.Evaluaciones (Feedback Mitad de Año Realizados( Equipo)).png",
    "10.Evaluaciones (Evaluaciones finales pendientes).png",
    "11.Evaluaciones (Evaluaciones finales realizadas).png",
    "12.Evaluaciones (mis evaluaciones recibidas).png",
    "13.Cursos y capacitaciones (Catalogo de Cursos).png",
    "14.Cursos y capacitaciones (Mis Cursos asignados).png",
    "15.Calendario de Eventos.png",
    "16.Guia de Uso - PQ Talent (Pilares Estrategicos).png",
    "17.Guia de Uso - PQ Talent (Competencias).png",
    "18.Guia de Uso - PQ Talent (Manual de uso).png",
    "19.Guia de Uso - PQ Talent (Escala de Valoracion).png",
    "20.Administracion de Configuracion (Pilares Estratégicos).png",
    "21.Administracion de usuarios.png",
    "01.Pantalla de Login.png",  # cierre
]

# Clean previous clips only (keep audio files)
for f in glob.glob(os.path.join(out_dir, "clip_*.mp4")):
    try: os.remove(f)
    except: pass

clips = []
total = len(images)

for i, img_filename in enumerate(images, 1):
    print(f"--- Recompilando Clip {i}/{total}: {img_filename} ---")

    img_path = os.path.join(capturas_dir, img_filename)
    audio_path = os.path.join(out_dir, f"audio_{i}.mp3")

    if not os.path.exists(img_path):
        raise FileNotFoundError(f"Imagen no encontrada: {img_path}")
    if not os.path.exists(audio_path):
        raise FileNotFoundError(f"Audio no encontrado: {audio_path}")

    # Get audio duration
    res = subprocess.run(
        ["ffprobe", "-v", "error", "-show_entries", "format=duration",
         "-of", "default=noprint_wrappers=1:nokey=1", audio_path],
        stdout=subprocess.PIPE, text=True, check=True
    )
    audio_duration = float(res.stdout.strip())

    if i == total:
        final_duration = max(5.0, audio_duration + 0.5)
    else:
        final_duration = audio_duration + 2.0

    print(f"  Audio {audio_duration:.2f}s -> Video {final_duration:.2f}s")

    video_path = os.path.join(out_dir, f"clip_{i}.mp4")
    vf = (
        "scale=1920:1080:force_original_aspect_ratio=decrease,"
        "pad=1920:1080:(ow-iw)/2:(oh-ih)/2,"
        "format=yuv420p"
    )

    subprocess.run([
        "ffmpeg", "-y",
        "-loop", "1", "-framerate", "25", "-i", img_path,
        "-i", audio_path,
        "-vf", vf,
        "-af", "apad",
        "-c:v", "libx264", "-preset", "fast",
        "-c:a", "aac", "-b:a", "192k",
        "-t", str(final_duration),
        video_path
    ], check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

    clips.append(video_path)

# Concatenate
print("\n--- Concatenando Clips ---")
concat_file = os.path.join(out_dir, "concat.txt")
with open(concat_file, "w", encoding='utf-8') as f:
    for c in clips:
        f.write(f"file '{os.path.basename(c)}'\n")

subprocess.run([
    "ffmpeg", "-y",
    "-f", "concat", "-safe", "0",
    "-i", concat_file,
    "-c", "copy",
    final_video
], check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

print(f"\n¡Video recompilado exitosamente en: {final_video}!")
