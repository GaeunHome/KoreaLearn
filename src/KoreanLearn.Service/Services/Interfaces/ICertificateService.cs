using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>修課證書業務邏輯介面（資格檢查與 PDF 證書產生）</summary>
public interface ICertificateService
{
    /// <summary>檢查使用者是否符合取得證書的資格（所有單元完成 + 測驗及格）</summary>
    Task<CertificateEligibility> CheckEligibilityAsync(string userId, int courseId, CancellationToken ct = default);

    /// <summary>產生修課證書 PDF（使用 QuestPDF），不符資格時回傳 null</summary>
    Task<byte[]?> GenerateCertificateAsync(string userId, int courseId, CancellationToken ct = default);
}

/// <summary>證書資格檢查結果</summary>
public class CertificateEligibility
{
    /// <summary>是否符合資格</summary>
    public bool IsEligible { get; set; }

    /// <summary>不符資格原因</summary>
    public string? Reason { get; set; }

    /// <summary>已完成單元數</summary>
    public int CompletedLessons { get; set; }

    /// <summary>課程總單元數</summary>
    public int TotalLessons { get; set; }

    /// <summary>測驗最高分（百分比）</summary>
    public int? QuizScore { get; set; }

    /// <summary>及格分數</summary>
    public int PassingScore { get; set; }
}
