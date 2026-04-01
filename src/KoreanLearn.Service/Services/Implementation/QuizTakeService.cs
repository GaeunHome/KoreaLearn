using KoreanLearn.Data.Entities;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Enums;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Learn;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

/// <summary>前台測驗作答業務邏輯實作，處理測驗取得、答案提交與成績計算</summary>
public class QuizTakeService(
    IUnitOfWork uow,
    ILogger<QuizTakeService> logger) : IQuizTakeService
{
    /// <inheritdoc />
    public async Task<QuizTakeViewModel?> GetQuizForTakeAsync(int quizId, CancellationToken ct = default)
    {
        logger.LogInformation("取得測驗作答資料 | QuizId={QuizId}", quizId);
        var quiz = await uow.Quizzes.GetWithQuestionsAsync(quizId, ct).ConfigureAwait(false);
        if (quiz is null)
        {
            logger.LogWarning("測驗不存在 | QuizId={QuizId}", quizId);
            return null;
        }

        var (lesson, course) = await GetQuizContextAsync(quiz.LessonId, ct).ConfigureAwait(false);

        return new QuizTakeViewModel
        {
            QuizId = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            PassingScore = quiz.PassingScore,
            TimeLimitMinutes = quiz.TimeLimitMinutes,
            TotalPoints = quiz.Questions.Sum(q => q.Points),
            LessonId = quiz.LessonId,
            LessonTitle = lesson?.Title,
            CourseId = course?.Id ?? 0,
            CourseTitle = course?.Title,
            Questions = quiz.Questions.Select(MapToQuestionItem).ToList()
        };
    }

    /// <inheritdoc />
    public async Task<ServiceResult<int>> SubmitQuizAsync(
        string userId, QuizSubmitModel model, CancellationToken ct = default)
    {
        logger.LogInformation("提交測驗 | UserId={UserId} | QuizId={QuizId}", userId, model.QuizId);

        var quiz = await uow.Quizzes.GetWithQuestionsAsync(model.QuizId, ct).ConfigureAwait(false);
        if (quiz is null)
            return ServiceResult<int>.Failure("測驗不存在");

        var attempt = GradeQuiz(userId, quiz, model);

        await uow.QuizAttempts.AddAsync(attempt, ct).ConfigureAwait(false);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("測驗提交完成 | AttemptId={AttemptId} | Score={Score}/{Total} | Passed={Passed}",
            attempt.Id, attempt.Score, attempt.TotalPoints, attempt.IsPassed);

        return ServiceResult<int>.Success(attempt.Id);
    }

    /// <inheritdoc />
    public async Task<QuizResultViewModel?> GetResultAsync(
        int attemptId, string userId, CancellationToken ct = default)
    {
        logger.LogInformation("查詢測驗結果 | AttemptId={AttemptId} | UserId={UserId}", attemptId, userId);
        var attempt = await uow.QuizAttempts.GetWithAnswersAsync(attemptId, ct).ConfigureAwait(false);
        if (attempt is null || attempt.UserId != userId)
        {
            logger.LogWarning("查詢測驗結果失敗：作答紀錄不存在或無權限 | AttemptId={AttemptId} | UserId={UserId}", attemptId, userId);
            return null;
        }

        var quiz = await uow.Quizzes.GetWithQuestionsAsync(attempt.QuizId, ct).ConfigureAwait(false);
        var (_, course) = await GetQuizContextAsync(quiz?.LessonId ?? 0, ct).ConfigureAwait(false);

        return new QuizResultViewModel
        {
            AttemptId = attempt.Id,
            QuizTitle = quiz?.Title ?? "",
            Score = attempt.Score,
            TotalPoints = attempt.TotalPoints,
            PassingScore = quiz?.PassingScore ?? 70,
            IsPassed = attempt.IsPassed,
            StartedAt = attempt.StartedAt,
            FinishedAt = attempt.FinishedAt,
            QuizId = attempt.QuizId,
            LessonId = quiz?.LessonId ?? 0,
            CourseId = course?.Id ?? 0,
            Answers = attempt.Answers.Select(MapToAnswerResult).ToList()
        };
    }

    // ── 私有輔助方法 ──

    /// <summary>反查測驗所屬的 Lesson 與 Course</summary>
    private async Task<(Lesson? lesson, Course? course)> GetQuizContextAsync(
        int lessonId, CancellationToken ct)
    {
        var lesson = await uow.Lessons.GetByIdAsync(lessonId, ct).ConfigureAwait(false);
        var sec = lesson is not null
            ? await uow.Sections.GetByIdAsync(lesson.SectionId, ct).ConfigureAwait(false)
            : null;
        var course = sec is not null
            ? await uow.Courses.GetByIdAsync(sec.CourseId, ct).ConfigureAwait(false)
            : null;
        return (lesson, course);
    }

    /// <summary>批改測驗並建立 QuizAttempt 紀錄</summary>
    private static QuizAttempt GradeQuiz(string userId, Quiz quiz, QuizSubmitModel model)
    {
        var attempt = new QuizAttempt
        {
            UserId = userId,
            QuizId = quiz.Id,
            StartedAt = DateTime.UtcNow,
            FinishedAt = DateTime.UtcNow
        };

        var totalScore = 0;
        var totalPoints = 0;

        foreach (var question in quiz.Questions)
        {
            totalPoints += question.Points;
            var userAnswer = model.Answers.GetValueOrDefault(question.Id, "");
            var (isCorrect, selectedOptionId) = GradeQuestion(question, userAnswer);
            var pointsEarned = isCorrect ? question.Points : 0;
            totalScore += pointsEarned;

            attempt.Answers.Add(new QuizAnswer
            {
                QuestionId = question.Id,
                SelectedOptionId = selectedOptionId,
                TextAnswer = question.Type == QuestionType.FillInBlank ? userAnswer : null,
                IsCorrect = isCorrect,
                PointsEarned = pointsEarned
            });
        }

        attempt.Score = totalScore;
        attempt.TotalPoints = totalPoints;
        attempt.IsPassed = totalPoints > 0 && totalScore * 100 / totalPoints >= quiz.PassingScore;
        return attempt;
    }

    /// <summary>批改單一題目，回傳是否正確與選項 ID</summary>
    private static (bool isCorrect, int? selectedOptionId) GradeQuestion(
        QuizQuestion question, string userAnswer)
    {
        return question.Type switch
        {
            QuestionType.SingleChoice => GradeSingleChoice(question, userAnswer),
            QuestionType.FillInBlank => (GradeFillInBlank(question, userAnswer), null),
            QuestionType.MultipleChoice => (GradeMultipleChoice(question, userAnswer), null),
            _ => (false, null)
        };
    }

    private static (bool, int?) GradeSingleChoice(QuizQuestion question, string userAnswer)
    {
        if (!int.TryParse(userAnswer, out var optId)) return (false, null);
        var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);
        return (correctOption?.Id == optId, optId);
    }

    private static bool GradeFillInBlank(QuizQuestion question, string userAnswer)
        => !string.IsNullOrWhiteSpace(question.CorrectAnswer) &&
           string.Equals(userAnswer.Trim(), question.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase);

    private static bool GradeMultipleChoice(QuizQuestion question, string userAnswer)
    {
        var selectedIds = userAnswer.Split(',')
            .Where(s => int.TryParse(s, out _))
            .Select(int.Parse)
            .ToHashSet();
        var correctIds = question.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToHashSet();
        return selectedIds.SetEquals(correctIds);
    }

    private static QuizQuestionItem MapToQuestionItem(QuizQuestion q) => new()
    {
        QuestionId = q.Id,
        Content = q.Content,
        Type = q.Type,
        Points = q.Points,
        Options = q.Options.Select(o => new QuizOptionItem
        {
            OptionId = o.Id,
            Content = o.Content
        }).ToList()
    };

    private static QuizAnswerResult MapToAnswerResult(QuizAnswer a) => new()
    {
        QuestionContent = a.Question.Content,
        QuestionType = a.Question.Type,
        Points = a.Question.Points,
        PointsEarned = a.PointsEarned,
        IsCorrect = a.IsCorrect,
        SelectedAnswer = a.SelectedOption?.Content ?? a.TextAnswer,
        CorrectAnswer = a.Question.Type == QuestionType.FillInBlank
            ? a.Question.CorrectAnswer
            : string.Join(", ", a.Question.Options.Where(o => o.IsCorrect).Select(o => o.Content))
    };
}
