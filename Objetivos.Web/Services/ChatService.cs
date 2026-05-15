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

    public async Task<List<MensajeChat>> GetConversacionAsync(int jefeId, int empleadoId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.MensajesChat
            .Where(m => m.JefeId == jefeId && m.DestinatarioEmpleadoId == empleadoId)
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
    /// esJefe=true → marca los enviados por el empleado (RemitenteEsJefe=false).
    /// esJefe=false → marca los enviados por el jefe (RemitenteEsJefe=true).
    /// </summary>
    public async Task MarcarMensajesComoLeidosAsync(int jefeId, int empleadoId, bool esJefe)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var sinLeer = await db.MensajesChat
            .Where(m => m.JefeId == jefeId
                     && m.DestinatarioEmpleadoId == empleadoId
                     && m.RemitenteEsJefe == !esJefe
                     && !m.Leido)
            .ToListAsync();

        foreach (var m in sinLeer)
            m.Leido = true;

        if (sinLeer.Count > 0)
            await db.SaveChangesAsync();
    }

    /// <summary>
    /// Para la vista de Seguimientos del jefe: devuelve cuántos mensajes no leídos
    /// (enviados por cada empleado) hay pendientes para el jefe dado.
    /// Clave = EmpleadoId, Valor = cantidad de mensajes sin leer.
    /// </summary>
    public async Task<Dictionary<int, int>> GetConversacionesConNoLeidosAsync(int jefeId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var grupos = await db.MensajesChat
            .Where(m => m.JefeId == jefeId && !m.RemitenteEsJefe && !m.Leido)
            .GroupBy(m => m.DestinatarioEmpleadoId)
            .Select(g => new { EmpleadoId = g.Key, Count = g.Count() })
            .ToListAsync();

        return grupos.ToDictionary(x => x.EmpleadoId, x => x.Count);
    }

    /// <summary>
    /// Cuenta total de mensajes no leídos para el usuario — usado por el badge global.
    /// esJefe=true: mensajes de empleados sin leer.
    /// esJefe=false: mensajes del jefe sin leer.
    /// </summary>
    public async Task<int> GetMensajesNoLeidosCountAsync(int usuarioId, bool esJefe)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        if (esJefe)
            return await db.MensajesChat.CountAsync(m => m.JefeId == usuarioId && !m.RemitenteEsJefe && !m.Leido);
        else
            return await db.MensajesChat.CountAsync(m => m.DestinatarioEmpleadoId == usuarioId && m.RemitenteEsJefe && !m.Leido);
    }
}
