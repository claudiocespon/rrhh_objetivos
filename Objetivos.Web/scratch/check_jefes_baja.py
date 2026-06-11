import sqlite3

def run():
    conn = sqlite3.connect(r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\objetivos.db')
    cursor = conn.cursor()

    cursor.execute("SELECT Id, Email, FechaBaja FROM Jefes WHERE Email IN ('jcabral@permaquim.com', 'ezoroastro@permaquim.com')")
    print(cursor.fetchall())

    conn.close()

if __name__ == '__main__':
    run()
