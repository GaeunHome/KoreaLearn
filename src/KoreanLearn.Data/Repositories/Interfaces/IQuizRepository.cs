using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

/// <summary>測驗 Repository 介面，提供測驗與題目的查詢</summary>
public interface IQuizRepository : IRepository<Quiz>
{
    /// <summary>取得測驗及其所有題目與選項</summary>
    Task<Quiz?> GetWithQuestionsAsync(int id, CancellationToken ct = default);

    /// <summary>依單元 ID 取得對應的測驗</summary>
    Task<Quiz?> GetByLessonIdAsync(int lessonId, CancellationToken ct = default);

    /// <summary>依 ID 取得單一題目</summary>
    Task<QuizQuestion?> GetQuestionByIdAsync(int questionId, CancellationToken ct = default);
}
