using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

/// <summary>幻燈片橫幅 Repository 介面</summary>
public interface IBannerRepository : IRepository<Banner>
{
    /// <summary>取得所有已啟用的幻燈片（依顯示順序排列）</summary>
    Task<IReadOnlyList<Banner>> GetActiveOrderedAsync(CancellationToken ct = default);
}
