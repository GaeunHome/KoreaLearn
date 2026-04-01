using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Learn;
using KoreanLearn.Web.Infrastructure;

namespace KoreanLearn.Web.Areas.Learn.Controllers;

/// <summary>測驗作答 Controller，提供測驗作答介面、提交答案與成績檢視</summary>
public class QuizController(
    IQuizTakeService quizTakeService,
    ILogger<QuizController> logger) : LearnBaseController
{
    private const string AnswerFieldPrefix = "q_";

    /// <summary>測驗作答頁面，顯示所有題目（選擇題/填空題）供學生作答</summary>
    public async Task<IActionResult> Take(int id, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        logger.LogInformation("學生開始作答測驗 | QuizId={QuizId} | UserId={UserId}", id, userId);
        var vm = await quizTakeService.GetQuizForTakeAsync(id, ct);
        if (vm is null)
        {
            logger.LogWarning("學生查看測驗失敗：資料不存在 | QuizId={QuizId} | UserId={UserId}", id, userId);
            return NotFound();
        }
        return View(vm);
    }

    /// <summary>提交測驗答案（POST），計算成績後導向成績頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int id, IFormCollection form, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();

        var model = new QuizSubmitModel { QuizId = id };
        foreach (var key in form.Keys)
        {
            if (key.StartsWith(AnswerFieldPrefix) && int.TryParse(key[AnswerFieldPrefix.Length..], out var questionId))
            {
                model.Answers[questionId] = form[key].ToString();
            }
        }

        var result = await quizTakeService.SubmitQuizAsync(userId, model, ct);
        if (result is { IsSuccess: true, Data: var attemptId })
        {
            logger.LogInformation("學生提交測驗成功 | QuizId={QuizId} | AttemptId={AttemptId} | UserId={UserId}", id, attemptId, userId);
            return RedirectToAction(nameof(Result), new { id = attemptId });
        }

        logger.LogWarning("學生提交測驗失敗 | QuizId={QuizId} | Error={Error} | UserId={UserId}", id, result.ErrorMessage, userId);
        TempData[TempDataKeys.Error] = result.ErrorMessage ?? "提交失敗";
        return RedirectToAction(nameof(Take), new { id });
    }

    /// <summary>測驗成績頁面，顯示得分、答對率與各題詳細結果</summary>
    public async Task<IActionResult> Result(int id, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        logger.LogInformation("學生查看測驗成績 | AttemptId={AttemptId} | UserId={UserId}", id, userId);
        var vm = await quizTakeService.GetResultAsync(id, userId, ct);
        if (vm is null)
        {
            logger.LogWarning("學生查看測驗成績失敗：資料不存在 | AttemptId={AttemptId} | UserId={UserId}", id, userId);
            return NotFound();
        }
        return View(vm);
    }
}
