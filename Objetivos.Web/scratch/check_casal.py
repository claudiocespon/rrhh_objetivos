import sqlite3

def run():
    conn = sqlite3.connect(r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\objetivos.db')
    cursor = conn.cursor()

    print("--- Searching for Eduardo Casal ---")
    cursor.execute("SELECT Id, Nombre, Apellido, Email, JefeId FROM Empleados WHERE Email='ecasal@permaquim.com'")
    print("Empleado:", cursor.fetchone())
    
    cursor.execute("SELECT Id, Nombre, Apellido, Email FROM Jefes WHERE Email='ecasal@permaquim.com'")
    print("Jefe:", cursor.fetchone())

    print("--- Searching for Matias Marquez ---")
    cursor.execute("SELECT Id, Nombre, Apellido, Email, JefeId FROM Empleados WHERE Email='mmarquez@permaquim.com'")
    print("Empleado:", cursor.fetchone())

    cursor.execute("SELECT Id, Nombre, Apellido, Email FROM Jefes WHERE Email='mmarquez@permaquim.com'")
    print("Jefe:", cursor.fetchone())

    conn.close()

if __name__ == '__main__':
    run()
