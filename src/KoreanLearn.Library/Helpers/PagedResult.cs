namespace KoreanLearn.Library.Helpers;

/// <summary>通用分頁結果，封裝分頁資料與頁碼資訊</summary>
/// <typeparam name="T">分頁項目的型別</typeparam>
/// <param name="Items">目前頁的資料項目</param>
/// <param name="TotalCount">符合條件的總筆數</param>
/// <param name="Page">目前頁碼（從 1 開始）</param>
/// <param name="PageSize">每頁筆數</param>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    /// <summary>總頁數</summary>
    public int TotalPages   => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>是否有上一頁</summary>
    public bool HasPrevious => Page > 1;

    /// <summary>是否有下一頁</summary>
    public bool HasNext     => Page < TotalPages;
}
