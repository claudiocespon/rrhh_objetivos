using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;

namespace Objetivos.Web.Services;

public class EmailSettings
{
    public string SmtpServer { get; set; } = "";
    public int SmtpPort { get; set; }
    public string SenderName { get; set; } = "";
    public string SenderEmail { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public bool EnableSsl { get; set; }
}

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        if (string.IsNullOrEmpty(_settings.Password))
        {
            _logger.LogWarning("SMTP Password empty. Email to {to} not sent. Body: {body}", to, body);
            return;
        }

        try
        {
            using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent to {to}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {to}", to);
            // Don't throw, we don't want to break the app if email fails
        }
    }
}
