using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

/// <summary>系統參數 Repository 介面</summary>
public interface ISystemSettingRepository
{
    /// <summary>取得所有參數（依群組與鍵排序）</summary>
    Task<IReadOnlyList<SystemSetting>> GetAllOrderedAsync(CancellationToken ct = default);

    /// <summary>依參數鍵取得設定</summary>
    Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken ct = default);

    /// <summary>依主鍵取得設定</summary>
    Task<SystemSetting?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>新增設定</summary>
    Task AddAsync(SystemSetting entity, CancellationToken ct = default);

    /// <summary>更新設定</summary>
    void Update(SystemSetting entity);

    /// <summary>移除設定</summary>
    void Remove(SystemSetting entity);
}
