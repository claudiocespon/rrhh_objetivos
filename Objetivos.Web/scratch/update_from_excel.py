import pandas as pd
import sqlite3
import unicodedata

def normalize(text):
    if not isinstance(text, str):
        return ""
    if not text or text == '-' or text.lower() == 'no aplica':
        return ""
    text = unicodedata.normalize('NFKD', text).encode('ASCII', 'ignore').decode('utf-8')
    return text.lower().replace('\xa0', ' ').strip()

def run():
    df = pd.read_excel(r'c:\Development\Antigravity\RRHH_Objetivos\Planilla Final Nomina Regional Finalizada 3.xlsx', header=2)
    
    conn = sqlite3.connect(r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\objetivos.db')
    cursor = conn.cursor()

    # Load Jefes
    cursor.execute("SELECT Id, Nombre, Apellido, Email FROM Jefes")
    jefes = cursor.fetchall()

    jefes_norm = []
    for j in jefes:
        j_id, j_nombre, j_apellido, j_email = j
        jefes_norm.append({
            'id': j_id,
            'nombre': normalize(j_nombre),
            'apellido': normalize(j_apellido),
            'email': j_email,
            'full': normalize(j_apellido + " " + j_nombre)
        })

    updated = 0
    not_found = 0

    for idx, row in df.iterrows():
        email = str(row.get('Mail', '')).strip()
        resp_str = str(row.get('Responsable de evaluación', '')).strip()

        if pd.isna(row.get('Mail')) or not email or not resp_str or resp_str.lower() == 'no aplica':
            continue
        
        # Find Empleado
        cursor.execute("SELECT Id, JefeId FROM Empleados WHERE LOWER(Email) = ?", (email.lower(),))
        emp = cursor.fetchone()
        if not emp:
            continue
        
        emp_id, current_jefe_id = emp
        
        # Parse "Apellido, Nombre"
        parts = [p.strip() for p in resp_str.split(',')]
        resp_apellido = normalize(parts[0])
        resp_nombre = normalize(parts[1]) if len(parts) > 1 else ""
        
        matched_jefe_id = None
        
        # 1. Try exact match by apellido and nombre
        for j in jefes_norm:
            if j['apellido'] == resp_apellido and (not resp_nombre or j['nombre'] == resp_nombre):
                matched_jefe_id = j['id']
                break
        
        # 2. Contains match
        if not matched_jefe_id:
            norm_str = normalize(resp_str).replace(',', '')
            for j in jefes_norm:
                if j['apellido'] in norm_str and (not j['nombre'] or j['nombre'] in norm_str):
                    matched_jefe_id = j['id']
                    break
        
        if matched_jefe_id:
            if matched_jefe_id != current_jefe_id:
                cursor.execute("UPDATE Empleados SET JefeId = ? WHERE Id = ?", (matched_jefe_id, emp_id))
                updated += 1
        else:
            print(f"[WARNING] Jefe no encontrado para: '{resp_str}' (Empleado: {email})")
            not_found += 1

    conn.commit()
    conn.close()
    print(f"\nProceso completado. Empleados actualizados: {updated}. Jefes no encontrados: {not_found}")

if __name__ == '__main__':
    run()
