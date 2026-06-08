import os

admin_md = r"C:\Users\ccespon\.gemini\antigravity-ide\brain\2c89c6bb-9222-48f5-b2d3-3b9ef5c5b014\video_script_admin_mapped.md"
emp_md = r"C:\Users\ccespon\.gemini\antigravity-ide\brain\2c89c6bb-9222-48f5-b2d3-3b9ef5c5b014\video_script_empleado_mapped.md"
gen_py = r"c:\Development\Antigravity\RRHH_Objetivos\scratch\generate_video.py"

def fix_md(path):
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    content = content.replace("RRHH Objetivos", "plataforma de gestión de talento PQ Talent")
    content = content.replace("portal de plataforma de gestión de talento PQ Talent", "portal de la plataforma de gestión de talento PQ Talent")
    
    # Just in case there are weird capitalizations or 'portal de'
    with open(path, 'w', encoding='utf-8') as f:
        f.write(content)

fix_md(admin_md)
fix_md(emp_md)

with open(gen_py, 'r', encoding='utf-8') as f:
    py_content = f.read()

# Replace the final_duration logic
py_content = py_content.replace(
    "final_duration = max(float(target_duration), audio_duration + 0.5)",
    "final_duration = audio_duration + 3.0"
)

with open(gen_py, 'w', encoding='utf-8') as f:
    f.write(py_content)

print("Updates completed.")
