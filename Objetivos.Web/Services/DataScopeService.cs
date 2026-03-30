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
            return query.Where(o => o.Empleado.AreaId == user.AreaId);
        }

        if (user.EsJefe)
        {
            return query.Where(o => o.Empleado.JefeId == user.UsuarioId);
        }

        // Default to no access or just personal (handled in services usually by ID)
        return query.Where(o => false);
    }

    public IQueryable<RevisionCuatrimestral> AplicarScope(IQueryable<RevisionCuatrimestral> query, ICurrentUserService user)
    {
        if (PuedeVerTodo(user))
        {
            return query;
        }

        if (user.Rol == "DIRECTOR")
        {
            return query.Where(r => r.Objetivo.Empleado.AreaId == user.AreaId);
        }

        if (user.EsJefe)
        {
            return query.Where(r => r.Objetivo.Empleado.JefeId == user.UsuarioId);
        }

        return query.Where(r => false);
    }
}
