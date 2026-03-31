using KoreanLearn.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace KoreanLearn.Web.Areas.Identity.Controllers;

/// <summary>Identity 基底 Controller，統一設定 Area</summary>
[Area("Identity")]
public abstract class IdentityBaseController : BaseController;
