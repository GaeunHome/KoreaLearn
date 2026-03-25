using KoreanLearn.Data.Entities;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Enums;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Quiz;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

public class QuizAdminService(
    IUnitOfWork uow,
    ILogger<QuizAdminService> logger) : IQuizAdminService
{
    public async Task<QuizDetailViewModel?> GetQuizDetailAsync(int quizId, CancellationToken ct = default)
    {
        var quiz = await uow.Quizzes.GetWithQuestionsAsync(quizId, ct).ConfigureAwait(false);
        if (quiz is null) return null;

        var lesson = await uow.Lessons.GetByIdAsync(quiz.LessonId, ct).ConfigureAwait(false);
        var section = lesson is not null
            ? await uow.Sections.GetByIdAsync(lesson.SectionId, ct).ConfigureAwait(false)
            : null;
        var course = section is not null
            ? await uow.Courses.GetByIdAsync(section.CourseId, ct).ConfigureAwait(false)
            : null;

        return new QuizDetailViewModel
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            PassingScore = quiz.PassingScore,
            TimeLimitMinutes = quiz.TimeLimitMinutes,
            LessonId = quiz.LessonId,
            LessonTitle = lesson?.Title,
            CourseTitle = course?.Title,
            CourseId = course?.Id,
            Questions = quiz.Questions.Select(q => new QuestionViewModel
            {
                Id = q.Id,
                Content = q.Content,
                Type = q.Type switch
                {
                    QuestionType.SingleChoice => "單選題",
                    QuestionType.MultipleChoice => "多選題",
                    QuestionType.FillInBlank => "填空題",
                    _ => "未知"
                },
                TypeValue = (int)q.Type,
                Points = q.Points,
                SortOrder = q.SortOrder,
                CorrectAnswer = q.CorrectAnswer,
                Options = q.Options.Select(o => new OptionViewModel
                {
                    Id = o.Id,
                    Content = o.Content,
                    IsCorrect = o.IsCorrect,
                    SortOrder = o.SortOrder
                }).ToList()
            }).ToList()
        };
    }

    public async Task<QuizFormViewModel?> GetQuizForEditAsync(int quizId, CancellationToken ct = default)
    {
        var quiz = await uow.Quizzes.GetByIdAsync(quizId, ct).ConfigureAwait(false);
        if (quiz is null) return null;

        var lesson = await uow.Lessons.GetByIdAsync(quiz.LessonId, ct).ConfigureAwait(false);
        var section = lesson is not null
            ? await uow.Sections.GetByIdAsync(lesson.SectionId, ct).ConfigureAwait(false)
            : null;
        var course = section is not null
            ? await uow.Courses.GetByIdAsync(section.CourseId, ct).ConfigureAwait(false)
            : null;

        return new QuizFormViewModel
        {
            Id = quiz.Id,
            LessonId = quiz.LessonId,
            Title = quiz.Title,
            Description = quiz.Description,
            PassingScore = quiz.PassingScore,
            TimeLimitMinutes = quiz.TimeLimitMinutes,
            LessonTitle = lesson?.Title,
            CourseTitle = course?.Title,
            CourseId = course?.Id
        };
    }

    public async Task<QuizFormViewModel> PrepareCreateFormAsync(int lessonId, CancellationToken ct = default)
    {
        var lesson = await uow.Lessons.GetByIdAsync(lessonId, ct).ConfigureAwait(false);
        var section = lesson is not null
            ? await uow.Sections.GetByIdAsync(lesson.SectionId, ct).ConfigureAwait(false)
            : null;
        var course = section is not null
            ? await uow.Courses.GetByIdAsync(section.CourseId, ct).ConfigureAwait(false)
            : null;

        return new QuizFormViewModel
        {
            LessonId = lessonId,
            LessonTitle = lesson?.Title,
            CourseTitle = course?.Title,
            CourseId = course?.Id,
            PassingScore = 70
        };
    }

    public async Task<ServiceResult<int>> CreateQuizAsync(QuizFormViewModel vm, CancellationToken ct = default)
    {
        logger.LogInformation("建立測驗 | LessonId={LessonId} | Title={Title}", vm.LessonId, vm.Title);

        var existing = await uow.Quizzes.GetByLessonIdAsync(vm.LessonId, ct).ConfigureAwait(false);
        if (existing is not null)
            return ServiceResult<int>.Failure("此單元已有測驗");

        var quiz = new Quiz
        {
            LessonId = vm.LessonId,
            Title = vm.Title,
            Description = vm.Description,
            PassingScore = vm.PassingScore,
            TimeLimitMinutes = vm.TimeLimitMinutes
        };

        await uow.Quizzes.AddAsync(quiz, ct).ConfigureAwait(false);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("測驗建立成功 | QuizId={QuizId}", quiz.Id);
        return ServiceResult<int>.Success(quiz.Id);
    }

    public async Task<ServiceResult> UpdateQuizAsync(QuizFormViewModel vm, CancellationToken ct = default)
    {
        var quiz = await uow.Quizzes.GetByIdAsync(vm.Id, ct).ConfigureAwait(false);
        if (quiz is null) return ServiceResult.Failure("測驗不存在");

        quiz.Title = vm.Title;
        quiz.Description = vm.Description;
        quiz.PassingScore = vm.PassingScore;
        quiz.TimeLimitMinutes = vm.TimeLimitMinutes;

        uow.Quizzes.Update(quiz);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteQuizAsync(int quizId, CancellationToken ct = default)
    {
        var quiz = await uow.Quizzes.GetByIdAsync(quizId, ct).ConfigureAwait(false);
        if (quiz is null) return ServiceResult.Failure("測驗不存在");

        uow.Quizzes.Remove(quiz);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("測驗刪除成功（軟刪除）| QuizId={QuizId}", quizId);
        return ServiceResult.Success();
    }

    // ── Question ──────────────────────────────────────

    public async Task<QuestionFormViewModel?> GetQuestionForEditAsync(int questionId, CancellationToken ct = default)
    {
        var question = await uow.Quizzes.GetQuestionByIdAsync(questionId, ct).ConfigureAwait(false);
        if (question?.Quiz is null) return null;

        var lesson = await uow.Lessons.GetByIdAsync(question.Quiz.LessonId, ct).ConfigureAwait(false);
        var section = lesson is not null
            ? await uow.Sections.GetByIdAsync(lesson.SectionId, ct).ConfigureAwait(false)
            : null;
        var course = section is not null
            ? await uow.Courses.GetByIdAsync(section.CourseId, ct).ConfigureAwait(false)
            : null;

        return new QuestionFormViewModel
        {
            Id = question.Id,
            QuizId = question.Quiz.Id,
            Content = question.Content,
            Type = question.Type,
            Points = question.Points,
            SortOrder = question.SortOrder,
            CorrectAnswer = question.CorrectAnswer,
            CourseId = course?.Id,
            Options = question.Options.Select(o => new OptionFormViewModel
            {
                Id = o.Id,
                Content = o.Content,
                IsCorrect = o.IsCorrect,
                SortOrder = o.SortOrder
            }).ToList()
        };
    }

    public async Task<ServiceResult<int>> AddQuestionAsync(QuestionFormViewModel vm, CancellationToken ct = default)
    {
        logger.LogInformation("新增題目 | QuizId={QuizId} | Type={Type}", vm.QuizId, vm.Type);

        var quiz = await uow.Quizzes.GetWithQuestionsAsync(vm.QuizId, ct).ConfigureAwait(false);
        if (quiz is null) return ServiceResult<int>.Failure("測驗不存在");

        var question = new QuizQuestion
        {
            QuizId = vm.QuizId,
            Content = vm.Content,
            Type = vm.Type,
            Points = vm.Points,
            SortOrder = vm.SortOrder,
            CorrectAnswer = vm.Type == QuestionType.FillInBlank ? vm.CorrectAnswer : null
        };

        if (vm.Type is QuestionType.SingleChoice or QuestionType.MultipleChoice)
        {
            foreach (var opt in vm.Options.Where(o => !string.IsNullOrWhiteSpace(o.Content)))
            {
                question.Options.Add(new QuizOption
                {
                    Content = opt.Content,
                    IsCorrect = opt.IsCorrect,
                    SortOrder = opt.SortOrder
                });
            }
        }

        quiz.Questions.Add(question);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("題目新增成功 | QuestionId={QuestionId}", question.Id);
        return ServiceResult<int>.Success(question.Id);
    }

    public async Task<ServiceResult> UpdateQuestionAsync(QuestionFormViewModel vm, CancellationToken ct = default)
    {
        var quiz = await uow.Quizzes.GetWithQuestionsAsync(vm.QuizId, ct).ConfigureAwait(false);
        if (quiz is null) return ServiceResult.Failure("測驗不存在");

        var question = quiz.Questions.FirstOrDefault(q => q.Id == vm.Id);
        if (question is null) return ServiceResult.Failure("題目不存在");

        question.Content = vm.Content;
        question.Type = vm.Type;
        question.Points = vm.Points;
        question.SortOrder = vm.SortOrder;
        question.CorrectAnswer = vm.Type == QuestionType.FillInBlank ? vm.CorrectAnswer : null;

        // Update options for choice questions
        if (vm.Type is QuestionType.SingleChoice or QuestionType.MultipleChoice)
        {
            // Remove old options
            question.Options.Clear();

            // Add new options
            foreach (var opt in vm.Options.Where(o => !string.IsNullOrWhiteSpace(o.Content)))
            {
                question.Options.Add(new QuizOption
                {
                    Content = opt.Content,
                    IsCorrect = opt.IsCorrect,
                    SortOrder = opt.SortOrder
                });
            }
        }

        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteQuestionAsync(int questionId, CancellationToken ct = default)
    {
        // Find the quiz containing this question
        var allQuizzes = await uow.Quizzes.GetAllAsync(ct).ConfigureAwait(false);
        foreach (var q in allQuizzes)
        {
            var quiz = await uow.Quizzes.GetWithQuestionsAsync(q.Id, ct).ConfigureAwait(false);
            var question = quiz?.Questions.FirstOrDefault(qq => qq.Id == questionId);
            if (question is null) continue;

            quiz!.Questions.Remove(question);
            await uow.SaveChangesAsync(ct).ConfigureAwait(false);

            logger.LogInformation("題目刪除成功 | QuestionId={QuestionId}", questionId);
            return ServiceResult.Success();
        }

        return ServiceResult.Failure("題目不存在");
    }
}
