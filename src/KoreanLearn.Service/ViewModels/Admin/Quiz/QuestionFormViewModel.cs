using System.ComponentModel.DataAnnotations;
using KoreanLearn.Library.Enums;

namespace KoreanLearn.Service.ViewModels.Admin.Quiz;

/// <summary>建立/編輯題目表單 ViewModel（支援單選、多選、填空）</summary>
public class QuestionFormViewModel
{
    /// <summary>題目 ID（編輯時使用）</summary>
    public int Id { get; set; }

    /// <summary>所屬測驗 ID</summary>
    public int QuizId { get; set; }

    [Required(ErrorMessage = "題目內容為必填")]
    [MaxLength(2000)]
    [Display(Name = "題目內容")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "請選擇題型")]
    [Display(Name = "題型")]
    public QuestionType Type { get; set; }

    [Range(1, 100, ErrorMessage = "配分須在 1–100 之間")]
    [Display(Name = "配分")]
    public int Points { get; set; } = 1;

    [Display(Name = "排序")]
    public int SortOrder { get; set; }

    [Display(Name = "正確答案（填空題）")]
    [MaxLength(500)]
    public string? CorrectAnswer { get; set; }

    /// <summary>選擇題的選項列表</summary>
    public List<OptionFormViewModel> Options { get; set; } = [];

    /// <summary>所屬課程 ID（導覽用）</summary>
    public int? CourseId { get; set; }
}

/// <summary>建立/編輯選項表單 ViewModel</summary>
public class OptionFormViewModel
{
    /// <summary>選項 ID（編輯時使用）</summary>
    public int Id { get; set; }

    [Required(ErrorMessage = "選項內容為必填")]
    [MaxLength(1000)]
    [Display(Name = "選項內容")]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "正確答案")]
    public bool IsCorrect { get; set; }

    [Display(Name = "排序")]
    public int SortOrder { get; set; }
}
