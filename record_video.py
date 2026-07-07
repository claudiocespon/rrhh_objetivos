import os
import subprocess
import time
from playwright.sync_api import sync_playwright

def main():
    print("Iniciando grabacion con Playwright...")
    with sync_playwright() as p:
        # Launch with specific viewport settings and device scale factor to ensure it matches the 1920x1080 without weird scaling
        browser = p.chromium.launch(headless=True)
        context = browser.new_context(
            record_video_dir=".", 
            record_video_size={"width": 1920, "height": 1080},
            viewport={"width": 1920, "height": 1080},
            device_scale_factor=1
        )
        start_time = time.time()
        page = context.new_page()
        page.goto("http://localhost:5118/login")
        page.wait_for_timeout(2000)
        
        print("Logueando como ptripodi...")
        page.type("input[name='Email']", "ptripodi@permaquim.com", delay=150)
        page.wait_for_timeout(1000)
        page.type("input[name='Password']", "18", delay=150)
        
        elapsed = time.time() - start_time
        remaining = 23.0 - elapsed
        if remaining > 0:
            page.wait_for_timeout(int(remaining * 1000))
            
        page.click("button[type='submit']")
        
        def wait_until(target_elapsed):
            while True:
                current = time.time() - start_time
                if current >= target_elapsed:
                    break
                page.wait_for_timeout(100)

        # Login completes around 23-25s. We are on Dashboard.
        wait_until(42.7)
        print("Visitando /objetivos...")
        page.goto("http://localhost:5118/objetivos")
        
        wait_until(61.3)
        print("Visitando /seguimientos...")
        page.goto("http://localhost:5118/seguimientos")
        
        wait_until(78.0)
        print("Visitando /autoevaluacion...")
        page.goto("http://localhost:5118/autoevaluacion")
        
        wait_until(104.3)
        print("Visitando /evaluacion...")
        page.goto("http://localhost:5118/evaluacion")
        
        wait_until(129.2)
        print("Visitando /cursos...")
        page.goto("http://localhost:5118/cursos")
        
        wait_until(144.8)
        print("Visitando /calendario...")
        page.goto("http://localhost:5118/calendario")
        
        wait_until(179.0)
        print("Visitando /guia...")
        page.goto("http://localhost:5118/guia")
        
        wait_until(211.2)
        print("Visitando /admin/usuarios...")
        page.goto("http://localhost:5118/admin/usuarios")
        
        wait_until(228.3)
        print("Visitando /admin/organigrama...")
        page.goto("http://localhost:5118/admin/organigrama")
        
        wait_until(243.9)
        print("Visitando /admin/configuracion...")
        page.goto("http://localhost:5118/admin/configuracion")
        
        # Tabs inside configuracion
        wait_until(256.9)
        print("Clic en pestaña: Competencias")
        try: page.click("text=Competencias")
        except Exception as e: print(e)
        
        wait_until(269.4)
        print("Clic en pestaña: Escala de Valoración")
        try: page.click("text=Escala de Valoración")
        except Exception as e: print(e)
        
        wait_until(281.6)
        print("Clic en pestaña: Estados Objetivo")
        try: page.click("text=Estados Objetivo")
        except Exception as e: print(e)
        
        wait_until(294.1)
        print("Clic en pestaña: Estados Evaluación")
        try: page.click("text=Estados Evaluación")
        except Exception as e: print(e)
        
        wait_until(314.7)
        print("Clic en pestaña: Áreas")
        try: page.click("text=Áreas")
        except Exception as e: print(e)

        # Wait a few more seconds to finish the video
        wait_until(320.0)

        video_path = page.video.path()
        context.close()
        browser.close()

    print(f"Video guardado temporalmente en: {video_path}")
    output_file = "video_admin_v6.0.mp4"
    if os.path.exists(output_file):
        os.remove(output_file)

    print(f"Convirtiendo a {output_file} y agregando pista de audio original...")
    # Merge the video and the original audio track
    merge_cmd = [
        "ffmpeg", 
        "-i", video_path, 
        "-i", "video_admin_v5.6.mp4",
        "-c:v", "libx264", 
        "-pix_fmt", "yuv420p", 
        "-c:a", "aac",
        "-map", "0:v:0",
        "-map", "1:a:0",
        "-shortest",
        output_file
    ]
    subprocess.run(merge_cmd)
    
    os.remove(video_path)
        
    print("Video generado exitosamente:", output_file)

if __name__ == "__main__":
    main()
