using KoreanLearn.Data.Entities;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

public class DiscussionService(
    IUnitOfWork uow,
    ILogger<DiscussionService> logger) : IDiscussionService
{
    public async Task<PagedResult<DiscussionListItem>> GetByCourseAsync(
        int courseId, int page, int pageSize, CancellationToken ct = default)
    {
        var result = await uow.Discussions.GetByCourseIdAsync(courseId, page, pageSize, ct).ConfigureAwait(false);
        var items = result.Items.Select(d => new DiscussionListItem
        {
            Id = d.Id, Title = d.Title,
            AuthorName = d.User?.DisplayName ?? "匿名",
            CreatedAt = d.CreatedAt,
            ReplyCount = d.Replies.Count
        }).ToList();
        return new PagedResult<DiscussionListItem>(items, result.TotalCount, result.Page, result.PageSize);
    }

    public async Task<DiscussionDetailViewModel?> GetDetailAsync(int id, CancellationToken ct = default)
    {
        var d = await uow.Discussions.GetWithRepliesAsync(id, ct).ConfigureAwait(false);
        if (d is null) return null;
        return new DiscussionDetailViewModel
        {
            Id = d.Id, CourseId = d.CourseId, Title = d.Title, Content = d.Content,
            AuthorName = d.User?.DisplayName ?? "匿名", AuthorId = d.UserId,
            CreatedAt = d.CreatedAt,
            Replies = d.Replies.Select(r => new ReplyViewModel
            {
                Id = r.Id, Content = r.Content,
                AuthorName = r.User?.DisplayName ?? "匿名", AuthorId = r.UserId,
                CreatedAt = r.CreatedAt
            }).ToList()
        };
    }

    public async Task<ServiceResult<int>> CreateAsync(
        string userId, int courseId, string title, string content, CancellationToken ct = default)
    {
        var discussion = new Discussion
        {
            UserId = userId, CourseId = courseId, Title = title, Content = content
        };
        await uow.Discussions.AddAsync(discussion, ct).ConfigureAwait(false);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("討論建立 | Id={Id} | Title={Title}", discussion.Id, title);
        return ServiceResult<int>.Success(discussion.Id);
    }

    public async Task<ServiceResult> ReplyAsync(
        string userId, int discussionId, string content, CancellationToken ct = default)
    {
        var discussion = await uow.Discussions.GetWithRepliesAsync(discussionId, ct).ConfigureAwait(false);
        if (discussion is null) return ServiceResult.Failure("討論不存在");

        discussion.Replies.Add(new DiscussionReply
        {
            UserId = userId, DiscussionId = discussionId, Content = content
        });
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(
        int id, string userId, bool isAdmin, CancellationToken ct = default)
    {
        var discussion = await uow.Discussions.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (discussion is null) return ServiceResult.Failure("討論不存在");
        if (discussion.UserId != userId && !isAdmin) return ServiceResult.Failure("無權限");

        uow.Discussions.Remove(discussion);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        return ServiceResult.Success();
    }
}
