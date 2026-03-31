using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.ViewModels.Learn;

namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>前台字卡學習業務邏輯介面（牌組瀏覽、學習排程、SM-2 間隔重複複習）</summary>
public interface IFlashcardLearnService
{
    /// <summary>取得所有牌組清單（附帶使用者的待複習數量）</summary>
    Task<IReadOnlyList<FlashcardDeckListViewModel>> GetDecksForStudyAsync(string userId, CancellationToken ct = default);

    /// <summary>取得學習 Session（到期卡片優先，其次新卡片，每次最多 20 張）</summary>
    Task<FlashcardStudyViewModel?> GetStudySessionAsync(int deckId, string userId, CancellationToken ct = default);

    /// <summary>記錄字卡複習結果，使用 SM-2 演算法計算下次複習時間</summary>
    Task<ServiceResult> ReviewCardAsync(string userId, int cardId, int quality, CancellationToken ct = default);

    /// <summary>取得使用者待複習字卡總數</summary>
    Task<int> GetDueCardCountAsync(string userId, CancellationToken ct = default);
}
