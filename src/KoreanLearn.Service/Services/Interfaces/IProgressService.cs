using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Service.Services.Interfaces;

public interface IProgressService
{
    Task<ServiceResult<int>> SaveVideoProgressAsync(string userId, int lessonId, int progressSeconds, CancellationToken ct = default);
    Task<ServiceResult> MarkLessonCompleteAsync(string userId, int lessonId, CancellationToken ct = default);
    Task<int> GetVideoProgressAsync(string userId, int lessonId, CancellationToken ct = default);
}
