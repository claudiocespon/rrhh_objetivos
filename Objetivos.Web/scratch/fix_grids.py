import os

def fix_file(filepath, broken_str, fix_str):
    with open(filepath, "r", encoding="utf-8") as f:
        content = f.read()
    
    if broken_str in content:
        content = content.replace(broken_str, fix_str)
        with open(filepath, "w", encoding="utf-8") as f:
            f.write(content)
        print(f"Fixed {filepath}")

base = r"c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web\Components\Pages"

# 1. AdminUsuarios.razor
fix_file(os.path.join(base, "Admin", "AdminUsuarios.razor"), 
         'Data="@(usuarios.Where(u = FilterCaseSensitivity="FilterCaseSensitivity.CaseInsensitive"> mostrarBajas || !u.Baja))"',
         'Data="@(usuarios.Where(u => mostrarBajas || !u.Baja))"')

# 2. Seguimientos\EmpleadoDetalle.razor
fix_file(os.path.join(base, "Seguimientos", "EmpleadoDetalle.razor"),
         'Data="@empleado.Objetivos.Where(o = FilterCaseSensitivity="FilterCaseSensitivity.CaseInsensitive"> o.Anio == DateTime.Now.Year)" TItem="Objetivo"',
         'Data="@empleado.Objetivos.Where(o => o.Anio == DateTime.Now.Year)" TItem="Objetivo" FilterCaseSensitivity="FilterCaseSensitivity.CaseInsensitive"')

# 3. Guia.razor
fix_file(os.path.join(base, "Guia.razor"),
         'Data="@escalas.Where(e = FilterCaseSensitivity="FilterCaseSensitivity.CaseInsensitive"> e.Activo).OrderBy(e => e.Orden).ToList()" TItem="EscalaValoracion" Responsive="true"',
         'Data="@escalas.Where(e => e.Activo).OrderBy(e => e.Orden).ToList()" TItem="EscalaValoracion" Responsive="true" FilterCaseSensitivity="FilterCaseSensitivity.CaseInsensitive"')

