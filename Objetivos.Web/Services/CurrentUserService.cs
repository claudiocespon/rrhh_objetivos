using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Objetivos.Web.Services;

public interface ICurrentUserService {
    int UsuarioId { get; }
    int AreaId { get; }
    string NombreCompleto { get; }
    string Email { get; }
    string Rol { get; }
    bool EsJefe { get; }
    bool EstaAutenticado { get; }
    bool DebeCambiarPassword { get; }
    bool EsSuperusuario { get; }
    Task SetUsuarioAsync(int id, string nombreCompleto, string email, string rol, int areaId, bool esJefe, bool debeCambiarPassword, bool esSuperusuario);
    Task CerrarSesionAsync();
    Task InitializeAsync();
}

public class SessionCurrentUserService : ICurrentUserService {
    private readonly ProtectedSessionStorage _sessionStorage;

    public int UsuarioId { get; private set; }
    public int AreaId { get; private set; }
    public string NombreCompleto { get; private set; } = "";
    public string Email { get; private set; } = "";
    public string Rol { get; private set; } = "";
    public bool EsJefe { get; private set; }
    public bool EstaAutenticado { get; private set; }
    public bool DebeCambiarPassword { get; private set; }
    public bool EsSuperusuario { get; private set; }

    public SessionCurrentUserService(ProtectedSessionStorage sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    public async Task InitializeAsync()
    {
        try {
            var result = await _sessionStorage.GetAsync<UserSessionData>("user_session");
            if (result.Success && result.Value != null)
            {
                var data = result.Value;
                UsuarioId = data.UsuarioId;
                NombreCompleto = data.NombreCompleto;
                Email = data.Email;
                Rol = data.Rol;
                AreaId = data.AreaId;
                EsJefe = data.EsJefe;
                DebeCambiarPassword = data.DebeCambiarPassword;
                EsSuperusuario = data.EsSuperusuario;
                EstaAutenticado = true;
            }
        } catch { /* Ignorar si falla en prerendering */ }
    }

    public async Task SetUsuarioAsync(int id, string nombreCompleto, string email, string rol, int areaId, bool esJefe, bool debeCambiarPassword, bool esSuperusuario)
    {
        UsuarioId = id;
        NombreCompleto = nombreCompleto;
        Email = email;
        Rol = rol;
        AreaId = areaId;
        EsJefe = esJefe;
        EstaAutenticado = true;
        DebeCambiarPassword = debeCambiarPassword;
        EsSuperusuario = esSuperusuario;

        await _sessionStorage.SetAsync("user_session", new UserSessionData {
            UsuarioId = id,
            NombreCompleto = nombreCompleto,
            Email = email,
            Rol = rol,
            AreaId = areaId,
            EsJefe = esJefe,
            DebeCambiarPassword = debeCambiarPassword,
            EsSuperusuario = esSuperusuario
        });
    }

    public async Task CerrarSesionAsync()
    {
        UsuarioId = 0;
        AreaId = 0;
        NombreCompleto = "";
        Email = "";
        Rol = "";
        EsJefe = false;
        EstaAutenticado = false;
        DebeCambiarPassword = false;
        EsSuperusuario = false;

        await _sessionStorage.DeleteAsync("user_session");
    }

    private class UserSessionData {
        public int UsuarioId { get; set; }
        public int AreaId { get; set; }
        public string NombreCompleto { get; set; } = "";
        public string Email { get; set; } = "";
        public string Rol { get; set; } = "";
        public bool EsJefe { get; set; }
        public bool DebeCambiarPassword { get; set; }
        public bool EsSuperusuario { get; set; }
    }
}
