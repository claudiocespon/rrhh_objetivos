import sqlite3

def run():
    conn = sqlite3.connect(r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\objetivos.db')
    cursor = conn.cursor()

    cursor.execute("SELECT e.Email, e.Nombre, e.Apellido FROM Empleados e LEFT JOIN Jefes j ON e.JefeId = j.Id WHERE e.JefeId IS NULL OR e.JefeId = 0")
    print("--- ROOT EMPLOYEES (NO JEFE) ---")
    for row in cursor.fetchall():
        print(row)

    conn.close()

if __name__ == '__main__':
    run()
