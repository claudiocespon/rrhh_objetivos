import sqlite3

conn = sqlite3.connect('Objetivos.Web/objetivos.db')
c = conn.cursor()

print("--- Jefes ---")
c.execute("SELECT * FROM Jefes WHERE Email='ptripodi@permaquim.com'")
row = c.fetchone()
if row:
    colnames = [d[0] for d in c.description]
    for col, val in zip(colnames, row):
        print(f"  {col}: {val}")
else:
    print("Not found in Jefes")

print("\n--- Empleados ---")
c.execute("SELECT * FROM Empleados WHERE Email='ptripodi@permaquim.com'")
row = c.fetchone()
if row:
    colnames = [d[0] for d in c.description]
    for col, val in zip(colnames, row):
        print(f"  {col}: {val}")
else:
    print("Not found in Empleados")

conn.close()
