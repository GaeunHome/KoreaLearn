using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.ViewModels.Admin.Quiz;

namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>後台測驗管理業務邏輯介面（涵蓋測驗與題目的 CRUD）</summary>
public interface IQuizAdminService
{
    /// <summary>取得測驗詳情（含題目與選項）</summary>
    Task<QuizDetailViewModel?> GetQuizDetailAsync(int quizId, CancellationToken ct = default);

    /// <summary>取得測驗編輯表單資料</summary>
    Task<QuizFormViewModel?> GetQuizForEditAsync(int quizId, CancellationToken ct = default);

    /// <summary>準備建立測驗的空白表單（預填單元資訊與預設及格分數）</summary>
    Task<QuizFormViewModel> PrepareCreateFormAsync(int lessonId, CancellationToken ct = default);

    /// <summary>建立新測驗，回傳測驗 ID</summary>
    Task<ServiceResult<int>> CreateQuizAsync(QuizFormViewModel vm, CancellationToken ct = default);

    /// <summary>更新測驗基本資訊</summary>
    Task<ServiceResult> UpdateQuizAsync(QuizFormViewModel vm, CancellationToken ct = default);

    /// <summary>軟刪除測驗</summary>
    Task<ServiceResult> DeleteQuizAsync(int quizId, CancellationToken ct = default);

    /// <summary>取得題目編輯表單資料</summary>
    Task<QuestionFormViewModel?> GetQuestionForEditAsync(int questionId, CancellationToken ct = default);

    /// <summary>新增題目至指定測驗</summary>
    Task<ServiceResult<int>> AddQuestionAsync(QuestionFormViewModel vm, CancellationToken ct = default);

    /// <summary>更新題目內容與選項</summary>
    Task<ServiceResult> UpdateQuestionAsync(QuestionFormViewModel vm, CancellationToken ct = default);

    /// <summary>刪除題目</summary>
    Task<ServiceResult> DeleteQuestionAsync(int questionId, CancellationToken ct = default);
}
