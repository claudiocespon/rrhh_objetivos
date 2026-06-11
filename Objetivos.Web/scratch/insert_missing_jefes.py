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
    for j in jefes:
        n_nombre = normalize(j[1])
        n_apellido = normalize(j[2])
        variations = [f"{n_nombre} {n_apellido}".strip(), f"{n_apellido} {n_nombre}".strip()]
        jefes_norm.append({'id': j[0], 'email': j[3], 'variations': variations})

    # Find missing Jefes
    cursor.execute("SELECT Id, Nombre, Apellido, Email, Legajo, PasswordHash, DebeCambiarPassword, AreaId, PaisId, Activo, FechaBaja, EsSuperusuario FROM Jefes WHERE Email NOT IN (SELECT Email FROM Empleados)")
    missing_jefes = cursor.fetchall()

    inserted = 0
    
    for mj in missing_jefes:
        mj_id, nombre, apellido, email, legajo, pash, debe_cambiar, area_id, pais_id, activo, baja, superuser = mj
        
        # Don't insert Eduardo Casal because he's the root
        if email == 'ecasal@permaquim.com':
            continue
            
        # Find manager in Excel
        row = df[df['Mail'].str.lower().str.strip() == email.lower()]
        if row.empty:
            print(f"Skipping {email}: not found in Excel")
            continue
            
        resp_str = str(row.iloc[0]['Responsable de evaluación']).strip()
        norm_resp = normalize(resp_str)
        
        if not norm_resp:
            print(f"Skipping {email}: no manager in Excel")
            continue
            
        # Match manager
        matched_jefe_id = None
        best_score = 0
        for j in jefes_norm:
            for var in j['variations']:
                score = difflib.SequenceMatcher(None, norm_resp, var).ratio()
                if score > best_score:
                    best_score = score
                    matched_jefe_id = j['id']
                    
        if matched_jefe_id and best_score > 0.6:
            # Insert into Empleados
            cursor.execute("""
                INSERT INTO Empleados (Nombre, Apellido, Email, Legajo, PasswordHash, DebeCambiarPassword, AreaId, PaisId, Activo, FechaBaja, EsSuperusuario, FechaIngreso, JefeId)
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, CURRENT_TIMESTAMP, ?)
            """, (nombre, apellido, email, legajo, pash, debe_cambiar, area_id, pais_id, activo, baja, superuser, matched_jefe_id))
            inserted += 1
            print(f"Inserted missing Jefe {email} under JefeId {matched_jefe_id}")
        else:
            print(f"Skipping {email}: manager '{resp_str}' not matched (Best score: {best_score})")

    conn.commit()
    conn.close()
    print(f"\nInserted {inserted} Jefes into Empleados")

if __name__ == '__main__':
    run()
