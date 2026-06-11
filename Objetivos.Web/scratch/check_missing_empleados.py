import sqlite3

def run():
    conn = sqlite3.connect(r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\objetivos.db')
    cursor = conn.cursor()

    cursor.execute("SELECT Email FROM Jefes WHERE Email NOT IN (SELECT Email FROM Empleados)")
    rows = cursor.fetchall()
    print(f"There are {len(rows)} Jefes not in Empleados:")
    for r in rows:
        print(r[0])
    
    conn.close()

if __name__ == '__main__':
    run()
