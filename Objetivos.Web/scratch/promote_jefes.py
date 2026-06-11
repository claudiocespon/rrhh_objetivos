import sqlite3

def run():
    conn = sqlite3.connect(r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\objetivos.db')
    cursor = conn.cursor()

    emails = ['jcabral@permaquim.com', 'ezoroastro@permaquim.com']

    for email in emails:
        cursor.execute("SELECT Nombre, Apellido, Email, Legajo, PasswordHash, DebeCambiarPassword, AreaId, PaisId, Activo, FechaBaja, EsSuperusuario FROM Empleados WHERE Email=?", (email,))
        emp = cursor.fetchone()
        if emp:
            # Check if already in Jefes
            cursor.execute("SELECT Id FROM Jefes WHERE Email=?", (email,))
            if cursor.fetchone():
                print(f"Already in Jefes: {email}")
                continue
            
            # Insert into Jefes
            nombre, apellido, em, legajo, pash, debe_cambiar, area_id, pais_id, activo, baja, superuser = emp
            cursor.execute("""
                INSERT INTO Jefes (Nombre, Apellido, Email, Legajo, PasswordHash, DebeCambiarPassword, AreaId, PaisId, Activo, FechaBaja, EsSuperusuario, Rol, FechaCreacion)
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, 'JEFE', CURRENT_TIMESTAMP)
            """, (nombre, apellido, em, legajo, pash, debe_cambiar, area_id, pais_id, activo, baja, superuser))
            print(f"Promoted to Jefe: {email}")
        else:
            print(f"Empleado not found: {email}")

    conn.commit()
    conn.close()

if __name__ == '__main__':
    run()
