using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

public interface IFlashcardLogRepository : IRepository<FlashcardLog>
{
    Task<FlashcardLog?> GetByUserAndCardAsync(string userId, int flashcardId, CancellationToken ct = default);
    Task<IReadOnlyList<FlashcardLog>> GetDueCardsAsync(string userId, int deckId, CancellationToken ct = default);
}
