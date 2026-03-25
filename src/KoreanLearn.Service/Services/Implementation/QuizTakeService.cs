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
        var quiz = await uow.Quizzes.GetWithQuestionsAsync(quizId, ct).ConfigureAwait(false);
        if (quiz is null) return null;

        // 反查所屬課程資訊以供頁面導覽
        var lesson = await uow.Lessons.GetByIdAsync(quiz.LessonId, ct).ConfigureAwait(false);
        var sec = lesson is not null
            ? await uow.Sections.GetByIdAsync(lesson.SectionId, ct).ConfigureAwait(false)
            : null;
        var course = sec is not null
            ? await uow.Courses.GetByIdAsync(sec.CourseId, ct).ConfigureAwait(false)
            : null;

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
            Questions = quiz.Questions.Select(q => new QuizQuestionItem
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
            }).ToList()
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

        var attempt = new QuizAttempt
        {
            UserId = userId,
            QuizId = quiz.Id,
            StartedAt = DateTime.UtcNow,
            FinishedAt = DateTime.UtcNow
        };

        var totalScore = 0;
        var totalPoints = 0;

        // 逐題批改
        foreach (var question in quiz.Questions)
        {
            totalPoints += question.Points;
            var userAnswer = model.Answers.GetValueOrDefault(question.Id, "");
            var isCorrect = false;
            int? selectedOptionId = null;
            var pointsEarned = 0;

            switch (question.Type)
            {
                // 單選題：比對使用者選擇的選項 ID 與正確選項
                case QuestionType.SingleChoice:
                    if (int.TryParse(userAnswer, out var optId))
                    {
                        selectedOptionId = optId;
                        var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);
                        isCorrect = correctOption?.Id == optId;
                    }
                    break;

                // 填空題：不分大小寫比對使用者答案與正確答案
                case QuestionType.FillInBlank:
                    isCorrect = !string.IsNullOrWhiteSpace(question.CorrectAnswer) &&
                                string.Equals(userAnswer.Trim(), question.CorrectAnswer.Trim(),
                                    StringComparison.OrdinalIgnoreCase);
                    break;

                // 多選題：使用者選擇的選項集合必須與正確選項集合完全一致
                case QuestionType.MultipleChoice:
                    var selectedIds = userAnswer.Split(',')
                        .Where(s => int.TryParse(s, out _))
                        .Select(int.Parse)
                        .ToHashSet();
                    var correctIds = question.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToHashSet();
                    isCorrect = selectedIds.SetEquals(correctIds);
                    break;
            }

            if (isCorrect) pointsEarned = question.Points;
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

        await uow.QuizAttempts.AddAsync(attempt, ct).ConfigureAwait(false);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("測驗提交完成 | AttemptId={AttemptId} | Score={Score}/{Total} | Passed={Passed}",
            attempt.Id, totalScore, totalPoints, attempt.IsPassed);

        return ServiceResult<int>.Success(attempt.Id);
    }

    /// <inheritdoc />
    public async Task<QuizResultViewModel?> GetResultAsync(
        int attemptId, string userId, CancellationToken ct = default)
    {
        var attempt = await uow.QuizAttempts.GetWithAnswersAsync(attemptId, ct).ConfigureAwait(false);
        if (attempt is null || attempt.UserId != userId) return null;

        var quiz = await uow.Quizzes.GetWithQuestionsAsync(attempt.QuizId, ct).ConfigureAwait(false);
        var lesson = quiz is not null
            ? await uow.Lessons.GetByIdAsync(quiz.LessonId, ct).ConfigureAwait(false)
            : null;
        var sec = lesson is not null
            ? await uow.Sections.GetByIdAsync(lesson.SectionId, ct).ConfigureAwait(false)
            : null;
        var course = sec is not null
            ? await uow.Courses.GetByIdAsync(sec.CourseId, ct).ConfigureAwait(false)
            : null;

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
            Answers = attempt.Answers.Select(a => new QuizAnswerResult
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
            }).ToList()
        };
    }
}
