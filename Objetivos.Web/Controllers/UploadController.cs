using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Objetivos.Web.Controllers;

[Route("api/[controller]")]
public class UploadController : Controller
{
    private readonly IWebHostEnvironment _env;

    public UploadController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpPost("autoevaluacion")]
    public async Task<IActionResult> PostAutoevaluacion(IFormFile[] files)
    {
        try
        {
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "autoevaluaciones");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            var uploadedFiles = new List<string>();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    // Clean filename and add timestamp to avoid collisions
                    var fileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.Now.Ticks}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    uploadedFiles.Add($"/uploads/autoevaluaciones/{fileName}");
                }
            }

            return Ok(new { urls = uploadedFiles });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
