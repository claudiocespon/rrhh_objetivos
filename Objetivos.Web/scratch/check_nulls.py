import sqlite3

def run():
    conn = sqlite3.connect(r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\objetivos.db')
    cursor = conn.cursor()

    cursor.execute("SELECT Id, Apellido, Nombre FROM Jefes WHERE Apellido IS NULL OR Nombre IS NULL")
    rows = cursor.fetchall()
    print(f"Jefes with NULL Apellido or Nombre: {len(rows)}")
    for r in rows:
        print(r)

    conn.close()

if __name__ == '__main__':
    run()
