namespace KoreanLearn.Web.Infrastructure.Settings;

/// <summary>SMTP 郵件設定模型</summary>
public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "KoreanLearn";
    public string? Username { get; set; }
    public string Password { get; set; } = string.Empty;
}
