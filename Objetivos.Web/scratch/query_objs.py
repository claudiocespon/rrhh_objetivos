import sqlite3

conn = sqlite3.connect('C:\\Development\\Antigravity\\RRHH_Objetivos\\Objetivos.Web\\objetivos.db')
cursor = conn.cursor()

cursor.execute("""
    SELECT Id, Nombre, Estado, AprobadoPorJefe, EstadoObjetivoConfigId 
    FROM Objetivos 
    WHERE UsuarioId = 318
""")
for row in cursor.fetchall():
    print(row)

conn.close()
