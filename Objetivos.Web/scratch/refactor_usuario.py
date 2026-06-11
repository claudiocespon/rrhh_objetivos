import os
import re

directories = [
    r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\Services',
    r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\Components',
    r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\Data',
]

def replace_in_file(path):
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()

    original = content

    # Replace DbSet references
    content = content.replace('db.Empleados', 'db.Usuarios')
    content = content.replace('db.Jefes', 'db.Usuarios')
    
    # Replace Empleado ID fields
    content = content.replace('EmpleadoId', 'UsuarioId')
    content = content.replace('empleadoId', 'usuarioId')

    # Types
    content = re.sub(r'\bEmpleado\b', 'Usuario', content)
    content = re.sub(r'\bEmpleados\b', 'Usuarios', content)

    # Variables (safe replacements)
    content = re.sub(r'\bempleado\b', 'usuario', content)
    content = re.sub(r'\bempleados\b', 'usuarios', content)

    # For Jefe, it's trickier because of EsJefe or JefeId
    # Replace Type Jefe
    content = re.sub(r'(?<!Es)Jefe\b(?!\w)', 'Usuario', content)
    # We will NOT replace `jefe` variable or `EsJefe` because it's too risky, unless it's `db.Jefes`.
    
    if content != original:
        with open(path, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f"Updated {path}")

def run():
    for d in directories:
        for root, dirs, files in os.walk(d):
            for file in files:
                if file.endswith('.cs') or file.endswith('.razor'):
                    replace_in_file(os.path.join(root, file))

if __name__ == '__main__':
    run()
