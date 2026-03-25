using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Service.Services.Interfaces;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KoreanLearn.Service.Services.Implementation;

public class CertificateService(
    IUnitOfWork uow,
    ILogger<CertificateService> logger) : ICertificateService
{
    public async Task<CertificateEligibility> CheckEligibilityAsync(
        string userId, int courseId, CancellationToken ct = default)
    {
        var course = await uow.Courses.GetWithSectionsAndLessonsAsync(courseId, ct).ConfigureAwait(false);
        if (course is null)
            return new CertificateEligibility { Reason = "課程不存在" };

        var totalLessons = course.Sections.SelectMany(s => s.Lessons).Count();
        var progresses = await uow.Progresses.GetByUserAndCourseAsync(userId, courseId, ct).ConfigureAwait(false);
        var completedLessons = progresses.Count(p => p.IsCompleted);

        var allCompleted = completedLessons >= totalLessons && totalLessons > 0;

        // Check quiz score (best attempt)
        int? bestScore = null;
        var passingScore = 70;
        var quizLessons = course.Sections.SelectMany(s => s.Lessons).Where(l => l.Quiz is not null);
        // Simplified: check if any quiz attempt passed
        var quizPassed = true; // Default pass if no quiz

        foreach (var lesson in course.Sections.SelectMany(s => s.Lessons))
        {
            var quiz = await uow.Quizzes.GetByLessonIdAsync(lesson.Id, ct).ConfigureAwait(false);
            if (quiz is null) continue;

            var attempts = await uow.QuizAttempts.GetByUserAndQuizAsync(userId, quiz.Id, ct).ConfigureAwait(false);
            if (!attempts.Any())
            {
                quizPassed = false;
                break;
            }

            var best = attempts.Max(a => a.TotalPoints > 0 ? a.Score * 100 / a.TotalPoints : 0);
            bestScore = best;
            if (best < passingScore)
            {
                quizPassed = false;
                break;
            }
        }

        var isEligible = allCompleted && quizPassed;
        return new CertificateEligibility
        {
            IsEligible = isEligible,
            Reason = !allCompleted ? "尚未完成所有單元" : !quizPassed ? "測驗成績未達及格" : null,
            CompletedLessons = completedLessons,
            TotalLessons = totalLessons,
            QuizScore = bestScore,
            PassingScore = passingScore
        };
    }

    public async Task<byte[]?> GenerateCertificateAsync(
        string userId, int courseId, CancellationToken ct = default)
    {
        var eligibility = await CheckEligibilityAsync(userId, courseId, ct).ConfigureAwait(false);
        if (!eligibility.IsEligible) return null;

        var course = await uow.Courses.GetByIdAsync(courseId, ct).ConfigureAwait(false);
        if (course is null) return null;

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(50);

                page.Content().Column(col =>
                {
                    col.Spacing(20);

                    col.Item().AlignCenter().Text("KoreanLearn").FontSize(16).FontColor(Colors.Grey.Medium);

                    col.Item().AlignCenter().Text("修課證書").FontSize(36).Bold().FontColor(Colors.Blue.Darken2);

                    col.Item().AlignCenter().PaddingTop(30).Text("茲證明").FontSize(14).FontColor(Colors.Grey.Darken1);

                    col.Item().AlignCenter().Text($"User: {userId}").FontSize(20).Bold();

                    col.Item().AlignCenter().Text("已完成以下課程之所有學習內容與測驗").FontSize(14).FontColor(Colors.Grey.Darken1);

                    col.Item().AlignCenter().PaddingTop(10).Text(course.Title).FontSize(24).Bold().FontColor(Colors.Blue.Darken1);

                    col.Item().AlignCenter().PaddingTop(40).Text($"發證日期：{DateTime.UtcNow:yyyy 年 MM 月 dd 日}").FontSize(12).FontColor(Colors.Grey.Medium);

                    col.Item().AlignCenter().Text($"證書編號：CERT-{courseId}-{userId[..Math.Min(8, userId.Length)]}-{DateTime.UtcNow:yyyyMMdd}").FontSize(10).FontColor(Colors.Grey.Medium);
                });
            });
        });

        logger.LogInformation("生成證書 | UserId={UserId} | CourseId={CourseId}", userId, courseId);
        return document.GeneratePdf();
    }
}
