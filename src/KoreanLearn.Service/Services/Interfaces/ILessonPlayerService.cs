using KoreanLearn.Service.ViewModels.Learn;

namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>前台單元播放器業務邏輯介面（依單元類型提供對應的播放資料與導覽資訊）</summary>
public interface ILessonPlayerService
{
    /// <summary>取得影片播放器所需資料（含進度、上下單元導覽）</summary>
    Task<VideoPlayerViewModel?> GetVideoPlayerAsync(int lessonId, string userId, CancellationToken ct = default);

    /// <summary>取得文章閱讀器所需資料（含上下單元導覽）</summary>
    Task<ArticlePlayerViewModel?> GetArticlePlayerAsync(int lessonId, string userId, CancellationToken ct = default);

    /// <summary>取得 PDF 閱讀器所需資料（含下載連結與上下單元導覽）</summary>
    Task<PdfPlayerViewModel?> GetPdfPlayerAsync(int lessonId, string userId, CancellationToken ct = default);
}
