import sqlite3
import csv
import unicodedata

def normalize(text):
    if not text or text == '-':
        return ""
    text = unicodedata.normalize('NFKD', text).encode('ASCII', 'ignore').decode('utf-8')
    return text.lower().replace('\xa0', ' ').strip()

def run():
    conn = sqlite3.connect(r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\objetivos.db')
    cursor = conn.cursor()

    mismatches = 0
    matches = 0
    missing = 0

    with open(r'c:\Development\Antigravity\RRHH_Objetivos\usuarios_responsables.csv', 'r', encoding='utf-8-sig') as f:
        reader = csv.DictReader(f, delimiter=';')
        for row in reader:
            email = row.get('Email', '').strip()
            resp_str = row.get('Responsable', '').strip()

            if not email or not resp_str or resp_str == '-':
                continue

            # Fetch DB
            cursor.execute("SELECT e.JefeId, j.Apellido, j.Nombre FROM Empleados e LEFT JOIN Jefes j ON e.JefeId = j.Id WHERE LOWER(e.Email) = ?", (email.lower(),))
            res = cursor.fetchone()
            
            if res:
                jefe_id, j_apellido, j_nombre = res
                db_resp = f"{j_apellido}, {j_nombre}"
                if normalize(db_resp) == normalize(resp_str):
                    matches += 1
                else:
                    print(f"MISMATCH: CSV='{resp_str}' vs DB='{db_resp}' (Email: {email})")
                    mismatches += 1
            else:
                missing += 1

    print(f"Matches: {matches}, Mismatches: {mismatches}, Missing: {missing}")
    conn.close()

if __name__ == '__main__':
    run()
