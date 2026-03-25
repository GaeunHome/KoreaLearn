using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Enums;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Learn;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

public class LessonPlayerService(
    IUnitOfWork uow,
    ILogger<LessonPlayerService> logger) : ILessonPlayerService
{
    public async Task<VideoPlayerViewModel?> GetVideoPlayerAsync(
        int lessonId, string userId, CancellationToken ct = default)
    {
        var lesson = await uow.Lessons.GetByIdAsync(lessonId, ct).ConfigureAwait(false);
        if (lesson is null || lesson.Type != LessonType.Video)
        {
            logger.LogWarning("影片單元不存在或類型不符 | LessonId={LessonId}", lessonId);
            return null;
        }

        var section = await uow.Sections.GetByIdAsync(lesson.SectionId, ct).ConfigureAwait(false);
        var course = section is not null
            ? await uow.Courses.GetByIdAsync(section.CourseId, ct).ConfigureAwait(false)
            : null;

        var progress = await uow.Progresses.GetByUserAndLessonAsync(userId, lessonId, ct).ConfigureAwait(false);
        var videoProgress = progress?.VideoProgressSeconds ?? 0;

        // Get sibling lessons for prev/next navigation
        int? prevId = null;
        int? nextId = null;
        if (section is not null)
        {
            var siblings = await uow.Lessons.GetBySectionIdAsync(section.Id, ct).ConfigureAwait(false);
            var sorted = siblings.OrderBy(l => l.SortOrder).ThenBy(l => l.Id).ToList();
            var currentIndex = sorted.FindIndex(l => l.Id == lessonId);
            if (currentIndex > 0) prevId = sorted[currentIndex - 1].Id;
            if (currentIndex >= 0 && currentIndex < sorted.Count - 1) nextId = sorted[currentIndex + 1].Id;
        }

        return new VideoPlayerViewModel
        {
            LessonId = lesson.Id,
            Title = lesson.Title,
            Description = lesson.Description,
            VideoUrl = lesson.VideoUrl,
            VideoDurationSeconds = lesson.VideoDurationSeconds,
            VideoProgressSeconds = videoProgress,
            IsCompleted = progress?.IsCompleted ?? false,
            SectionId = lesson.SectionId,
            SectionTitle = section?.Title,
            CourseId = course?.Id ?? 0,
            CourseTitle = course?.Title,
            PreviousLessonId = prevId,
            NextLessonId = nextId
        };
    }
}
