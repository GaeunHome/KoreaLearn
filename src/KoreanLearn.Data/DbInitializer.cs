using KoreanLearn.Data.Entities;
using KoreanLearn.Library.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KoreanLearn.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await db.Database.MigrateAsync().ConfigureAwait(false);

        await SeedRolesAsync(roleManager).ConfigureAwait(false);
        await SeedUsersAsync(userManager).ConfigureAwait(false);
        await SeedCoursesAsync(db).ConfigureAwait(false);
        await SeedAnnouncementsAsync(db).ConfigureAwait(false);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = ["Admin", "Student"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role).ConfigureAwait(false))
                await roleManager.CreateAsync(new IdentityRole(role)).ConfigureAwait(false);
        }
    }

    private static async Task SeedUsersAsync(UserManager<AppUser> userManager)
    {
        // Admin
        if (await userManager.FindByEmailAsync("admin@koreanlearn.com").ConfigureAwait(false) is null)
        {
            var admin = new AppUser
            {
                UserName = "admin@koreanlearn.com",
                Email = "admin@koreanlearn.com",
                DisplayName = "管理員",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await userManager.CreateAsync(admin, "Admin@123").ConfigureAwait(false);
            await userManager.AddToRoleAsync(admin, "Admin").ConfigureAwait(false);
        }

        // Student
        if (await userManager.FindByEmailAsync("student@koreanlearn.com").ConfigureAwait(false) is null)
        {
            var student = new AppUser
            {
                UserName = "student@koreanlearn.com",
                Email = "student@koreanlearn.com",
                DisplayName = "測試學生",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await userManager.CreateAsync(student, "Student@123").ConfigureAwait(false);
            await userManager.AddToRoleAsync(student, "Student").ConfigureAwait(false);
        }
    }

    private static async Task SeedCoursesAsync(ApplicationDbContext db)
    {
        if (await db.Courses.AnyAsync().ConfigureAwait(false)) return;

        var courses = new List<Course>
        {
            new()
            {
                Title = "韓語入門：從零開始學韓文",
                Description = "適合完全零基礎的學習者，從韓文字母（한글）開始，學會基本發音、簡單問候語和自我介紹。",
                Price = 990m,
                Level = DifficultyLevel.Beginner,
                IsPublished = true,
                SortOrder = 1,
                Sections =
                [
                    new Section
                    {
                        Title = "第一章：認識韓文字母",
                        SortOrder = 1,
                        Lessons =
                        [
                            new Lesson { Title = "母音（모음）介紹", Type = LessonType.Article, SortOrder = 1, IsFreePreview = true,
                                ArticleContent = "<h2>韓文母音</h2><p>韓文有 21 個母音，分為基本母音和複合母音...</p>" },
                            new Lesson { Title = "子音（자음）介紹", Type = LessonType.Article, SortOrder = 2,
                                ArticleContent = "<h2>韓文子音</h2><p>韓文有 19 個子音...</p>" },
                            new Lesson { Title = "收尾音（받침）", Type = LessonType.Article, SortOrder = 3,
                                ArticleContent = "<h2>收尾音</h2><p>收尾音是指音節最後的子音...</p>" }
                        ]
                    },
                    new Section
                    {
                        Title = "第二章：基本問候語",
                        SortOrder = 2,
                        Lessons =
                        [
                            new Lesson { Title = "打招呼與自我介紹", Type = LessonType.Article, SortOrder = 1,
                                ArticleContent = "<h2>안녕하세요!</h2><p>最常用的問候語...</p>" },
                            new Lesson { Title = "數字與日期", Type = LessonType.Article, SortOrder = 2,
                                ArticleContent = "<h2>韓文數字</h2><p>韓文有兩套數字系統...</p>" }
                        ]
                    }
                ]
            },
            new()
            {
                Title = "韓語初級：生活會話",
                Description = "學習日常生活中最常用的韓語會話，包含購物、點餐、問路等實用情境。",
                Price = 1490m,
                Level = DifficultyLevel.Elementary,
                IsPublished = true,
                SortOrder = 2,
                Sections =
                [
                    new Section
                    {
                        Title = "第一章：在餐廳",
                        SortOrder = 1,
                        Lessons =
                        [
                            new Lesson { Title = "點餐用語", Type = LessonType.Article, SortOrder = 1, IsFreePreview = true,
                                ArticleContent = "<h2>점심 먹으러 가요!</h2><p>在韓國餐廳點餐的實用句型...</p>" },
                            new Lesson { Title = "結帳與感謝", Type = LessonType.Article, SortOrder = 2,
                                ArticleContent = "<h2>계산해 주세요</h2><p>結帳時的常用表達...</p>" }
                        ]
                    }
                ]
            },
            new()
            {
                Title = "韓語中級：文法深化",
                Description = "深入學習韓語文法結構，包含連接語尾、間接引用、被動與使動表現。",
                Price = 1990m,
                Level = DifficultyLevel.Intermediate,
                IsPublished = true,
                SortOrder = 3
            }
        };

        db.Courses.AddRange(courses);
        await db.SaveChangesAsync().ConfigureAwait(false);
    }

    private static async Task SeedAnnouncementsAsync(ApplicationDbContext db)
    {
        if (await db.Announcements.AnyAsync().ConfigureAwait(false)) return;

        db.Announcements.AddRange(
            new Announcement
            {
                Title = "歡迎來到 KoreanLearn！",
                Content = "感謝您加入韓文學習平台，我們提供從入門到進階的完整課程。",
                IsActive = true,
                StartDate = DateTime.UtcNow,
            },
            new Announcement
            {
                Title = "新課程上線通知",
                Content = "「韓語中級：文法深化」課程已上線，歡迎選購！",
                IsActive = true,
                StartDate = DateTime.UtcNow,
            }
        );
        await db.SaveChangesAsync().ConfigureAwait(false);
    }
}
