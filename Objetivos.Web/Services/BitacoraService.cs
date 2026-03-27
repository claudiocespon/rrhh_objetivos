using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;
using Objetivos.Web.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Objetivos.Web.Services;

public class BitacoraService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public BitacoraService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<BitacoraEntrada>> GetByObjetivoAsync(int objetivoId)
    {
        return await _db.BitacoraEntradas
            .Where(b => b.ObjetivoId == objetivoId)
            .OrderByDescending(b => b.Fecha)
            .ToListAsync();
    }

    public async Task CrearEntradaAsync(BitacoraEntrada nueva)
    {
        nueva.Fecha = DateTime.UtcNow;
        nueva.Estado = EstadoBitacora.PENDIENTE_REVISION;
        _db.BitacoraEntradas.Add(nueva);
        await _db.SaveChangesAsync();
    }

    // RN-05: Acciones de Bitácora
    public async Task ComentarAsync(int entryId, string texto)
    {
        var entry = await _db.BitacoraEntradas.FindAsync(entryId);
        if (entry != null)
        {
            entry.Estado = EstadoBitacora.COMENTADO_JEFE;
            entry.FeedbackJefe = texto;
            entry.FechaFeedback = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    public async Task RequiereAjusteAsync(int entryId, string nota, int empleadoId)
    {
        var entry = await _db.BitacoraEntradas.FindAsync(entryId);
        if (entry != null)
        {
            entry.Estado = EstadoBitacora.REQUIERE_AJUSTE;
            entry.FeedbackJefe = nota;
            entry.FechaFeedback = DateTime.UtcNow;

            _db.Notificaciones.Add(new Notificacion
            {
                UsuarioId = empleadoId,
                Tipo = TipoNotificacion.SOLICITUD_ACTUALIZACION,
                Mensaje = "El jefe solicita una actualización en la bitácora",
                Fecha = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }
    }

    public async Task CerrarAsync(int entryId)
    {
        var entry = await _db.BitacoraEntradas.FindAsync(entryId);
        if (entry != null)
        {
            entry.Estado = EstadoBitacora.CERRADO;
            await _db.SaveChangesAsync();
        }
    }

    public async Task SolicitarActualizacionAsync(int empleadoId)
    {
        _db.Notificaciones.Add(new Notificacion
        {
            UsuarioId = empleadoId,
            Tipo = TipoNotificacion.SOLICITUD_ACTUALIZACION,
            Mensaje = "El jefe solicita una actualización en la bitácora",
            Fecha = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }
}
