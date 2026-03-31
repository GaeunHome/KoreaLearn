namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>Email 寄送服務介面</summary>
public interface IEmailService
{
    /// <summary>寄送 HTML Email</summary>
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
}
