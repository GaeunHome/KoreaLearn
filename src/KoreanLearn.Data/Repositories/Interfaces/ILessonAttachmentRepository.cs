using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

public interface ILessonAttachmentRepository : IRepository<LessonAttachment>
{
    Task<IReadOnlyList<LessonAttachment>> GetByLessonIdAsync(int lessonId, CancellationToken ct = default);
}
