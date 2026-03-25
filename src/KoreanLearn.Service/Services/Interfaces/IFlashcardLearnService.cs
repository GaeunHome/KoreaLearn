using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.ViewModels.Learn;

namespace KoreanLearn.Service.Services.Interfaces;

public interface IFlashcardLearnService
{
    Task<IReadOnlyList<FlashcardDeckListViewModel>> GetDecksForStudyAsync(string userId, CancellationToken ct = default);
    Task<FlashcardStudyViewModel?> GetStudySessionAsync(int deckId, string userId, CancellationToken ct = default);
    Task<ServiceResult> ReviewCardAsync(string userId, int cardId, int quality, CancellationToken ct = default);
}
