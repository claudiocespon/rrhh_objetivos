import sqlite3
import csv
import os

db_path = r"c:\Development\Antigravity\RRHH_Objetivos\objetivos.db"
output_path = r"c:\Development\Antigravity\RRHH_Objetivos\usuarios_responsables.csv"

conn = sqlite3.connect(db_path)
cursor = conn.cursor()

query_empleados = """
SELECT e.Nombre, e.Apellido, e.Email, e.Legajo, 'Colaborador' as Rol,
       j.Nombre as JefeNombre, j.Apellido as JefeApellido
FROM Empleados e
LEFT JOIN Jefes j ON e.JefeId = j.Id
"""

cursor.execute(query_empleados)
empleados = cursor.fetchall()

query_jefes = """
SELECT j1.Nombre, j1.Apellido, j1.Email, j1.Legajo, j1.Rol,
       j2.Nombre as JefeNombre, j2.Apellido as JefeApellido
FROM Jefes j1
LEFT JOIN Empleados e ON LOWER(e.Email) = LOWER(j1.Email)
LEFT JOIN Jefes j2 ON e.JefeId = j2.Id
"""

cursor.execute(query_jefes)
jefes = cursor.fetchall()

# Combine and write to CSV
all_users = []
for row in jefes:
    all_users.append({
        'Nombre': row[0],
        'Apellido': row[1],
        'Email': row[2],
        'Legajo': row[3],
        'Rol': row[4],
        'Responsable': f"{row[6]}, {row[5]}" if row[5] and row[6] else "-"
    })

for row in empleados:
    all_users.append({
        'Nombre': row[0],
        'Apellido': row[1],
        'Email': row[2],
        'Legajo': row[3],
        'Rol': row[4],
        'Responsable': f"{row[6]}, {row[5]}" if row[5] and row[6] else "-"
    })

# Write CSV with Excel-friendly settings (utf-8-sig for BOM, semicolon separator)
with open(output_path, 'w', newline='', encoding='utf-8-sig') as f:
    writer = csv.writer(f, delimiter=';')
    writer.writerow(['Nombre', 'Apellido', 'Email', 'Legajo', 'Rol', 'Responsable'])
    for u in all_users:
        writer.writerow([u['Nombre'], u['Apellido'], u['Email'], u['Legajo'], u['Rol'], u['Responsable']])

print(f"Exportado correctamente a: {output_path}")
