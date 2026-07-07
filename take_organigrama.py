from playwright.sync_api import sync_playwright
import time

with sync_playwright() as p:
    browser = p.chromium.launch(headless=True)
    context = browser.new_context(viewport={'width': 1920, 'height': 1080})
    page = context.new_page()
    page.goto('http://localhost:5118/login')
    page.type("input[name='Email']", 'ptripodi@permaquim.com')
    page.type("input[name='Password']", '18')
    page.click("button[type='submit']")
    page.wait_for_timeout(3000)
    page.goto('http://localhost:5118/admin/organigrama')
    page.wait_for_load_state('networkidle')
    page.wait_for_timeout(3000)
    page.screenshot(path='Capturas/60_Admin_Organigrama.png')
    browser.close()
