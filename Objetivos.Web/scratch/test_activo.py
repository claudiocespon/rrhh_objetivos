import sqlite3

conn = sqlite3.connect('C:\\Development\\Antigravity\\RRHH_Objetivos\\Objetivos.Web\\objetivos.db')
cursor = conn.cursor()

cursor.execute("SELECT Id, Activo FROM Usuarios WHERE Id = 318")
print(cursor.fetchall())

conn.close()
