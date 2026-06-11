import sqlite3

def run():
    conn = sqlite3.connect(r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\objetivos.db')
    cursor = conn.cursor()

    print("--- Searching for Cabral ---")
    cursor.execute("SELECT Id, Nombre, Apellido, Email, EsSuperusuario FROM Empleados WHERE Apellido LIKE '%Cabral%' OR Nombre LIKE '%Cabral%'")
    for row in cursor.fetchall():
        print(row)

    print("--- Searching for Zoroastro ---")
    cursor.execute("SELECT Id, Nombre, Apellido, Email, EsSuperusuario FROM Empleados WHERE Apellido LIKE '%Zoroastro%' OR Nombre LIKE '%Zoroastro%'")
    for row in cursor.fetchall():
        print(row)

    conn.close()

if __name__ == '__main__':
    run()
