using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Data.Repositories.Interfaces;

/// <summary>泛型 Repository 介面，提供所有實體共用的 CRUD 基本操作</summary>
public interface IRepository<T> where T : class
{
    /// <summary>依主鍵取得單一實體</summary>
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>取得所有實體</summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);

    /// <summary>取得分頁結果</summary>
    Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>取得實體總數</summary>
    Task<int> CountAsync(CancellationToken ct = default);

    /// <summary>新增實體</summary>
    Task<T> AddAsync(T entity, CancellationToken ct = default);

    /// <summary>更新實體（標記為 Modified）</summary>
    void Update(T entity);

    /// <summary>移除實體（ISoftDeletable 會由 DbContext 攔截為軟刪除）</summary>
    void Remove(T entity);
}
