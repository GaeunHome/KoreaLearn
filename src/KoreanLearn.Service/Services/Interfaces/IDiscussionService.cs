using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Service.Services.Interfaces;

public interface IDiscussionService
{
    Task<PagedResult<DiscussionListItem>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<DiscussionListItem>> GetByCourseAsync(int courseId, int page, int pageSize, CancellationToken ct = default);
    Task<DiscussionDetailViewModel?> GetDetailAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<int>> CreateAsync(string userId, int courseId, string title, string content, CancellationToken ct = default);
    Task<ServiceResult> ReplyAsync(string userId, int discussionId, string content, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(int id, string userId, bool isAdmin, CancellationToken ct = default);
}

public class DiscussionListItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string? CourseName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ReplyCount { get; set; }
}

public class DiscussionDetailViewModel
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<ReplyViewModel> Replies { get; set; } = [];
}

public class ReplyViewModel
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
