using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

/// <summary>公告附件 Repository 實作</summary>
public class AnnouncementAttachmentRepository(ApplicationDbContext db)
    : Repository<AnnouncementAttachment>(db), IAnnouncementAttachmentRepository
{
    public async Task<IReadOnlyList<AnnouncementAttachment>> GetByAnnouncementIdAsync(
        int announcementId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(a => a.AnnouncementId == announcementId)
            .OrderBy(a => a.SortOrder)
            .ToListAsync(ct).ConfigureAwait(false);
}
