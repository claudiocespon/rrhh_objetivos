import json
import sqlite3
import datetime

def run():
    db_path = r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\objetivos.db'
    backup_path = r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\scratch\db_backup.json'

    with open(backup_path, 'r', encoding='utf-8') as f:
        data = json.load(f)

    e_emails = {e['Email'].lower().strip(): e for e in data['empleados']}
    j_emails = {j['Email'].lower().strip(): j for j in data['jefes']}
    
    # Mapping old JefeId -> new UsuarioId
    jefe_to_usuario = {}
    
    # We will build a unified list of users
    unified_users = []
    
    now = datetime.datetime.utcnow().isoformat()
    
    # 1. Process all Empleados
    for e in data['empleados']:
        email = e['Email'].lower().strip()
        user_record = {
            'Id': e['Id'],
            'Nombre': e['Nombre'],
            'Apellido': e['Apellido'],
            'Email': email,
            'Legajo': e['Legajo'],
            'PasswordHash': e['PasswordHash'],
            'DebeCambiarPassword': e['DebeCambiarPassword'],
            'PuestoId': e.get('PuestoId'),
            'AreaId': e['AreaId'],
            'PaisId': e['PaisId'],
            'JefeId': e.get('JefeId'), # We will map this later
            'Rol': 'COLABORADOR',
            'Activo': e['Activo'],
            'FechaBaja': e.get('FechaBaja'),
            'EsSuperusuario': e['EsSuperusuario'],
            'FechaCreacion': now,
            'FechaIngreso': e.get('FechaIngreso', now)
        }
        
        # If this Empleado is also a Jefe, update their Rol and map their JefeId
        if email in j_emails:
            j = j_emails[email]
            user_record['Rol'] = j['Rol']
            if j.get('FechaCreacion'):
                user_record['FechaCreacion'] = j['FechaCreacion']
            # Map their old Jefe ID to this Empleado ID
            jefe_to_usuario[j['Id']] = e['Id']
            
        unified_users.append(user_record)
        
    # 2. Process Jefes that are NOT Empleados
    # We need to give them a new ID, e.g., offset by 10000
    for j in data['jefes']:
        email = j['Email'].lower().strip()
        if email not in e_emails:
            new_id = j['Id'] + 10000
            jefe_to_usuario[j['Id']] = new_id
            
            user_record = {
                'Id': new_id,
                'Nombre': j['Nombre'],
                'Apellido': j['Apellido'],
                'Email': email,
                'Legajo': j['Legajo'],
                'PasswordHash': j['PasswordHash'],
                'DebeCambiarPassword': j['DebeCambiarPassword'],
                'PuestoId': None,
                'AreaId': j['AreaId'],
                'PaisId': j['PaisId'],
                'JefeId': None, # Jefes didn't have a JefeId in the old schema
                'Rol': j['Rol'],
                'Activo': j['Activo'],
                'FechaBaja': j.get('FechaBaja'),
                'EsSuperusuario': j['EsSuperusuario'],
                'FechaCreacion': j.get('FechaCreacion', now),
                'FechaIngreso': now
            }
            unified_users.append(user_record)
            
    # 3. Remap JefeId for all unified users
    for u in unified_users:
        if u['JefeId'] is not None:
            old_jefe_id = u['JefeId']
            if old_jefe_id in jefe_to_usuario:
                u['JefeId'] = jefe_to_usuario[old_jefe_id]
            else:
                # If for some reason the JefeId doesn't exist, set to None
                u['JefeId'] = None

    # 4. Insert into database
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()
    
    try:
        cursor.execute("DELETE FROM Usuarios")
        
        for u in unified_users:
            cursor.execute("""
                INSERT INTO Usuarios (
                    Id, Nombre, Apellido, Email, Legajo, PasswordHash, DebeCambiarPassword,
                    PuestoId, AreaId, PaisId, JefeId, Rol, Activo, FechaBaja, EsSuperusuario,
                    FechaCreacion, FechaIngreso
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            """, (
                u['Id'], u['Nombre'], u['Apellido'], u['Email'], u['Legajo'],
                u['PasswordHash'], u['DebeCambiarPassword'], u['PuestoId'],
                u['AreaId'], u['PaisId'], u['JefeId'], u['Rol'], u['Activo'],
                u['FechaBaja'], u['EsSuperusuario'], u['FechaCreacion'], u['FechaIngreso']
            ))

        # 5. Update MensajesChat with the mapping
        # First, fetch all messages where RemitenteEsJefe = 1 or JefeId is set
        cursor.execute("SELECT Id, RemitenteId, RemitenteEsJefe, JefeId FROM MensajesChat")
        mensajes = cursor.fetchall()
        for msg in mensajes:
            msg_id, rem_id, rem_es_jefe, msg_jefe_id = msg
            
            new_rem_id = rem_id
            if rem_es_jefe == 1 and rem_id in jefe_to_usuario:
                new_rem_id = jefe_to_usuario[rem_id]
                
            new_msg_jefe_id = msg_jefe_id
            if msg_jefe_id in jefe_to_usuario:
                new_msg_jefe_id = jefe_to_usuario[msg_jefe_id]
                
            if new_rem_id != rem_id or new_msg_jefe_id != msg_jefe_id:
                cursor.execute("""
                    UPDATE MensajesChat 
                    SET RemitenteId = ?, JefeId = ?
                    WHERE Id = ?
                """, (new_rem_id, new_msg_jefe_id, msg_id))

        conn.commit()
        print(f"Data merged and imported successfully. Total users: {len(unified_users)}")
        print(f"Jefes mapped to Empleados: {len(jefe_to_usuario)}")
    except Exception as e:
        print(f"Error: {e}")
        conn.rollback()
    finally:
        conn.close()

if __name__ == '__main__':
    run()
