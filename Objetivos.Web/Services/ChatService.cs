using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Objetivos.Web.Services;

public class ChatService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public ChatService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<MensajeChat>> GetConversacionAsync(int jefeId, int usuarioId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.MensajesChat
            .Where(m => m.JefeId == jefeId && m.DestinatarioUsuarioId == usuarioId)
            .OrderBy(m => m.Fecha)
            .ToListAsync();
    }

    public async Task EnviarMensajeAsync(MensajeChat mensaje)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        mensaje.Fecha = DateTime.UtcNow;
        mensaje.Leido = false;
        db.MensajesChat.Add(mensaje);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Marca como leídos los mensajes donde el usuario actual es el destinatario.
    /// esUsuario=true → marca los enviados por el usuario (RemitenteEsJefe=false).
    /// esUsuario=false → marca los enviados por el jefe (RemitenteEsJefe=true).
    /// </summary>
    public async Task MarcarMensajesComoLeidosAsync(int jefeId, int usuarioId, bool esUsuario)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var sinLeer = await db.MensajesChat
            .Where(m => m.JefeId == jefeId
                     && m.DestinatarioUsuarioId == usuarioId
                     && m.RemitenteEsJefe == !esUsuario
                     && !m.Leido)
            .ToListAsync();

        foreach (var m in sinLeer)
            m.Leido = true;

        if (sinLeer.Count > 0)
            await db.SaveChangesAsync();
    }

    /// <summary>
    /// Para la vista de Seguimientos del jefe: devuelve cuántos mensajes no leídos
    /// (enviados por cada usuario) hay pendientes para el jefe dado.
    /// Clave = UsuarioId, Valor = cantidad de mensajes sin leer.
    /// </summary>
    public async Task<Dictionary<int, int>> GetConversacionesConNoLeidosAsync(int jefeId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var grupos = await db.MensajesChat
            .Where(m => m.JefeId == jefeId && !m.RemitenteEsJefe && !m.Leido)
            .GroupBy(m => m.DestinatarioUsuarioId)
            .Select(g => new { UsuarioId = g.Key, Count = g.Count() })
            .ToListAsync();

        return grupos.ToDictionary(x => x.UsuarioId, x => x.Count);
    }

    /// <summary>
    /// Cuenta total de mensajes no leídos para el usuario — usado por el badge global.
    /// esUsuario=true: mensajes de usuarios sin leer.
    /// esUsuario=false: mensajes del jefe sin leer.
    /// </summary>
    public async Task<int> GetMensajesNoLeidosCountAsync(int usuarioId, bool esUsuario)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        if (esUsuario)
            return await db.MensajesChat.CountAsync(m => m.JefeId == usuarioId && !m.RemitenteEsJefe && !m.Leido);
        else
            return await db.MensajesChat.CountAsync(m => m.DestinatarioUsuarioId == usuarioId && m.RemitenteEsJefe && !m.Leido);
    }
}
