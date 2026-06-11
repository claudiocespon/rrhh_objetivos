import sqlite3

conn = sqlite3.connect('C:\\Development\\Antigravity\\RRHH_Objetivos\\Objetivos.Web\\objetivos.db')
cursor = conn.cursor()

current_user_id = 10028

# 1. empleadosDelUsuario
cursor.execute("SELECT Id FROM Usuarios WHERE JefeId = ? AND Activo = 1", (current_user_id,))
empleados_del_usuario = [row[0] for row in cursor.fetchall()]
print(f"Empleados del usuario ({current_user_id}): {empleados_del_usuario}")

# 2. Objetivos pendientes
if empleados_del_usuario:
    placeholders = ','.join('?' for _ in empleados_del_usuario)
    query = f"""
        SELECT Id, Nombre, UsuarioId 
        FROM Objetivos 
        WHERE UsuarioId IN ({placeholders})
          AND EstadoObjetivoConfigId = 2
          AND Estado != 2  -- 2 in EstadoObjetivo is usually CANCELADO? Wait. Let's check ENUM
    """
    cursor.execute(query, empleados_del_usuario)
    pendientes = cursor.fetchall()
    print("Objetivos pendientes:", pendientes)

conn.close()
