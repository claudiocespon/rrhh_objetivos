using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Objetivos.Web.Services;

public class ChatService
{
    private readonly AppDbContext _db;

    public ChatService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<MensajeChat>> GetConversacionAsync(int jefeId, int empleadoId)
    {
        return await _db.MensajesChat
            .Where(m => m.JefeId == jefeId && m.DestinatarioEmpleadoId == empleadoId)
            .OrderBy(m => m.Fecha)
            .ToListAsync();
    }

    public async Task EnviarMensajeAsync(MensajeChat mensaje)
    {
        mensaje.Fecha = DateTime.UtcNow;
        mensaje.Leido = false;
        _db.MensajesChat.Add(mensaje);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Marca como leídos los mensajes donde el usuario actual es el destinatario.
    /// esJefe=true → marca los enviados por el empleado (RemitenteEsJefe=false).
    /// esJefe=false → marca los enviados por el jefe (RemitenteEsJefe=true).
    /// </summary>
    public async Task MarcarMensajesComoLeidosAsync(int jefeId, int empleadoId, bool esJefe)
    {
        var sinLeer = await _db.MensajesChat
            .Where(m => m.JefeId == jefeId
                     && m.DestinatarioEmpleadoId == empleadoId
                     && m.RemitenteEsJefe == !esJefe
                     && !m.Leido)
            .ToListAsync();

        foreach (var m in sinLeer)
            m.Leido = true;

        if (sinLeer.Count > 0)
            await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Para la vista de Seguimientos del jefe: devuelve cuántos mensajes no leídos
    /// (enviados por cada empleado) hay pendientes para el jefe dado.
    /// Clave = EmpleadoId, Valor = cantidad de mensajes sin leer.
    /// </summary>
    public async Task<Dictionary<int, int>> GetConversacionesConNoLeidosAsync(int jefeId)
    {
        var grupos = await _db.MensajesChat
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
        if (esJefe)
            return await _db.MensajesChat.CountAsync(m => m.JefeId == usuarioId && !m.RemitenteEsJefe && !m.Leido);
        else
            return await _db.MensajesChat.CountAsync(m => m.DestinatarioEmpleadoId == usuarioId && m.RemitenteEsJefe && !m.Leido);
    }
}
