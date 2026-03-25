using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

public interface IAnnouncementRepository : IRepository<Announcement>
{
    Task<IReadOnlyList<Announcement>> GetActiveAsync(CancellationToken ct = default);
}
