using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

/// <summary>字卡學習紀錄 Repository 介面，提供 SM-2 複習排程相關查詢</summary>
public interface IFlashcardLogRepository : IRepository<FlashcardLog>
{
    /// <summary>取得使用者對指定字卡的最新學習紀錄</summary>
    Task<FlashcardLog?> GetByUserAndCardAsync(string userId, int flashcardId, CancellationToken ct = default);

    /// <summary>取得使用者在指定牌組中已到期需複習的字卡紀錄</summary>
    Task<IReadOnlyList<FlashcardLog>> GetDueCardsAsync(string userId, int deckId, CancellationToken ct = default);

    /// <summary>取得使用者在指定牌組的所有學習紀錄</summary>
    Task<IReadOnlyList<FlashcardLog>> GetByUserAndDeckAsync(string userId, int deckId, CancellationToken ct = default);
}
