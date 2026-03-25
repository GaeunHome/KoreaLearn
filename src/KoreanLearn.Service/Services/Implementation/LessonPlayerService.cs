using KoreanLearn.Data.Entities;
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

        var context = await GetLessonContextAsync(lesson, userId, ct).ConfigureAwait(false);
        if (context is null) return null;
        var (section, course, progress, prevId, nextId) = context.Value;

        return new VideoPlayerViewModel
        {
            LessonId = lesson.Id,
            Title = lesson.Title,
            Description = lesson.Description,
            VideoUrl = lesson.VideoUrl,
            VideoDurationSeconds = lesson.VideoDurationSeconds,
            VideoProgressSeconds = progress?.VideoProgressSeconds ?? 0,
            IsCompleted = progress?.IsCompleted ?? false,
            SectionId = lesson.SectionId,
            SectionTitle = section?.Title,
            CourseId = course?.Id ?? 0,
            CourseTitle = course?.Title,
            PreviousLessonId = prevId,
            NextLessonId = nextId,
            Attachments = await GetAttachmentsAsync(lesson.Id, ct).ConfigureAwait(false)
        };
    }

    public async Task<ArticlePlayerViewModel?> GetArticlePlayerAsync(
        int lessonId, string userId, CancellationToken ct = default)
    {
        var lesson = await uow.Lessons.GetByIdAsync(lessonId, ct).ConfigureAwait(false);
        if (lesson is null || lesson.Type != LessonType.Article)
        {
            logger.LogWarning("文章單元不存在或類型不符 | LessonId={LessonId}", lessonId);
            return null;
        }

        var context = await GetLessonContextAsync(lesson, userId, ct).ConfigureAwait(false);
        if (context is null) return null;
        var (section, course, progress, prevId, nextId) = context.Value;

        return new ArticlePlayerViewModel
        {
            LessonId = lesson.Id,
            Title = lesson.Title,
            Description = lesson.Description,
            ArticleContent = lesson.ArticleContent,
            IsCompleted = progress?.IsCompleted ?? false,
            SectionId = lesson.SectionId,
            SectionTitle = section?.Title,
            CourseId = course?.Id ?? 0,
            CourseTitle = course?.Title,
            PreviousLessonId = prevId,
            NextLessonId = nextId,
            Attachments = await GetAttachmentsAsync(lesson.Id, ct).ConfigureAwait(false)
        };
    }

    public async Task<PdfPlayerViewModel?> GetPdfPlayerAsync(
        int lessonId, string userId, CancellationToken ct = default)
    {
        var lesson = await uow.Lessons.GetByIdAsync(lessonId, ct).ConfigureAwait(false);
        if (lesson is null || lesson.Type != LessonType.Pdf)
        {
            logger.LogWarning("PDF 單元不存在或類型不符 | LessonId={LessonId}", lessonId);
            return null;
        }

        var context = await GetLessonContextAsync(lesson, userId, ct).ConfigureAwait(false);
        if (context is null) return null;
        var (section, course, progress, prevId, nextId) = context.Value;

        return new PdfPlayerViewModel
        {
            LessonId = lesson.Id,
            Title = lesson.Title,
            Description = lesson.Description,
            PdfUrl = lesson.PdfUrl,
            PdfFileName = lesson.PdfFileName,
            IsCompleted = progress?.IsCompleted ?? false,
            SectionId = lesson.SectionId,
            SectionTitle = section?.Title,
            CourseId = course?.Id ?? 0,
            CourseTitle = course?.Title,
            PreviousLessonId = prevId,
            NextLessonId = nextId,
            Attachments = await GetAttachmentsAsync(lesson.Id, ct).ConfigureAwait(false)
        };
    }

    /// <summary>取得課程上下文，同時檢查用戶是否有存取權限。無權限時回傳 null。</summary>
    private async Task<(Section? section, Course? course, Progress? progress, int? prevId, int? nextId)?>
        GetLessonContextAsync(Lesson lesson, string userId, CancellationToken ct)
    {
        var section = await uow.Sections.GetByIdAsync(lesson.SectionId, ct).ConfigureAwait(false);
        var course = section is not null
            ? await uow.Courses.GetByIdAsync(section.CourseId, ct).ConfigureAwait(false)
            : null;

        // 權限檢查：非免費試看的課程必須有購買或訂閱
        if (!lesson.IsFreePreview && course is not null)
        {
            var hasAccess = await uow.Enrollments.HasActiveAccessAsync(userId, course.Id, ct).ConfigureAwait(false);
            if (!hasAccess)
            {
                logger.LogWarning("未購買課程 | UserId={UserId} | CourseId={CourseId} | LessonId={LessonId}",
                    userId, course.Id, lesson.Id);
                return null;
            }
        }

        var progress = await uow.Progresses.GetByUserAndLessonAsync(userId, lesson.Id, ct).ConfigureAwait(false);

        int? prevId = null;
        int? nextId = null;
        if (section is not null)
        {
            var siblings = await uow.Lessons.GetBySectionIdAsync(section.Id, ct).ConfigureAwait(false);
            var sorted = siblings.OrderBy(l => l.SortOrder).ThenBy(l => l.Id).ToList();
            var idx = sorted.FindIndex(l => l.Id == lesson.Id);
            if (idx > 0) prevId = sorted[idx - 1].Id;
            if (idx >= 0 && idx < sorted.Count - 1) nextId = sorted[idx + 1].Id;
        }

        return (section, course, progress, prevId, nextId);
    }

    private async Task<IReadOnlyList<LessonAttachmentViewModel>> GetAttachmentsAsync(
        int lessonId, CancellationToken ct)
    {
        var attachments = await uow.LessonAttachments.GetByLessonIdAsync(lessonId, ct).ConfigureAwait(false);
        return attachments.Select(a => new LessonAttachmentViewModel
        {
            Id = a.Id,
            FileName = a.FileName,
            FileUrl = a.FileUrl,
            FileSizeDisplay = a.FileSizeBytes switch
            {
                < 1024 => $"{a.FileSizeBytes} B",
                < 1024 * 1024 => $"{a.FileSizeBytes / 1024.0:F1} KB",
                _ => $"{a.FileSizeBytes / (1024.0 * 1024.0):F1} MB"
            }
        }).ToList();
    }
}
