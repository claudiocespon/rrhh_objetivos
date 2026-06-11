import sqlite3
import unicodedata

def run():
    conn = sqlite3.connect(r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\objetivos.db')
    cursor = conn.cursor()

    cursor.execute("SELECT e.Email, j.Apellido, j.Nombre FROM Empleados e LEFT JOIN Jefes j ON e.JefeId = j.Id")
    for r in cursor.fetchall()[:10]:
        print(r)
    
    conn.close()

if __name__ == '__main__':
    run()
