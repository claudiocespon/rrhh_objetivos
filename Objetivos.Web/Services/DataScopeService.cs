using Objetivos.Web.Domain.Entities;
using Objetivos.Web.Domain.Enums;

namespace Objetivos.Web.Services;

public class DataScopeService
{
    public bool PuedeVerTodo(ICurrentUserService user)
    {
        return user.Rol == "DIRECTOR_GENERAL" || user.Rol == "RRHH" || user.EsSuperusuario;
    }

    public IQueryable<Objetivo> AplicarScope(IQueryable<Objetivo> query, ICurrentUserService user)
    {
        if (PuedeVerTodo(user))
        {
            return query;
        }

        if (user.Rol == "DIRECTOR")
        {
            return query.Where(o => o.Usuario.AreaId == user.AreaId);
        }

        if (user.EsJefe)
        {
            return query.Where(o => o.Usuario.JefeId == user.UsuarioId);
        }

        // Default to no access or just personal (handled in services usually by ID)
        return query.Where(o => false);
    }

    public IQueryable<RevisionCuatrimestral> AplicarScope(IQueryable<RevisionCuatrimestral> query, ICurrentUserService user)
    {
        if (PuedeVerTodo(user)) return query;
        if (user.Rol == "DIRECTOR") return query.Where(r => r.Objetivo.Usuario.AreaId == user.AreaId);
        if (user.EsJefe) return query.Where(r => r.Objetivo.Usuario.JefeId == user.UsuarioId);
        return query.Where(r => false);
    }

    // A-08: Overload para Usuarios
    public IQueryable<Usuario> AplicarScope(IQueryable<Usuario> query, ICurrentUserService user)
    {
        if (PuedeVerTodo(user)) return query;
        if (user.Rol == "DIRECTOR") return query.Where(e => e.AreaId == user.AreaId);
        if (user.EsJefe) return query.Where(e => e.JefeId == user.UsuarioId);
        return query.Where(e => false);
    }

    // A-08: Overload para Autoevaluaciones
    public IQueryable<Autoevaluacion> AplicarScope(IQueryable<Autoevaluacion> query, ICurrentUserService user)
    {
        if (PuedeVerTodo(user)) return query;
        if (user.Rol == "DIRECTOR") return query.Where(ae => ae.Objetivo.Usuario.AreaId == user.AreaId);
        if (user.EsJefe) return query.Where(ae => ae.Objetivo.Usuario.JefeId == user.UsuarioId);
        return query.Where(ae => false);
    }
}
