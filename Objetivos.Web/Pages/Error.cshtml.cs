using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Objetivos.Web.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public string? RequestId { get; set; }
    public string? ExceptionMessage { get; set; }
    public string? ExceptionStackTrace { get; set; }
    public bool IsDevelopment { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public bool ShowException => IsDevelopment && !string.IsNullOrEmpty(ExceptionMessage);

    private readonly ILogger<ErrorModel> _logger;
    private readonly IWebHostEnvironment _env;

    public ErrorModel(ILogger<ErrorModel> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public void OnGet()
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        IsDevelopment = _env.IsDevelopment();

        var exceptionHandlerPathFeature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        if (exceptionHandlerPathFeature?.Error is Exception ex)
        {
            ExceptionMessage = ex.Message;
            ExceptionStackTrace = IsDevelopment ? ex.StackTrace : null;
            _logger.LogError(ex, "Unhandled exception occurred. RequestId: {RequestId}", RequestId);
        }
    }
}
