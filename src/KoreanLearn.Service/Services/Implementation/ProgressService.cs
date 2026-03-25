using KoreanLearn.Data.Entities;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

public class ProgressService(
    IUnitOfWork uow,
    ILogger<ProgressService> logger) : IProgressService
{
    public async Task<ServiceResult<int>> SaveVideoProgressAsync(
        string userId, int lessonId, int progressSeconds, CancellationToken ct = default)
    {
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

    public async Task<ServiceResult> MarkLessonCompleteAsync(
        string userId, int lessonId, CancellationToken ct = default)
    {
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

    public async Task<int> GetVideoProgressAsync(
        string userId, int lessonId, CancellationToken ct = default)
    {
        var progress = await uow.Progresses.GetByUserAndLessonAsync(userId, lessonId, ct).ConfigureAwait(false);
        return progress?.VideoProgressSeconds ?? 0;
    }
}
