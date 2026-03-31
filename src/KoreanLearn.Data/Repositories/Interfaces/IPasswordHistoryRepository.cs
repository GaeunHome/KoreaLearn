using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

/// <summary>密碼歷史記錄 Repository 介面</summary>
public interface IPasswordHistoryRepository
{
    /// <summary>取得使用者最近的密碼歷史記錄</summary>
    Task<IReadOnlyList<PasswordHistory>> GetByUserIdAsync(string userId, int count = 5, CancellationToken ct = default);

    /// <summary>新增密碼歷史記錄</summary>
    Task AddAsync(PasswordHistory entity, CancellationToken ct = default);
}
