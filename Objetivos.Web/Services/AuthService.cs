using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;

namespace Objetivos.Web.Services;

public class AuthResult
{
    public bool Exitoso { get; set; }
    public string? MensajeError { get; set; }
    public bool EsJefe { get; set; }
    public int UsuarioId { get; set; }
    public string NombreCompleto { get; set; } = "";
    public string Email { get; set; } = "";
    public string Rol { get; set; } = "";
    public int AreaId { get; set; }
    public bool DebeCambiarPassword { get; set; }
    public bool EsSuperusuario { get; set; }
}

public class AuthService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IEmailService _emailService;

    public AuthService(IDbContextFactory<AppDbContext> dbFactory, IEmailService emailService)
    {
        _dbFactory = dbFactory;
        _emailService = emailService;
    }

    public static string HashPassword(string password)
    {
        const int iterations = 100000;
        const int saltSize = 16;
        const int hashSize = 32;

        var salt = RandomNumberGenerator.GetBytes(saltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, hashSize);

        return $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        var parts = hashedPassword.Split('.');
        if (parts.Length != 3) return false;

        var iterations = int.Parse(parts[0]);
        var salt = Convert.FromBase64String(parts[1]);
        var hash = Convert.FromBase64String(parts[2]);

        var testHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, hash.Length);

        return CryptographicOperations.FixedTimeEquals(hash, testHash);
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return new AuthResult { Exitoso = false, MensajeError = "Debe ingresar email y contraseña" };

        email = email.Trim().ToLowerInvariant();

        using var db = await _dbFactory.CreateDbContextAsync();

        // Check Jefe first
        var jefe = await db.Jefes
            .Include(j => j.Area)
            .FirstOrDefaultAsync(j => j.Email.ToLower() == email && j.Activo);

        if (jefe != null)
        {
            if (!VerifyPassword(password, jefe.PasswordHash))
                return new AuthResult { Exitoso = false, MensajeError = "Contraseña incorrecta" };

            return new AuthResult
            {
                Exitoso = true,
                EsJefe = true,
                UsuarioId = jefe.Id,
                NombreCompleto = $"{jefe.Nombre} {jefe.Apellido}",
                Email = jefe.Email,
                Rol = jefe.Rol,
                AreaId = jefe.AreaId,
                DebeCambiarPassword = jefe.DebeCambiarPassword,
                EsSuperusuario = jefe.EsSuperusuario
            };
        }

        // Check Empleado
        var empleado = await db.Empleados
            .Include(e => e.Area)
            .FirstOrDefaultAsync(e => e.Email.ToLower() == email && e.Activo);

        if (empleado != null)
        {
            if (!VerifyPassword(password, empleado.PasswordHash))
                return new AuthResult { Exitoso = false, MensajeError = "Contraseña incorrecta" };

            return new AuthResult
            {
                Exitoso = true,
                EsJefe = false,
                UsuarioId = empleado.Id,
                NombreCompleto = $"{empleado.Nombre} {empleado.Apellido}",
                Email = empleado.Email,
                Rol = "COLABORADOR",
                AreaId = empleado.AreaId,
                DebeCambiarPassword = empleado.DebeCambiarPassword,
                EsSuperusuario = empleado.EsSuperusuario
            };
        }

        return new AuthResult { Exitoso = false, MensajeError = "No se encontró un usuario con ese email" };
    }

    public async Task<bool> CambiarPasswordAsync(int usuarioId, bool esJefe, string nuevaPassword)
    {
        if (string.IsNullOrWhiteSpace(nuevaPassword) || nuevaPassword.Length < 8)
            return false;

        bool hasLetter = nuevaPassword.Any(char.IsLetter);
        bool hasNumber = nuevaPassword.Any(char.IsDigit);

        if (!hasLetter || !hasNumber)
            return false;

        var hash = HashPassword(nuevaPassword);

        using var db = await _dbFactory.CreateDbContextAsync();

        if (esJefe)
        {
            var jefe = await db.Jefes.FindAsync(usuarioId);
            if (jefe == null) return false;
            jefe.PasswordHash = hash;
            jefe.DebeCambiarPassword = false;
        }
        else
        {
            var empleado = await db.Empleados.FindAsync(usuarioId);
            if (empleado == null) return false;
            empleado.PasswordHash = hash;
            empleado.DebeCambiarPassword = false;
        }

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RecuperarPasswordAsync(string email)
    {
        email = email.Trim().ToLowerInvariant();
        var randomPassword = GenerarPasswordAleatorio();
        var hash = HashPassword(randomPassword);

        using var db = await _dbFactory.CreateDbContextAsync();

        var jefe = await db.Jefes.FirstOrDefaultAsync(j => j.Email.ToLower() == email && j.Activo);
        if (jefe != null)
        {
            jefe.PasswordHash = hash;
            jefe.DebeCambiarPassword = true;
            await db.SaveChangesAsync();
            await _emailService.SendEmailAsync(email, "Recuperación de Contraseña - PQ-Talent",
                $"Hola {jefe.Nombre},<br/><br/>Tu nueva contraseña temporal es: <b>{randomPassword}</b><br/>" +
                "Deberás cambiarla en tu próximo inicio de sesión.<br/><br/>Saludos,<br/>Equipo de RRHH");
            return true;
        }

        var empleado = await db.Empleados.FirstOrDefaultAsync(e => e.Email.ToLower() == email && e.Activo);
        if (empleado != null)
        {
            empleado.PasswordHash = hash;
            empleado.DebeCambiarPassword = true;
            await db.SaveChangesAsync();
            await _emailService.SendEmailAsync(email, "Recuperación de Contraseña - PQ-Talent",
                $"Hola {empleado.Nombre},<br/><br/>Tu nueva contraseña temporal es: <b>{randomPassword}</b><br/>" +
                "Deberás cambiarla en tu próximo inicio de sesión.<br/><br/>Saludos,<br/>Equipo de RRHH");
            return true;
        }

        return false; // Email not found
    }

    private static string GenerarPasswordAleatorio()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$";
        var random = RandomNumberGenerator.Create();
        var bytes = new byte[10];
        random.GetBytes(bytes);
        var sb = new StringBuilder(10);
        foreach (var b in bytes)
            sb.Append(chars[b % chars.Length]);
        return sb.ToString();
    }
}
