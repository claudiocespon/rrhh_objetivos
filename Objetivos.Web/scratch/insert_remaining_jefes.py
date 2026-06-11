import sqlite3

def run():
    conn = sqlite3.connect(r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\objetivos.db')
    cursor = conn.cursor()

    # Get data for Marianela Artoni
    cursor.execute("SELECT Nombre, Apellido, Email, Legajo, PasswordHash, DebeCambiarPassword, AreaId, PaisId, Activo, FechaBaja, EsSuperusuario FROM Jefes WHERE Email='martoni@permaquim.com'")
    mj = cursor.fetchone()
    if mj:
        nombre, apellido, email, legajo, pash, debe_cambiar, area_id, pais_id, activo, baja, superuser = mj
        cursor.execute("""
            INSERT INTO Empleados (Nombre, Apellido, Email, Legajo, PasswordHash, DebeCambiarPassword, AreaId, PaisId, Activo, FechaBaja, EsSuperusuario, FechaIngreso, JefeId)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, CURRENT_TIMESTAMP, 28)
        """, (nombre, apellido, email, legajo, pash, debe_cambiar, area_id, pais_id, activo, baja, superuser))
        print("Inserted Marianela Artoni under Eduardo Casal (28)")

    # Get data for Cristian Jara
    cursor.execute("SELECT Nombre, Apellido, Email, Legajo, PasswordHash, DebeCambiarPassword, AreaId, PaisId, Activo, FechaBaja, EsSuperusuario FROM Jefes WHERE Email='cjara@permaquim.com'")
    mj = cursor.fetchone()
    if mj:
        nombre, apellido, email, legajo, pash, debe_cambiar, area_id, pais_id, activo, baja, superuser = mj
        cursor.execute("""
            INSERT INTO Empleados (Nombre, Apellido, Email, Legajo, PasswordHash, DebeCambiarPassword, AreaId, PaisId, Activo, FechaBaja, EsSuperusuario, FechaIngreso, JefeId)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, CURRENT_TIMESTAMP, 37)
        """, (nombre, apellido, email, legajo, pash, debe_cambiar, area_id, pais_id, activo, baja, superuser))
        print("Inserted Cristian Jara under Marianela Artoni (37)")

    conn.commit()
    conn.close()

if __name__ == '__main__':
    run()
