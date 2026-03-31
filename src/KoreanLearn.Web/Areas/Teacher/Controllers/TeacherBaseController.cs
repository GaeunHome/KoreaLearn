using KoreanLearn.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KoreanLearn.Web.Areas.Teacher.Controllers;

/// <summary>教師管理基底 Controller，統一設定 Area 與 Teacher 權限</summary>
[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public abstract class TeacherBaseController : BaseController
{
    /// <summary>取得當前教師的使用者 ID</summary>
    protected string TeacherId => GetAuthorizedUserId();
}
