using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
using KoreanLearn.Library.Helpers;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

public class OrderRepository(ApplicationDbContext db) : Repository<Order>(db), IOrderRepository
{
    public async Task<Order?> GetWithItemsAsync(int id, CancellationToken ct = default)
        => await DbSet
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Course)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == id, ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(string userId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Course)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct).ConfigureAwait(false);

    public async Task<PagedResult<Order>> GetByUserIdPagedAsync(string userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Course)
            .Where(o => o.UserId == userId);

        var total = await query.CountAsync(ct).ConfigureAwait(false);
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct).ConfigureAwait(false);
        return new PagedResult<Order>(items, total, page, pageSize);
    }

    public async Task<PagedResult<Order>> GetPagedWithItemsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var total = await DbSet.CountAsync(ct).ConfigureAwait(false);
        var items = await DbSet.AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Course)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct).ConfigureAwait(false);
        return new PagedResult<Order>(items, total, page, pageSize);
    }
}
