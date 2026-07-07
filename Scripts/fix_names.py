import os
import shutil

ADMIN_TEMP_DIR = r"C:\Users\ccespon\Downloads\ManualAdminTemp"
CAPTURAS_DIR = r"C:\Development\Antigravity\RRHH_Objetivos\Capturas"

admin_map = {
    "imagen (41).png": "41_Admin_Eval_Feedback_Mitad_Ano.png",
    "imagen (42).png": "42_Admin_Eval_Feedback_Mitad_Ano_Realizados.png",
    "imagen (43).png": "43_Admin_Eval_Finales_Pendientes.png",
    "imagen (44).png": "44_Admin_Eval_Finales_Realizadas.png",
    "imagen (45).png": "45_Admin_Eval_Mis_Recibidas.png",
    "imagen (46).png": "46_Admin_Cursos.png",
    "imagen (47).png": "47_Admin_Calendario.png",
    "imagen (48).png": "48_Admin_Guia.png",
    "imagen (49).png": "49_Admin_Guia_2.png",
    "imagen (50).png": "50_Admin_Guia_3.png",
    "imagen (51).png": "51_Admin_Config_Pilares.png",
    "imagen (52).png": "52_Admin_Config_Competencias.png",
    "imagen (53).png": "53_Admin_Config_Escalas.png",
    "imagen (54).png": "54_Admin_Config_Estados_Obj.png",
    "imagen (55).png": "55_Admin_Config_Estados_Eval.png",
    "imagen (56).png": "56_Admin_Config_Areas.png",
    "imagen (57).png": "57_Admin_Config_Puestos.png",
    "imagen (58).png": "58_Admin_Config_Configuracion.png",
    "imagen (59).png": "59_Admin_Usuarios.png",
}

print("Limpiando imagenes del 41 al 59 en Capturas...")
for i in range(41, 60):
    for f in os.listdir(CAPTURAS_DIR):
        if f.startswith(f"{i}_") and f.endswith(".png"):
            os.remove(os.path.join(CAPTURAS_DIR, f))
            print(f"Eliminado: {f}")

print("Copiando imagenes corregidas de Admin...")
for old_name, new_name in admin_map.items():
    src = os.path.join(ADMIN_TEMP_DIR, old_name)
    dst = os.path.join(CAPTURAS_DIR, new_name)
    if os.path.exists(src):
        shutil.copy2(src, dst)
        print(f"Copiado: {new_name}")
    else:
        print(f"Falta: {src}")

print("Renombrado y copiado finalizado.")
