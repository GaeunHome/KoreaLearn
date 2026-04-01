using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

/// <summary>公告附件 Repository 介面</summary>
public interface IAnnouncementAttachmentRepository : IRepository<AnnouncementAttachment>
{
    /// <summary>取得指定公告的所有附件</summary>
    Task<IReadOnlyList<AnnouncementAttachment>> GetByAnnouncementIdAsync(int announcementId, CancellationToken ct = default);
}
