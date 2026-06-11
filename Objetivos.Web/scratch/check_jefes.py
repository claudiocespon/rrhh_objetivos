import sqlite3

def run():
    conn = sqlite3.connect(r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\objetivos.db')
    cursor = conn.cursor()

    print("--- Checking Jefes ---")
    cursor.execute("SELECT Id, Nombre, Apellido, Email FROM Jefes WHERE Email IN ('jcabral@permaquim.com', 'ezoroastro@permaquim.com')")
    for row in cursor.fetchall():
        print(row)

    conn.close()

if __name__ == '__main__':
    run()
