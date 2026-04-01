using KoreanLearn.Data.Entities;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Enums;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Learn;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

/// <summary>前台單元播放器業務邏輯實作，依單元類型組裝播放器所需的 ViewModel（含導覽、進度、附件）</summary>
public class LessonPlayerService(
    IUnitOfWork uow,
    ILogger<LessonPlayerService> logger) : ILessonPlayerService
{
    /// <inheritdoc />
    public async Task<VideoPlayerViewModel?> GetVideoPlayerAsync(
        int lessonId, string userId, IEnumerable<string> userRoles, CancellationToken ct = default)
    {
        logger.LogDebug("取得影片播放器 | LessonId={LessonId} | UserId={UserId}", lessonId, userId);
        var lesson = await uow.Lessons.GetByIdAsync(lessonId, ct).ConfigureAwait(false);
        if (lesson is null || lesson.Type != LessonType.Video)
        {
            logger.LogWarning("影片單元不存在或類型不符 | LessonId={LessonId}", lessonId);
            return null;
        }

        var context = await GetLessonContextAsync(lesson, userId, userRoles, ct).ConfigureAwait(false);
        if (context is null) return null;
        var (section, course, progress, prevId, prevAction, nextId, nextAction) = context.Value;

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
            PreviousLessonAction = prevAction,
            NextLessonId = nextId,
            NextLessonAction = nextAction,
            Attachments = await GetAttachmentsAsync(lesson.Id, ct).ConfigureAwait(false)
        };
    }

    /// <inheritdoc />
    public async Task<ArticlePlayerViewModel?> GetArticlePlayerAsync(
        int lessonId, string userId, IEnumerable<string> userRoles, CancellationToken ct = default)
    {
        logger.LogDebug("取得文章播放器 | LessonId={LessonId} | UserId={UserId}", lessonId, userId);
        var lesson = await uow.Lessons.GetByIdAsync(lessonId, ct).ConfigureAwait(false);
        if (lesson is null || lesson.Type != LessonType.Article)
        {
            logger.LogWarning("文章單元不存在或類型不符 | LessonId={LessonId}", lessonId);
            return null;
        }

        var context = await GetLessonContextAsync(lesson, userId, userRoles, ct).ConfigureAwait(false);
        if (context is null) return null;
        var (section, course, progress, prevId, prevAction, nextId, nextAction) = context.Value;

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
            PreviousLessonAction = prevAction,
            NextLessonId = nextId,
            NextLessonAction = nextAction,
            Attachments = await GetAttachmentsAsync(lesson.Id, ct).ConfigureAwait(false)
        };
    }

    /// <inheritdoc />
    public async Task<PdfPlayerViewModel?> GetPdfPlayerAsync(
        int lessonId, string userId, IEnumerable<string> userRoles, CancellationToken ct = default)
    {
        logger.LogDebug("取得 PDF 播放器 | LessonId={LessonId} | UserId={UserId}", lessonId, userId);
        var lesson = await uow.Lessons.GetByIdAsync(lessonId, ct).ConfigureAwait(false);
        if (lesson is null || lesson.Type != LessonType.Pdf)
        {
            logger.LogWarning("PDF 單元不存在或類型不符 | LessonId={LessonId}", lessonId);
            return null;
        }

        var context = await GetLessonContextAsync(lesson, userId, userRoles, ct).ConfigureAwait(false);
        if (context is null) return null;
        var (section, course, progress, prevId, prevAction, nextId, nextAction) = context.Value;

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
            PreviousLessonAction = prevAction,
            NextLessonId = nextId,
            NextLessonAction = nextAction,
            Attachments = await GetAttachmentsAsync(lesson.Id, ct).ConfigureAwait(false)
        };
    }

    /// <summary>取得單元的上下文資訊（章節、課程、學習進度、前後單元 ID 與類型），同時檢查存取權限</summary>
    private async Task<(Section? section, Course? course, Progress? progress, int? prevId, string? prevAction, int? nextId, string? nextAction)?>
        GetLessonContextAsync(Lesson lesson, string userId, IEnumerable<string> userRoles, CancellationToken ct)
    {
        var section = await uow.Sections.GetByIdAsync(lesson.SectionId, ct).ConfigureAwait(false);
        var course = section is not null
            ? await uow.Courses.GetByIdAsync(section.CourseId, ct).ConfigureAwait(false)
            : null;

        if (!await HasLessonAccessAsync(lesson, course, userId, userRoles, ct).ConfigureAwait(false))
            return null;

        var progress = await uow.Progresses.GetByUserAndLessonAsync(userId, lesson.Id, ct).ConfigureAwait(false);
        var (prevId, prevAction, nextId, nextAction) = await GetSiblingNavigationAsync(lesson, section, ct).ConfigureAwait(false);

        return (section, course, progress, prevId, prevAction, nextId, nextAction);
    }

    /// <summary>檢查使用者是否有權存取該單元（Admin/Teacher/已購買/免費預覽）</summary>
    private async Task<bool> HasLessonAccessAsync(
        Lesson lesson, Course? course, string userId, IEnumerable<string> userRoles, CancellationToken ct)
    {
        if (lesson.IsFreePreview || course is null) return true;

        var roles = userRoles.ToList();
        if (roles.Contains("Admin", StringComparer.OrdinalIgnoreCase)) return true;
        if (roles.Contains("Teacher", StringComparer.OrdinalIgnoreCase) && course.TeacherId == userId) return true;

        var hasAccess = await uow.Enrollments.HasActiveAccessAsync(userId, course.Id, ct).ConfigureAwait(false);
        if (!hasAccess)
        {
            logger.LogWarning("課程存取被拒 | UserId={UserId} | CourseId={CourseId} | LessonId={LessonId}",
                userId, course.Id, lesson.Id);
        }
        return hasAccess;
    }

    /// <summary>計算同章節內的前後單元導覽資訊</summary>
    private async Task<(int? prevId, string? prevAction, int? nextId, string? nextAction)>
        GetSiblingNavigationAsync(Lesson lesson, Section? section, CancellationToken ct)
    {
        if (section is null) return (null, null, null, null);

        var siblings = await uow.Lessons.GetBySectionIdAsync(section.Id, ct).ConfigureAwait(false);
        var sorted = siblings.OrderBy(l => l.SortOrder).ThenBy(l => l.Id).ToList();
        var idx = sorted.FindIndex(l => l.Id == lesson.Id);

        var prevId = idx > 0 ? sorted[idx - 1].Id : (int?)null;
        var prevAction = idx > 0 ? GetLessonAction(sorted[idx - 1].Type) : null;
        var nextId = idx >= 0 && idx < sorted.Count - 1 ? sorted[idx + 1].Id : (int?)null;
        var nextAction = idx >= 0 && idx < sorted.Count - 1 ? GetLessonAction(sorted[idx + 1].Type) : null;

        return (prevId, prevAction, nextId, nextAction);
    }

    /// <summary>根據 LessonType 取得對應的 Controller Action 名稱</summary>
    private static string GetLessonAction(LessonType type) => type switch
    {
        LessonType.Video => "Video",
        LessonType.Article => "Article",
        LessonType.Pdf => "Pdf",
        _ => "Article"
    };

    /// <summary>取得單元的附件列表並轉換檔案大小為可讀格式</summary>
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
