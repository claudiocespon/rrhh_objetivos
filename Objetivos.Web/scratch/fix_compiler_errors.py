import os
import re

def modify_file(filepath, replacements):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    original = content
    for old, new in replacements.items():
        content = content.replace(old, new)

    if content != original:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f"Updated {filepath}")

def run():
    base = r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web'

    modify_file(os.path.join(base, 'Domain', 'Entities', 'Entities.cs'), {
        'public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;': 'public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;\n    public DateTime FechaIngreso { get; set; }',
        'public string Comentario { get; set; } = "";': 'public string ComentarioUsuario { get; set; } = "";', # for Autoevaluacion, wait, no. The error was for RevisionCuatrimestral and EvaluacionFinal.
        'public string ComentarioJefe { get; set; } = "";': 'public string ComentarioJefe { get; set; } = "";\n    public string ComentarioUsuario { get; set; } = "";', # we need ComentarioUsuario in Revision and EvaluacionFinal
        'public string? FeedbackJefe { get; set; }': 'public string? FeedbackJefe { get; set; }\n    public string? FeedbackUsuario { get; set; }', # in BitacoraEntrada
        'public int DestinatarioEmpleadoId { get; set; }': 'public int DestinatarioUsuarioId { get; set; }' # in MensajeChat
    })

if __name__ == '__main__':
    run()
