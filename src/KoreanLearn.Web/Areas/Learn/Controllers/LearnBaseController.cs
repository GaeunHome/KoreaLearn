using KoreanLearn.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KoreanLearn.Web.Areas.Learn.Controllers;

/// <summary>學習區基底 Controller，統一設定 Area 與登入驗證</summary>
[Area("Learn")]
[Authorize]
public abstract class LearnBaseController : BaseController;
