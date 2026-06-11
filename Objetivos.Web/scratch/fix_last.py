import os
import re

def run():
    base = r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web'

    # Fix Entities.cs
    entities_path = os.path.join(base, 'Domain', 'Entities', 'Entities.cs')
    with open(entities_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Let's replace 'public string Comentario { get; set; } = "";' with 'public string ComentarioUsuario { get; set; } = "";'
    # ONLY for RevisionCuatrimestral and EvaluacionFinal
    
    # RevisionCuatrimestral
    content = re.sub(r'(public class RevisionCuatrimestral {.*?public string ComentarioJefe { get; set; } = "";\s+)public string Comentario { get; set; } = "";', r'\1public string ComentarioUsuario { get; set; } = "";', content, flags=re.DOTALL)
    
    # EvaluacionFinal
    content = re.sub(r'(public class EvaluacionFinal {.*?public string ComentarioJefe { get; set; } = "";\s+)public string Comentario { get; set; } = "";', r'\1public string ComentarioUsuario { get; set; } = "";', content, flags=re.DOTALL)
    
    with open(entities_path, 'w', encoding='utf-8') as f:
        f.write(content)

    # Fix UsuarioService.cs
    usuariosvc_path = os.path.join(base, 'Services', 'UsuarioService.cs')
    with open(usuariosvc_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    content = content.replace('.Include(u => u.Usuario)', '.Include(u => u.Jefe)')
    
    with open(usuariosvc_path, 'w', encoding='utf-8') as f:
        f.write(content)

if __name__ == '__main__':
    run()
