using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Service.Services.Interfaces;

public interface ICertificateService
{
    Task<CertificateEligibility> CheckEligibilityAsync(string userId, int courseId, CancellationToken ct = default);
    Task<byte[]?> GenerateCertificateAsync(string userId, int courseId, CancellationToken ct = default);
}

public class CertificateEligibility
{
    public bool IsEligible { get; set; }
    public string? Reason { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public int? QuizScore { get; set; }
    public int PassingScore { get; set; }
}
