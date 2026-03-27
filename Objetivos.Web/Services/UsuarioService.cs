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
        public string NombreCompleto => $"{Apellido}, {Nombre}";
        public string Email { get; set; } = "";
        public string Legajo { get; set; } = "";
        public string Sector { get; set; } = "";
        public string Area { get; set; } = "";
        public int AreaId { get; set; }
        public string Rol { get; set; } = "";
        public string Pais { get; set; } = "";
        public int PaisId { get; set; }
        public string Responsable { get; set; } = "";
        public int? JefeId { get; set; }
        public bool Activo { get; set; }
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
            
            var jefes = await db.Jefes
                .Include(j => j.Area)
                .Include(j => j.Pais)
                .Select(j => new UsuarioDto
                {
                    Id = j.Id,
                    EsJefe = true,
                    Nombre = j.Nombre,
                    Apellido = j.Apellido,
                    Email = j.Email,
                    Legajo = j.Legajo,
                    Area = j.Area.Nombre,
                    AreaId = j.AreaId,
                    Rol = j.Rol,
                    Pais = j.Pais.Nombre,
                    PaisId = j.PaisId,
                    Activo = j.Activo,
                    EsSuperusuario = j.EsSuperusuario,
                    Responsable = "-" // Directors usually report to board or general director
                }).ToListAsync();

            var empleados = await db.Empleados
                .Include(e => e.Area)
                .Include(e => e.Pais)
                .Include(e => e.Jefe)
                .Select(e => new UsuarioDto
                {
                    Id = e.Id,
                    EsJefe = false,
                    Nombre = e.Nombre,
                    Apellido = e.Apellido,
                    Email = e.Email,
                    Legajo = e.Legajo,
                    Sector = e.Puesto,
                    Area = e.Area.Nombre,
                    AreaId = e.AreaId,
                    Rol = "COLABORADOR",
                    Pais = e.Pais.Nombre,
                    PaisId = e.PaisId,
                    JefeId = e.JefeId,
                    Responsable = e.Jefe.Apellido + ", " + e.Jefe.Nombre,
                    Activo = e.Activo,
                    EsSuperusuario = e.EsSuperusuario
                }).ToListAsync();

            return jefes.Concat(empleados).OrderBy(u => u.Apellido).ToList();
        }

        public async Task<bool> UpdateUsuarioAsync(UsuarioDto dto)
        {
            using var db = await _dbFactory.CreateDbContextAsync();

            if (dto.EsJefe)
            {
                var jefe = await db.Jefes.FindAsync(dto.Id);
                if (jefe == null) return false;

                jefe.Nombre = dto.Nombre;
                jefe.Apellido = dto.Apellido;
                jefe.Email = dto.Email;
                jefe.Legajo = dto.Legajo;
                jefe.Rol = dto.Rol;
                jefe.AreaId = dto.AreaId;
                jefe.PaisId = dto.PaisId;
                jefe.Activo = dto.Activo;
                jefe.EsSuperusuario = dto.EsSuperusuario;

                db.Jefes.Update(jefe);
            }
            else
            {
                var emp = await db.Empleados.FindAsync(dto.Id);
                if (emp == null) return false;

                emp.Nombre = dto.Nombre;
                emp.Apellido = dto.Apellido;
                emp.Email = dto.Email;
                emp.Legajo = dto.Legajo;
                emp.Puesto = dto.Sector;
                emp.AreaId = dto.AreaId;
                emp.PaisId = dto.PaisId;
                emp.JefeId = dto.JefeId ?? emp.JefeId;
                emp.Activo = dto.Activo;
                emp.EsSuperusuario = dto.EsSuperusuario;

                db.Empleados.Update(emp);
            }

            return await db.SaveChangesAsync() > 0;
        }

        public async Task<bool> CreateUsuarioAsync(UsuarioDto dto)
        {
            using var db = await _dbFactory.CreateDbContextAsync();

            if (!dto.EsJefe && dto.JefeId.HasValue)
            {
                var jefeExiste = await db.Jefes.AnyAsync(j => j.Id == dto.JefeId.Value);
                if (!jefeExiste)
                    throw new Exception($"El Jefe con ID {dto.JefeId} no existe.");
            }
            
            if (dto.EsJefe)
            {
                var jefe = new Jefe
                {
                    Nombre = dto.Nombre,
                    Apellido = dto.Apellido,
                    Email = dto.Email,
                    Legajo = dto.Legajo,
                    Rol = dto.Rol,
                    AreaId = dto.AreaId,
                    PaisId = dto.PaisId,
                    Activo = true,
                    EsSuperusuario = dto.EsSuperusuario,
                    DebeCambiarPassword = true,
                    PasswordHash = AuthService.HashPassword(dto.Legajo)
                };
                db.Jefes.Add(jefe);
            }
            else
            {
                var emp = new Empleado
                {
                    Nombre = dto.Nombre,
                    Apellido = dto.Apellido,
                    Email = dto.Email,
                    Legajo = dto.Legajo,
                    Puesto = dto.Sector,
                    AreaId = dto.AreaId,
                    PaisId = dto.PaisId,
                    JefeId = dto.JefeId ?? 0, // Should be validated in UI
                    Activo = true,
                    EsSuperusuario = dto.EsSuperusuario,
                    DebeCambiarPassword = true,
                    PasswordHash = AuthService.HashPassword(dto.Legajo)
                };
                db.Empleados.Add(emp);
            }

            return await db.SaveChangesAsync() > 0;
        }

        public async Task<bool> ResetPasswordAsync(int id, bool esJefe)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            string legajo;

            if (esJefe)
            {
                var jefe = await db.Jefes.FindAsync(id);
                if (jefe == null) return false;
                legajo = jefe.Legajo;
                jefe.PasswordHash = AuthService.HashPassword(legajo);
                jefe.DebeCambiarPassword = true;
                db.Jefes.Update(jefe);
            }
            else
            {
                var emp = await db.Empleados.FindAsync(id);
                if (emp == null) return false;
                legajo = emp.Legajo;
                emp.PasswordHash = AuthService.HashPassword(legajo);
                emp.DebeCambiarPassword = true;
                db.Empleados.Update(emp);
            }

            return await db.SaveChangesAsync() > 0;
        }
    }
}
