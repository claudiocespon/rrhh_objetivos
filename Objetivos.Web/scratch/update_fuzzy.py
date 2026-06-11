import pandas as pd
import sqlite3
import unicodedata
import difflib

def normalize(text):
    if not isinstance(text, str):
        return ""
    if not text or text == '-' or text.lower() == 'no aplica':
        return ""
    text = unicodedata.normalize('NFKD', text).encode('ASCII', 'ignore').decode('utf-8')
    return text.lower().replace('\xa0', ' ').replace(',', ' ').strip()

def run():
    df = pd.read_excel(r'c:\Development\Antigravity\RRHH_Objetivos\Planilla Final Nomina Regional Finalizada 3.xlsx', header=2)
    
    conn = sqlite3.connect(r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\objetivos.db')
    cursor = conn.cursor()

    cursor.execute("SELECT Id, Nombre, Apellido, Email FROM Jefes")
    jefes = cursor.fetchall()

    jefes_norm = []
    jefes_names = []
    for j in jefes:
        j_id, j_nombre, j_apellido, j_email = j
        n_nombre = normalize(j_nombre)
        n_apellido = normalize(j_apellido)
        # Add a few variations for fuzzy matching
        variations = [
            f"{n_nombre} {n_apellido}".strip(),
            f"{n_apellido} {n_nombre}".strip()
        ]
        
        jefes_norm.append({
            'id': j_id,
            'email': j_email,
            'variations': variations
        })

    updated = 0
    not_found = 0

    for idx, row in df.iterrows():
        email = str(row.get('Mail', '')).strip()
        resp_str = str(row.get('Responsable de evaluación', '')).strip()

        if pd.isna(row.get('Mail')) or not email or not resp_str or resp_str.lower() == 'no aplica':
            continue
        
        cursor.execute("SELECT Id, JefeId FROM Empleados WHERE LOWER(Email) = ?", (email.lower(),))
        emp = cursor.fetchone()
        if not emp:
            continue
        
        emp_id, current_jefe_id = emp
        norm_resp = normalize(resp_str)
        
        matched_jefe_id = None
        best_score = 0
        
        for j in jefes_norm:
            for var in j['variations']:
                # Calculate similarity score
                score = difflib.SequenceMatcher(None, norm_resp, var).ratio()
                if score > best_score:
                    best_score = score
                    matched_jefe_id = j['id']
        
        if matched_jefe_id and best_score > 0.6:
            if matched_jefe_id != current_jefe_id:
                cursor.execute("UPDATE Empleados SET JefeId = ? WHERE Id = ?", (matched_jefe_id, emp_id))
                updated += 1
        else:
            print(f"[WARNING] Jefe no encontrado para: '{resp_str}' (Best score: {best_score})")
            not_found += 1

    conn.commit()
    conn.close()
    print(f"\nProceso completado. Empleados actualizados: {updated}. Jefes no encontrados: {not_found}")

if __name__ == '__main__':
    run()
