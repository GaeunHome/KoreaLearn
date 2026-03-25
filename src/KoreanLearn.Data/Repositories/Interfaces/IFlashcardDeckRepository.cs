using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

/// <summary>字卡牌組 Repository 介面，提供牌組與字卡的查詢</summary>
public interface IFlashcardDeckRepository : IRepository<FlashcardDeck>
{
    /// <summary>取得牌組及其所有字卡</summary>
    Task<FlashcardDeck?> GetWithCardsAsync(int id, CancellationToken ct = default);

    /// <summary>依課程 ID 取得所有關聯牌組</summary>
    Task<IReadOnlyList<FlashcardDeck>> GetByCourseIdAsync(int courseId, CancellationToken ct = default);

    /// <summary>依 ID 取得單一字卡</summary>
    Task<Flashcard?> GetCardByIdAsync(int cardId, CancellationToken ct = default);
}
