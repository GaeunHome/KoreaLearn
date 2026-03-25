using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KoreanLearn.Web.Areas.Learn.Controllers;

[Area("Learn")]
[Authorize]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
