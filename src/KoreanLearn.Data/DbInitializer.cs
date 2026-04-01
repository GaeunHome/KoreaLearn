using KoreanLearn.Data.Entities;
using KoreanLearn.Library.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Data;

/// <summary>資料庫初始化器，負責執行 Migration 並植入種子資料（角色、帳號、範例課程等）</summary>
public static class DbInitializer
{
    /// <summary>執行資料庫初始化：套用 Migration + 植入種子資料</summary>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        logger.LogInformation("開始資料庫初始化...");
        await db.Database.MigrateAsync().ConfigureAwait(false);
        logger.LogInformation("資料庫 Migration 完成");

        await SeedRolesAsync(roleManager, logger).ConfigureAwait(false);
        var (teacherId, studentId) = await SeedUsersAsync(userManager, logger).ConfigureAwait(false);
        await SeedCoursesAsync(db, teacherId, logger).ConfigureAwait(false);
        await SeedStudentDataAsync(db, studentId, teacherId, logger).ConfigureAwait(false);
        await SeedAnnouncementsAsync(db, logger).ConfigureAwait(false);
        await SeedSubscriptionPlansAsync(db, logger).ConfigureAwait(false);
        logger.LogInformation("資料庫初始化完成");
    }

    // ── 角色種子資料 ──────────────────────────────────────
    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        string[] roles = ["Admin", "Student", "Teacher"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role).ConfigureAwait(false))
            {
                await roleManager.CreateAsync(new IdentityRole(role)).ConfigureAwait(false);
                logger.LogInformation("建立角色: {Role}", role);
            }
        }
    }

    // ── 使用者種子資料（Admin / Student / Teacher）────────
    private static async Task<(string TeacherId, string StudentId)> SeedUsersAsync(UserManager<AppUser> userManager, ILogger logger)
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
            logger.LogInformation("建立管理員帳號: {Email}", admin.Email);
        }

        // Student
        var student = await userManager.FindByEmailAsync("student@koreanlearn.com").ConfigureAwait(false);
        if (student is null)
        {
            student = new AppUser
            {
                UserName = "student@koreanlearn.com",
                Email = "student@koreanlearn.com",
                DisplayName = "測試學生",
                EmailConfirmed = true,
                ConsecutiveLearningDays = 7,
                LastLearningDate = DateTime.UtcNow.Date,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow
            };
            await userManager.CreateAsync(student, "Student@123").ConfigureAwait(false);
            await userManager.AddToRoleAsync(student, "Student").ConfigureAwait(false);
            logger.LogInformation("建立測試學生帳號: {Email}", student.Email);
        }

        // Teacher
        var teacher = await userManager.FindByEmailAsync("teacher@koreanlearn.com").ConfigureAwait(false);
        if (teacher is null)
        {
            teacher = new AppUser
            {
                UserName = "teacher@koreanlearn.com",
                Email = "teacher@koreanlearn.com",
                DisplayName = "김수진 선생님",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await userManager.CreateAsync(teacher, "Teacher@123").ConfigureAwait(false);
            await userManager.AddToRoleAsync(teacher, "Teacher").ConfigureAwait(false);
            logger.LogInformation("建立測試教師帳號: {Email}", teacher.Email);
        }

        return (teacher.Id, student.Id);
    }

    // ══════════════════════════════════════════════════════
    //  課程種子資料
    // ══════════════════════════════════════════════════════
    private static async Task SeedCoursesAsync(ApplicationDbContext db, string teacherId, ILogger logger)
    {
        if (await db.Courses.AnyAsync().ConfigureAwait(false))
        {
            logger.LogInformation("課程種子資料已存在，跳過");
            return;
        }

        // ── 課程 1：韓語入門 ─────────────────────────────
        var course1 = new Course
        {
            Title = "韓語入門：從零開始學韓文",
            Description = "專為完全零基礎的學習者設計。從韓文字母（한글）的由來與結構開始，逐步學會 40 個基本字母的發音與書寫，再到音節組合、收尾音規則，最後能夠自己拼讀簡單的韓文單字與句子。課程搭配影片教學、PDF 練習單與文章講義，讓你打下最扎實的韓語基礎。",
            CoverImageUrl = "/images/courses/course-beginner.jpg",
            Price = 990m,
            Level = DifficultyLevel.Beginner,
            IsPublished = true,
            SortOrder = 1,
            TeacherId = teacherId,
            Sections =
            [
                new Section
                {
                    Title = "第一章：認識韓文字母 한글",
                    Description = "從韓文的誕生故事開始，學會所有母音與子音的發音和書寫",
                    SortOrder = 1,
                    Lessons =
                    [
                        new Lesson
                        {
                            Title = "韓文的起源與字母系統",
                            Type = LessonType.Article,
                            SortOrder = 1,
                            IsFreePreview = true,
                            ArticleContent = @"<h2>한글（韓文字母）的誕生</h2>
<p>韓文字母 한글（Hangul）由朝鮮王朝第四代國王<strong>世宗大王（세종대왕）</strong>於 1443 年創制，1446 年正式頒布《訓民正音（훈민정음）》。世宗大王認為當時使用的漢字對一般百姓來說過於困難，因此決定創造一套所有人都能輕鬆學會的文字系統。</p>

<div class='alert alert-info'>
<strong>💡 你知道嗎？</strong>每年 10 月 9 日是韓國的「한글날（韓文日）」，是紀念韓文字母誕生的國定假日！
</div>

<h3>한글 的結構特色</h3>
<p>韓文是一種<strong>音素文字</strong>，每個字母代表一個音素。韓文字母共有 40 個：</p>
<ul>
  <li><strong>基本母音</strong>（기본 모음）：10 個 — ㅏ ㅑ ㅓ ㅕ ㅗ ㅛ ㅜ ㅠ ㅡ ㅣ</li>
  <li><strong>複合母音</strong>（이중 모음）：11 個 — ㅐ ㅒ ㅔ ㅖ ㅘ ㅙ ㅚ ㅝ ㅞ ㅟ ㅢ</li>
  <li><strong>基本子音</strong>（기본 자음）：14 個 — ㄱ ㄴ ㄷ ㄹ ㅁ ㅂ ㅅ ㅇ ㅈ ㅊ ㅋ ㅌ ㅍ ㅎ</li>
  <li><strong>雙子音</strong>（쌍자음）：5 個 — ㄲ ㄸ ㅃ ㅆ ㅉ</li>
</ul>

<h3>音節組合方式</h3>
<p>韓文的音節由<strong>初聲（子音）+ 中聲（母音）</strong>組合而成，有些音節還會加上<strong>終聲（收尾音，받침）</strong>：</p>
<table class='table table-bordered'>
  <thead><tr><th>結構</th><th>範例</th><th>發音</th><th>說明</th></tr></thead>
  <tbody>
    <tr><td>子音 + 母音</td><td>나</td><td>na</td><td>ㄴ + ㅏ</td></tr>
    <tr><td>子音 + 母音 + 收尾音</td><td>한</td><td>han</td><td>ㅎ + ㅏ + ㄴ</td></tr>
    <tr><td>子音 + 母音 + 雙收尾音</td><td>닭</td><td>dak</td><td>ㄷ + ㅏ + ㄹㄱ</td></tr>
  </tbody>
</table>

<h3>為什麼韓文被稱為「最科學的文字」？</h3>
<p>語言學家普遍認為韓文是世界上最合理、最科學的書寫系統之一。子音字母的形狀模仿了發音時<strong>口腔器官的形狀</strong>：</p>
<ul>
  <li><strong>ㄱ</strong>（g/k）— 舌根觸及軟顎的側面形狀</li>
  <li><strong>ㄴ</strong>（n）— 舌尖頂住上顎的形狀</li>
  <li><strong>ㅁ</strong>（m）— 嘴巴閉合的形狀</li>
  <li><strong>ㅅ</strong>（s）— 牙齒的形狀</li>
  <li><strong>ㅇ</strong>（ng）— 喉嚨的形狀</li>
</ul>

<h3>學習路線圖</h3>
<p>本課程將帶你按以下順序學習，每一步都打好基礎再往下走：</p>
<ol>
  <li>10 個基本母音 → 發音 + 書寫</li>
  <li>14 個基本子音 → 發音 + 書寫</li>
  <li>送氣音與緊音 → 三種子音的區別</li>
  <li>複合母音 → 11 個組合母音</li>
  <li>收尾音（받침）→ 7 大代表音</li>
  <li>音變規則 → 連音、鼻音化、硬音化</li>
</ol>"
                        },
                        new Lesson
                        {
                            Title = "基本母音發音教學影片",
                            Type = LessonType.Video,
                            SortOrder = 2,
                            IsFreePreview = true,
                            Description = "由金老師親自示範 10 個基本母音的嘴型與發音，搭配慢速重複練習",
                            VideoUrl = "/videos/lessons/hangul-vowels-basic.mp4",
                            VideoDurationSeconds = 1200
                        },
                        new Lesson
                        {
                            Title = "基本母音（모음）詳解：ㅏ ㅑ ㅓ ㅕ ㅗ ㅛ ㅜ ㅠ ㅡ ㅣ",
                            Type = LessonType.Article,
                            SortOrder = 3,
                            ArticleContent = @"<h2>10 個基本母音</h2>
<p>韓文的母音可以分為<strong>陽性母音</strong>和<strong>陰性母音</strong>，這個概念源自東方的陰陽哲學：</p>

<h3>陽性母音（양성 모음）— 聲音明亮、開闊</h3>
<table class='table table-bordered'>
  <thead><tr><th>字母</th><th>羅馬拼音</th><th>發音說明</th><th>嘴型提示</th></tr></thead>
  <tbody>
    <tr><td style='font-size:2rem;text-align:center'>ㅏ</td><td>a</td><td>嘴巴張大，發「啊」的音</td><td>像看牙醫時說「啊～」</td></tr>
    <tr><td style='font-size:2rem;text-align:center'>ㅑ</td><td>ya</td><td>先發「一」再快速接「啊」</td><td>像說「呀」</td></tr>
    <tr><td style='font-size:2rem;text-align:center'>ㅗ</td><td>o</td><td>嘴唇圓起，發「喔」的音</td><td>嘴巴成小圓形</td></tr>
    <tr><td style='font-size:2rem;text-align:center'>ㅛ</td><td>yo</td><td>先發「一」再接「喔」</td><td>像說「唷」</td></tr>
  </tbody>
</table>

<h3>陰性母音（음성 모음）— 聲音深沉、收斂</h3>
<table class='table table-bordered'>
  <thead><tr><th>字母</th><th>羅馬拼音</th><th>發音說明</th><th>嘴型提示</th></tr></thead>
  <tbody>
    <tr><td style='font-size:2rem;text-align:center'>ㅓ</td><td>eo</td><td>嘴巴微張，介於「喔」和「餓」之間</td><td>比 ㅏ 嘴巴張小一點</td></tr>
    <tr><td style='font-size:2rem;text-align:center'>ㅕ</td><td>yeo</td><td>先發「一」再接 ㅓ 的音</td><td>像「唷」但嘴巴不圓</td></tr>
    <tr><td style='font-size:2rem;text-align:center'>ㅜ</td><td>u</td><td>嘴唇前突圓起，發「烏」的音</td><td>像在吹口哨</td></tr>
    <tr><td style='font-size:2rem;text-align:center'>ㅠ</td><td>yu</td><td>先發「一」再接「烏」</td><td>像說「有」</td></tr>
  </tbody>
</table>

<h3>中性母音（중성 모음）</h3>
<table class='table table-bordered'>
  <thead><tr><th>字母</th><th>羅馬拼音</th><th>發音說明</th><th>嘴型提示</th></tr></thead>
  <tbody>
    <tr><td style='font-size:2rem;text-align:center'>ㅡ</td><td>eu</td><td>嘴巴左右拉開，嘴唇不圓</td><td>像微笑但不出聲，然後從喉嚨發音</td></tr>
    <tr><td style='font-size:2rem;text-align:center'>ㅣ</td><td>i</td><td>嘴角向兩旁拉開，發「衣」的音</td><td>跟中文的「衣」幾乎一樣</td></tr>
  </tbody>
</table>

<h3>⚡ 記憶技巧</h3>
<div class='alert alert-success'>
<p>觀察母音的結構，會發現一個超級好記的規律：</p>
<ul>
  <li>短橫/豎在右邊 → <strong>ㅏ ㅓ</strong>（嘴巴上下開）</li>
  <li>短橫/豎在上面 → <strong>ㅗ ㅜ</strong>（嘴巴圓起）</li>
  <li>多一短橫 = 加 <strong>y</strong> 音：ㅏ→ㅑ、ㅓ→ㅕ、ㅗ→ㅛ、ㅜ→ㅠ</li>
</ul>
</div>

<h3>書寫筆順</h3>
<p>母音的筆順很簡單：先寫長豎（或長橫），再寫短橫（或短豎）。例如：</p>
<ul>
  <li>ㅏ：先畫一豎「|」，再從中間往右畫一短橫「—」</li>
  <li>ㅗ：先畫一短豎「|」（往上），再畫一長橫「—」</li>
</ul>"
                        },
                        new Lesson
                        {
                            Title = "母音練習單",
                            Type = LessonType.Pdf,
                            SortOrder = 4,
                            Description = "包含 10 個基本母音的書寫練習格、配對練習與發音對照表，列印出來跟著寫",
                            PdfUrl = "/uploads/pdfs/hangul-vowels-worksheet.pdf",
                            PdfFileName = "韓文母音練習單.pdf"
                        },
                        new Lesson
                        {
                            Title = "基本子音（자음）：ㄱ ㄴ ㄷ ㄹ ㅁ ㅂ ㅅ ㅇ ㅈ",
                            Type = LessonType.Article,
                            SortOrder = 5,
                            ArticleContent = @"<h2>14 個基本子音</h2>
<p>韓文子音（자음）共有 14 個基本字母和 5 個雙子音。本課先學習最常用的基本子音。</p>

<table class='table table-bordered'>
  <thead><tr><th>字母</th><th>名稱</th><th>初聲發音</th><th>終聲發音</th><th>範例單字</th></tr></thead>
  <tbody>
    <tr><td style='font-size:1.8rem;text-align:center'>ㄱ</td><td>기역 giyeok</td><td>g / k（不送氣）</td><td>k</td><td>가방（包包）、국（湯）</td></tr>
    <tr><td style='font-size:1.8rem;text-align:center'>ㄴ</td><td>니은 nieun</td><td>n</td><td>n</td><td>나라（國家）、눈（眼睛/雪）</td></tr>
    <tr><td style='font-size:1.8rem;text-align:center'>ㄷ</td><td>디귿 digeut</td><td>d / t（不送氣）</td><td>t</td><td>다리（橋/腿）、달（月亮）</td></tr>
    <tr><td style='font-size:1.8rem;text-align:center'>ㄹ</td><td>리을 rieul</td><td>r / l</td><td>l</td><td>라면（泡麵）、말（話/馬）</td></tr>
    <tr><td style='font-size:1.8rem;text-align:center'>ㅁ</td><td>미음 mieum</td><td>m</td><td>m</td><td>마음（心）、밤（夜晚）</td></tr>
    <tr><td style='font-size:1.8rem;text-align:center'>ㅂ</td><td>비읍 bieup</td><td>b / p（不送氣）</td><td>p</td><td>바다（海）、밥（飯）</td></tr>
    <tr><td style='font-size:1.8rem;text-align:center'>ㅅ</td><td>시옷 siot</td><td>s</td><td>t</td><td>사람（人）、산（山）</td></tr>
    <tr><td style='font-size:1.8rem;text-align:center'>ㅇ</td><td>이응 ieung</td><td>無聲（佔位）</td><td>ng</td><td>아이（小孩）、강（江）</td></tr>
    <tr><td style='font-size:1.8rem;text-align:center'>ㅈ</td><td>지읒 jieut</td><td>j / ch（不送氣）</td><td>t</td><td>자동차（汽車）、점심（午餐）</td></tr>
    <tr><td style='font-size:1.8rem;text-align:center'>ㅊ</td><td>치읓 chieut</td><td>ch（送氣）</td><td>t</td><td>차（茶/車）、친구（朋友）</td></tr>
    <tr><td style='font-size:1.8rem;text-align:center'>ㅋ</td><td>키읔 kieuk</td><td>k（送氣）</td><td>k</td><td>커피（咖啡）、크다（大）</td></tr>
    <tr><td style='font-size:1.8rem;text-align:center'>ㅌ</td><td>티읕 tieut</td><td>t（送氣）</td><td>t</td><td>토끼（兔子）、탈（面具）</td></tr>
    <tr><td style='font-size:1.8rem;text-align:center'>ㅍ</td><td>피읖 pieup</td><td>p（送氣）</td><td>p</td><td>포도（葡萄）、편지（信）</td></tr>
    <tr><td style='font-size:1.8rem;text-align:center'>ㅎ</td><td>히읗 hieut</td><td>h</td><td>t（弱化）</td><td>하늘（天空）、한국（韓國）</td></tr>
  </tbody>
</table>

<h3>三種子音的區分（最重要！）</h3>
<div class='alert alert-warning'>
<p>韓語子音分三組，這是<strong>中文母語者最大的挑戰</strong>，因為中文沒有這種區分：</p>
</div>
<table class='table table-bordered'>
  <thead><tr><th>類型</th><th>特徵</th><th>ㄱ 系</th><th>ㄷ 系</th><th>ㅂ 系</th><th>ㅈ 系</th></tr></thead>
  <tbody>
    <tr><td><strong>平音</strong>（基本音）</td><td>不送氣、聲帶放鬆</td><td>ㄱ</td><td>ㄷ</td><td>ㅂ</td><td>ㅈ</td></tr>
    <tr><td><strong>送氣音</strong>（激音）</td><td>送氣、聲帶放鬆</td><td>ㅋ</td><td>ㅌ</td><td>ㅍ</td><td>ㅊ</td></tr>
    <tr><td><strong>緊音</strong>（硬音）</td><td>不送氣、聲帶緊繃</td><td>ㄲ</td><td>ㄸ</td><td>ㅃ</td><td>ㅉ</td></tr>
  </tbody>
</table>

<h3>💡 練習技巧</h3>
<p>在嘴巴前放一張衛生紙：</p>
<ul>
  <li>發 <strong>ㄱ（가）</strong>時 — 紙<strong>不動</strong></li>
  <li>發 <strong>ㅋ（카）</strong>時 — 紙<strong>明顯飄動</strong></li>
  <li>發 <strong>ㄲ（까）</strong>時 — 紙<strong>不動</strong>，但喉嚨有緊繃感</li>
</ul>"
                        },
                        new Lesson
                        {
                            Title = "子音發音教學影片",
                            Type = LessonType.Video,
                            SortOrder = 6,
                            Description = "14 個基本子音 + 5 個雙子音的發音示範，含平音/送氣音/緊音對比練習",
                            VideoUrl = "/videos/lessons/hangul-consonants.mp4",
                            VideoDurationSeconds = 1500
                        },
                        new Lesson
                        {
                            Title = "收尾音（받침）完全攻略",
                            Type = LessonType.Article,
                            SortOrder = 7,
                            ArticleContent = @"<h2>什麼是收尾音（받침）？</h2>
<p>收尾音是韓文音節最下方的子音。雖然有 27 種書寫形式，但實際發音只有 <strong>7 種代表音</strong>。</p>

<h3>七大代表收尾音</h3>
<table class='table table-bordered'>
  <thead><tr><th>代表音</th><th>發音方式</th><th>對應的收尾音</th><th>範例</th></tr></thead>
  <tbody>
    <tr><td style='font-size:1.3rem'>ㄱ [k]</td><td>喉嚨後方閉合，氣流停住</td><td>ㄱ, ㅋ, ㄲ</td><td>국（湯）、부엌（廚房）</td></tr>
    <tr><td style='font-size:1.3rem'>ㄴ [n]</td><td>舌尖頂住上顎</td><td>ㄴ</td><td>산（山）、눈（眼睛）</td></tr>
    <tr><td style='font-size:1.3rem'>ㄷ [t]</td><td>舌尖頂住上齒齦</td><td>ㄷ, ㅌ, ㅅ, ㅆ, ㅈ, ㅊ, ㅎ</td><td>맛（味道）、꽃（花）</td></tr>
    <tr><td style='font-size:1.3rem'>ㄹ [l]</td><td>舌尖輕頂上顎不放開</td><td>ㄹ</td><td>달（月亮）、발（腳）</td></tr>
    <tr><td style='font-size:1.3rem'>ㅁ [m]</td><td>雙唇閉合</td><td>ㅁ</td><td>밤（夜晚）、봄（春天）</td></tr>
    <tr><td style='font-size:1.3rem'>ㅂ [p]</td><td>雙唇閉合，氣流停住</td><td>ㅂ, ㅍ</td><td>밥（飯）、앞（前面）</td></tr>
    <tr><td style='font-size:1.3rem'>ㅇ [ng]</td><td>鼻腔共鳴</td><td>ㅇ</td><td>강（江）、방（房間）</td></tr>
  </tbody>
</table>

<h3>連音規則（연음법칙）</h3>
<p>當有收尾音的字後面接著以 ㅇ（無聲）開頭的字時，收尾音會「移動」到下一個字的初聲位置發音：</p>
<ul>
  <li>한국<strong>어</strong> → 한구<strong>거</strong> [han-gu-geo]</li>
  <li>음<strong>악</strong> → 으<strong>막</strong> [eu-mak]</li>
  <li>독<strong>일</strong> → 도<strong>길</strong> [do-gil]</li>
  <li>먹<strong>어요</strong> → 머<strong>거</strong>요 [meo-geo-yo]</li>
</ul>

<div class='alert alert-info'>
<strong>💡 小提醒：</strong>連音是韓語最基本也最重要的音變規則，之後學的所有句子都會用到！
</div>"
                        },
                        new Lesson
                        {
                            Title = "子音與收尾音練習單",
                            Type = LessonType.Pdf,
                            SortOrder = 8,
                            Description = "包含子音書寫練習、收尾音辨識練習、連音規則填空題",
                            PdfUrl = "/uploads/pdfs/hangul-consonants-worksheet.pdf",
                            PdfFileName = "韓文子音與收尾音練習單.pdf"
                        }
                    ]
                },
                new Section
                {
                    Title = "第二章：基本問候與自我介紹",
                    Description = "學會韓語最基本的問候語、自我介紹，以及兩套數字系統",
                    SortOrder = 2,
                    Lessons =
                    [
                        new Lesson
                        {
                            Title = "問候語教學影片",
                            Type = LessonType.Video,
                            SortOrder = 1,
                            IsFreePreview = true,
                            Description = "韓語日常問候語的正確發音與使用場合，含敬語/半語的差異",
                            VideoUrl = "/videos/lessons/greetings-korean.mp4",
                            VideoDurationSeconds = 900
                        },
                        new Lesson
                        {
                            Title = "打招呼：안녕하세요！",
                            Type = LessonType.Article,
                            SortOrder = 2,
                            ArticleContent = @"<h2>韓語問候語</h2>
<p>韓語的敬語系統非常重要。根據對象和場合的不同，問候方式也不同。</p>

<h3>日常問候</h3>
<table class='table table-bordered'>
  <thead><tr><th>韓文</th><th>羅馬拼音</th><th>中文</th><th>使用場合</th></tr></thead>
  <tbody>
    <tr><td>안녕하세요</td><td>annyeonghaseyo</td><td>你好</td><td>通用敬語，最常用</td></tr>
    <tr><td>안녕하십니까</td><td>annyeonghasimnikka</td><td>您好</td><td>非常正式的場合</td></tr>
    <tr><td>안녕</td><td>annyeong</td><td>嗨</td><td>朋友之間、非正式</td></tr>
    <tr><td>안녕히 가세요</td><td>annyeonghi gaseyo</td><td>再見（對離開的人說）</td><td>你留下、對方走</td></tr>
    <tr><td>안녕히 계세요</td><td>annyeonghi gyeseyo</td><td>再見（對留下的人說）</td><td>你走、對方留下</td></tr>
    <tr><td>감사합니다</td><td>gamsahamnida</td><td>謝謝</td><td>正式感謝</td></tr>
    <tr><td>고마워요</td><td>gomawoyo</td><td>謝謝</td><td>日常感謝</td></tr>
    <tr><td>죄송합니다</td><td>joesonghamnida</td><td>抱歉</td><td>正式道歉</td></tr>
    <tr><td>미안해요</td><td>mianhaeyo</td><td>抱歉</td><td>日常道歉</td></tr>
    <tr><td>네</td><td>ne</td><td>是</td><td>回應「是的」</td></tr>
    <tr><td>아니요</td><td>aniyo</td><td>不是</td><td>回應「不是」</td></tr>
  </tbody>
</table>

<h3>實用對話練習</h3>
<div style='background:#f0f4f8; padding:1rem; border-radius:8px; margin:1rem 0;'>
  <p><strong>A:</strong> 안녕하세요! (你好！)</p>
  <p><strong>B:</strong> 안녕하세요! 만나서 반갑습니다. (你好！很高興認識你。)</p>
  <p><strong>A:</strong> 저도 반갑습니다. (我也很高興。)</p>
</div>
<div style='background:#f0f4f8; padding:1rem; border-radius:8px; margin:1rem 0;'>
  <p><strong>A:</strong> 감사합니다! (謝謝！)</p>
  <p><strong>B:</strong> 아니에요. / 천만에요. (不客氣。)</p>
</div>"
                        },
                        new Lesson
                        {
                            Title = "自我介紹：저는 ___입니다",
                            Type = LessonType.Article,
                            SortOrder = 3,
                            ArticleContent = @"<h2>韓語自我介紹</h2>

<h3>基本句型</h3>
<table class='table table-bordered'>
  <thead><tr><th>韓文</th><th>中文</th><th>說明</th></tr></thead>
  <tbody>
    <tr><td>저는 [名字]입니다.</td><td>我是 [名字]。</td><td>正式自我介紹</td></tr>
    <tr><td>저는 [國家] 사람입니다.</td><td>我是 [國家] 人。</td><td>說明國籍</td></tr>
    <tr><td>저는 [職業]입니다.</td><td>我是 [職業]。</td><td>說明工作</td></tr>
    <tr><td>[數字]살입니다.</td><td>我 [數字] 歲。</td><td>說明年齡（用固有數字）</td></tr>
    <tr><td>취미는 [興趣]입니다.</td><td>興趣是 [興趣]。</td><td>說明喜好</td></tr>
  </tbody>
</table>

<h3>常用國家與職業名稱</h3>
<table class='table table-bordered'>
  <thead><tr><th>韓文</th><th>中文</th><th>分類</th></tr></thead>
  <tbody>
    <tr><td>대만 / 타이완</td><td>臺灣</td><td>國家</td></tr>
    <tr><td>한국</td><td>韓國</td><td>國家</td></tr>
    <tr><td>일본</td><td>日本</td><td>國家</td></tr>
    <tr><td>학생</td><td>學生</td><td>職業</td></tr>
    <tr><td>회사원</td><td>上班族</td><td>職業</td></tr>
    <tr><td>선생님</td><td>老師</td><td>職業</td></tr>
    <tr><td>의사</td><td>醫生</td><td>職業</td></tr>
  </tbody>
</table>

<h3>完整自我介紹範例</h3>
<div style='background:#f0f4f8; padding:1rem; border-radius:8px; margin:1rem 0;'>
  <p>안녕하세요.</p>
  <p>저는 <strong>린메이</strong>입니다.</p>
  <p>대만 사람입니다.</p>
  <p>스물다섯 살입니다.</p>
  <p>직업은 학생입니다.</p>
  <p>취미는 한국 드라마 보기입니다.</p>
  <p>만나서 반갑습니다!</p>
</div>
<p><em>翻譯：你好。我是林美。我是臺灣人。我 25 歲。職業是學生。興趣是看韓劇。很高興認識你！</em></p>"
                        },
                        new Lesson
                        {
                            Title = "韓語數字系統：固有數字與漢字數字",
                            Type = LessonType.Article,
                            SortOrder = 4,
                            ArticleContent = @"<h2>韓文的兩套數字</h2>
<p>韓語有兩套數字系統，使用場合不同，這是初學者最容易混淆的地方。</p>

<h3>漢字數字（한자 숫자）</h3>
<p>用於日期、月份、金額、電話號碼、地址。源自漢字，發音跟中文有關聯。</p>
<table class='table table-bordered'>
  <thead><tr><th>數字</th><th>韓文</th><th>羅馬拼音</th><th>數字</th><th>韓文</th><th>羅馬拼音</th></tr></thead>
  <tbody>
    <tr><td>0</td><td>영/공</td><td>yeong/gong</td><td>6</td><td>육</td><td>yuk</td></tr>
    <tr><td>1</td><td>일</td><td>il</td><td>7</td><td>칠</td><td>chil</td></tr>
    <tr><td>2</td><td>이</td><td>i</td><td>8</td><td>팔</td><td>pal</td></tr>
    <tr><td>3</td><td>삼</td><td>sam</td><td>9</td><td>구</td><td>gu</td></tr>
    <tr><td>4</td><td>사</td><td>sa</td><td>10</td><td>십</td><td>sip</td></tr>
    <tr><td>5</td><td>오</td><td>o</td><td>100</td><td>백</td><td>baek</td></tr>
  </tbody>
</table>

<h3>固有數字（고유 숫자）</h3>
<p>用於年齡、時（幾點）、數量詞。注意 1~4 和 20 在接量詞時會縮短！</p>
<table class='table table-bordered'>
  <thead><tr><th>數字</th><th>韓文</th><th>接量詞時</th><th>數字</th><th>韓文</th><th>接量詞時</th></tr></thead>
  <tbody>
    <tr><td>1</td><td>하나</td><td>한</td><td>6</td><td>여섯</td><td>여섯</td></tr>
    <tr><td>2</td><td>둘</td><td>두</td><td>7</td><td>일곱</td><td>일곱</td></tr>
    <tr><td>3</td><td>셋</td><td>세</td><td>8</td><td>여덟</td><td>여덟</td></tr>
    <tr><td>4</td><td>넷</td><td>네</td><td>9</td><td>아홉</td><td>아홉</td></tr>
    <tr><td>5</td><td>다섯</td><td>다섯</td><td>10</td><td>열</td><td>열</td></tr>
  </tbody>
</table>

<h3>使用場合速查表</h3>
<table class='table table-bordered'>
  <thead><tr><th>場合</th><th>用哪套</th><th>範例</th></tr></thead>
  <tbody>
    <tr><td>年齡</td><td>固有</td><td>스물다섯 <strong>살</strong>（25 歲）</td></tr>
    <tr><td>幾點（時）</td><td>固有</td><td><strong>세</strong> 시（3 點）</td></tr>
    <tr><td>幾分</td><td>漢字</td><td><strong>삼십</strong> 분（30 分）</td></tr>
    <tr><td>日期</td><td>漢字</td><td><strong>삼</strong>월 <strong>이십오</strong>일（3月25日）</td></tr>
    <tr><td>價格</td><td>漢字</td><td><strong>만</strong> 원（一萬元）</td></tr>
    <tr><td>數量（個、杯）</td><td>固有</td><td>커피 <strong>두</strong> 잔（兩杯咖啡）</td></tr>
  </tbody>
</table>"
                        },
                        new Lesson
                        {
                            Title = "問候與數字練習單",
                            Type = LessonType.Pdf,
                            SortOrder = 5,
                            Description = "自我介紹填空練習、數字聽寫練習、情境對話配對題",
                            PdfUrl = "/uploads/pdfs/greetings-numbers-worksheet.pdf",
                            PdfFileName = "問候語與數字練習單.pdf"
                        }
                    ]
                }
            ]
        };

        // ── 課程 2：韓語初級 生活會話 ────────────────────
        var course2 = new Course
        {
            Title = "韓語初級：生活會話",
            Description = "學習日常生活中最實用的韓語會話。涵蓋在韓國旅遊、餐廳點餐、便利商店購物、搭乘交通工具、問路等真實場景。每個單元都有影片情境對話、文章講義與 PDF 統整表，讓你能實際開口說韓語。",
            CoverImageUrl = "/images/courses/course-elementary.jpg",
            Price = 1490m,
            Level = DifficultyLevel.Elementary,
            IsPublished = true,
            SortOrder = 2,
            TeacherId = teacherId,
            Sections =
            [
                new Section
                {
                    Title = "第一章：在餐廳用餐",
                    Description = "學會從進店、看菜單、點餐到結帳的完整韓語會話",
                    SortOrder = 1,
                    Lessons =
                    [
                        new Lesson
                        {
                            Title = "餐廳情境對話影片",
                            Type = LessonType.Video,
                            SortOrder = 1,
                            IsFreePreview = true,
                            Description = "完整模擬韓國餐廳用餐情境，從進門到結帳的實景對話",
                            VideoUrl = "/videos/lessons/restaurant-dialogue.mp4",
                            VideoDurationSeconds = 1080
                        },
                        new Lesson
                        {
                            Title = "點餐用語與菜單閱讀",
                            Type = LessonType.Article,
                            SortOrder = 2,
                            ArticleContent = @"<h2>韓國餐廳實用會話</h2>

<h3>進入餐廳</h3>
<div style='background:#f0f4f8; padding:1rem; border-radius:8px; margin:1rem 0;'>
  <p><strong>店員：</strong> 어서오세요! 몇 분이세요?（歡迎光臨！請問幾位？）</p>
  <p><strong>你：</strong> 두 명이요.（兩位。）</p>
  <p><strong>店員：</strong> 이쪽으로 앉으세요.（請坐這邊。）</p>
</div>

<h3>常用點餐句型</h3>
<table class='table table-bordered'>
  <thead><tr><th>韓文</th><th>中文</th><th>使用時機</th></tr></thead>
  <tbody>
    <tr><td>메뉴판 주세요.</td><td>請給我菜單。</td><td>需要菜單時</td></tr>
    <tr><td>이거 주세요.</td><td>請給我這個。</td><td>手指菜單點餐</td></tr>
    <tr><td>_____ 하나 주세요.</td><td>請給我一份_____。</td><td>知道菜名時</td></tr>
    <tr><td>추천 메뉴가 뭐예요?</td><td>推薦菜是什麼？</td><td>不知道點什麼時</td></tr>
    <tr><td>매운 거예요?</td><td>這是辣的嗎？</td><td>確認辣度</td></tr>
    <tr><td>안 맵게 해 주세요.</td><td>請做不辣的。</td><td>怕辣時</td></tr>
    <tr><td>물 좀 주세요.</td><td>請給我水。</td><td>需要水時</td></tr>
  </tbody>
</table>

<h3>韓國必吃料理</h3>
<table class='table table-bordered'>
  <thead><tr><th>韓文</th><th>中文</th><th>說明</th></tr></thead>
  <tbody>
    <tr><td>비빔밥</td><td>拌飯</td><td>韓國最具代表性的料理，加辣醬拌著吃</td></tr>
    <tr><td>김치찌개</td><td>泡菜鍋</td><td>韓國人最常吃的鍋物，用老泡菜煮</td></tr>
    <tr><td>된장찌개</td><td>大醬湯</td><td>韓式味噌湯，韓國家庭常備菜</td></tr>
    <tr><td>불고기</td><td>烤肉</td><td>醬油醃漬的烤牛肉，甜甜的</td></tr>
    <tr><td>삼겹살</td><td>五花肉</td><td>韓式烤五花肉，配生菜吃</td></tr>
    <tr><td>냉면</td><td>冷麵</td><td>夏天必吃，分水冷麵和拌冷麵</td></tr>
    <tr><td>떡볶이</td><td>辣炒年糕</td><td>韓國街頭小吃代表</td></tr>
    <tr><td>김밥</td><td>紫菜飯捲</td><td>韓國版壽司，方便攜帶</td></tr>
    <tr><td>라면</td><td>泡麵/拉麵</td><td>韓國人的靈魂食物</td></tr>
    <tr><td>치킨</td><td>炸雞</td><td>韓式炸雞，配啤酒是經典</td></tr>
  </tbody>
</table>

<h3>結帳</h3>
<div style='background:#f0f4f8; padding:1rem; border-radius:8px; margin:1rem 0;'>
  <p><strong>你：</strong> 여기요, 계산해 주세요.（不好意思，請幫我結帳。）</p>
  <p><strong>店員：</strong> 총 만오천 원입니다.（總共一萬五千元。）</p>
  <p><strong>你：</strong> 카드 돼요?（可以刷卡嗎？）</p>
  <p><strong>店員：</strong> 네, 됩니다.（是的，可以。）</p>
  <p><strong>你：</strong> 잘 먹었습니다!（我吃得很好！/ 謝謝招待！）</p>
</div>"
                        },
                        new Lesson
                        {
                            Title = "餐廳常用單字與句型 PDF",
                            Type = LessonType.Pdf,
                            SortOrder = 3,
                            Description = "餐廳場景完整單字表、句型公式卡、菜單中韓對照表",
                            PdfUrl = "/uploads/pdfs/restaurant-vocabulary.pdf",
                            PdfFileName = "餐廳韓語單字與句型.pdf"
                        }
                    ]
                },
                new Section
                {
                    Title = "第二章：交通與問路",
                    Description = "學會搭地鐵、公車、計程車，以及問路的完整韓語會話",
                    SortOrder = 2,
                    Lessons =
                    [
                        new Lesson
                        {
                            Title = "搭乘地鐵與公車",
                            Type = LessonType.Article,
                            SortOrder = 1,
                            ArticleContent = @"<h2>韓國交通用語</h2>

<h3>搭地鐵常用句</h3>
<table class='table table-bordered'>
  <thead><tr><th>韓文</th><th>中文</th></tr></thead>
  <tbody>
    <tr><td>지하철역이 어디예요?</td><td>地鐵站在哪裡？</td></tr>
    <tr><td>___역에서 내려야 돼요.</td><td>要在___站下車。</td></tr>
    <tr><td>몇 호선이에요?</td><td>是幾號線？</td></tr>
    <tr><td>갈아타야 돼요?</td><td>需要轉車嗎？</td></tr>
    <tr><td>교통카드 충전해 주세요.</td><td>請幫我加值交通卡。</td></tr>
    <tr><td>다음 역은 어디예요?</td><td>下一站是哪裡？</td></tr>
  </tbody>
</table>

<h3>問路對話</h3>
<div style='background:#f0f4f8; padding:1rem; border-radius:8px; margin:1rem 0;'>
  <p><strong>你：</strong> 실례합니다. 명동이 어디예요?（不好意思，明洞在哪裡？）</p>
  <p><strong>路人：</strong> 여기서 직진하세요. 그리고 두 번째 사거리에서 오른쪽으로 가세요.</p>
  <p>（從這裡直走，然後在第二個十字路口右轉。）</p>
  <p><strong>你：</strong> 걸어서 얼마나 걸려요?（走路要多久？）</p>
  <p><strong>路人：</strong> 한 십 분 정도 걸려요.（大約十分鐘。）</p>
</div>

<h3>方向詞彙</h3>
<table class='table table-bordered'>
  <tbody>
    <tr><td>직진（直走）</td><td>왼쪽（左邊）</td><td>오른쪽（右邊）</td></tr>
    <tr><td>앞（前面）</td><td>뒤（後面）</td><td>옆（旁邊）</td></tr>
    <tr><td>건너편（對面）</td><td>사거리（十字路口）</td><td>신호등（紅綠燈）</td></tr>
  </tbody>
</table>"
                        },
                        new Lesson
                        {
                            Title = "購物與殺價",
                            Type = LessonType.Article,
                            SortOrder = 2,
                            ArticleContent = @"<h2>韓國購物會話</h2>

<h3>在商店</h3>
<table class='table table-bordered'>
  <thead><tr><th>韓文</th><th>中文</th></tr></thead>
  <tbody>
    <tr><td>이거 얼마예요?</td><td>這個多少錢？</td></tr>
    <tr><td>좀 깎아 주세요.</td><td>請便宜一點。</td></tr>
    <tr><td>다른 색 있어요?</td><td>有其他顏色嗎？</td></tr>
    <tr><td>입어 봐도 돼요?</td><td>可以試穿嗎？</td></tr>
    <tr><td>너무 비싸요.</td><td>太貴了。</td></tr>
    <tr><td>현금으로 할게요.</td><td>我用現金。</td></tr>
    <tr><td>영수증 주세요.</td><td>請給我收據。</td></tr>
    <tr><td>교환 / 환불 돼요?</td><td>可以換貨 / 退貨嗎？</td></tr>
  </tbody>
</table>

<h3>市場購物對話</h3>
<div style='background:#f0f4f8; padding:1rem; border-radius:8px; margin:1rem 0;'>
  <p><strong>你：</strong> 아저씨, 이거 얼마예요?（老闆，這個多少錢？）</p>
  <p><strong>老闆：</strong> 이만 원이에요.（兩萬元。）</p>
  <p><strong>你：</strong> 너무 비싸요! 만오천 원에 안 돼요?（太貴了！一萬五不行嗎？）</p>
  <p><strong>老闆：</strong> 에이, 그건 좀... 만팔천 원에 드릴게요.（哎呀，那有點... 一萬八給你吧。）</p>
  <p><strong>你：</strong> 좋아요! 감사합니다.（好！謝謝。）</p>
</div>"
                        }
                    ]
                },
                new Section
                {
                    Title = "第三章：便利商店與咖啡廳",
                    Description = "韓國便利商店與咖啡廳的實用會話",
                    SortOrder = 3,
                    Lessons =
                    [
                        new Lesson
                        {
                            Title = "便利商店韓語",
                            Type = LessonType.Article,
                            SortOrder = 1,
                            ArticleContent = @"<h2>편의점（便利商店）韓語</h2>
<p>韓國的便利商店密度是全世界最高的之一，走到哪都有 CU、GS25、7-Eleven。學會這些句子就能輕鬆購物！</p>

<h3>常用句型</h3>
<table class='table table-bordered'>
  <thead><tr><th>韓文</th><th>中文</th></tr></thead>
  <tbody>
    <tr><td>이거 데워 주세요.</td><td>請幫我加熱這個。</td></tr>
    <tr><td>봉투 주세요.</td><td>請給我袋子。</td></tr>
    <tr><td>젓가락 주세요.</td><td>請給我筷子。</td></tr>
    <tr><td>여기서 먹을 거예요.</td><td>我要在這裡吃。</td></tr>
    <tr><td>충전기 있어요?</td><td>有充電器嗎？</td></tr>
    <tr><td>포인트 적립할게요.</td><td>我要累積點數。</td></tr>
  </tbody>
</table>

<h3>便利商店必買韓國零食</h3>
<table class='table table-bordered'>
  <thead><tr><th>韓文</th><th>中文</th></tr></thead>
  <tbody>
    <tr><td>삼각김밥</td><td>三角飯糰</td></tr>
    <tr><td>컵라면</td><td>杯麵</td></tr>
    <tr><td>바나나우유</td><td>香蕉牛奶</td></tr>
    <tr><td>소주</td><td>燒酒</td></tr>
    <tr><td>맥주</td><td>啤酒</td></tr>
  </tbody>
</table>"
                        },
                        new Lesson
                        {
                            Title = "咖啡廳點餐影片",
                            Type = LessonType.Video,
                            SortOrder = 2,
                            Description = "韓國咖啡廳實景點餐教學，學會各種飲料的韓文說法",
                            VideoUrl = "/videos/lessons/cafe-ordering.mp4",
                            VideoDurationSeconds = 720
                        }
                    ]
                }
            ]
        };

        db.Courses.AddRange(course1, course2);
        await db.SaveChangesAsync().ConfigureAwait(false);
        logger.LogInformation("建立 2 門範例課程（含影片、文章、PDF 單元）");

        // ── 字卡牌組種子資料 ──────────────────────────────
        await SeedFlashcardDecksAsync(db, [course1, course2], logger).ConfigureAwait(false);

        // ── 測驗種子資料 ──────────────────────────────────
        await SeedQuizzesAsync(db, [course1, course2], logger).ConfigureAwait(false);

        // ── 發音練習種子資料 ──────────────────────────────
        await SeedPronunciationExercisesAsync(db, logger).ConfigureAwait(false);
    }

    // ══════════════════════════════════════════════════════
    //  字卡牌組種子資料
    // ══════════════════════════════════════════════════════
    private static async Task SeedFlashcardDecksAsync(ApplicationDbContext db, List<Course> courses, ILogger logger)
    {
        var decks = new List<FlashcardDeck>
        {
            new()
            {
                Title = "韓文基本母音字卡",
                Description = "10 個基本母音的發音與記憶訓練",
                CourseId = courses[0].Id,
                Flashcards =
                [
                    new Flashcard { Korean = "ㅏ", Chinese = "a（啊）", Romanization = "a", ExampleSentence = "아이 (a-i) = 小孩", SortOrder = 1 },
                    new Flashcard { Korean = "ㅑ", Chinese = "ya（呀）", Romanization = "ya", ExampleSentence = "야구 (ya-gu) = 棒球", SortOrder = 2 },
                    new Flashcard { Korean = "ㅓ", Chinese = "eo（喔/餓之間）", Romanization = "eo", ExampleSentence = "어머니 (eo-meo-ni) = 媽媽", SortOrder = 3 },
                    new Flashcard { Korean = "ㅕ", Chinese = "yeo（唷但不圓唇）", Romanization = "yeo", ExampleSentence = "여자 (yeo-ja) = 女生", SortOrder = 4 },
                    new Flashcard { Korean = "ㅗ", Chinese = "o（喔）", Romanization = "o", ExampleSentence = "오리 (o-ri) = 鴨子", SortOrder = 5 },
                    new Flashcard { Korean = "ㅛ", Chinese = "yo（唷）", Romanization = "yo", ExampleSentence = "요리 (yo-ri) = 料理", SortOrder = 6 },
                    new Flashcard { Korean = "ㅜ", Chinese = "u（烏）", Romanization = "u", ExampleSentence = "우유 (u-yu) = 牛奶", SortOrder = 7 },
                    new Flashcard { Korean = "ㅠ", Chinese = "yu（有）", Romanization = "yu", ExampleSentence = "유리 (yu-ri) = 玻璃", SortOrder = 8 },
                    new Flashcard { Korean = "ㅡ", Chinese = "eu（扁嘴ㄜ）", Romanization = "eu", ExampleSentence = "으악 (eu-ak) = 啊！（驚嚇）", SortOrder = 9 },
                    new Flashcard { Korean = "ㅣ", Chinese = "i（衣）", Romanization = "i", ExampleSentence = "이름 (i-reum) = 名字", SortOrder = 10 }
                ]
            },
            new()
            {
                Title = "基本問候語字卡",
                Description = "韓語最常用的問候語與日常用語",
                CourseId = courses[0].Id,
                Flashcards =
                [
                    new Flashcard { Korean = "안녕하세요", Chinese = "你好", Romanization = "annyeonghaseyo", ExampleSentence = "안녕하세요! 만나서 반갑습니다.", SortOrder = 1 },
                    new Flashcard { Korean = "감사합니다", Chinese = "謝謝（正式）", Romanization = "gamsahamnida", ExampleSentence = "도와주셔서 감사합니다.", SortOrder = 2 },
                    new Flashcard { Korean = "죄송합니다", Chinese = "對不起（正式）", Romanization = "joesonghamnida", ExampleSentence = "늦어서 죄송합니다.", SortOrder = 3 },
                    new Flashcard { Korean = "안녕히 가세요", Chinese = "再見（對離開的人）", Romanization = "annyeonghi gaseyo", ExampleSentence = "안녕히 가세요! 조심히 가세요.", SortOrder = 4 },
                    new Flashcard { Korean = "네", Chinese = "是", Romanization = "ne", ExampleSentence = "네, 맞아요.", SortOrder = 5 },
                    new Flashcard { Korean = "아니요", Chinese = "不是", Romanization = "aniyo", ExampleSentence = "아니요, 괜찮아요.", SortOrder = 6 },
                    new Flashcard { Korean = "잘 먹겠습니다", Chinese = "我要開動了", Romanization = "jal meokgesseumnida", ExampleSentence = "（吃飯前說）잘 먹겠습니다!", SortOrder = 7 },
                    new Flashcard { Korean = "잘 먹었습니다", Chinese = "我吃飽了 / 謝謝招待", Romanization = "jal meogeosseumnida", ExampleSentence = "（吃完後說）잘 먹었습니다!", SortOrder = 8 }
                ]
            },
            new()
            {
                Title = "韓國美食單字卡",
                Description = "去韓國必知的食物名稱",
                CourseId = courses[1].Id,
                Flashcards =
                [
                    new Flashcard { Korean = "비빔밥", Chinese = "拌飯", Romanization = "bibimbap", ExampleSentence = "비빔밥 하나 주세요.", SortOrder = 1 },
                    new Flashcard { Korean = "김치찌개", Chinese = "泡菜鍋", Romanization = "gimchi-jjigae", ExampleSentence = "김치찌개는 매워요.", SortOrder = 2 },
                    new Flashcard { Korean = "삼겹살", Chinese = "五花肉", Romanization = "samgyeopsal", ExampleSentence = "삼겹살 이인분 주세요.", SortOrder = 3 },
                    new Flashcard { Korean = "떡볶이", Chinese = "辣炒年糕", Romanization = "tteokbokki", ExampleSentence = "떡볶이 맛있어요!", SortOrder = 4 },
                    new Flashcard { Korean = "냉면", Chinese = "冷麵", Romanization = "naengmyeon", ExampleSentence = "여름에는 냉면이 최고예요.", SortOrder = 5 },
                    new Flashcard { Korean = "김밥", Chinese = "紫菜飯捲", Romanization = "gimbap", ExampleSentence = "김밥은 간편한 음식이에요.", SortOrder = 6 },
                    new Flashcard { Korean = "치킨", Chinese = "炸雞", Romanization = "chikin", ExampleSentence = "치킨이랑 맥주 시킬까요?", SortOrder = 7 },
                    new Flashcard { Korean = "라면", Chinese = "泡麵", Romanization = "ramyeon", ExampleSentence = "라면 먹고 갈래요?", SortOrder = 8 },
                    new Flashcard { Korean = "소주", Chinese = "燒酒", Romanization = "soju", ExampleSentence = "소주 한 병 주세요.", SortOrder = 9 },
                    new Flashcard { Korean = "커피", Chinese = "咖啡", Romanization = "keopi", ExampleSentence = "아이스 아메리카노 하나 주세요.", SortOrder = 10 }
                ]
            },
            new()
            {
                Title = "數字與量詞字卡",
                Description = "漢字數字、固有數字與常用量詞",
                CourseId = courses[0].Id,
                Flashcards =
                [
                    new Flashcard { Korean = "하나 (한)", Chinese = "一", Romanization = "hana (han)", ExampleSentence = "커피 한 잔 주세요.", SortOrder = 1 },
                    new Flashcard { Korean = "둘 (두)", Chinese = "二", Romanization = "dul (du)", ExampleSentence = "사과 두 개 주세요.", SortOrder = 2 },
                    new Flashcard { Korean = "셋 (세)", Chinese = "三", Romanization = "set (se)", ExampleSentence = "세 시에 만나요.", SortOrder = 3 },
                    new Flashcard { Korean = "넷 (네)", Chinese = "四", Romanization = "net (ne)", ExampleSentence = "네 명이에요.", SortOrder = 4 },
                    new Flashcard { Korean = "개", Chinese = "個（量詞）", Romanization = "gae", ExampleSentence = "이거 세 개 주세요.", SortOrder = 5 },
                    new Flashcard { Korean = "잔", Chinese = "杯（量詞）", Romanization = "jan", ExampleSentence = "물 한 잔 주세요.", SortOrder = 6 },
                    new Flashcard { Korean = "명", Chinese = "人/位（量詞）", Romanization = "myeong", ExampleSentence = "두 명이요.", SortOrder = 7 },
                    new Flashcard { Korean = "원", Chinese = "元（韓幣）", Romanization = "won", ExampleSentence = "만 원입니다.", SortOrder = 8 }
                ]
            }
        };

        db.FlashcardDecks.AddRange(decks);
        await db.SaveChangesAsync().ConfigureAwait(false);
        logger.LogInformation("建立 {Count} 組字卡牌組", decks.Count);
    }

    // ══════════════════════════════════════════════════════
    //  測驗種子資料
    // ══════════════════════════════════════════════════════
    private static async Task SeedQuizzesAsync(ApplicationDbContext db, List<Course> courses, ILogger logger)
    {
        // 取得第一門課程第一章最後一個 Article 課程作為測驗對象
        var course1Lessons = await db.Lessons
            .Where(l => l.Section.CourseId == courses[0].Id)
            .OrderBy(l => l.Section.SortOrder).ThenBy(l => l.SortOrder)
            .ToListAsync().ConfigureAwait(false);

        // 找 Article 類型的 lesson 來附加測驗
        var articleLesson = course1Lessons.FirstOrDefault(l => l.Type == LessonType.Article && l.SortOrder == 3);
        if (articleLesson is null) return;

        var quiz = new Quiz
        {
            Title = "基本母音小測驗",
            Description = "測試你對 10 個基本母音的掌握程度",
            LessonId = articleLesson.Id,
            PassingScore = 70,
            TimeLimitMinutes = 10,
            Questions =
            [
                new QuizQuestion
                {
                    Content = "韓文字母 ㅏ 的羅馬拼音是什麼？",
                    Type = QuestionType.SingleChoice,
                    Points = 10,
                    SortOrder = 1,
                    Options =
                    [
                        new QuizOption { Content = "o", IsCorrect = false, SortOrder = 1 },
                        new QuizOption { Content = "a", IsCorrect = true, SortOrder = 2 },
                        new QuizOption { Content = "u", IsCorrect = false, SortOrder = 3 },
                        new QuizOption { Content = "e", IsCorrect = false, SortOrder = 4 }
                    ]
                },
                new QuizQuestion
                {
                    Content = "下列哪個是「陽性母音」？",
                    Type = QuestionType.SingleChoice,
                    Points = 10,
                    SortOrder = 2,
                    Options =
                    [
                        new QuizOption { Content = "ㅓ", IsCorrect = false, SortOrder = 1 },
                        new QuizOption { Content = "ㅜ", IsCorrect = false, SortOrder = 2 },
                        new QuizOption { Content = "ㅗ", IsCorrect = true, SortOrder = 3 },
                        new QuizOption { Content = "ㅡ", IsCorrect = false, SortOrder = 4 }
                    ]
                },
                new QuizQuestion
                {
                    Content = "ㅑ 的發音是在 ㅏ 前面加上什麼音？",
                    Type = QuestionType.SingleChoice,
                    Points = 10,
                    SortOrder = 3,
                    Options =
                    [
                        new QuizOption { Content = "w", IsCorrect = false, SortOrder = 1 },
                        new QuizOption { Content = "y", IsCorrect = true, SortOrder = 2 },
                        new QuizOption { Content = "n", IsCorrect = false, SortOrder = 3 },
                        new QuizOption { Content = "h", IsCorrect = false, SortOrder = 4 }
                    ]
                },
                new QuizQuestion
                {
                    Content = "「嘴唇圓起，發烏的音」是哪個母音？",
                    Type = QuestionType.SingleChoice,
                    Points = 10,
                    SortOrder = 4,
                    Options =
                    [
                        new QuizOption { Content = "ㅡ", IsCorrect = false, SortOrder = 1 },
                        new QuizOption { Content = "ㅗ", IsCorrect = false, SortOrder = 2 },
                        new QuizOption { Content = "ㅜ", IsCorrect = true, SortOrder = 3 },
                        new QuizOption { Content = "ㅓ", IsCorrect = false, SortOrder = 4 }
                    ]
                },
                new QuizQuestion
                {
                    Content = "請填寫：ㅗ 的羅馬拼音是 ____",
                    Type = QuestionType.FillInBlank,
                    Points = 10,
                    SortOrder = 5,
                    CorrectAnswer = "o"
                }
            ]
        };

        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync().ConfigureAwait(false);
        logger.LogInformation("建立測驗種子資料");
    }

    // ══════════════════════════════════════════════════════
    //  發音練習種子資料
    // ══════════════════════════════════════════════════════
    private static async Task SeedPronunciationExercisesAsync(ApplicationDbContext db, ILogger logger)
    {
        var exercises = new List<PronunciationExercise>
        {
            new() { Korean = "안녕하세요", Romanization = "annyeonghaseyo", Chinese = "你好", StandardAudioUrl = "/audio/pronunciation/annyeonghaseyo.mp3" },
            new() { Korean = "감사합니다", Romanization = "gamsahamnida", Chinese = "謝謝", StandardAudioUrl = "/audio/pronunciation/gamsahamnida.mp3" },
            new() { Korean = "죄송합니다", Romanization = "joesonghamnida", Chinese = "對不起", StandardAudioUrl = "/audio/pronunciation/joesonghamnida.mp3" },
            new() { Korean = "사랑해요", Romanization = "saranghaeyo", Chinese = "我愛你", StandardAudioUrl = "/audio/pronunciation/saranghaeyo.mp3" },
            new() { Korean = "만나서 반갑습니다", Romanization = "mannaseo bangapseumnida", Chinese = "很高興認識你", StandardAudioUrl = "/audio/pronunciation/mannaseo-bangapseumnida.mp3" },
            new() { Korean = "잘 먹겠습니다", Romanization = "jal meokgesseumnida", Chinese = "我要開動了", StandardAudioUrl = "/audio/pronunciation/jal-meokgesseumnida.mp3" },
            new() { Korean = "한국어", Romanization = "hangugeo", Chinese = "韓語", StandardAudioUrl = "/audio/pronunciation/hangugeo.mp3" },
            new() { Korean = "대한민국", Romanization = "daehanminguk", Chinese = "大韓民國", StandardAudioUrl = "/audio/pronunciation/daehanminguk.mp3" },
            new() { Korean = "맛있어요", Romanization = "masisseoyo", Chinese = "好吃", StandardAudioUrl = "/audio/pronunciation/masisseoyo.mp3" },
            new() { Korean = "어떻게 지내세요", Romanization = "eotteoke jinaeseyo", Chinese = "你過得怎麼樣？", StandardAudioUrl = "/audio/pronunciation/eotteoke-jinaeseyo.mp3" }
        };

        db.PronunciationExercises.AddRange(exercises);
        await db.SaveChangesAsync().ConfigureAwait(false);
        logger.LogInformation("建立 {Count} 個發音練習", exercises.Count);
    }

    // ══════════════════════════════════════════════════════
    //  學生體驗資料（選課、訂單、進度、討論）
    // ══════════════════════════════════════════════════════
    private static async Task SeedStudentDataAsync(
        ApplicationDbContext db, string studentId, string teacherId, ILogger logger)
    {
        if (await db.Enrollments.AnyAsync().ConfigureAwait(false))
        {
            logger.LogInformation("學生體驗資料已存在，跳過");
            return;
        }

        var courses = await db.Courses.OrderBy(c => c.SortOrder).ToListAsync().ConfigureAwait(false);
        if (courses.Count == 0) return;

        var course1 = courses[0];
        var now = DateTime.UtcNow;

        // ── 訂單（已付款）──────────────────────────────
        var order = new Order
        {
            OrderNumber = $"KL-{now:yyyyMMdd}-0001",
            TotalAmount = course1.Price,
            Status = OrderStatus.Completed,
            PaymentStatus = PaymentStatus.Completed,
            PaidAt = now.AddDays(-7),
            UserId = studentId,
            CreatedAt = now.AddDays(-7),
            UpdatedAt = now.AddDays(-7),
            OrderItems =
            [
                new OrderItem
                {
                    CourseId = course1.Id,
                    Price = course1.Price,
                    CreatedAt = now.AddDays(-7),
                    UpdatedAt = now.AddDays(-7)
                }
            ]
        };
        db.Orders.Add(order);

        // ── 選課紀錄 ──────────────────────────────────
        var enrollment = new Enrollment
        {
            UserId = studentId,
            CourseId = course1.Id,
            Status = EnrollmentStatus.Active,
            ProgressPercent = 25,
            CreatedAt = now.AddDays(-7),
            UpdatedAt = now
        };
        db.Enrollments.Add(enrollment);

        // ── 學習進度（完成前兩個 Lesson）──────────────
        var lessons = await db.Lessons
            .Where(l => l.Section.CourseId == course1.Id)
            .OrderBy(l => l.Section.SortOrder).ThenBy(l => l.SortOrder)
            .Take(2)
            .ToListAsync().ConfigureAwait(false);

        foreach (var lesson in lessons)
        {
            db.Progresses.Add(new Progress
            {
                UserId = studentId,
                LessonId = lesson.Id,
                IsCompleted = true,
                CompletedAt = now.AddDays(-5),
                CreatedAt = now.AddDays(-5),
                UpdatedAt = now.AddDays(-5)
            });
        }

        // ── 討論區（含回覆）──────────────────────────
        var discussion = new Discussion
        {
            Title = "韓文字母學習心得分享",
            Content = "大家好！我剛開始學韓文，發現母音的嘴型變化真的很重要。特別是 ㅓ 和 ㅗ 的差別，一開始很容易搞混。有人跟我一樣嗎？大家有什麼好的練習方法嗎？",
            UserId = studentId,
            CourseId = course1.Id,
            CreatedAt = now.AddDays(-3),
            UpdatedAt = now.AddDays(-3),
            Replies =
            [
                new DiscussionReply
                {
                    Content = "你好！我建議你可以對著鏡子練習，注意嘴巴的形狀。ㅓ 是嘴巴稍微張開，而 ㅗ 是嘴唇圓起來。多聽多練就會自然分辨了！加油！",
                    UserId = teacherId,
                    CreatedAt = now.AddDays(-2),
                    UpdatedAt = now.AddDays(-2)
                },
                new DiscussionReply
                {
                    Content = "推薦看 YouTube 上的口型教學影片，看嘴巴的形狀比光用聽的更容易理解差異。韓文母音的邏輯性很強，掌握規律後很好記！",
                    UserId = studentId,
                    CreatedAt = now.AddDays(-1),
                    UpdatedAt = now.AddDays(-1)
                }
            ]
        };
        db.Discussions.Add(discussion);

        var discussion2 = new Discussion
        {
            Title = "收尾音（받침）怎麼練習比較好？",
            Content = "學到收尾音了，7 個代表音搞得我頭好痛。特別是 ㄷ 代表音對應那麼多收尾音（ㄷ, ㅌ, ㅅ, ㅆ, ㅈ, ㅊ, ㅎ），真的很難記。有沒有什麼口訣或記憶方法？",
            UserId = studentId,
            CourseId = course1.Id,
            CreatedAt = now.AddDays(-1),
            UpdatedAt = now.AddDays(-1)
        };
        db.Discussions.Add(discussion2);

        await db.SaveChangesAsync().ConfigureAwait(false);
        logger.LogInformation("建立學生體驗資料（訂單、選課、進度、討論）");
    }

    // ══════════════════════════════════════════════════════
    //  公告種子資料
    // ══════════════════════════════════════════════════════
    private static async Task SeedAnnouncementsAsync(ApplicationDbContext db, ILogger logger)
    {
        if (await db.Announcements.AnyAsync().ConfigureAwait(false))
        {
            logger.LogInformation("公告種子資料已存在，跳過");
            return;
        }

        db.Announcements.AddRange(
            new Announcement
            {
                Title = "🎉 歡迎來到 KoreanLearn！",
                Content = "感謝您加入韓文學習平台。我們提供從入門到進階的完整課程，包含影片教學、文章講義、PDF 練習單、字卡複習和測驗系統，幫助你系統化地學習韓語。",
                IsActive = true,
                IsPinned = true,
                SortOrder = 0,
                StartDate = DateTime.UtcNow,
            },
            new Announcement
            {
                Title = "📚 全新影片課程上線",
                Content = "我們為每門課程加入了專業的影片教學單元，由金老師親自錄製，包含發音示範、口型特寫與慢速練習。搭配字卡系統的間隔重複複習法，讓你的學習效果加倍！",
                IsActive = true,
                SortOrder = 1,
                StartDate = DateTime.UtcNow,
            },
            new Announcement
            {
                Title = "🏆 TOPIK 備考課程上線",
                Content = "全新「TOPIK 備考：中高級衝刺」課程已上線，涵蓋聽力、閱讀、寫作三大題型的完整攻略，附有 PDF 必考文法整理。",
                IsActive = true,
                SortOrder = 2,
                StartDate = DateTime.UtcNow,
            }
        );
        await db.SaveChangesAsync().ConfigureAwait(false);
        logger.LogInformation("建立範例公告");
    }

    // ══════════════════════════════════════════════════════
    //  訂閱方案種子資料
    // ══════════════════════════════════════════════════════
    private static async Task SeedSubscriptionPlansAsync(ApplicationDbContext db, ILogger logger)
    {
        if (await db.SubscriptionPlans.AnyAsync().ConfigureAwait(false))
        {
            logger.LogInformation("訂閱方案種子資料已存在，跳過");
            return;
        }

        db.SubscriptionPlans.AddRange(
            new SubscriptionPlan
            {
                Name = "月訂閱",
                Description = "每月自動續訂，隨時可取消。適合想先試試看的學習者。",
                MonthlyPrice = 299m,
                DurationMonths = 1,
                IsActive = true
            },
            new SubscriptionPlan
            {
                Name = "季訂閱",
                Description = "一次購買 3 個月，每月只要 NT$249，省下 15%。",
                MonthlyPrice = 249m,
                DurationMonths = 3,
                IsActive = true
            },
            new SubscriptionPlan
            {
                Name = "年訂閱",
                Description = "最超值方案！一次購買 12 個月，每月只要 NT$199，省下 33%。",
                MonthlyPrice = 199m,
                DurationMonths = 12,
                IsActive = true
            }
        );
        await db.SaveChangesAsync().ConfigureAwait(false);
        logger.LogInformation("建立訂閱方案種子資料");
    }
}
