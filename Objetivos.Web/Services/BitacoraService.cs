using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;
using Objetivos.Web.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Objetivos.Web.Services;

public class BitacoraService(IDbContextFactory<AppDbContext> dbFactory, ICurrentUserService currentUser)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<List<BitacoraEntrada>> GetByObjetivoAsync(int objetivoId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.BitacoraEntradas
            .Where(b => b.ObjetivoId == objetivoId)
            .OrderByDescending(b => b.Fecha)
            .ToListAsync();
    }

    public async Task CrearEntradaAsync(BitacoraEntrada nueva)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        nueva.Fecha = DateTime.UtcNow;
        nueva.Estado = EstadoBitacora.PENDIENTE_REVISION;
        db.BitacoraEntradas.Add(nueva);
        await db.SaveChangesAsync();
    }

    // RN-05: Acciones de Bitácora
    public async Task ComentarAsync(int entryId, string texto)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var entry = await db.BitacoraEntradas.FindAsync(entryId);
        if (entry != null)
        {
            entry.Estado = EstadoBitacora.COMENTADO_JEFE;
            entry.FeedbackUsuario = texto;
            entry.FechaFeedback = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }

    public async Task RequiereAjusteAsync(int entryId, string nota, int usuarioId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var entry = await db.BitacoraEntradas.FindAsync(entryId);
        if (entry != null)
        {
            entry.Estado = EstadoBitacora.REQUIERE_AJUSTE;
            entry.FeedbackUsuario = nota;
            entry.FechaFeedback = DateTime.UtcNow;

            db.Notificaciones.Add(new Notificacion
            {
                UsuarioId = usuarioId,
                Tipo = TipoNotificacion.SOLICITUD_ACTUALIZACION,
                Mensaje = "El jefe solicita una actualización en la bitácora",
                Fecha = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }
    }

    public async Task CerrarAsync(int entryId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var entry = await db.BitacoraEntradas.FindAsync(entryId);
        if (entry != null)
        {
            entry.Estado = EstadoBitacora.CERRADO;
            await db.SaveChangesAsync();
        }
    }

    public async Task SolicitarActualizacionAsync(int usuarioId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        db.Notificaciones.Add(new Notificacion
        {
            UsuarioId = usuarioId,
            Tipo = TipoNotificacion.SOLICITUD_ACTUALIZACION,
            Mensaje = "El jefe solicita una actualización en la bitácora",
            Fecha = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}
