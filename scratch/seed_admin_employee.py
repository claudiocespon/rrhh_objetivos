import sqlite3
import datetime

db_path = 'Objetivos.Web/objetivos.db'
conn = sqlite3.connect(db_path)
c = conn.cursor()

# Find Pablo's details in Jefes
c.execute("SELECT Id, Nombre, Apellido, Email, Legajo, PasswordHash, AreaId, PaisId, EsSuperusuario FROM Jefes WHERE Email = 'ptripodi@permaquim.com'")
jefe_row = c.fetchone()

if jefe_row:
    jefe_id, nombre, apellido, email, legajo, pass_hash, area_id, pais_id, es_super = jefe_row
    
    # Check if he is already in Empleados
    c.execute("SELECT Id FROM Empleados WHERE Email = 'ptripodi@permaquim.com'")
    emp_row = c.fetchone()
    
    if not emp_row:
        # Get next ID for Empleados
        c.execute("SELECT MAX(Id) FROM Empleados")
        max_id = c.fetchone()[0] or 0
        emp_id = max_id + 1
        
        # Insert into Empleados
        c.execute("""
            INSERT INTO Empleados (Id, Nombre, Apellido, Email, Legajo, PasswordHash, DebeCambiarPassword, Puesto, AreaId, JefeId, PaisId, Activo, EsSuperusuario, FechaIngreso)
            VALUES (?, ?, ?, ?, ?, ?, 0, 'Gerente de RRHH', ?, ?, ?, 1, ?, ?)
        """, (emp_id, nombre, apellido, email, legajo, pass_hash, area_id, jefe_id, pais_id, es_super, datetime.datetime.utcnow().isoformat()))
        
        print(f"Created Empleado record for Pablo Tripodi with ID {emp_id}")
    else:
        emp_id = emp_row[0]
        print(f"Empleado record for Pablo Tripodi already exists with ID {emp_id}")
        
    # Check if he has any objectives
    c.execute("SELECT Id FROM Objetivos WHERE EmpleadoId = ?", (emp_id,))
    obj_row = c.fetchone()
    
    if not obj_row:
        # Insert a mock personal objective for Pablo Tripodi
        c.execute("SELECT MAX(Id) FROM Objetivos")
        max_obj_id = c.fetchone()[0] or 0
        obj_id = max_obj_id + 1
        
        c.execute("""
            INSERT INTO Objetivos (Id, Anio, AprobadoPorJefe, AreaEspecificaId, CreadoPorId, Deadline, Descripcion, EmpleadoId, Estado, EstadoObjetivoConfigId, FechaCreacion, Nombre, PilarId, PorcentajeArea, PorcentajePilar, Progreso, SoftSkill1Id, SoftSkill2Id)
            VALUES (?, 2026, 1, NULL, ?, ?, 'Liderar el proceso anual de evaluación de desempeño de la compañía, alcanzando el 100% de finalización en los plazos previstos.', ?, 'ACTIVO', 4, ?, 'Planificar y Coordinar Ciclo de Evaluación de Desempeño 2026', 1, 100, 100, 50, 26, 27)
        """, (obj_id, jefe_id, (datetime.datetime.now() + datetime.timedelta(days=180)).isoformat(), emp_id, datetime.datetime.now().isoformat()))
        
        print(f"Created mock personal objective for Pablo Tripodi with ID {obj_id}")
    else:
        print("Pablo Tripodi already has objectives.")
        
    conn.commit()
else:
    print("Error: ptripodi@permaquim.com not found in Jefes!")

conn.close()
