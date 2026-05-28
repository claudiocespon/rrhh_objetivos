import sqlite3
import sys

db_path = r"X:\wwwroot\Objetivos\objetivos.db"

tables_to_clear = [
    "RevisionesCuatrimestrales",
    "EvaluacionesFinales",
    "Autoevaluaciones",
    "BitacoraEntradas",
    "EventosCalendario",
    "Objetivos",
    "MensajesChat",
    "AuditoriaLogs",
    "Notificaciones",
    "CursoAsignaciones"
]

try:
    conn = sqlite3.connect(db_path)
    c = conn.cursor()
    
    for table in tables_to_clear:
        print(f"Borrando {table}...")
        c.execute(f"DELETE FROM {table}")
        
    conn.commit()
    conn.close()
    print("Datos operativos eliminados exitosamente.")
except Exception as e:
    print(f"Error: {e}")
    sys.exit(1)
