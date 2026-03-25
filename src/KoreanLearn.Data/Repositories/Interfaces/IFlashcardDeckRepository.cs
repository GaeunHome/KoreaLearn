using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

public interface IFlashcardDeckRepository : IRepository<FlashcardDeck>
{
    Task<FlashcardDeck?> GetWithCardsAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<FlashcardDeck>> GetByCourseIdAsync(int courseId, CancellationToken ct = default);
    Task<Flashcard?> GetCardByIdAsync(int cardId, CancellationToken ct = default);
}
