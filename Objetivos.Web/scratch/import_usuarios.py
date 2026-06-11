import json
import sqlite3
import datetime

def run():
    db_path = r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\objetivos.db'
    backup_path = r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\scratch\db_backup.json'

    with open(backup_path, 'r', encoding='utf-8') as f:
        data = json.load(f)

    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()

    try:
        # Clear the table just in case
        cursor.execute("DELETE FROM Usuarios")

        now = datetime.datetime.utcnow().isoformat()

        # Insert Empleados
        for emp in data['empleados']:
            cursor.execute("""
                INSERT INTO Usuarios (
                    Id, Nombre, Apellido, Email, Legajo, PasswordHash, DebeCambiarPassword,
                    PuestoId, AreaId, PaisId, JefeId, Rol, Activo, FechaBaja, EsSuperusuario,
                    FechaCreacion, FechaIngreso
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            """, (
                emp['Id'], emp['Nombre'], emp['Apellido'], emp['Email'], emp['Legajo'],
                emp['PasswordHash'], emp['DebeCambiarPassword'], emp.get('PuestoId'),
                emp['AreaId'], emp['PaisId'], (emp['JefeId'] + 10000) if emp.get('JefeId') else None,
                "COLABORADOR", emp['Activo'], emp.get('FechaBaja'), emp['EsSuperusuario'],
                now, emp.get('FechaIngreso', now)
            ))

        # Insert Jefes
        for jefe in data['jefes']:
            cursor.execute("""
                INSERT INTO Usuarios (
                    Id, Nombre, Apellido, Email, Legajo, PasswordHash, DebeCambiarPassword,
                    PuestoId, AreaId, PaisId, JefeId, Rol, Activo, FechaBaja, EsSuperusuario,
                    FechaCreacion, FechaIngreso
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            """, (
                jefe['Id'] + 10000, jefe['Nombre'], jefe['Apellido'], jefe['Email'], jefe['Legajo'],
                jefe['PasswordHash'], jefe['DebeCambiarPassword'], None,
                jefe['AreaId'], jefe['PaisId'], None,
                jefe['Rol'], jefe['Activo'], jefe.get('FechaBaja'), jefe['EsSuperusuario'],
                jefe.get('FechaCreacion', now), now
            ))

        # Update MensajesChat
        cursor.execute("UPDATE MensajesChat SET RemitenteId = RemitenteId + 10000 WHERE RemitenteEsJefe = 1")
        cursor.execute("UPDATE MensajesChat SET JefeId = JefeId + 10000")

        conn.commit()
        print("Data imported successfully.")
    except Exception as e:
        print(f"Error: {e}")
        conn.rollback()
    finally:
        conn.close()

if __name__ == '__main__':
    run()
