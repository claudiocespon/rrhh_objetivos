import sqlite3
import json

def run():
    conn = sqlite3.connect(r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\objetivos.db')
    conn.row_factory = sqlite3.Row
    cursor = conn.cursor()

    # Get all Empleados
    cursor.execute("SELECT * FROM Empleados")
    empleados = [dict(row) for row in cursor.fetchall()]

    # Get all Jefes
    cursor.execute("SELECT * FROM Jefes")
    jefes = [dict(row) for row in cursor.fetchall()]

    data = {
        'empleados': empleados,
        'jefes': jefes
    }

    with open(r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\scratch\db_backup.json', 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=2)

    print(f"Exported {len(empleados)} Empleados and {len(jefes)} Jefes to db_backup.json")
    conn.close()

if __name__ == '__main__':
    run()
