import os

mapping = {
    "imagen (1).png": "01_Login.png",
    "imagen (2).png": "02_Mis_Datos.png",
    "imagen (3).png": "03_Objetivos_Mis_Objetivos.png",
    "imagen (4).png": "04_Objetivos_De_Mi_Equipo.png",
    "imagen (5).png": "05_Objetivos_Agregar_Objetivo.png",
    "imagen (6).png": "06_Objetivos_Asignar_Objetivos.png",
    "imagen (7).png": "07_Objetivos_Asignar_Objetivos_Masivo.png",
    "imagen (8).png": "08_Objetivos_Validar_Objetivos.png",
    "imagen (9).png": "09_Objetivos_Validar_Objetivos_Masivo.png",
    "imagen (10).png": "10_Competencias_Mis_Competencias.png",
    "imagen (11).png": "11_Competencias_De_Mi_Equipo.png",
    "imagen (12).png": "12_Seguimientos_Mis_Seguimientos.png",
    "imagen (13).png": "13_Seguimientos_De_Mi_Equipo.png",
    "imagen (14).png": "14_Autoevaluacion_Mis_Autoevaluaciones.png",
    "imagen (15).png": "15_Autoevaluacion_De_Mi_Equipo.png",
    "imagen (16).png": "16_Evaluacion_Feedback_Mitad_Ano.png",
    "imagen (17).png": "17_Evaluacion_Feedback_Mitad_Ano_Realizados.png",
    "imagen (18).png": "18_Evaluacion_Finales_Pendientes.png",
    "imagen (19).png": "19_Evaluacion_Finales_Realizadas.png",
    "imagen (20).png": "20_Evaluacion_Mis_Evaluaciones_Recibidas.png",
    "imagen (21).png": "21_Cursos_Mis_Cursos.png",
    "imagen (22).png": "22_Calendario.png",
    "imagen (23).png": "23_Guia.png",
    "imagen (24).png": "24_Admin_Dashboard.png",
    "imagen (25).png": "25_Admin_Objetivos_Mis_Objetivos.png",
    "imagen (26).png": "26_Admin_Objetivos_De_Mi_Equipo.png",
    "imagen (27).png": "27_Admin_Objetivos_Asignar_Objetivos.png",
    "imagen (28).png": "28_Admin_Objetivos_Asignar_Objetivos_Masivo.png",
    "imagen (29).png": "29_Admin_Objetivos_Validar_Objetivos.png",
    "imagen (30).png": "30_Admin_Objetivos_Toda_La_Organizacion.png",
    "imagen (31).png": "31_Admin_Objetivos_Aprobar_Objetivos_Toda_La_Organizacion.png",
    "imagen (32).png": "32_Admin_Objetivos_Fijacion_De_Objetivos.png",
    "imagen (33).png": "33_Admin_Competencias_Mis_Competencias.png",
    "imagen (34).png": "34_Admin_Competencias_De_Mi_Equipo.png",
    "imagen (35).png": "35_Admin_Competencias_Evaluacion_Toda_La_Organizacion.png",
    "imagen (36).png": "36_Admin_Seguimientos_Mis_Seguimientos.png",
    "imagen (37).png": "37_Admin_Seguimientos_Toda_La_Organizacion.png",
    "imagen (38).png": "38_Admin_Seguimientos_Mi_Rendimiento.png",
    "imagen (39).png": "39_Admin_Autoevaluacion_De_Mi_Equipo.png",
    "imagen (40).png": "40_Admin_Autoevaluacion_Mis_Autoevaluaciones.png",
    "imagen (41).png": "41_Admin_Evaluacion_Feedback_Mitad_Ano.png",
    "imagen (42).png": "42_Admin_Evaluacion_Feedback_Mitad_Ano_Realizados.png",
    "imagen (43).png": "43_Admin_Evaluacion_Finales_Pendientes.png",
    "imagen (44).png": "44_Admin_Evaluacion_Finales_Realizadas.png",
    "imagen (45).png": "45_Admin_Evaluacion_Mis_Evaluaciones_Recibidas.png",
    "imagen (46).png": "46_Admin_Cursos_Catalogo.png",
    "imagen (47).png": "47_Admin_Calendario.png",
    "imagen (48).png": "48_Guia_Pilares_Estrategicos.png",
    "imagen (49).png": "49_Guia_Competencias.png",
    "imagen (50).png": "50_Guia_Escala_De_Valoracion.png",
    "imagen (51).png": "51_Admin_Config_Pilares_Estrategicos.png",
    "imagen (52).png": "52_Admin_Config_Competencias.png",
    "imagen (53).png": "53_Admin_Config_Escala_De_Valoracion.png",
    "imagen (54).png": "54_Admin_Config_Estados_Objetivo.png",
    "imagen (55).png": "55_Admin_Config_Estados_Evaluacion.png",
    "imagen (56).png": "56_Admin_Config_Areas.png",
    "imagen (57).png": "57_Admin_Config_Puestos.png",
    "imagen (58).png": "58_Admin_Config_Configuraciones.png",
    "imagen (59).png": "59_Admin_Usuarios.png",
}

capturas_dir = r"c:\Development\Antigravity\RRHH_Objetivos\Capturas"

for old_name, new_name in mapping.items():
    old_path = os.path.join(capturas_dir, old_name)
    new_path = os.path.join(capturas_dir, new_name)
    if os.path.exists(old_path):
        os.rename(old_path, new_path)
        print(f"Renamed {old_name} to {new_name}")
    else:
        print(f"File {old_name} not found")

print("Done renaming!")
