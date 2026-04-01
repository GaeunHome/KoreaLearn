using System.Net;
using System.Net.Mail;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Web.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace KoreanLearn.Web.Infrastructure.Services;

/// <summary>SMTP Email 寄送服務實作</summary>
public class SmtpEmailService(IOptions<SmtpSettings> settings, ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly SmtpSettings _settings = settings.Value;

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            using var message = new MailMessage();
            message.From = new MailAddress(_settings.FromEmail, _settings.FromName);
            message.To.Add(new MailAddress(toEmail));
            message.Subject = subject;
            message.Body = htmlBody;
            message.IsBodyHtml = true;

            using var client = new SmtpClient(_settings.Host, _settings.Port);
            client.EnableSsl = _settings.EnableSsl;
            client.Credentials = new NetworkCredential(
                _settings.Username ?? _settings.FromEmail, _settings.Password);

            await client.SendMailAsync(message);
            logger.LogInformation("Email 寄送成功 | To={ToEmail} | Subject={Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Email 寄送失敗 | To={ToEmail} | Subject={Subject}", toEmail, subject);
            throw;
        }
    }
}
