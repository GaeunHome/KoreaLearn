using KoreanLearn.Data.Repositories.Interfaces;
using KoreanLearn.Library.Helpers;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

public class Repository<T>(ApplicationDbContext db) : IRepository<T> where T : class
{
    protected readonly DbSet<T> DbSet = db.Set<T>();
    protected readonly ApplicationDbContext Db = db;

    public Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => DbSet.FindAsync([id], ct).AsTask();

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await DbSet.AsNoTracking().ToListAsync(ct).ConfigureAwait(false);

    public async Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var total = await DbSet.CountAsync(ct).ConfigureAwait(false);
        var items = await DbSet.AsNoTracking()
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct).ConfigureAwait(false);
        return new PagedResult<T>(items, total, page, pageSize);
    }

    public async Task<int> CountAsync(CancellationToken ct = default)
        => await DbSet.CountAsync(ct).ConfigureAwait(false);

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await DbSet.AddAsync(entity, ct).ConfigureAwait(false);
        return entity;
    }

    public void Update(T entity) => DbSet.Update(entity);
    public void Remove(T entity) => DbSet.Remove(entity);
}
