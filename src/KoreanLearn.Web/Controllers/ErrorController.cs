using Microsoft.AspNetCore.Mvc;

namespace KoreanLearn.Web.Controllers;

/// <summary>錯誤頁面 Controller，處理各種 HTTP 狀態碼對應的錯誤頁面</summary>
public class ErrorController : Controller
{
    /// <summary>500 通用錯誤頁面</summary>
    [Route("Error")]
    public IActionResult Index()
    {
        Response.StatusCode = 500;
        return View("Error500");
    }

    /// <summary>404 找不到頁面</summary>
    [Route("Error/NotFound")]
    public IActionResult NotFoundPage()
    {
        Response.StatusCode = 404;
        return View("Error404");
    }

    /// <summary>依 HTTP 狀態碼顯示對應錯誤頁面（404、403、其他歸為 500）</summary>
    [Route("Error/{statusCode:int}")]
    public IActionResult StatusCodePage(int statusCode)
    {
        Response.StatusCode = statusCode;
        return statusCode switch
        {
            404 => View("Error404"),
            403 => View("Error403"),
            _ => View("Error500")
        };
    }
}
