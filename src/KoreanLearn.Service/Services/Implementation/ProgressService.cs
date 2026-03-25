using KoreanLearn.Data.Entities;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

/// <summary>學習進度追蹤業務邏輯實作，管理影片播放進度與單元完成標記</summary>
public class ProgressService(
    IUnitOfWork uow,
    ILogger<ProgressService> logger) : IProgressService
{
    /// <inheritdoc />
    public async Task<ServiceResult<int>> SaveVideoProgressAsync(
        string userId, int lessonId, int progressSeconds, CancellationToken ct = default)
    {
        // 權限檢查：確認使用者有存取此單元的權限
        var accessCheck = await CheckLessonAccessAsync(userId, lessonId, ct).ConfigureAwait(false);
        if (!accessCheck.IsSuccess) return ServiceResult<int>.Failure(accessCheck.ErrorMessage!);

        var progress = await uow.Progresses.GetByUserAndLessonAsync(userId, lessonId, ct).ConfigureAwait(false);

        if (progress is null)
        {
            progress = new Progress
            {
                UserId = userId,
                LessonId = lessonId,
                VideoProgressSeconds = progressSeconds
            };
            await uow.Progresses.AddAsync(progress, ct).ConfigureAwait(false);
        }
        else
        {
            progress.VideoProgressSeconds = progressSeconds;
            uow.Progresses.Update(progress);
        }

        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("儲存影片進度 | UserId={UserId} | LessonId={LessonId} | Seconds={Seconds}",
            userId, lessonId, progressSeconds);

        return ServiceResult<int>.Success(progressSeconds);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> MarkLessonCompleteAsync(
        string userId, int lessonId, CancellationToken ct = default)
    {
        // 權限檢查：確認使用者有存取此單元的權限
        var accessCheck = await CheckLessonAccessAsync(userId, lessonId, ct).ConfigureAwait(false);
        if (!accessCheck.IsSuccess) return accessCheck;

        var progress = await uow.Progresses.GetByUserAndLessonAsync(userId, lessonId, ct).ConfigureAwait(false);

        if (progress is null)
        {
            progress = new Progress
            {
                UserId = userId,
                LessonId = lessonId,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow
            };
            await uow.Progresses.AddAsync(progress, ct).ConfigureAwait(false);
        }
        else if (!progress.IsCompleted)
        {
            progress.IsCompleted = true;
            progress.CompletedAt = DateTime.UtcNow;
            uow.Progresses.Update(progress);
        }

        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("標記課程完成 | UserId={UserId} | LessonId={LessonId}", userId, lessonId);

        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> UndoLessonCompleteAsync(
        string userId, int lessonId, CancellationToken ct = default)
    {
        var accessCheck = await CheckLessonAccessAsync(userId, lessonId, ct).ConfigureAwait(false);
        if (!accessCheck.IsSuccess) return accessCheck;

        var progress = await uow.Progresses.GetByUserAndLessonAsync(userId, lessonId, ct).ConfigureAwait(false);
        if (progress is null || !progress.IsCompleted)
            return ServiceResult.Failure("此單元尚未標記完成");

        progress.IsCompleted = false;
        progress.CompletedAt = null;
        uow.Progresses.Update(progress);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("取消完成標記 | UserId={UserId} | LessonId={LessonId}", userId, lessonId);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<int> GetVideoProgressAsync(
        string userId, int lessonId, CancellationToken ct = default)
    {
        var progress = await uow.Progresses.GetByUserAndLessonAsync(userId, lessonId, ct).ConfigureAwait(false);
        return progress?.VideoProgressSeconds ?? 0;
    }

    /// <summary>檢查使用者是否有存取單元的權限（免費試看或已購買課程）</summary>
    private async Task<ServiceResult> CheckLessonAccessAsync(
        string userId, int lessonId, CancellationToken ct)
    {
        var lesson = await uow.Lessons.GetByIdAsync(lessonId, ct).ConfigureAwait(false);
        if (lesson is null) return ServiceResult.Failure("單元不存在");
        if (lesson.IsFreePreview) return ServiceResult.Success();

        var section = await uow.Sections.GetByIdAsync(lesson.SectionId, ct).ConfigureAwait(false);
        if (section is null) return ServiceResult.Failure("章節不存在");

        var hasAccess = await uow.Enrollments.HasActiveAccessAsync(userId, section.CourseId, ct).ConfigureAwait(false);
        return hasAccess ? ServiceResult.Success() : ServiceResult.Failure("您尚未購買此課程");
    }
}
