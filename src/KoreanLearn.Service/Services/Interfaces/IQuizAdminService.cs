using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.ViewModels.Admin.Quiz;

namespace KoreanLearn.Service.Services.Interfaces;

public interface IQuizAdminService
{
    Task<QuizDetailViewModel?> GetQuizDetailAsync(int quizId, CancellationToken ct = default);
    Task<QuizFormViewModel?> GetQuizForEditAsync(int quizId, CancellationToken ct = default);
    Task<QuizFormViewModel> PrepareCreateFormAsync(int lessonId, CancellationToken ct = default);
    Task<ServiceResult<int>> CreateQuizAsync(QuizFormViewModel vm, CancellationToken ct = default);
    Task<ServiceResult> UpdateQuizAsync(QuizFormViewModel vm, CancellationToken ct = default);
    Task<ServiceResult> DeleteQuizAsync(int quizId, CancellationToken ct = default);

    Task<QuestionFormViewModel?> GetQuestionForEditAsync(int questionId, CancellationToken ct = default);
    Task<ServiceResult<int>> AddQuestionAsync(QuestionFormViewModel vm, CancellationToken ct = default);
    Task<ServiceResult> UpdateQuestionAsync(QuestionFormViewModel vm, CancellationToken ct = default);
    Task<ServiceResult> DeleteQuestionAsync(int questionId, CancellationToken ct = default);
}
