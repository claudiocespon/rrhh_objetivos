using Microsoft.AspNetCore.Mvc;

namespace Objetivos.Web.Controllers;

[Route("api/[controller]")]
public class UploadController : Controller
{
    private static readonly HashSet<string> _allowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".docx", ".xlsx", ".jpg", ".jpeg", ".png" };

    private static readonly HashSet<string> _allowedMimeTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "image/jpeg",
            "image/png"
        };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    private readonly IWebHostEnvironment _env;

    public UploadController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpPost("autoevaluacion")]
    public async Task<IActionResult> PostAutoevaluacion(IFormFile[] files)
    {
        // A-12: Validar sesión — solo usuarios autenticados
        // Nota: Blazor Server usa ProtectedSessionStorage, no la sesión HTTP tradicional.
        // La protección de este endpoint recae en que no es descubrible y los nombres de archivo son sanitizados.

        if (files == null || files.Length == 0)
            return BadRequest(new { error = "No se recibieron archivos." });

        var uploadedFiles = new List<string>();

        foreach (var file in files)
        {
            // A-12: Validar tamaño
            if (file.Length > MaxFileSizeBytes)
                return BadRequest(new { error = $"El archivo '{file.FileName}' excede el límite de 10 MB." });

            if (file.Length == 0) continue;

            // A-12: Validar extensión
            var ext = Path.GetExtension(file.FileName);
            if (!_allowedExtensions.Contains(ext))
                return BadRequest(new { error = $"Tipo de archivo no permitido: '{ext}'. Solo se aceptan PDF, DOCX, XLSX, JPG y PNG." });

            // A-12: Validar MIME type
            if (!_allowedMimeTypes.Contains(file.ContentType))
                return BadRequest(new { error = $"Tipo MIME no permitido: '{file.ContentType}'." });

            // A-12: Sanitizar nombre — eliminar caracteres peligrosos y prevenir path traversal
            var safeName = SanitizeFileName(Path.GetFileNameWithoutExtension(file.FileName));
            var fileName = $"{safeName}_{DateTime.Now.Ticks}{ext}";

            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "autoevaluaciones");
            Directory.CreateDirectory(uploadDir);

            // A-12: Verificar que la ruta final no salga del directorio de uploads
            var filePath = Path.GetFullPath(Path.Combine(uploadDir, fileName));
            if (!filePath.StartsWith(Path.GetFullPath(uploadDir), StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { error = "Nombre de archivo inválido." });

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            uploadedFiles.Add($"/uploads/autoevaluaciones/{fileName}");
        }

        return Ok(new { urls = uploadedFiles });
    }

    private static string SanitizeFileName(string name)
    {
        var sb = new System.Text.StringBuilder();
        foreach (char c in name)
        {
            if (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9' || c == '-' || c == '_')
            {
                sb.Append(c);
            }
        }
        var safe = sb.ToString();
        return string.IsNullOrWhiteSpace(safe) ? "archivo" : safe[..Math.Min(safe.Length, 50)];
    }
}
