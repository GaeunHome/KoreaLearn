using KoreanLearn.Service.ViewModels.Learn;

namespace KoreanLearn.Service.Services.Interfaces;

public interface ILessonPlayerService
{
    Task<VideoPlayerViewModel?> GetVideoPlayerAsync(int lessonId, string userId, CancellationToken ct = default);
}
