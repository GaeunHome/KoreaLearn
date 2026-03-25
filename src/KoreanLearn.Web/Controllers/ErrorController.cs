using Microsoft.AspNetCore.Mvc;

namespace KoreanLearn.Web.Controllers;

public class ErrorController : Controller
{
    [Route("Error")]
    public IActionResult Index()
    {
        Response.StatusCode = 500;
        return View("Error500");
    }

    [Route("Error/NotFound")]
    public IActionResult NotFoundPage()
    {
        Response.StatusCode = 404;
        return View("Error404");
    }

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
