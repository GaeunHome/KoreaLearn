using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>討論區業務邏輯介面（涵蓋討論主題與回覆的 CRUD）</summary>
public interface IDiscussionService
{
    /// <summary>取得全站討論列表（分頁）</summary>
    Task<PagedResult<DiscussionListItem>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>取得指定課程的討論列表（分頁）</summary>
    Task<PagedResult<DiscussionListItem>> GetByCourseAsync(int courseId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>取得討論詳情（含所有回覆）</summary>
    Task<DiscussionDetailViewModel?> GetDetailAsync(int id, CancellationToken ct = default);

    /// <summary>建立新討論主題，回傳討論 ID</summary>
    Task<ServiceResult<int>> CreateAsync(string userId, int courseId, string title, string content, CancellationToken ct = default);

    /// <summary>新增回覆至指定討論</summary>
    Task<ServiceResult> ReplyAsync(string userId, int discussionId, string content, CancellationToken ct = default);

    /// <summary>軟刪除討論（本人或管理員可刪）</summary>
    Task<ServiceResult> DeleteAsync(int id, string userId, bool isAdmin, CancellationToken ct = default);
}

/// <summary>討論列表項目 ViewModel</summary>
public class DiscussionListItem
{
    /// <summary>討論 ID</summary>
    public int Id { get; set; }

    /// <summary>討論標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>作者顯示名稱</summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>所屬課程 ID</summary>
    public int CourseId { get; set; }

    /// <summary>所屬課程名稱</summary>
    public string? CourseName { get; set; }

    /// <summary>建立時間</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>回覆數量</summary>
    public int ReplyCount { get; set; }
}

/// <summary>討論詳情 ViewModel（含回覆清單）</summary>
public class DiscussionDetailViewModel
{
    /// <summary>討論 ID</summary>
    public int Id { get; set; }

    /// <summary>所屬課程 ID</summary>
    public int CourseId { get; set; }

    /// <summary>討論標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>討論內容</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>作者顯示名稱</summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>作者使用者 ID</summary>
    public string AuthorId { get; set; } = string.Empty;

    /// <summary>建立時間</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>回覆列表</summary>
    public IReadOnlyList<ReplyViewModel> Replies { get; set; } = [];
}

/// <summary>討論回覆 ViewModel</summary>
public class ReplyViewModel
{
    /// <summary>回覆 ID</summary>
    public int Id { get; set; }

    /// <summary>回覆內容</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>作者顯示名稱</summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>作者使用者 ID</summary>
    public string AuthorId { get; set; } = string.Empty;

    /// <summary>建立時間</summary>
    public DateTime CreatedAt { get; set; }
}
