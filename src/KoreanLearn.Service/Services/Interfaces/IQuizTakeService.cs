using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.ViewModels.Learn;

namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>前台測驗作答業務邏輯介面（取得作答資料、提交答案、查看成績）</summary>
public interface IQuizTakeService
{
    /// <summary>取得測驗作答頁面所需資料（題目、選項，不含正確答案）</summary>
    Task<QuizTakeViewModel?> GetQuizForTakeAsync(int quizId, CancellationToken ct = default);

    /// <summary>提交測驗答案並計算成績，回傳作答紀錄 ID</summary>
    Task<ServiceResult<int>> SubmitQuizAsync(string userId, QuizSubmitModel model, CancellationToken ct = default);

    /// <summary>取得測驗結果（含各題答對與否、正確答案）</summary>
    Task<QuizResultViewModel?> GetResultAsync(int attemptId, string userId, CancellationToken ct = default);
}
