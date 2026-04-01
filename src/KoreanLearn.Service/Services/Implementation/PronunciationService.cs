using KoreanLearn.Data.Entities;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Pronunciation;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

/// <summary>發音練習業務邏輯實作，處理後台管理（CRUD）與前台練習（錄音上傳）</summary>
public class PronunciationService(
    IUnitOfWork uow,
    ILogger<PronunciationService> logger) : IPronunciationService
{
    /// <inheritdoc />
    public async Task<PagedResult<PronunciationListViewModel>> GetPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var result = await uow.Pronunciations.GetPagedAsync(page, pageSize, ct).ConfigureAwait(false);
        var items = result.Items.Select(p => new PronunciationListViewModel
        {
            Id = p.Id, Korean = p.Korean, Romanization = p.Romanization,
            Chinese = p.Chinese, StandardAudioUrl = p.StandardAudioUrl, LessonId = p.LessonId
        }).ToList();
        return new PagedResult<PronunciationListViewModel>(items, result.TotalCount, result.Page, result.PageSize);
    }

    /// <inheritdoc />
    public async Task<PronunciationFormViewModel?> GetForEditAsync(int id, CancellationToken ct = default)
    {
        var p = await uow.Pronunciations.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (p is null) return null;
        return new PronunciationFormViewModel
        {
            Id = p.Id, Korean = p.Korean, Romanization = p.Romanization,
            Chinese = p.Chinese, ExistingAudioUrl = p.StandardAudioUrl, LessonId = p.LessonId
        };
    }

    /// <inheritdoc />
    public async Task<ServiceResult<int>> CreateAsync(
        PronunciationFormViewModel vm, string audioUrl, CancellationToken ct = default)
    {
        var entity = new PronunciationExercise
        {
            Korean = vm.Korean, Romanization = vm.Romanization,
            Chinese = vm.Chinese, StandardAudioUrl = audioUrl, LessonId = vm.LessonId
        };
        await uow.Pronunciations.AddAsync(entity, ct).ConfigureAwait(false);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("發音練習建立成功 | Id={Id} | Korean={Korean}", entity.Id, entity.Korean);
        return ServiceResult<int>.Success(entity.Id);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> UpdateAsync(
        PronunciationFormViewModel vm, string? newAudioUrl, CancellationToken ct = default)
    {
        logger.LogInformation("更新發音練習 | Id={Id} | Korean={Korean}", vm.Id, vm.Korean);
        var entity = await uow.Pronunciations.GetByIdAsync(vm.Id, ct).ConfigureAwait(false);
        if (entity is null)
        {
            logger.LogWarning("更新發音練習失敗：不存在 | Id={Id}", vm.Id);
            return ServiceResult.Failure("發音練習不存在");
        }

        entity.Korean = vm.Korean;
        entity.Romanization = vm.Romanization;
        entity.Chinese = vm.Chinese;
        entity.LessonId = vm.LessonId;
        if (!string.IsNullOrEmpty(newAudioUrl)) entity.StandardAudioUrl = newAudioUrl;

        uow.Pronunciations.Update(entity);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("發音練習更新成功 | Id={Id}", vm.Id);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await uow.Pronunciations.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (entity is null)
        {
            logger.LogWarning("刪除發音練習失敗：不存在 | Id={Id}", id);
            return ServiceResult.Failure("發音練習不存在");
        }
        uow.Pronunciations.Remove(entity);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("發音練習刪除成功（軟刪除）| Id={Id} | Korean={Korean}", id, entity.Korean);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PronunciationListViewModel>> GetAllForPracticeAsync(
        CancellationToken ct = default)
    {
        var all = await uow.Pronunciations.GetAllAsync(ct).ConfigureAwait(false);
        return all.Select(p => new PronunciationListViewModel
        {
            Id = p.Id, Korean = p.Korean, Romanization = p.Romanization,
            Chinese = p.Chinese, StandardAudioUrl = p.StandardAudioUrl
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> SaveAttemptAsync(
        string userId, int exerciseId, string recordingUrl, CancellationToken ct = default)
    {
        var exercise = await uow.Pronunciations.GetByIdAsync(exerciseId, ct).ConfigureAwait(false);
        if (exercise is null)
        {
            logger.LogWarning("儲存發音嘗試失敗：練習不存在 | ExerciseId={ExerciseId}", exerciseId);
            return ServiceResult.Failure("發音練習不存在");
        }

        exercise.Attempts.Add(new PronunciationAttempt
        {
            UserId = userId, ExerciseId = exerciseId, RecordingUrl = recordingUrl
        });
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("發音錄音上傳成功 | UserId={UserId} | ExerciseId={ExerciseId}", userId, exerciseId);
        return ServiceResult.Success();
    }
}
