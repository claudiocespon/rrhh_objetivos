from playwright.sync_api import sync_playwright
try:
    with sync_playwright() as p:
        browser = p.chromium.launch()
        print("Playwright launched successfully!")
        browser.close()
except Exception as e:
    print(f"Error launching: {e}")
