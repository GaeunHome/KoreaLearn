using KoreanLearn.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

/// <summary>後台管理基底 Controller，統一設定 Area 與 Admin 權限</summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public abstract class AdminBaseController : BaseController;
