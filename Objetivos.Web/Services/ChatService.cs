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
}
