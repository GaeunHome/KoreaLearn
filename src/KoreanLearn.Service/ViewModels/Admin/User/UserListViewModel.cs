namespace KoreanLearn.Service.ViewModels.Admin.User;

/// <summary>後台使用者列表項目 ViewModel</summary>
public class UserListViewModel
{
    /// <summary>使用者 ID</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>電子信箱</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>顯示名稱</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>角色列表</summary>
    public IReadOnlyList<string> Roles { get; set; } = [];

    /// <summary>註冊時間</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>後台使用者詳情 ViewModel</summary>
public class UserDetailViewModel
{
    /// <summary>使用者 ID</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>電子信箱</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>顯示名稱</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>手機號碼</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>角色列表</summary>
    public IReadOnlyList<string> Roles { get; set; } = [];

    /// <summary>選課數量</summary>
    public int EnrollmentCount { get; set; }

    /// <summary>訂單數量</summary>
    public int OrderCount { get; set; }

    /// <summary>開設課程數量（教師角色）</summary>
    public int CourseCount { get; set; }

    /// <summary>註冊時間</summary>
    public DateTime CreatedAt { get; set; }
}
