using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Learn;
using KoreanLearn.Web.Infrastructure;

namespace KoreanLearn.Web.Areas.Learn.Controllers;

/// <summary>測驗作答 Controller，提供測驗作答介面、提交答案與成績檢視</summary>
public class QuizController(IQuizTakeService quizTakeService) : LearnBaseController
{
    /// <summary>測驗作答頁面，顯示所有題目（選擇題/填空題）供學生作答</summary>
    public async Task<IActionResult> Take(int id, CancellationToken ct = default)
    {
        var vm = await quizTakeService.GetQuizForTakeAsync(id, ct);
        if (vm is null) return NotFound();
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
            if (key.StartsWith("q_") && int.TryParse(key[2..], out var questionId))
            {
                model.Answers[questionId] = form[key].ToString();
            }
        }

        var result = await quizTakeService.SubmitQuizAsync(userId, model, ct);
        if (result is { IsSuccess: true, Data: var attemptId })
            return RedirectToAction(nameof(Result), new { id = attemptId });

        TempData[TempDataKeys.Error] = result.ErrorMessage ?? "提交失敗";
        return RedirectToAction(nameof(Take), new { id });
    }

    /// <summary>測驗成績頁面，顯示得分、答對率與各題詳細結果</summary>
    public async Task<IActionResult> Result(int id, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        var vm = await quizTakeService.GetResultAsync(id, userId, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }
}
