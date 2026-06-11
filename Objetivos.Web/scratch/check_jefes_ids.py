import sqlite3

def run():
    conn = sqlite3.connect(r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\objetivos.db')
    cursor = conn.cursor()

    # Get JefeIds for Eduardo Casal, Marianela Artoni, Alejandro Malamud
    cursor.execute("SELECT Id, Nombre, Apellido, Email FROM Jefes WHERE Email IN ('ecasal@permaquim.com', 'martoni@permaquim.com', 'amalamud@permaquim.com')")
    for r in cursor.fetchall():
        print(r)

    conn.close()

if __name__ == '__main__':
    run()
