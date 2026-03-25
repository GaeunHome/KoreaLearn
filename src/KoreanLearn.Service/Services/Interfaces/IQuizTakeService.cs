using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.ViewModels.Learn;

namespace KoreanLearn.Service.Services.Interfaces;

public interface IQuizTakeService
{
    Task<QuizTakeViewModel?> GetQuizForTakeAsync(int quizId, CancellationToken ct = default);
    Task<ServiceResult<int>> SubmitQuizAsync(string userId, QuizSubmitModel model, CancellationToken ct = default);
    Task<QuizResultViewModel?> GetResultAsync(int attemptId, string userId, CancellationToken ct = default);
}
