using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Quiz;
using KoreanLearn.Web.Infrastructure;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

/// <summary>後台測驗管理 Controller，提供測驗與題目的 CRUD 操作</summary>
public class QuizController(IQuizAdminService quizAdminService, ILogger<QuizController> logger) : AdminBaseController
{
    /// <summary>測驗首頁，導向課程管理（測驗依附於單元管理）</summary>
    public IActionResult Index()
    {
        logger.LogInformation("管理員查看測驗列表（導向課程管理） | UserId={UserId}", GetCurrentUserId());
        return RedirectToAction("Index", "Course", new { area = "Admin" });
    }

    /// <summary>新增測驗表單頁（GET），依指定單元準備表單</summary>
    public async Task<IActionResult> Create(int lessonId, CancellationToken ct = default)
    {
        logger.LogInformation("管理員進入新增測驗頁面 | LessonId={LessonId} | UserId={UserId}", lessonId, GetCurrentUserId());
        var vm = await quizAdminService.PrepareCreateFormAsync(lessonId, ct);
        return View(vm);
    }

    /// <summary>新增測驗（POST），成功後導向測驗詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(QuizFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("管理員新增測驗失敗：模型驗證錯誤 | LessonId={LessonId} | UserId={UserId}", vm.LessonId, GetCurrentUserId());
            return View(vm);
        }

        var result = await quizAdminService.CreateQuizAsync(vm, ct);
        if (result is { IsSuccess: true, Data: var quizId })
        {
            logger.LogInformation("管理員新增測驗成功 | QuizId={QuizId} | LessonId={LessonId} | UserId={UserId}", quizId, vm.LessonId, GetCurrentUserId());
            TempData[TempDataKeys.Success] = "測驗建立成功";
            return RedirectToAction(nameof(Detail), new { id = quizId });
        }

        logger.LogWarning("管理員新增測驗失敗 | LessonId={LessonId} | Error={Error} | UserId={UserId}", vm.LessonId, result.ErrorMessage, GetCurrentUserId());
        ModelState.AddModelError("", result.ErrorMessage ?? "建立失敗");
        return View(vm);
    }

    /// <summary>測驗詳情頁，顯示測驗資訊與所有題目</summary>
    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        logger.LogInformation("管理員查看測驗詳情 | QuizId={QuizId} | UserId={UserId}", id, GetCurrentUserId());
        var vm = await quizAdminService.GetQuizDetailAsync(id, ct);
        if (vm is null)
        {
            logger.LogWarning("管理員查看測驗失敗：資料不存在 | QuizId={QuizId} | UserId={UserId}", id, GetCurrentUserId());
            return NotFound();
        }
        return View(vm);
    }

    /// <summary>編輯測驗表單頁（GET），載入現有測驗資料</summary>
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        logger.LogInformation("管理員進入編輯測驗頁面 | QuizId={QuizId} | UserId={UserId}", id, GetCurrentUserId());
        var vm = await quizAdminService.GetQuizForEditAsync(id, ct);
        if (vm is null)
        {
            logger.LogWarning("管理員查看測驗失敗：資料不存在 | QuizId={QuizId} | UserId={UserId}", id, GetCurrentUserId());
            return NotFound();
        }
        return View(vm);
    }

    /// <summary>更新測驗（POST），成功後導回測驗詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(QuizFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("管理員更新測驗失敗：模型驗證錯誤 | QuizId={QuizId} | UserId={UserId}", vm.Id, GetCurrentUserId());
            return View(vm);
        }

        var result = await quizAdminService.UpdateQuizAsync(vm, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("管理員更新測驗成功 | QuizId={QuizId} | UserId={UserId}", vm.Id, GetCurrentUserId());
            TempData[TempDataKeys.Success] = "測驗更新成功";
            return RedirectToAction(nameof(Detail), new { id = vm.Id });
        }

        logger.LogWarning("管理員更新測驗失敗 | QuizId={QuizId} | Error={Error} | UserId={UserId}", vm.Id, result.ErrorMessage, GetCurrentUserId());
        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    /// <summary>刪除測驗（POST，軟刪除），完成後導回課程詳情或課程列表</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int? courseId, CancellationToken ct = default)
    {
        var result = await quizAdminService.DeleteQuizAsync(id, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("管理員刪除測驗 | QuizId={QuizId} | UserId={UserId}", id, GetCurrentUserId());
            TempData[TempDataKeys.Success] = "測驗已刪除";
        }
        else
        {
            logger.LogWarning("管理員刪除測驗失敗 | QuizId={QuizId} | Error={Error} | UserId={UserId}", id, result.ErrorMessage, GetCurrentUserId());
            TempData[TempDataKeys.Error] = result.ErrorMessage ?? "刪除失敗";
        }

        if (courseId.HasValue)
            return RedirectToAction("Detail", "Course", new { area = "Admin", id = courseId });
        return RedirectToAction("Index", "Course", new { area = "Admin" });
    }

    // ── 題目管理 ──

    /// <summary>新增題目表單頁（GET），預設 4 個選項欄位</summary>
    public IActionResult AddQuestion(int quizId, int? courseId)
    {
        logger.LogInformation("管理員進入新增題目頁面 | QuizId={QuizId} | UserId={UserId}", quizId, GetCurrentUserId());
        var vm = new QuestionFormViewModel
        {
            QuizId = quizId,
            CourseId = courseId,
            Points = 1,
            Options = [new(), new(), new(), new()] // 4 empty options by default
        };
        return View(vm);
    }

    /// <summary>新增題目（POST），成功後導回測驗詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuestion(QuestionFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("管理員新增題目失敗：模型驗證錯誤 | QuizId={QuizId} | UserId={UserId}", vm.QuizId, GetCurrentUserId());
            return View(vm);
        }

        var result = await quizAdminService.AddQuestionAsync(vm, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("管理員新增題目成功 | QuizId={QuizId} | UserId={UserId}", vm.QuizId, GetCurrentUserId());
            TempData[TempDataKeys.Success] = "題目新增成功";
            return RedirectToAction(nameof(Detail), new { id = vm.QuizId });
        }

        logger.LogWarning("管理員新增題目失敗 | QuizId={QuizId} | Error={Error} | UserId={UserId}", vm.QuizId, result.ErrorMessage, GetCurrentUserId());
        ModelState.AddModelError("", result.ErrorMessage ?? "新增失敗");
        return View(vm);
    }

    /// <summary>編輯題目表單頁（GET），載入題目與選項資料（至少 4 個選項）</summary>
    public async Task<IActionResult> EditQuestion(int id, CancellationToken ct = default)
    {
        logger.LogInformation("管理員進入編輯題目頁面 | QuestionId={QuestionId} | UserId={UserId}", id, GetCurrentUserId());
        var vm = await quizAdminService.GetQuestionForEditAsync(id, ct);
        if (vm is null)
        {
            logger.LogWarning("管理員查看題目失敗：資料不存在 | QuestionId={QuestionId} | UserId={UserId}", id, GetCurrentUserId());
            return NotFound();
        }
        // Ensure at least 4 options
        while (vm.Options.Count < 4) vm.Options.Add(new OptionFormViewModel());
        return View(vm);
    }

    /// <summary>更新題目（POST），成功後導回測驗詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditQuestion(QuestionFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("管理員更新題目失敗：模型驗證錯誤 | QuestionId={QuestionId} | QuizId={QuizId} | UserId={UserId}", vm.Id, vm.QuizId, GetCurrentUserId());
            return View(vm);
        }

        var result = await quizAdminService.UpdateQuestionAsync(vm, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("管理員更新題目成功 | QuestionId={QuestionId} | QuizId={QuizId} | UserId={UserId}", vm.Id, vm.QuizId, GetCurrentUserId());
            TempData[TempDataKeys.Success] = "題目更新成功";
            return RedirectToAction(nameof(Detail), new { id = vm.QuizId });
        }

        logger.LogWarning("管理員更新題目失敗 | QuestionId={QuestionId} | Error={Error} | UserId={UserId}", vm.Id, result.ErrorMessage, GetCurrentUserId());
        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    /// <summary>刪除題目（POST），完成後導回測驗詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestion(int id, int quizId, CancellationToken ct = default)
    {
        var result = await quizAdminService.DeleteQuestionAsync(id, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("管理員刪除題目 | QuestionId={QuestionId} | QuizId={QuizId} | UserId={UserId}", id, quizId, GetCurrentUserId());
            TempData[TempDataKeys.Success] = "題目已刪除";
        }
        else
        {
            logger.LogWarning("管理員刪除題目失敗 | QuestionId={QuestionId} | QuizId={QuizId} | Error={Error} | UserId={UserId}", id, quizId, result.ErrorMessage, GetCurrentUserId());
            TempData[TempDataKeys.Error] = result.ErrorMessage ?? "刪除失敗";
        }

        return RedirectToAction(nameof(Detail), new { id = quizId });
    }
}
