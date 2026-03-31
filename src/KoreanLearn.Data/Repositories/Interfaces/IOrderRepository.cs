using KoreanLearn.Data.Entities;
using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Data.Repositories.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetWithItemsAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<PagedResult<Order>> GetByUserIdPagedAsync(string userId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<Order>> GetPagedWithItemsAsync(int page, int pageSize, CancellationToken ct = default);
}
