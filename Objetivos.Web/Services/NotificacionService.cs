using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;

namespace Objetivos.Web.Services;

public class NotificacionService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public NotificacionService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<Notificacion>> GetNotificacionesAsync(int usuarioId, int take = 20)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Notificaciones
            .Where(n => n.UsuarioId == usuarioId)
            .OrderByDescending(n => n.Fecha)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetNoLeidasCountAsync(int usuarioId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Notificaciones
            .CountAsync(n => n.UsuarioId == usuarioId && !n.Leida);
    }

    public async Task MarcarTodasComoLeidasAsync(int usuarioId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        await db.Notificaciones
            .Where(n => n.UsuarioId == usuarioId && !n.Leida)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.Leida, true));
    }

    public async Task MarcarComoLeidaAsync(int notificacionId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        await db.Notificaciones
            .Where(n => n.Id == notificacionId)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.Leida, true));
    }
}
