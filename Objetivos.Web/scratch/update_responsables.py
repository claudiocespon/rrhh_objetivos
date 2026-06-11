import sqlite3
import csv
import unicodedata

def normalize(text):
    if not text or text == '-':
        return ""
    # Remove accents
    text = unicodedata.normalize('NFKD', text).encode('ASCII', 'ignore').decode('utf-8')
    # Lowercase and replace weird spaces
    return text.lower().replace('\xa0', ' ').strip()

def run():
    conn = sqlite3.connect(r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\objetivos.db')
    cursor = conn.cursor()

    # Load Jefes
    cursor.execute("SELECT Id, Nombre, Apellido, Email FROM Jefes")
    jefes = cursor.fetchall()

    # Create matching structures for fast lookup
    jefes_norm = []
    for j in jefes:
        j_id, j_nombre, j_apellido, j_email = j
        jefes_norm.append({
            'id': j_id,
            'nombre': normalize(j_nombre),
            'apellido': normalize(j_apellido),
            'email': j_email
        })

    # Read CSV
    updated = 0
    not_found = 0

    with open(r'c:\Development\Antigravity\RRHH_Objetivos\usuarios_responsables.csv', 'r', encoding='utf-8-sig') as f:
        reader = csv.DictReader(f, delimiter=';')
        for row in reader:
            email = row.get('Email', '').strip()
            resp_str = row.get('Responsable', '').strip()

            if not email or not resp_str or resp_str == '-':
                continue
            
            # Check if this user exists as Empleado
            cursor.execute("SELECT Id, JefeId FROM Empleados WHERE LOWER(Email) = ?", (email.lower(),))
            emp = cursor.fetchone()
            if not emp:
                continue
            
            emp_id, current_jefe_id = emp

            # Parse "Apellido, Nombre"
            parts = [p.strip() for p in resp_str.split(',')]
            resp_apellido = normalize(parts[0])
            resp_nombre = normalize(parts[1]) if len(parts) > 1 else ""

            # Try exact match first
            matched_jefe_id = None
            for j in jefes_norm:
                if j['apellido'] == resp_apellido and (not resp_nombre or j['nombre'] == resp_nombre):
                    matched_jefe_id = j['id']
                    break
            
            # Fallback contains match
            if not matched_jefe_id:
                norm_str = normalize(resp_str)
                for j in jefes_norm:
                    if j['apellido'] in norm_str and (not j['nombre'] or j['nombre'] in norm_str):
                        matched_jefe_id = j['id']
                        break
            
            if matched_jefe_id:
                if matched_jefe_id != current_jefe_id:
                    cursor.execute("UPDATE Empleados SET JefeId = ? WHERE Id = ?", (matched_jefe_id, emp_id))
                    updated += 1
            else:
                print(f"[WARNING] Jefe no encontrado para responsable: '{resp_str}' (Empleado: {email})")
                not_found += 1
    
    conn.commit()
    conn.close()
    print(f"\nProceso completado. Empleados actualizados: {updated}. Jefes no encontrados: {not_found}")

if __name__ == '__main__':
    run()
