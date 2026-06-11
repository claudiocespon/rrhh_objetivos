using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;

namespace Objetivos.Web.Services
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public bool EsJefe { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string NombreCompleto => string.IsNullOrWhiteSpace(Apellido) ? Nombre : $"{Apellido}, {Nombre}";
        public string Email { get; set; } = "";
        public string Legajo { get; set; } = "";
        public int? PuestoId { get; set; }
        public string PuestoNombre { get; set; } = "";
        public string Area { get; set; } = "";
        public int AreaId { get; set; }
        public string Rol { get; set; } = "";
        public string Pais { get; set; } = "";
        public int PaisId { get; set; }
        public string Responsable { get; set; } = "";
        public int? JefeId { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaBaja { get; set; }
        public bool Baja => FechaBaja.HasValue;
        public bool EsSuperusuario { get; set; }
    }

    public class UsuarioService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public UsuarioService(IDbContextFactory<AppDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<UsuarioDto>> GetUsuariosAsync()
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            
            var usuarios = await db.Usuarios
                .Include(u => u.Area)
                .Include(u => u.Pais)
                .Include(u => u.Jefe)
                .Include(u => u.Puesto)
                .Select(u => new UsuarioDto
                {
                    Id = u.Id,
                    EsJefe = u.Rol != "COLABORADOR",
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Email = u.Email,
                    Legajo = u.Legajo,
                    PuestoId = u.PuestoId,
                    PuestoNombre = u.Puesto != null ? u.Puesto.Nombre : "Sin Puesto",
                    Area = u.Area.Nombre,
                    AreaId = u.AreaId,
                    Rol = u.Rol,
                    Pais = u.Pais.Nombre,
                    PaisId = u.PaisId,
                    Activo = u.Activo,
                    FechaBaja = u.FechaBaja,
                    EsSuperusuario = u.EsSuperusuario,
                    Responsable = u.Jefe != null ? (u.Jefe.Apellido + ", " + u.Jefe.Nombre) : "-",
                    JefeId = u.JefeId
                }).ToListAsync();

            return usuarios.OrderBy(u => u.Apellido).ToList();
        }

        public async Task<bool> UpdateUsuarioAsync(UsuarioDto dto)
        {
            using var db = await _dbFactory.CreateDbContextAsync();

            var usuario = await db.Usuarios.FindAsync(dto.Id);
            if (usuario == null) return false;

            usuario.Nombre = dto.Nombre;
            usuario.Apellido = dto.Apellido;
            usuario.Email = dto.Email;
            usuario.Legajo = dto.Legajo;
            usuario.PuestoId = dto.PuestoId;
            usuario.AreaId = dto.AreaId;
            usuario.PaisId = dto.PaisId;
            usuario.JefeId = dto.JefeId;
            usuario.Rol = dto.EsJefe && string.IsNullOrWhiteSpace(dto.Rol) ? "JEFE" : (dto.EsJefe ? dto.Rol : "COLABORADOR");
            usuario.Activo = dto.Activo;
            usuario.FechaBaja = dto.FechaBaja;
            usuario.EsSuperusuario = dto.EsSuperusuario;

            db.Usuarios.Update(usuario);
            return await db.SaveChangesAsync() > 0;
        }

        public async Task<bool> CreateUsuarioAsync(UsuarioDto dto)
        {
            using var db = await _dbFactory.CreateDbContextAsync();

            if (dto.JefeId.HasValue)
            {
                var jefeExiste = await db.Usuarios.AnyAsync(j => j.Id == dto.JefeId.Value);
                if (!jefeExiste)
                    throw new Exception($"El Responsable con ID {dto.JefeId} no existe.");
            }
            
            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Email = dto.Email,
                Legajo = dto.Legajo,
                PuestoId = dto.PuestoId,
                AreaId = dto.AreaId,
                PaisId = dto.PaisId,
                JefeId = dto.JefeId,
                Rol = dto.EsJefe && string.IsNullOrWhiteSpace(dto.Rol) ? "JEFE" : (dto.EsJefe ? dto.Rol : "COLABORADOR"),
                Activo = dto.Activo,
                FechaBaja = dto.FechaBaja,
                EsSuperusuario = dto.EsSuperusuario,
                DebeCambiarPassword = true,
                PasswordHash = AuthService.HashPassword(dto.Legajo)
            };
            db.Usuarios.Add(usuario);

            return await db.SaveChangesAsync() > 0;
        }

        public async Task<bool> ResetPasswordAsync(int id, bool esUsuario)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            var usuario = await db.Usuarios.FindAsync(id);
            if (usuario == null) return false;
            
            usuario.PasswordHash = AuthService.HashPassword(usuario.Legajo);
            usuario.DebeCambiarPassword = true;
            db.Usuarios.Update(usuario);

            return await db.SaveChangesAsync() > 0;
        }

        public async Task<int?> GetUsuarioIdByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            using var db = await _dbFactory.CreateDbContextAsync();
            var emp = await db.Usuarios.FirstOrDefaultAsync(e => e.Email.ToLower() == email.ToLower());
            return emp?.Id;
        }
    }
}
