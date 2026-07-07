import os
import shutil

USER_TEMP_DIR = r"C:\Users\ccespon\Downloads\ManualUsuarioTemp"
ADMIN_TEMP_DIR = r"C:\Users\ccespon\Downloads\ManualAdminTemp"
CAPTURAS_DIR = r"C:\Development\Antigravity\RRHH_Objetivos\Capturas"

# Mapeo manual basado en las visualizaciones
user_map = {
    "imagen (22).png": "22_Usuario_Dashboard.png",
    "imagen (23).png": "23_Usuario_Objetivos_Equipo.png",
    "imagen (24).png": "24_Usuario_Objetivos_Propios.png",
    "imagen (25).png": "25_Usuario_Seguimientos_Rendimiento.png",
    "imagen (26).png": "26_Usuario_Autoevaluaciones_Pendientes.png",
    "imagen (27).png": "27_Usuario_Autoevaluaciones_Propias.png",
    "imagen (28).png": "28_Usuario_Evaluaciones_Feedback.png",
    "imagen (29).png": "29_Usuario_Evaluaciones_Finales.png",
    "imagen (30).png": "30_Usuario_Cursos.png",
    "imagen (31).png": "31_Usuario_Calendario.png",
    "imagen (32).png": "32_Usuario_Guia.png"
}

# Para el admin, voy a usar nombres secuenciales descriptivos genéricos para los que no vi,
# pero asumiendo el orden lógico del menú.
admin_map = {
    "imagen (33).png": "33_Admin_Dashboard.png",
    "imagen (34).png": "34_Admin_Objetivos_Pendientes.png",
    "imagen (35).png": "35_Admin_Objetivos_Equipo.png",
    "imagen (36).png": "36_Admin_Objetivos_Propios.png",
    "imagen (37).png": "37_Admin_Seguimientos_Organizacion.png",
    "imagen (38).png": "38_Admin_Seguimientos_Rendimiento.png",
    "imagen (39).png": "39_Admin_Autoevaluaciones_Equipo.png",
    "imagen (40).png": "40_Admin_Autoevaluaciones_Propias.png",
    "imagen (41).png": "41_Admin_Evaluaciones_Pendientes.png",
    "imagen (42).png": "42_Admin_Evaluaciones_Realizadas.png",
    "imagen (43).png": "43_Admin_Evaluaciones_Recibidas.png",
    "imagen (44).png": "44_Admin_Cursos.png",
    "imagen (45).png": "45_Admin_Calendario.png",
    "imagen (46).png": "46_Admin_Guia.png",
    "imagen (47).png": "47_Admin_Config_General.png",
    "imagen (48).png": "48_Admin_Config_General_2.png",
    "imagen (49).png": "49_Admin_Config_General_3.png",
    "imagen (50).png": "50_Admin_Config_General_4.png",
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

print("Limpiando imagenes del 22 al 59 en Capturas...")
for i in range(22, 60):
    for f in os.listdir(CAPTURAS_DIR):
        if f.startswith(f"{i}_") and f.endswith(".png"):
            os.remove(os.path.join(CAPTURAS_DIR, f))
            print(f"Eliminado: {f}")

print("Copiando imagenes de Usuario...")
for old_name, new_name in user_map.items():
    src = os.path.join(USER_TEMP_DIR, old_name)
    dst = os.path.join(CAPTURAS_DIR, new_name)
    if os.path.exists(src):
        shutil.copy2(src, dst)
        print(f"Copiado: {new_name}")
    else:
        print(f"Falta: {src}")

print("Copiando imagenes de Admin...")
for old_name, new_name in admin_map.items():
    src = os.path.join(ADMIN_TEMP_DIR, old_name)
    dst = os.path.join(CAPTURAS_DIR, new_name)
    if os.path.exists(src):
        shutil.copy2(src, dst)
        print(f"Copiado: {new_name}")
    else:
        print(f"Falta: {src}")

print("Renombrado y copiado finalizado.")
