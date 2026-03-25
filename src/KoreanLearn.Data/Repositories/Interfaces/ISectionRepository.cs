using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

public interface ISectionRepository : IRepository<Section>
{
    Task<IReadOnlyList<Section>> GetByCourseIdAsync(int courseId, CancellationToken ct = default);
}
