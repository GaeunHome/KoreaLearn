using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

/// <summary>單元附件 Repository 介面，提供附件檔案的查詢</summary>
public interface ILessonAttachmentRepository : IRepository<LessonAttachment>
{
    /// <summary>取得指定單元的所有附件</summary>
    Task<IReadOnlyList<LessonAttachment>> GetByLessonIdAsync(int lessonId, CancellationToken ct = default);
}
