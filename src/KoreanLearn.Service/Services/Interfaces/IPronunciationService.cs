using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.ViewModels.Admin.Pronunciation;

namespace KoreanLearn.Service.Services.Interfaces;

public interface IPronunciationService
{
    Task<PagedResult<PronunciationListViewModel>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<PronunciationFormViewModel?> GetForEditAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<int>> CreateAsync(PronunciationFormViewModel vm, string audioUrl, CancellationToken ct = default);
    Task<ServiceResult> UpdateAsync(PronunciationFormViewModel vm, string? newAudioUrl, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<PronunciationListViewModel>> GetAllForPracticeAsync(CancellationToken ct = default);
    Task<ServiceResult> SaveAttemptAsync(string userId, int exerciseId, string recordingUrl, CancellationToken ct = default);
}
