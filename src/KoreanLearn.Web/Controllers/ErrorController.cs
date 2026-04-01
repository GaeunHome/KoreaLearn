using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Web.Controllers;

/// <summary>錯誤頁面 Controller，處理各種 HTTP 狀態碼對應的錯誤頁面</summary>
public class ErrorController(ILogger<ErrorController> logger) : BaseController
{
    /// <summary>500 通用錯誤頁面</summary>
    [Route("Error")]
    public IActionResult Index()
    {
        logger.LogWarning("觸發錯誤頁面 | StatusCode=500 | Path={Path} | UserId={UserId}",
            HttpContext.Request.Path, GetCurrentUserId());
        Response.StatusCode = 500;
        return View("Error500");
    }

    /// <summary>404 找不到頁面</summary>
    [Route("Error/NotFound")]
    public IActionResult NotFoundPage()
    {
        logger.LogWarning("觸發 404 頁面 | Path={Path} | UserId={UserId}",
            HttpContext.Request.Path, GetCurrentUserId());
        Response.StatusCode = 404;
        return View("Error404");
    }

    /// <summary>依 HTTP 狀態碼顯示對應錯誤頁面（404、403、其他歸為 500）</summary>
    [Route("Error/{statusCode:int}")]
    public IActionResult StatusCodePage(int statusCode)
    {
        logger.LogWarning("觸發錯誤頁面 | StatusCode={StatusCode} | Path={Path} | UserId={UserId}",
            statusCode, HttpContext.Request.Path, GetCurrentUserId());
        Response.StatusCode = statusCode;
        return statusCode switch
        {
            404 => View("Error404"),
            403 => View("Error403"),
            409 => View("Error409"),
            _ => View("Error500")
        };
    }
}
