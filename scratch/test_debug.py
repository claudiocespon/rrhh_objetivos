import os
import time
import subprocess
import requests
from playwright.sync_api import sync_playwright

PORT = 5118
BASE_URL = f"http://localhost:{PORT}"
ADMIN_EMAIL = "ptripodi@permaquim.com"
ADMIN_PASS = "18"

# Start Blazor Server in background
server_process = subprocess.Popen(
    ["dotnet", "run", "--project", "Objetivos.Web/Objetivos.Web.csproj", "--urls", BASE_URL],
    stdout=subprocess.PIPE,
    stderr=subprocess.PIPE,
    text=True
)

# Wait up to 15 seconds for server availability
for i in range(15):
    try:
        r = requests.get(f"{BASE_URL}/login", timeout=2)
        if r.status_code == 200:
            print("Blazor Server launched successfully!")
            break
    except Exception:
        pass
    time.sleep(1)

try:
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        context = browser.new_context(viewport={"width": 1440, "height": 900})
        page = context.new_page()

        # Listen to console events
        page.on("console", lambda msg: print(f"BROWSER CONSOLE: [{msg.type}] {msg.text}"))
        page.on("pageerror", lambda err: print(f"BROWSER ERROR: {err}"))
        page.on("requestfailed", lambda req: print(f"REQUEST FAILED: {req.url} - {req.failure}"))

        steps = [
            ("/login", "Login Page"),
            ("LOGIN_ACTION", "Login Action"),
            ("/dashboard", "Dashboard Team"),
            ("TAB_PERSONAL", "Dashboard Personal"),
            ("/objetivos", "Objetivos"),
            ("/seguimientos", "Seguimientos"),
            ("/seguimientos/2", "Colaborador Detalle"),
            ("/seguimientos/2/objetivo/1", "Objetivo Detalle"),
            ("/autoevaluacion", "Autoevaluacion"),
            ("/evaluacion", "Evaluacion"),
            ("/cursos", "Cursos"),
            ("/calendario", "Calendario"),
            ("/guia", "Guia"),
            ("/admin/configuracion", "Configuracion"),
            ("/admin/usuarios", "Usuarios")
        ]

        for path, name in steps:
            print(f"\n--- STEP: {name} ({path}) ---")
            try:
                if path == "LOGIN_ACTION":
                    page.fill('input[name="Email"]', ADMIN_EMAIL)
                    page.fill('input[name="Password"]', ADMIN_PASS)
                    page.click("button.login-btn")
                    page.wait_for_url(f"{BASE_URL}/dashboard", timeout=10000)
                    print("Logged in successfully!")
                elif path == "TAB_PERSONAL":
                    page.click('text="Mis Objetivos"', timeout=5000)
                    page.wait_for_timeout(1000)
                    print("Clicked personal tab!")
                else:
                    page.goto(f"{BASE_URL}{path}", timeout=15000)
                    page.wait_for_timeout(1500)
                    # wait for loading bar if any
                    try:
                        page.wait_for_selector(".rz-progressbar", state="hidden", timeout=2000)
                    except Exception:
                        pass
                    print(f"Loaded {name} successfully!")
            except Exception as e:
                print(f"ERROR on step {name}: {e}")
                page.screenshot(path=f"docs/screenshots/error_{name.replace(' ', '_')}.png")
                print(f"Saved error screenshot for {name}")


        browser.close()
finally:
    print("Terminating server...")
    server_process.terminate()
    try:
        server_process.wait(timeout=5)
    except Exception:
        server_process.kill()
    print("Server terminated.")
