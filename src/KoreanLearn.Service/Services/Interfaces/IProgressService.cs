using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>學習進度追蹤業務邏輯介面（記錄影片播放進度與單元完成狀態）</summary>
public interface IProgressService
{
    /// <summary>儲存影片播放進度（秒數），回傳已儲存的秒數</summary>
    Task<ServiceResult<int>> SaveVideoProgressAsync(string userId, int lessonId, int progressSeconds, CancellationToken ct = default);

    /// <summary>標記單元為已完成</summary>
    Task<ServiceResult> MarkLessonCompleteAsync(string userId, int lessonId, CancellationToken ct = default);

    /// <summary>取消單元的完成標記</summary>
    Task<ServiceResult> UndoLessonCompleteAsync(string userId, int lessonId, CancellationToken ct = default);

    /// <summary>取得指定單元的影片播放進度（秒數）</summary>
    Task<int> GetVideoProgressAsync(string userId, int lessonId, CancellationToken ct = default);
}
