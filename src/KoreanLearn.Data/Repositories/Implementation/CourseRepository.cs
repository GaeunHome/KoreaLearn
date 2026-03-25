using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
using KoreanLearn.Library.Helpers;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

public class CourseRepository(ApplicationDbContext db) : Repository<Course>(db), ICourseRepository
{
    public async Task<bool> ExistsByTitleAsync(string title, CancellationToken ct = default)
        => await DbSet.AnyAsync(c => c.Title == title, ct).ConfigureAwait(false);

    public async Task<PagedResult<Course>> SearchAsync(string? keyword, int page, int pageSize, CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(c => c.Title.Contains(keyword) || (c.Description != null && c.Description.Contains(keyword)));

        var total = await query.CountAsync(ct).ConfigureAwait(false);
        var items = await query.OrderBy(c => c.SortOrder)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct).ConfigureAwait(false);
        return new PagedResult<Course>(items, total, page, pageSize);
    }

    public async Task<Course?> GetWithSectionsAndLessonsAsync(int id, CancellationToken ct = default)
        => await DbSet
            .Include(c => c.Teacher)
            .Include(c => c.Sections.OrderBy(s => s.SortOrder))
                .ThenInclude(s => s.Lessons.OrderBy(l => l.SortOrder))
            .FirstOrDefaultAsync(c => c.Id == id, ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<Course>> GetPublishedAsync(CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(c => c.IsPublished)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(ct).ConfigureAwait(false);

    public async Task<PagedResult<Course>> GetByTeacherIdPagedAsync(
        string teacherId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking().Where(c => c.TeacherId == teacherId);
        var total = await query.CountAsync(ct).ConfigureAwait(false);
        var items = await query.OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Include(c => c.Sections)
            .ToListAsync(ct).ConfigureAwait(false);
        return new PagedResult<Course>(items, total, page, pageSize);
    }

    public async Task<bool> IsOwnedByTeacherAsync(int courseId, string teacherId, CancellationToken ct = default)
        => await DbSet.AnyAsync(c => c.Id == courseId && c.TeacherId == teacherId, ct).ConfigureAwait(false);
}
