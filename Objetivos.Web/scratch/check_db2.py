import sqlite3
import unicodedata
import csv

def run():
    conn = sqlite3.connect(r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\objetivos.db')
    cursor = conn.cursor()

    with open(r'c:\Development\Antigravity\RRHH_Objetivos\usuarios_responsables.csv', 'r', encoding='utf-8-sig') as f:
        reader = csv.DictReader(f, delimiter=';')
        count = 0
        for row in reader:
            email = row.get('Email', '').strip()
            resp_str = row.get('Responsable', '').strip()

            if not email or not resp_str or resp_str == '-':
                continue

            # Fetch what is in the DB
            cursor.execute("SELECT e.JefeId, j.Apellido, j.Nombre FROM Empleados e LEFT JOIN Jefes j ON e.JefeId = j.Id WHERE LOWER(e.Email) = ?", (email.lower(),))
            res = cursor.fetchone()
            if res:
                jefe_id, j_apellido, j_nombre = res
                print(f"CSV: {resp_str}  --- DB: {j_apellido}, {j_nombre}")
                count += 1
            if count >= 10:
                break
    conn.close()

if __name__ == '__main__':
    run()
