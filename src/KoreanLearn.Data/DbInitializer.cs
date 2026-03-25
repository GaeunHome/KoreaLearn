using KoreanLearn.Data.Entities;
using KoreanLearn.Library.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Data;

public static class DbInitializer
{
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
        await SeedUsersAsync(userManager, logger).ConfigureAwait(false);
        await SeedCoursesAsync(db, logger).ConfigureAwait(false);
        await SeedAnnouncementsAsync(db, logger).ConfigureAwait(false);
        await SeedSubscriptionPlansAsync(db, logger).ConfigureAwait(false);
        logger.LogInformation("資料庫初始化完成");
    }

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

    private static async Task SeedUsersAsync(UserManager<AppUser> userManager, ILogger logger)
    {
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
            logger.LogInformation("建立測試學生帳號: {Email}", student.Email);
        }

        if (await userManager.FindByEmailAsync("teacher@koreanlearn.com").ConfigureAwait(false) is null)
        {
            var teacher = new AppUser
            {
                UserName = "teacher@koreanlearn.com",
                Email = "teacher@koreanlearn.com",
                DisplayName = "韓語教師",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await userManager.CreateAsync(teacher, "Teacher@123").ConfigureAwait(false);
            await userManager.AddToRoleAsync(teacher, "Teacher").ConfigureAwait(false);
            logger.LogInformation("建立測試教師帳號: {Email}", teacher.Email);
        }
    }

    private static async Task SeedCoursesAsync(ApplicationDbContext db, ILogger logger)
    {
        if (await db.Courses.AnyAsync().ConfigureAwait(false))
        {
            logger.LogInformation("課程種子資料已存在，跳過");
            return;
        }

        var courses = new List<Course>
        {
            // ── 入門課程 ──
            new()
            {
                Title = "韓語入門：從零開始學韓文",
                Description = "專為完全零基礎的學習者設計的韓語入門課程。從韓文字母（한글）的由來與結構開始，逐步學會 40 個基本字母的發音與書寫，再到音節組合、收尾音規則，最後能夠自己拼讀簡單的韓文單字與句子。課程附有大量發音範例與練習題，讓你打下扎實的韓語基礎。",
                Price = 990m,
                Level = DifficultyLevel.Beginner,
                IsPublished = true,
                SortOrder = 1,
                Sections =
                [
                    new Section
                    {
                        Title = "第一章：認識韓文字母 한글",
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
<p>韓文字母 한글（Hangul）由朝鮮王朝第四代國王<strong>世宗大王（세종대왕）</strong>於 1443 年創制，1446 年正式頒布《訓民正音》。世宗大王認為當時使用的漢字對一般百姓來說過於困難，因此決定創造一套所有人都能輕鬆學會的文字系統。</p>

<h3>한글 的結構特色</h3>
<p>韓文是一種<strong>音素文字</strong>，每個字母代表一個音素。韓文字母共有 40 個：</p>
<ul>
  <li><strong>基本母音</strong>（기본 모음）：10 個 — ㅏ ㅑ ㅓ ㅕ ㅗ ㅛ ㅜ ㅠ ㅡ ㅣ</li>
  <li><strong>複合母音</strong>（이중 모음）：11 個 — ㅐ ㅒ ㅔ ㅖ ㅘ ㅙ ㅚ ㅝ ㅞ ㅟ ㅢ</li>
  <li><strong>基本子音</strong>（기본 자음）：14 個 — ㄱ ㄴ ㄷ ㄹ ㄅ ㅂ ㅅ ㅇ ㅈ ㅊ ㅋ ㅌ ㅍ ㅎ</li>
  <li><strong>雙子音</strong>（쌍자음）：5 個 — ㄲ ㄸ ㅃ ㅆ ㅉ</li>
</ul>

<h3>音節組合方式</h3>
<p>韓文的音節由<strong>初聲（子音）+ 中聲（母音）</strong>組合而成，有些音節還會加上<strong>終聲（收尾音，받침）</strong>：</p>
<table class='table table-bordered'>
  <thead><tr><th>結構</th><th>範例</th><th>發音</th></tr></thead>
  <tbody>
    <tr><td>子音 + 母音</td><td>나</td><td>na</td></tr>
    <tr><td>子音 + 母音 + 收尾音</td><td>한</td><td>han</td></tr>
    <tr><td>子音 + 母音 + 雙收尾音</td><td>닭</td><td>dak</td></tr>
  </tbody>
</table>

<h3>為什麼韓文被稱為「最科學的文字」？</h3>
<p>語言學家普遍認為韓文是世界上最合理、最科學的書寫系統之一。子音字母的形狀模仿了發音時<strong>口腔器官的形狀</strong>：</p>
<ul>
  <li>ㄱ（g/k）— 舌根觸及軟顎的側面形狀</li>
  <li>ㄴ（n）— 舌尖頂住上顎的形狀</li>
  <li>ㅁ（m）— 嘴巴閉合的形狀</li>
  <li>ㅅ（s）— 牙齒的形狀</li>
  <li>ㅇ（ng）— 喉嚨的形狀</li>
</ul>"
                            },
                            new Lesson
                            {
                                Title = "基本母音（모음）：ㅏ ㅑ ㅓ ㅕ ㅗ ㅛ ㅜ ㅠ ㅡ ㅣ",
                                Type = LessonType.Article,
                                SortOrder = 2,
                                ArticleContent = @"<h2>10 個基本母音</h2>
<p>韓文的母音可以分為<strong>陽性母音</strong>和<strong>陰性母音</strong>，這個概念源自東方的陰陽哲學：</p>

<h3>陽性母音（양성 모음）</h3>
<table class='table table-bordered'>
  <thead><tr><th>字母</th><th>羅馬拼音</th><th>發音說明</th><th>類似中文音</th></tr></thead>
  <tbody>
    <tr><td style='font-size:1.5rem'>ㅏ</td><td>a</td><td>嘴巴張大，發「啊」的音</td><td>ㄚ</td></tr>
    <tr><td style='font-size:1.5rem'>ㅑ</td><td>ya</td><td>先發「一」再接「啊」</td><td>一ㄚ</td></tr>
    <tr><td style='font-size:1.5rem'>ㅗ</td><td>o</td><td>嘴唇圓起，發「喔」的音</td><td>ㄛ</td></tr>
    <tr><td style='font-size:1.5rem'>ㅛ</td><td>yo</td><td>先發「一」再接「喔」</td><td>一ㄛ</td></tr>
  </tbody>
</table>

<h3>陰性母音（음성 모음）</h3>
<table class='table table-bordered'>
  <thead><tr><th>字母</th><th>羅馬拼音</th><th>發音說明</th><th>類似中文音</th></tr></thead>
  <tbody>
    <tr><td style='font-size:1.5rem'>ㅓ</td><td>eo</td><td>嘴巴微張，介於「喔」和「餓」之間</td><td>ㄜ</td></tr>
    <tr><td style='font-size:1.5rem'>ㅕ</td><td>yeo</td><td>先發「一」再接 ㅓ 的音</td><td>一ㄜ</td></tr>
    <tr><td style='font-size:1.5rem'>ㅜ</td><td>u</td><td>嘴唇前突圓起，發「烏」的音</td><td>ㄨ</td></tr>
    <tr><td style='font-size:1.5rem'>ㅠ</td><td>yu</td><td>先發「一」再接「烏」</td><td>一ㄨ</td></tr>
  </tbody>
</table>

<h3>中性母音（중성 모음）</h3>
<table class='table table-bordered'>
  <thead><tr><th>字母</th><th>羅馬拼音</th><th>發音說明</th><th>類似中文音</th></tr></thead>
  <tbody>
    <tr><td style='font-size:1.5rem'>ㅡ</td><td>eu</td><td>嘴巴左右拉開，嘴唇不圓，類似微笑狀</td><td>ㄜ（扁嘴）</td></tr>
    <tr><td style='font-size:1.5rem'>ㅣ</td><td>i</td><td>嘴角向兩旁拉開，發「衣」的音</td><td>一</td></tr>
  </tbody>
</table>

<h3>記憶技巧</h3>
<p>觀察母音的結構，會發現一個規律：加了一短橫「ㅑ ㅕ ㅛ ㅠ」就是在對應母音前面加一個 <strong>y</strong> 的音。</p>"
                            },
                            new Lesson
                            {
                                Title = "基本子音（자음）：ㄱ ㄴ ㄷ ㄹ ㅁ ㅂ ㅅ ㅇ ㅈ",
                                Type = LessonType.Article,
                                SortOrder = 3,
                                ArticleContent = @"<h2>14 個基本子音</h2>
<p>韓文子音（자음）共有 14 個基本字母和 5 個雙子音。本課先學習前 9 個最常用的基本子音。</p>

<table class='table table-bordered'>
  <thead><tr><th>字母</th><th>名稱</th><th>初聲發音</th><th>終聲發音</th><th>範例單字</th></tr></thead>
  <tbody>
    <tr><td style='font-size:1.5rem'>ㄱ</td><td>기역 giyeok</td><td>g / k</td><td>k</td><td>가방（包包）、국（湯）</td></tr>
    <tr><td style='font-size:1.5rem'>ㄴ</td><td>니은 nieun</td><td>n</td><td>n</td><td>나라（國家）、눈（眼睛/雪）</td></tr>
    <tr><td style='font-size:1.5rem'>ㄷ</td><td>디귿 digeut</td><td>d / t</td><td>t</td><td>다리（橋/腿）、달（月亮）</td></tr>
    <tr><td style='font-size:1.5rem'>ㄹ</td><td>리을 rieul</td><td>r / l</td><td>l</td><td>라면（泡麵）、달（月亮）</td></tr>
    <tr><td style='font-size:1.5rem'>ㅁ</td><td>미음 mieum</td><td>m</td><td>m</td><td>마음（心）、밤（夜晚/栗子）</td></tr>
    <tr><td style='font-size:1.5rem'>ㅂ</td><td>비읍 bieup</td><td>b / p</td><td>p</td><td>바다（海）、밥（飯）</td></tr>
    <tr><td style='font-size:1.5rem'>ㅅ</td><td>시옷 siot</td><td>s</td><td>t</td><td>사람（人）、산（山）</td></tr>
    <tr><td style='font-size:1.5rem'>ㅇ</td><td>이응 ieung</td><td>無聲（佔位）</td><td>ng</td><td>아이（小孩）、강（江）</td></tr>
    <tr><td style='font-size:1.5rem'>ㅈ</td><td>지읒 jieut</td><td>j / ch</td><td>t</td><td>자동차（汽車）、점심（午餐）</td></tr>
  </tbody>
</table>

<h3>發音小提醒</h3>
<ul>
  <li>ㄱ、ㄷ、ㅂ、ㅈ 在字首時發音較輕（不送氣），在收尾音位置時發音不同</li>
  <li>ㅇ 在初聲位置<strong>不發音</strong>，只是佔位用；在終聲位置才發 ng 的鼻音</li>
  <li>ㄹ 在母音前發 r 音，在收尾音或連音時發 l 音</li>
</ul>"
                            },
                            new Lesson
                            {
                                Title = "送氣音與緊音：ㅊ ㅋ ㅌ ㅍ ㅎ ㄲ ㄸ ㅃ ㅆ ㅉ",
                                Type = LessonType.Article,
                                SortOrder = 4,
                                ArticleContent = @"<h2>送氣音（격음）</h2>
<p>送氣音是發音時帶有明顯氣流的子音。你可以在嘴巴前放一張紙，發音時紙會被吹動。</p>

<table class='table table-bordered'>
  <thead><tr><th>送氣音</th><th>對應基本音</th><th>發音</th><th>範例</th></tr></thead>
  <tbody>
    <tr><td style='font-size:1.3rem'>ㅊ</td><td>ㅈ</td><td>ch（氣流強）</td><td>차（茶/車）</td></tr>
    <tr><td style='font-size:1.3rem'>ㅋ</td><td>ㄱ</td><td>k（氣流強）</td><td>커피（咖啡）</td></tr>
    <tr><td style='font-size:1.3rem'>ㅌ</td><td>ㄷ</td><td>t（氣流強）</td><td>토끼（兔子）</td></tr>
    <tr><td style='font-size:1.3rem'>ㅍ</td><td>ㅂ</td><td>p（氣流強）</td><td>포도（葡萄）</td></tr>
    <tr><td style='font-size:1.3rem'>ㅎ</td><td>—</td><td>h</td><td>하늘（天空）</td></tr>
  </tbody>
</table>

<h2>雙子音 / 緊音（쌍자음 / 경음）</h2>
<p>緊音是發音時聲帶緊繃、不送氣的音。喉嚨要有點緊張的感覺。</p>

<table class='table table-bordered'>
  <thead><tr><th>緊音</th><th>對應基本音</th><th>發音</th><th>範例</th></tr></thead>
  <tbody>
    <tr><td style='font-size:1.3rem'>ㄲ</td><td>ㄱ</td><td>kk</td><td>꽃（花）、까치（喜鵲）</td></tr>
    <tr><td style='font-size:1.3rem'>ㄸ</td><td>ㄷ</td><td>tt</td><td>떡（年糕）、딸기（草莓）</td></tr>
    <tr><td style='font-size:1.3rem'>ㅃ</td><td>ㅂ</td><td>pp</td><td>빵（麵包）、빨리（快）</td></tr>
    <tr><td style='font-size:1.3rem'>ㅆ</td><td>ㅅ</td><td>ss</td><td>쓰다（寫/苦）、싸다（便宜）</td></tr>
    <tr><td style='font-size:1.3rem'>ㅉ</td><td>ㅈ</td><td>jj</td><td>짜다（鹹）、찌개（鍋）</td></tr>
  </tbody>
</table>

<h3>三種子音的比較</h3>
<p>以 ㄱ 系列為例：</p>
<ul>
  <li><strong>ㄱ</strong>（基本音）：가 — 聲帶輕鬆，不送氣</li>
  <li><strong>ㅋ</strong>（送氣音）：카 — 聲帶輕鬆，送氣</li>
  <li><strong>ㄲ</strong>（緊音）：까 — 聲帶緊繃，不送氣</li>
</ul>"
                            },
                            new Lesson
                            {
                                Title = "收尾音（받침）完全攻略",
                                Type = LessonType.Article,
                                SortOrder = 5,
                                ArticleContent = @"<h2>什麼是收尾音（받침）？</h2>
<p>收尾音是韓文音節最下方的子音。雖然有 27 種書寫形式，但實際發音只有 <strong>7 種代表音</strong>。</p>

<h3>七大代表收尾音</h3>
<table class='table table-bordered'>
  <thead><tr><th>代表音</th><th>發音</th><th>對應的收尾音</th><th>範例</th></tr></thead>
  <tbody>
    <tr><td>ㄱ [k]</td><td>喉嚨後方閉合</td><td>ㄱ, ㅋ, ㄲ</td><td>국（湯）、부엌（廚房）</td></tr>
    <tr><td>ㄴ [n]</td><td>舌尖頂上顎</td><td>ㄴ</td><td>산（山）、눈（眼睛）</td></tr>
    <tr><td>ㄷ [t]</td><td>舌尖頂上齒齦</td><td>ㄷ, ㅌ, ㅅ, ㅆ, ㅈ, ㅊ, ㅎ</td><td>맛（味道）、꽃（花）</td></tr>
    <tr><td>ㄹ [l]</td><td>舌尖頂上顎不放</td><td>ㄹ</td><td>달（月亮）、발（腳）</td></tr>
    <tr><td>ㅁ [m]</td><td>雙唇閉合</td><td>ㅁ</td><td>밤（夜晚）、봄（春天）</td></tr>
    <tr><td>ㅂ [p]</td><td>雙唇閉合</td><td>ㅂ, ㅍ</td><td>밥（飯）、앞（前面）</td></tr>
    <tr><td>ㅇ [ng]</td><td>鼻腔共鳴</td><td>ㅇ</td><td>강（江）、방（房間）</td></tr>
  </tbody>
</table>

<h3>連音規則（연음법칙）</h3>
<p>當有收尾音的字後面接著以 ㅇ（無聲）開頭的字時，收尾音會「移動」到下一個字的初聲位置發音：</p>
<ul>
  <li>한국어 → 한구거 [han-gu-geo]</li>
  <li>음악 → 으막 [eu-mak]</li>
  <li>독일 → 도길 [do-gil]</li>
</ul>"
                            }
                        ]
                    },
                    new Section
                    {
                        Title = "第二章：基本問候與自我介紹",
                        SortOrder = 2,
                        Lessons =
                        [
                            new Lesson
                            {
                                Title = "打招呼：안녕하세요！",
                                Type = LessonType.Article,
                                SortOrder = 1,
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
  </tbody>
</table>

<h3>實用對話</h3>
<div style='background:#f0f4f8; padding:1rem; border-radius:8px; margin:1rem 0;'>
  <p><strong>A:</strong> 안녕하세요! (你好！)</p>
  <p><strong>B:</strong> 안녕하세요! 만나서 반갑습니다. (你好！很高興認識你。)</p>
  <p><strong>A:</strong> 저도 반갑습니다. (我也很高興。)</p>
</div>"
                            },
                            new Lesson
                            {
                                Title = "自我介紹：저는 ___입니다",
                                Type = LessonType.Article,
                                SortOrder = 2,
                                ArticleContent = @"<h2>韓語自我介紹</h2>

<h3>基本句型</h3>
<table class='table table-bordered'>
  <thead><tr><th>韓文</th><th>中文</th><th>說明</th></tr></thead>
  <tbody>
    <tr><td>저는 [名字]입니다.</td><td>我是 [名字]。</td><td>正式自我介紹</td></tr>
    <tr><td>저는 [國家]사람입니다.</td><td>我是 [國家]人。</td><td>說明國籍</td></tr>
    <tr><td>저는 [職業]입니다.</td><td>我是 [職業]。</td><td>說明工作</td></tr>
    <tr><td>[數字]살입니다.</td><td>我 [數字] 歲。</td><td>說明年齡（用固有數字）</td></tr>
    <tr><td>취미는 [興趣]입니다.</td><td>興趣是 [興趣]。</td><td>說明喜好</td></tr>
  </tbody>
</table>

<h3>常用國家名稱</h3>
<table class='table table-bordered'>
  <thead><tr><th>韓文</th><th>中文</th></tr></thead>
  <tbody>
    <tr><td>대만（臺灣）/ 타이완</td><td>臺灣</td></tr>
    <tr><td>한국</td><td>韓國</td></tr>
    <tr><td>일본</td><td>日本</td></tr>
    <tr><td>미국</td><td>美國</td></tr>
    <tr><td>중국</td><td>中國</td></tr>
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
<p><em>（你好。我是林美。我是臺灣人。我 25 歲。職業是學生。興趣是看韓劇。很高興認識你！）</em></p>"
                            },
                            new Lesson
                            {
                                Title = "韓語數字系統：固有數字與漢字數字",
                                Type = LessonType.Article,
                                SortOrder = 3,
                                ArticleContent = @"<h2>韓文的兩套數字</h2>
<p>韓語有兩套數字系統，使用場合不同，這是初學者最容易混淆的地方。</p>

<h3>漢字數字（한자 숫자）</h3>
<p>用於日期、月份、金額、電話號碼、地址。</p>
<table class='table table-bordered'>
  <thead><tr><th>數字</th><th>韓文</th><th>羅馬拼音</th></tr></thead>
  <tbody>
    <tr><td>1</td><td>일</td><td>il</td></tr>
    <tr><td>2</td><td>이</td><td>i</td></tr>
    <tr><td>3</td><td>삼</td><td>sam</td></tr>
    <tr><td>4</td><td>사</td><td>sa</td></tr>
    <tr><td>5</td><td>오</td><td>o</td></tr>
    <tr><td>6</td><td>육</td><td>yuk</td></tr>
    <tr><td>7</td><td>칠</td><td>chil</td></tr>
    <tr><td>8</td><td>팔</td><td>pal</td></tr>
    <tr><td>9</td><td>구</td><td>gu</td></tr>
    <tr><td>10</td><td>십</td><td>sip</td></tr>
  </tbody>
</table>

<h3>固有數字（고유 숫자）</h3>
<p>用於年齡、時（幾點）、數量詞。1~4 在接量詞時會變形。</p>
<table class='table table-bordered'>
  <thead><tr><th>數字</th><th>韓文</th><th>接量詞時</th></tr></thead>
  <tbody>
    <tr><td>1</td><td>하나</td><td>한</td></tr>
    <tr><td>2</td><td>둘</td><td>두</td></tr>
    <tr><td>3</td><td>셋</td><td>세</td></tr>
    <tr><td>4</td><td>넷</td><td>네</td></tr>
    <tr><td>5</td><td>다섯</td><td>다섯</td></tr>
    <tr><td>10</td><td>열</td><td>열</td></tr>
    <tr><td>20</td><td>스물</td><td>스무</td></tr>
  </tbody>
</table>

<h3>使用場合比較</h3>
<table class='table table-bordered'>
  <thead><tr><th>場合</th><th>使用</th><th>範例</th></tr></thead>
  <tbody>
    <tr><td>年齡</td><td>固有數字</td><td>스물다섯 살（25 歲）</td></tr>
    <tr><td>幾點</td><td>固有數字</td><td>세 시（3 點）</td></tr>
    <tr><td>幾分</td><td>漢字數字</td><td>삼십 분（30 分）</td></tr>
    <tr><td>日期</td><td>漢字數字</td><td>삼월 이십오일（3 月 25 日）</td></tr>
    <tr><td>價格</td><td>漢字數字</td><td>만 원（一萬元）</td></tr>
    <tr><td>數量</td><td>固有數字</td><td>두 개（兩個）</td></tr>
  </tbody>
</table>"
                            }
                        ]
                    }
                ]
            },

            // ── 初級課程 ──
            new()
            {
                Title = "韓語初級：生活會話",
                Description = "學習日常生活中最實用的韓語會話。本課程涵蓋在韓國旅遊、餐廳點餐、便利商店購物、搭乘交通工具、問路等真實場景。每個單元都有情境對話、關鍵句型與文法解析，搭配大量練習讓你能實際開口說韓語。",
                Price = 1490m,
                Level = DifficultyLevel.Elementary,
                IsPublished = true,
                SortOrder = 2,
                Sections =
                [
                    new Section
                    {
                        Title = "第一章：在餐廳用餐",
                        SortOrder = 1,
                        Lessons =
                        [
                            new Lesson
                            {
                                Title = "點餐用語與菜單閱讀",
                                Type = LessonType.Article,
                                SortOrder = 1,
                                IsFreePreview = true,
                                ArticleContent = @"<h2>韓國餐廳實用會話</h2>

<h3>進入餐廳</h3>
<div style='background:#f0f4f8; padding:1rem; border-radius:8px; margin:1rem 0;'>
  <p><strong>店員：</strong> 어서오세요! 몇 분이세요?（歡迎光臨！請問幾位？）</p>
  <p><strong>你：</strong> 두 명이요.（兩位。）</p>
  <p><strong>店員：</strong> 이쪽으로 앉으세요.（請坐這邊。）</p>
</div>

<h3>常用點餐句型</h3>
<table class='table table-bordered'>
  <thead><tr><th>韓文</th><th>中文</th></tr></thead>
  <tbody>
    <tr><td>메뉴판 주세요.</td><td>請給我菜單。</td></tr>
    <tr><td>이거 주세요.</td><td>請給我這個。</td></tr>
    <tr><td>_____ 하나 주세요.</td><td>請給我一份 _____。</td></tr>
    <tr><td>이건 뭐예요?</td><td>這是什麼？</td></tr>
    <tr><td>매운 거예요?</td><td>這是辣的嗎？</td></tr>
    <tr><td>안 맵게 해 주세요.</td><td>請做不辣的。</td></tr>
    <tr><td>물 좀 주세요.</td><td>請給我水。</td></tr>
  </tbody>
</table>

<h3>韓國料理名稱</h3>
<table class='table table-bordered'>
  <thead><tr><th>韓文</th><th>中文</th><th>說明</th></tr></thead>
  <tbody>
    <tr><td>비빔밥</td><td>拌飯</td><td>韓國最具代表性的料理</td></tr>
    <tr><td>김치찌개</td><td>泡菜鍋</td><td>韓國人最常吃的鍋物</td></tr>
    <tr><td>된장찌개</td><td>大醬湯</td><td>韓式味噌湯</td></tr>
    <tr><td>불고기</td><td>烤肉</td><td>醬油醃漬的烤牛肉</td></tr>
    <tr><td>삼겹살</td><td>五花肉</td><td>韓式烤五花肉</td></tr>
    <tr><td>냉면</td><td>冷麵</td><td>夏天必吃</td></tr>
    <tr><td>떡볶이</td><td>辣炒年糕</td><td>韓國街頭小吃代表</td></tr>
    <tr><td>김밥</td><td>紫菜飯捲</td><td>韓國版壽司</td></tr>
  </tbody>
</table>"
                            },
                            new Lesson
                            {
                                Title = "結帳與表達感受",
                                Type = LessonType.Article,
                                SortOrder = 2,
                                ArticleContent = @"<h2>結帳用語</h2>

<h3>結帳對話</h3>
<div style='background:#f0f4f8; padding:1rem; border-radius:8px; margin:1rem 0;'>
  <p><strong>你：</strong> 여기요, 계산해 주세요.（不好意思，請幫我結帳。）</p>
  <p><strong>店員：</strong> 총 만오천 원입니다.（總共一萬五千元。）</p>
  <p><strong>你：</strong> 카드 돼요?（可以刷卡嗎？）</p>
  <p><strong>店員：</strong> 네, 됩니다.（是的，可以。）</p>
  <p><strong>你：</strong> 잘 먹었습니다!（我吃得很好！/ 謝謝招待！）</p>
</div>

<h3>味道表達</h3>
<table class='table table-bordered'>
  <thead><tr><th>韓文</th><th>中文</th></tr></thead>
  <tbody>
    <tr><td>맛있어요!</td><td>好吃！</td></tr>
    <tr><td>맛없어요.</td><td>不好吃。</td></tr>
    <tr><td>매워요!</td><td>好辣！</td></tr>
    <tr><td>달아요.</td><td>好甜。</td></tr>
    <tr><td>짜요.</td><td>好鹹。</td></tr>
    <tr><td>써요.</td><td>好苦。</td></tr>
    <tr><td>시원해요!</td><td>好爽口！（常用於湯品）</td></tr>
  </tbody>
</table>

<h3>文法重點：-아/어 주세요（請幫我...）</h3>
<p>這是非常實用的請求句型，意思是「請幫我做...」：</p>
<ul>
  <li>사진 <strong>찍어 주세요</strong>.（請幫我拍照。）</li>
  <li>천천히 <strong>말해 주세요</strong>.（請慢慢說。）</li>
  <li>다시 한번 <strong>말해 주세요</strong>.（請再說一次。）</li>
  <li>여기 <strong>써 주세요</strong>.（請寫在這裡。）</li>
</ul>"
                            }
                        ]
                    },
                    new Section
                    {
                        Title = "第二章：交通與問路",
                        SortOrder = 2,
                        Lessons =
                        [
                            new Lesson
                            {
                                Title = "搭乘地鐵與公車",
                                Type = LessonType.Article,
                                SortOrder = 1,
                                ArticleContent = @"<h2>韓國交通用語</h2>

<h3>搭地鐵</h3>
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

<h3>問路會話</h3>
<div style='background:#f0f4f8; padding:1rem; border-radius:8px; margin:1rem 0;'>
  <p><strong>你：</strong> 실례합니다. 명동이 어디예요?（不好意思，明洞在哪裡？）</p>
  <p><strong>路人：</strong> 여기서 직진하세요. 그리고 두 번째 사거리에서 오른쪽으로 가세요.</p>
  <p>（從這裡直走，然後在第二個十字路口右轉。）</p>
  <p><strong>你：</strong> 걸어서 얼마나 걸려요?（走路要多久？）</p>
  <p><strong>路人：</strong> 한 십 분 정도 걸려요.（大約十分鐘。）</p>
  <p><strong>你：</strong> 감사합니다!（謝謝！）</p>
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
    <tr><td>교환 / 환불 돼요?</td><td>可以換貨/退貨嗎？</td></tr>
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
                    }
                ]
            },

            // ── 中級課程 ──
            new()
            {
                Title = "韓語中級：文法深化",
                Description = "針對已有基礎會話能力的學習者，深入解析韓語文法體系。本課程涵蓋連接語尾（-고, -아서/-어서, -(으)면）、語尾變化、間接引用、被動與使動表現、敬語體系等核心文法。每個文法點都附有豐富的例句與比較分析，幫助你從「能溝通」提升到「說得準確」。",
                Price = 1990m,
                Level = DifficultyLevel.Intermediate,
                IsPublished = true,
                SortOrder = 3,
                Sections =
                [
                    new Section
                    {
                        Title = "第一章：連接語尾大全",
                        SortOrder = 1,
                        Lessons =
                        [
                            new Lesson
                            {
                                Title = "原因與理由：-아서/어서 vs -(으)니까",
                                Type = LessonType.Article,
                                SortOrder = 1,
                                IsFreePreview = true,
                                ArticleContent = @"<h2>表示原因的兩種連接語尾</h2>
<p>韓語中表示「因為...所以...」最常用的有兩種：<strong>-아서/어서</strong> 和 <strong>-(으)니까</strong>。很多學習者會搞混，本課幫你徹底釐清。</p>

<h3>-아서/어서（客觀原因）</h3>
<p>用於陳述客觀事實作為原因，後句<strong>不能接命令、勸誘句</strong>。</p>
<ul>
  <li>비가 <strong>와서</strong> 집에 있었어요.（因為下雨，所以待在家。）</li>
  <li>배가 <strong>고파서</strong> 밥을 먹었어요.（因為肚子餓，所以吃了飯。）</li>
  <li>시간이 <strong>없어서</strong> 못 갔어요.（因為沒時間，所以沒去。）</li>
</ul>

<h3>-(으)니까（主觀理由 / 建議命令）</h3>
<p>帶有說話者的主觀判斷，後句<strong>可以接命令、勸誘句</strong>。</p>
<ul>
  <li>비가 <strong>오니까</strong> 우산을 가져가세요.（因為下雨，請帶傘。）</li>
  <li>시간이 <strong>없으니까</strong> 빨리 가자!（因為沒時間，快走吧！）</li>
  <li>맛있<strong>으니까</strong> 한번 먹어 보세요.（因為很好吃，你試試看。）</li>
</ul>

<h3>比較整理</h3>
<table class='table table-bordered'>
  <thead><tr><th></th><th>-아서/어서</th><th>-(으)니까</th></tr></thead>
  <tbody>
    <tr><td>語感</td><td>客觀敘述</td><td>主觀判斷</td></tr>
    <tr><td>後接命令/勸誘</td><td>不行</td><td>可以</td></tr>
    <tr><td>過去式接法</td><td>不加 -았/었-</td><td>可加 -았/었-</td></tr>
    <tr><td>時態</td><td>前後一致</td><td>前後可不同</td></tr>
  </tbody>
</table>"
                            },
                            new Lesson
                            {
                                Title = "條件與假設：-(으)면 vs -다면",
                                Type = LessonType.Article,
                                SortOrder = 2,
                                ArticleContent = @"<h2>假設與條件的表達</h2>

<h3>-(으)면（如果...就...）</h3>
<p>最基本的條件句，表示一般條件或假設。</p>
<ul>
  <li>시간이 <strong>있으면</strong> 같이 갈게요.（如果有時間，我會一起去。）</li>
  <li>비가 <strong>오면</strong> 집에서 쉴 거예요.（如果下雨，打算在家休息。）</li>
  <li>한국에 <strong>가면</strong> 꼭 삼겹살을 먹어 보세요!（如果去韓國，一定要試試五花肉！）</li>
</ul>

<h3>-다면（假如...的話）</h3>
<p>語感比 -(으)면 更假設性，常用於不太可能發生的事或想像的情境。</p>
<ul>
  <li>만약 내가 새<strong>라면</strong> 하늘을 날고 싶어.（假如我是鳥，想在天上飛。）</li>
  <li>시간을 돌릴 수 있<strong>다면</strong> 뭘 하고 싶어요?（如果能倒轉時間，你想做什麼？）</li>
</ul>

<h3>常見搭配</h3>
<table class='table table-bordered'>
  <thead><tr><th>句型</th><th>中文</th><th>範例</th></tr></thead>
  <tbody>
    <tr><td>-면 좋겠다</td><td>...就好了</td><td>내일 날씨가 좋으면 좋겠다.（明天天氣好就好了。）</td></tr>
    <tr><td>-면 되다</td><td>...就行了</td><td>여기서 기다리면 돼요.（在這裡等就行了。）</td></tr>
    <tr><td>-면 안 되다</td><td>不可以...</td><td>여기서 사진 찍으면 안 돼요.（這裡不可以拍照。）</td></tr>
  </tbody>
</table>"
                            }
                        ]
                    },
                    new Section
                    {
                        Title = "第二章：敬語與語體",
                        SortOrder = 2,
                        Lessons =
                        [
                            new Lesson
                            {
                                Title = "韓語的六種語體等級",
                                Type = LessonType.Article,
                                SortOrder = 1,
                                ArticleContent = @"<h2>韓語敬語體系</h2>
<p>韓語的敬語系統是這個語言最獨特也最複雜的部分之一。韓語有<strong>六種語體等級</strong>（존댓말 / 반말），日常生活中最常用的有三種。</p>

<h3>最常用的三種語體</h3>
<table class='table table-bordered'>
  <thead><tr><th>語體</th><th>名稱</th><th>語尾</th><th>使用對象</th><th>範例</th></tr></thead>
  <tbody>
    <tr><td>格式體敬語</td><td>합쇼체</td><td>-ㅂ니다/습니다</td><td>長輩、正式場合</td><td>감사합니다.</td></tr>
    <tr><td>非格式體敬語</td><td>해요체</td><td>-아요/어요</td><td>日常對話（禮貌）</td><td>감사해요.</td></tr>
    <tr><td>半語</td><td>해체 (반말)</td><td>-아/어</td><td>朋友、晚輩</td><td>고마워.</td></tr>
  </tbody>
</table>

<h3>何時用哪種？</h3>
<ul>
  <li><strong>格式體（합쇼체）</strong>：新聞播報、演講、軍隊、商業會議、初次見面</li>
  <li><strong>非格式體（해요체）</strong>：日常生活最常用，禮貌但不過於正式</li>
  <li><strong>半語（반말）</strong>：同齡朋友之間確認可以用半語後才使用</li>
</ul>

<h3>語尾變化範例</h3>
<table class='table table-bordered'>
  <thead><tr><th>原型</th><th>格式體</th><th>非格式體</th><th>半語</th></tr></thead>
  <tbody>
    <tr><td>먹다（吃）</td><td>먹습니다</td><td>먹어요</td><td>먹어</td></tr>
    <tr><td>가다（去）</td><td>갑니다</td><td>가요</td><td>가</td></tr>
    <tr><td>하다（做）</td><td>합니다</td><td>해요</td><td>해</td></tr>
    <tr><td>있다（有/在）</td><td>있습니다</td><td>있어요</td><td>있어</td></tr>
    <tr><td>좋다（好）</td><td>좋습니다</td><td>좋아요</td><td>좋아</td></tr>
  </tbody>
</table>

<h3>重要提醒</h3>
<p>在韓國，對年齡比你大的人說半語是非常失禮的行為。初次見面時，即使對方看起來年紀相近，也要先用敬語（해요체），等互相確認年齡後，年紀大的那方會說「<strong>말 놓으세요</strong>（請說半語吧）」，這時才能切換成半語。</p>"
                            }
                        ]
                    }
                ]
            },

            // ── 進階課程 ──
            new()
            {
                Title = "TOPIK 備考：中高級衝刺",
                Description = "針對 TOPIK II（3~6 級）的備考課程。系統整理閱讀、聽力、寫作三大題型的應試技巧，收錄歷屆常考文法與詞彙，搭配模擬試題與詳解。適合計畫在韓國留學、就業，或需要取得韓語能力證明的學習者。",
                Price = 2490m,
                Level = DifficultyLevel.Advanced,
                IsPublished = true,
                SortOrder = 4,
                Sections =
                [
                    new Section
                    {
                        Title = "第一章：TOPIK 考試概覽",
                        SortOrder = 1,
                        Lessons =
                        [
                            new Lesson
                            {
                                Title = "TOPIK 考試制度與等級說明",
                                Type = LessonType.Article,
                                SortOrder = 1,
                                IsFreePreview = true,
                                ArticleContent = @"<h2>什麼是 TOPIK？</h2>
<p><strong>TOPIK</strong>（Test of Proficiency in Korean，韓國語能力試驗）是由韓國教育部主辦的官方韓語能力檢定考試，全球認可。</p>

<h3>考試分類</h3>
<table class='table table-bordered'>
  <thead><tr><th>類別</th><th>等級</th><th>考試科目</th><th>時間</th></tr></thead>
  <tbody>
    <tr><td>TOPIK I（初級）</td><td>1~2 級</td><td>聽力 + 閱讀</td><td>100 分鐘</td></tr>
    <tr><td>TOPIK II（中高級）</td><td>3~6 級</td><td>聽力 + 寫作 + 閱讀</td><td>180 分鐘</td></tr>
  </tbody>
</table>

<h3>分數與等級對照</h3>
<table class='table table-bordered'>
  <thead><tr><th>等級</th><th>TOPIK I 分數</th><th>TOPIK II 分數</th><th>能力描述</th></tr></thead>
  <tbody>
    <tr><td>1 級</td><td>80 分以上</td><td>—</td><td>能處理自我介紹、購物等基本生存需求</td></tr>
    <tr><td>2 級</td><td>140 分以上</td><td>—</td><td>能使用日常生活基本韓語</td></tr>
    <tr><td>3 級</td><td>—</td><td>120 分以上</td><td>日常生活溝通無困難</td></tr>
    <tr><td>4 級</td><td>—</td><td>150 分以上</td><td>能理解新聞和社會議題</td></tr>
    <tr><td>5 級</td><td>—</td><td>190 分以上</td><td>能在專業領域使用韓語</td></tr>
    <tr><td>6 級</td><td>—</td><td>230 分以上</td><td>接近母語水準</td></tr>
  </tbody>
</table>

<h3>臺灣考試資訊</h3>
<ul>
  <li><strong>考試時間</strong>：每年 4 月、10 月（各一場）</li>
  <li><strong>報名費</strong>：TOPIK I NT$800 / TOPIK II NT$1,000</li>
  <li><strong>考試地點</strong>：臺北、臺中、高雄</li>
  <li><strong>成績有效期</strong>：2 年</li>
</ul>"
                            }
                        ]
                    }
                ]
            },

            // ── 主題課程 ──
            new()
            {
                Title = "韓國文化與旅遊韓語",
                Description = "不只學語言，更深入了解韓國的文化底蘊。從飲食文化、節日習俗、職場禮儀到追星韓語，結合語言學習與文化認識，讓你的韓語更道地、更有溫度。適合所有對韓國文化有興趣的學習者。",
                Price = 1290m,
                Level = DifficultyLevel.Elementary,
                IsPublished = true,
                SortOrder = 5,
                Sections =
                [
                    new Section
                    {
                        Title = "第一章：韓國飲食文化",
                        SortOrder = 1,
                        Lessons =
                        [
                            new Lesson
                            {
                                Title = "韓國的餐桌禮儀與飲食習慣",
                                Type = LessonType.Article,
                                SortOrder = 1,
                                IsFreePreview = true,
                                ArticleContent = @"<h2>韓國餐桌文化</h2>
<p>韓國的餐桌禮儀反映了深厚的儒家文化傳統。了解這些禮儀不僅能避免失禮，還能讓韓國朋友對你刮目相看。</p>

<h3>重要餐桌禮儀</h3>
<ul>
  <li><strong>長輩先動筷</strong>：等年紀最大或地位最高的人先開始吃，其他人才能動筷</li>
  <li><strong>不要把筷子插在飯裡</strong>：這是祭祀時才有的行為，在日常用餐中是大忌</li>
  <li><strong>倒酒禮儀</strong>：為長輩倒酒時要用雙手；自己喝酒時要側身、用手遮杯</li>
  <li><strong>用餐後的感謝</strong>：吃飯前說「잘 먹겠습니다（我要開動了）」，吃完後說「잘 먹었습니다（我吃得很好）」</li>
</ul>

<h3>小菜文化（반찬）</h3>
<p>韓國餐廳的小菜是<strong>免費且可以續加</strong>的！常見的小菜包括：</p>
<table class='table table-bordered'>
  <thead><tr><th>韓文</th><th>中文</th><th>說明</th></tr></thead>
  <tbody>
    <tr><td>김치</td><td>泡菜</td><td>最代表性的韓國食物，每餐必備</td></tr>
    <tr><td>깍두기</td><td>蘿蔔泡菜</td><td>方塊狀的白蘿蔔泡菜</td></tr>
    <tr><td>시금치 나물</td><td>涼拌菠菜</td><td>用芝麻油和蒜拌的菠菜</td></tr>
    <tr><td>콩나물</td><td>豆芽菜</td><td>簡單爽口的小菜</td></tr>
    <tr><td>계란말이</td><td>韓式蛋捲</td><td>加了蔬菜的厚蛋捲</td></tr>
  </tbody>
</table>

<h3>相關實用韓語</h3>
<div style='background:#f0f4f8; padding:1rem; border-radius:8px; margin:1rem 0;'>
  <p><strong>반찬 좀 더 주세요.</strong>（請再給我一些小菜。）</p>
  <p><strong>김치 리필 돼요?</strong>（泡菜可以續嗎？）</p>
  <p><strong>이거 정말 맛있네요!</strong>（這個真的好好吃！）</p>
</div>"
                            }
                        ]
                    }
                ]
            }
        };

        db.Courses.AddRange(courses);
        await db.SaveChangesAsync().ConfigureAwait(false);
        logger.LogInformation("建立 {Count} 門範例課程", courses.Count);
    }

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
                Title = "歡迎來到 KoreanLearn！",
                Content = "感謝您加入韓文學習平台。我們提供從入門到進階的完整課程，幫助你系統化地學習韓語。",
                IsActive = true,
                StartDate = DateTime.UtcNow,
            },
            new Announcement
            {
                Title = "TOPIK 備考課程上線",
                Content = "全新「TOPIK 備考：中高級衝刺」課程已上線，涵蓋聽力、閱讀、寫作三大題型的完整攻略。",
                IsActive = true,
                StartDate = DateTime.UtcNow,
            }
        );
        await db.SaveChangesAsync().ConfigureAwait(false);
        logger.LogInformation("建立範例公告");
    }

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
