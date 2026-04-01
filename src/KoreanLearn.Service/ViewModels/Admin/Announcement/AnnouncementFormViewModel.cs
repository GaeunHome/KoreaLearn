using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace KoreanLearn.Service.ViewModels.Admin.Announcement;

/// <summary>公告表單 ViewModel（後台新增/編輯用）</summary>
public class AnnouncementFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "標題為必填")]
    [StringLength(200, ErrorMessage = "標題不得超過 200 字")]
    [Display(Name = "標題")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "內容為必填")]
    [StringLength(4000, ErrorMessage = "內容不得超過 4000 字")]
    [Display(Name = "內容")]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "啟用")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "置頂")]
    public bool IsPinned { get; set; }

    [Display(Name = "開始日期")]
    public DateTime? StartDate { get; set; }

    [Display(Name = "結束日期")]
    public DateTime? EndDate { get; set; }

    public int SortOrder { get; set; }

    /// <summary>上傳的附件檔案（多檔）</summary>
    [Display(Name = "附件")]
    public IReadOnlyList<IFormFile>? AttachmentFiles { get; set; }

    /// <summary>既有附件列表（編輯時顯示）</summary>
    public IReadOnlyList<ExistingAttachmentViewModel> ExistingAttachments { get; set; } = [];

    /// <summary>要刪除的附件 ID 清單</summary>
    public IReadOnlyList<int>? DeleteAttachmentIds { get; set; }
}

/// <summary>既有附件顯示 ViewModel</summary>
public class ExistingAttachmentViewModel
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string FileSizeDisplay { get; set; } = string.Empty;
}
