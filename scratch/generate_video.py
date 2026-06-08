import re
import os
import subprocess
import glob
import sys

if len(sys.argv) < 3:
    print("Usage: python generate_video.py <script_md_path> <output_mp4_path>")
    sys.exit(1)

MD_PATH = sys.argv[1]
FINAL_OUTPUT = sys.argv[2]
CAPTURAS_DIR = r"c:\Development\Antigravity\RRHH_Objetivos\Capturas"
OUT_DIR = r"c:\Development\Antigravity\RRHH_Objetivos\scratch\video_build"
EDGE_TTS_BIN = r"c:\users\ccespon\appdata\roaming\python\python314\Scripts\edge-tts.exe"

os.makedirs(OUT_DIR, exist_ok=True)

# Clean previous clips
for f in glob.glob(os.path.join(OUT_DIR, "clip_*.mp4")) + glob.glob(os.path.join(OUT_DIR, "audio_*.mp3")):
    try: os.remove(f)
    except: pass

with open(MD_PATH, 'r', encoding='utf-8') as f:
    content = f.read()

# Split into sections
sections = re.split(r'## 🎬', content)[1:]

clips = []
last_image = "01_Dashboard.png"

for i, sec in enumerate(sections, 1):
    print(f"--- Processing Section {i} ---")
    
    # Extract times
    time_match = re.search(r'\((\d+):(\d+) - (\d+):(\d+)\)', sec)
    target_duration = 30 # default
    if time_match:
        m1, s1, m2, s2 = map(int, time_match.groups())
        start_sec = m1 * 60 + s1
        end_sec = m2 * 60 + s2
        target_duration = end_sec - start_sec
    
    # Extract text
    text_match = re.search(r'> "(.*?)"', sec, re.DOTALL)
    text = text_match.group(1).replace('\n', ' ') if text_match else "Sección sin audio."
    
    # Extract image
    img_match = re.search(r'`(.*?\.png)`', sec)
    if img_match:
        last_image = img_match.group(1)
    
    img_path = os.path.join(CAPTURAS_DIR, last_image)
    if not os.path.exists(img_path):
        print(f"Warning: image {img_path} not found. Searching for any png...")
        fallback = glob.glob(os.path.join(CAPTURAS_DIR, "*.png"))
        if fallback:
            img_path = fallback[0]
            print(f"Using fallback: {img_path}")
        else:
            raise FileNotFoundError("No images found in Capturas dir.")
    
    # 1. Generate Audio
    audio_path = os.path.join(OUT_DIR, f"audio_{i}.mp3")
    tts_cmd = [
        EDGE_TTS_BIN,
        "-v", "es-MX-JorgeNeural",
        "-t", text,
        "--write-media", audio_path
    ]
    subprocess.run(tts_cmd, check=True)
    
    # 2. Get Audio Duration
    ffprobe_cmd = [
        "ffprobe", "-v", "error", "-show_entries", "format=duration", 
        "-of", "default=noprint_wrappers=1:nokey=1", audio_path
    ]
    res = subprocess.run(ffprobe_cmd, stdout=subprocess.PIPE, text=True, check=True)
    audio_duration = float(res.stdout.strip())
    
    # Final duration should be max of target_duration and audio_duration + 0.5s padding
    final_duration = audio_duration + 3.0
    
    print(f"Section {i}: Target {target_duration}s, Audio {audio_duration:.2f}s, Final {final_duration:.2f}s, Image {os.path.basename(img_path)}")
    
    # 3. Create Video Segment
    video_path = os.path.join(OUT_DIR, f"clip_{i}.mp4")
    
    # Filter graph:
    # Scale image to 1920x1080 letterboxed so aspect ratio is kept completely intact.
    # No zoompan to avoid any cropping of the menu or edges.
    vf = (
        "scale=1920:1080:force_original_aspect_ratio=decrease,"
        "pad=1920:1080:(ow-iw)/2:(oh-ih)/2,"
        "format=yuv420p"
    )
    
    ffmpeg_cmd = [
        "ffmpeg", "-y",
        "-loop", "1", "-framerate", "30", "-i", img_path,
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

# 4. Concatenate all segments
print("\n--- Concatenating Clips ---")
concat_file = os.path.join(OUT_DIR, "concat.txt")
with open(concat_file, "w", encoding='utf-8') as f:
    for c in clips:
        f.write(f"file '{os.path.basename(c)}'\n")

concat_cmd = [
    "ffmpeg", "-y",
    "-f", "concat",
    "-safe", "0",
    "-i", concat_file,
    "-c", "copy",
    FINAL_OUTPUT
]
subprocess.run(concat_cmd, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

print(f"\nDone! Video generated at: {FINAL_OUTPUT}")
