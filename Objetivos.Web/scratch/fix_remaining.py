import os

def run():
    base = r'c:\Development\Antigravity\RRHH_Objetivos\Objetivos.Web'

    # Fix Entities.cs
    entities_path = os.path.join(base, 'Domain', 'Entities', 'Entities.cs')
    with open(entities_path, 'r', encoding='utf-8') as f:
        content = f.read()
    content = content.replace('public string ComentarioUsuario { get; set; } = "";', 'public string Comentario { get; set; } = "";')
    content = content.replace('public bool AprobadoPorUsuario { get; set; } = false;', 'public bool AprobadoPorJefe { get; set; } = false;')
    with open(entities_path, 'w', encoding='utf-8') as f:
        f.write(content)

    # Fix AppDbContext.cs
    appdb_path = os.path.join(base, 'Data', 'AppDbContext.cs')
    with open(appdb_path, 'r', encoding='utf-8') as f:
        content = f.read()
    content = content.replace('.HasOne(e => e.Usuario)\n            .WithMany()\n            .HasForeignKey(e => e.JefeId);', '.HasOne(e => e.Jefe)\n            .WithMany()\n            .HasForeignKey(e => e.JefeId);')
    with open(appdb_path, 'w', encoding='utf-8') as f:
        f.write(content)

    # Fix UsuarioService.cs
    usuariosvc_path = os.path.join(base, 'Services', 'UsuarioService.cs')
    with open(usuariosvc_path, 'r', encoding='utf-8') as f:
        content = f.read()
    content = content.replace('.Include(u => u.Usuario)', '.Include(u => u.Jefe)')
    with open(usuariosvc_path, 'w', encoding='utf-8') as f:
        f.write(content)

    # Fix ObjetivoDetalle.razor
    objdet_path = os.path.join(base, 'Components', 'Pages', 'MisObjetivos', 'ObjetivoDetalle.razor')
    with open(objdet_path, 'r', encoding='utf-8') as f:
        content = f.read()
    content = content.replace('ChatService.GetConversacionAsync(objetivo.Usuario.JefeId, UsuarioId)', 'ChatService.GetConversacionAsync(objetivo.Usuario.JefeId ?? 0, UsuarioId)')
    content = content.replace('int conversationJefeId = objetivo.Usuario.JefeId;', 'int conversationJefeId = objetivo.Usuario.JefeId ?? 0;')
    content = content.replace('JefeId = CurrentUser.EsJefe ? CurrentUser.UsuarioId : objetivo!.Usuario.JefeId,', 'JefeId = CurrentUser.EsJefe ? CurrentUser.UsuarioId : (objetivo!.Usuario.JefeId ?? 0),')
    with open(objdet_path, 'w', encoding='utf-8') as f:
        f.write(content)

    # Fix ObjetivoService.cs
    objsvc_path = os.path.join(base, 'Services', 'ObjetivoService.cs')
    with open(objsvc_path, 'r', encoding='utf-8') as f:
        content = f.read()
    content = content.replace('AprobadoPorUsuario', 'AprobadoPorJefe')
    with open(objsvc_path, 'w', encoding='utf-8') as f:
        f.write(content)

if __name__ == '__main__':
    run()
