using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Learn;

namespace KoreanLearn.Web.Areas.Learn.Controllers;

[Area("Learn")]
[Authorize]
public class QuizController(IQuizTakeService quizTakeService) : Controller
{
    public async Task<IActionResult> Take(int id, CancellationToken ct = default)
    {
        var vm = await quizTakeService.GetQuizForTakeAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int id, IFormCollection form, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

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

        TempData["Error"] = result.ErrorMessage ?? "提交失敗";
        return RedirectToAction(nameof(Take), new { id });
    }

    public async Task<IActionResult> Result(int id, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var vm = await quizTakeService.GetResultAsync(id, userId, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }
}
