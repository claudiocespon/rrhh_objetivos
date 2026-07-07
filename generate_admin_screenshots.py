import os
from playwright.sync_api import sync_playwright

def main():
    capturas_dir = r"c:\Development\Antigravity\RRHH_Objetivos\Capturas"
    os.makedirs(capturas_dir, exist_ok=True)
    
    def take_screenshot(page, filename):
        path = os.path.join(capturas_dir, filename)
        print(f"Tomando captura: {filename}")
        page.screenshot(path=path, full_page=False)

    print("Iniciando Playwright...")
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        context = browser.new_context(
            viewport={"width": 1920, "height": 1080},
            device_scale_factor=1
        )
        page = context.new_page()
        
        # 1. Login
        page.goto("http://localhost:5118/login")
        page.fill("input[name='Email']", "ptripodi@permaquim.com")
        page.fill("input[name='Password']", "18")
        page.click("button[type='submit']")
        
        # Wait for Dashboard
        page.wait_for_timeout(3000)
        page.wait_for_url("**/dashboard")
        page.wait_for_timeout(2000)
        
        # 33_Admin_Dashboard.png
        take_screenshot(page, "33_Admin_Dashboard.png")

        # Objetivos
        page.goto("http://localhost:5118/objetivos")
        page.wait_for_timeout(2000)
        try:
            page.click("text=Pendientes de Aprobación", timeout=2000)
            page.wait_for_timeout(1000)
        except: pass
        take_screenshot(page, "34_Admin_Objetivos_Pendientes.png")

        try:
            page.click("text=Revisar Aprobaciones", timeout=2000)
            page.wait_for_timeout(2000)
            take_screenshot(page, "34b_Admin_Aprobar_Objetivos_Modal.png")
            page.keyboard.press("Escape")
            page.wait_for_timeout(1000)
        except: pass

        try:
            page.click("text=Objetivos de mi Equipo", timeout=2000)
            page.wait_for_timeout(1000)
        except: pass
        take_screenshot(page, "35_Admin_Objetivos_Equipo.png")

        try:
            page.click("text=Mis Objetivos", timeout=2000)
            page.wait_for_timeout(1000)
        except: pass
        take_screenshot(page, "36_Admin_Objetivos_Propios.png")

        # Seguimientos
        page.goto("http://localhost:5118/seguimientos")
        page.wait_for_timeout(2000)
        try:
            page.click("text=Toda la Organización", timeout=2000)
            page.wait_for_timeout(1000)
        except: pass
        take_screenshot(page, "37_Admin_Seguimientos_Organizacion.png")

        try:
            page.click("text=Mi Rendimiento", timeout=2000)
            page.wait_for_timeout(1000)
        except: pass
        take_screenshot(page, "38_Admin_Seguimientos_Rendimiento.png")

        # Autoevaluacion
        page.goto("http://localhost:5118/autoevaluacion")
        page.wait_for_timeout(2000)
        try:
            page.click("text=Mi Equipo", timeout=2000)
            page.wait_for_timeout(1000)
        except: pass
        take_screenshot(page, "39_Admin_Autoevaluaciones_Equipo.png")

        try:
            page.click("text=Mis Autoevaluaciones", timeout=2000)
            page.wait_for_timeout(1000)
        except: pass
        take_screenshot(page, "40_Admin_Autoevaluaciones_Propias.png")

        # Evaluacion
        page.goto("http://localhost:5118/evaluacion")
        page.wait_for_timeout(2000)
        try:
            page.click("text=Feedback Mitad de Año (Equipo)", timeout=2000)
            page.wait_for_timeout(1000)
        except: pass
        take_screenshot(page, "41_Admin_Eval_Feedback_Mitad_Ano.png")

        try:
            page.click("text=Feedback Mitad de Año Realizados (Equipo)", timeout=2000)
            page.wait_for_timeout(1000)
        except: pass
        take_screenshot(page, "42_Admin_Eval_Feedback_Mitad_Ano_Realizados.png")

        try:
            page.click("text=Evaluaciones Finales Pendientes", timeout=2000)
            page.wait_for_timeout(1000)
        except: pass
        take_screenshot(page, "43_Admin_Eval_Finales_Pendientes.png")

        try:
            page.click("text=Evaluaciones Finales Realizadas", timeout=2000)
            page.wait_for_timeout(1000)
        except: pass
        take_screenshot(page, "44_Admin_Eval_Finales_Realizadas.png")

        try:
            page.click("text=Mis Evaluaciones Recibidas", timeout=2000)
            page.wait_for_timeout(1000)
        except: pass
        take_screenshot(page, "45_Admin_Eval_Mis_Recibidas.png")

        # Cursos
        page.goto("http://localhost:5118/cursos")
        page.wait_for_timeout(2000)
        take_screenshot(page, "46_Admin_Cursos.png")

        # Calendario
        page.goto("http://localhost:5118/calendario")
        page.wait_for_timeout(2000)
        take_screenshot(page, "47_Admin_Calendario.png")

        # Guia
        page.goto("http://localhost:5118/guia")
        page.wait_for_timeout(2000)
        take_screenshot(page, "48_Admin_Guia.png")
        
        try:
            page.click("text=Competencias", timeout=2000)
            page.wait_for_timeout(1000)
        except: pass
        take_screenshot(page, "49_Admin_Guia_2.png")
        
        try:
            page.click("text=Escala de Valoración", timeout=2000)
            page.wait_for_timeout(1000)
        except: pass
        take_screenshot(page, "50_Admin_Guia_3.png")

        # Admin / Configuracion
        page.goto("http://localhost:5118/admin/configuracion")
        page.wait_for_timeout(2000)
        
        tabs = [
            ("Pilares Estratégicos", "51_Admin_Config_Pilares.png"),
            ("Competencias", "52_Admin_Config_Competencias.png"),
            ("Escala de Valoración", "53_Admin_Config_Escalas.png"),
            ("Estados Objetivo", "54_Admin_Config_Estados_Obj.png"),
            ("Estados Evaluación", "55_Admin_Config_Estados_Eval.png"),
            ("Áreas", "56_Admin_Config_Areas.png"),
            ("Puestos", "57_Admin_Config_Puestos.png"),
            ("Configuraciones", "58_Admin_Config_Configuracion.png")
        ]

        for text, filename in tabs:
            try:
                page.click(f"text={text}", timeout=2000)
                page.wait_for_timeout(1000)
            except: pass
            take_screenshot(page, filename)

        # Admin / Usuarios
        page.goto("http://localhost:5118/admin/usuarios")
        page.wait_for_timeout(2000)
        take_screenshot(page, "59_Admin_Usuarios.png")

        context.close()
        browser.close()
        print("Proceso finalizado.")

if __name__ == "__main__":
    main()
