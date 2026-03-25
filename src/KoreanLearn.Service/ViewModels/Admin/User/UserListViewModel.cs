namespace KoreanLearn.Service.ViewModels.Admin.User;

public class UserListViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public IReadOnlyList<string> Roles { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}

public class UserDetailViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
    public int EnrollmentCount { get; set; }
    public int OrderCount { get; set; }
    public int CourseCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
