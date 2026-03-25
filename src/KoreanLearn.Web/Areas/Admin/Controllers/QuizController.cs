using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Quiz;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class QuizController(IQuizAdminService quizAdminService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        // Redirect to course management - quizzes are managed per-lesson
        return RedirectToAction("Index", "Course", new { area = "Admin" });
    }

    public async Task<IActionResult> Create(int lessonId, CancellationToken ct = default)
    {
        var vm = await quizAdminService.PrepareCreateFormAsync(lessonId, ct);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(QuizFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await quizAdminService.CreateQuizAsync(vm, ct);
        if (result is { IsSuccess: true, Data: var quizId })
        {
            TempData["Success"] = "測驗建立成功";
            return RedirectToAction(nameof(Detail), new { id = quizId });
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "建立失敗");
        return View(vm);
    }

    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        var vm = await quizAdminService.GetQuizDetailAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var vm = await quizAdminService.GetQuizForEditAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(QuizFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await quizAdminService.UpdateQuizAsync(vm, ct);
        if (result.IsSuccess)
        {
            TempData["Success"] = "測驗更新成功";
            return RedirectToAction(nameof(Detail), new { id = vm.Id });
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int? courseId, CancellationToken ct = default)
    {
        var result = await quizAdminService.DeleteQuizAsync(id, ct);
        if (result.IsSuccess)
            TempData["Success"] = "測驗已刪除";
        else
            TempData["Error"] = result.ErrorMessage ?? "刪除失敗";

        if (courseId.HasValue)
            return RedirectToAction("Detail", "Course", new { area = "Admin", id = courseId });
        return RedirectToAction("Index", "Course", new { area = "Admin" });
    }

    // ── Question Management ──

    public IActionResult AddQuestion(int quizId, int? courseId)
    {
        var vm = new QuestionFormViewModel
        {
            QuizId = quizId,
            CourseId = courseId,
            Points = 1,
            Options = [new(), new(), new(), new()] // 4 empty options by default
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuestion(QuestionFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await quizAdminService.AddQuestionAsync(vm, ct);
        if (result.IsSuccess)
        {
            TempData["Success"] = "題目新增成功";
            return RedirectToAction(nameof(Detail), new { id = vm.QuizId });
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "新增失敗");
        return View(vm);
    }

    public async Task<IActionResult> EditQuestion(int id, CancellationToken ct = default)
    {
        var vm = await quizAdminService.GetQuestionForEditAsync(id, ct);
        if (vm is null) return NotFound();
        // Ensure at least 4 options
        while (vm.Options.Count < 4) vm.Options.Add(new OptionFormViewModel());
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditQuestion(QuestionFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await quizAdminService.UpdateQuestionAsync(vm, ct);
        if (result.IsSuccess)
        {
            TempData["Success"] = "題目更新成功";
            return RedirectToAction(nameof(Detail), new { id = vm.QuizId });
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestion(int id, int quizId, CancellationToken ct = default)
    {
        var result = await quizAdminService.DeleteQuestionAsync(id, ct);
        if (result.IsSuccess)
            TempData["Success"] = "題目已刪除";
        else
            TempData["Error"] = result.ErrorMessage ?? "刪除失敗";

        return RedirectToAction(nameof(Detail), new { id = quizId });
    }
}
