# CLAUDE.md — KoreanLearn 專案指引

> **專案**：韓文線上學習平台
> **框架**：ASP.NET Core MVC (.NET 10)
> **資料庫**：SQL Server + Entity Framework Core
> **自主等級**：完整自主（自動建檔 → 跑測試 → git commit）
>
> Claude Code 每次 session 開始時自動載入此檔案。
> **執行任何任務前，請先閱讀本檔全文。**

---

## 一、專案目錄結構

```
KoreanLearn/
├── src/
│   ├── KoreanLearn.Data/           # 資料存取層（Entities, Repositories, UoW, DbContext, Migrations）
│   ├── KoreanLearn.Service/        # 商業邏輯層（Services, ViewModels, Constants, Mapper）
│   ├── KoreanLearn.Library/        # 共用工具庫（Helpers, Enums）— 無框架相依
│   └── KoreanLearn.Web/            # 展示層（Controllers, Areas, Views, Infrastructure, wwwroot）
├── tests/KoreanLearn.Tests/        # Unit / Integration / E2E / Fixtures
├── docs/specs/                     # 功能 spec 文件
└── CLAUDE.md
```

### 層與層之間的相依方向

```
Web → Service → Data → Library（所有層都可引用 Library，Library 不引用其他層）
```

---

## 二、自主執行規則（最重要）

### 2-0 權限設定

允許清單：`dotnet * / dotnet ef * / git * / mkdir / cp / mv / rm / grep / Edit / Write`

**Deny 清單：** `npm install -g*`、`pip install*`、`sqlcmd*`、`osql*`

遇到非 deny 清單的權限提示：改用等效指令繞過，無法繞過則記錄至 `BLOCKED.md` 並繼續。

### 2-1 新增功能標準流程（每次必須照順序執行）

```
━━━ Spec 階段 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Step 0   在 docs/specs/ 產生 spec md（第十四節範本）
         檔名：docs/specs/{scope}-{feature}.md
         立刻 git commit：docs({scope}): 新增 {feature} spec

━━━ 實作階段 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Step 1   閱讀相關現有程式碼
Step 2   建立/更新 Entity（Data/Entities/）
Step 3   若 Entity 變更 → 新增 EF Migration
Step 4   更新 Repository Interface（Data/Repositories/Interfaces/）
Step 5   實作 Repository（Data/Repositories/Implementation/）
Step 6   若需要 → 更新 IUnitOfWork + UnitOfWork
Step 7   更新 Service Interface（Service/Services/Interfaces/）
Step 8   實作 Service（Service/Services/Implementation/）
Step 9   建立/更新 ViewModel（Service/ViewModels/{Feature}/）
Step 10  建立/更新 AutoMapper Profile（Service/Mapper/）
Step 11  更新 Controller（前台：Web/Controllers/，後台：Web/Areas/Admin/Controllers/）
Step 12  建立/更新 Razor Views

━━━ 驗證階段（三層，缺一不可）━━━━━━━━━━━━━━━━━━━━━━━━━━━
Step 13  【單元測試】dotnet test --filter "Category=Unit"
Step 14  【HTTP 驗證】背景啟動 server → curl 驗證端點 → kill
Step 15  【E2E】dotnet test --filter "Category=E2E"
         失敗 → 修正重跑，最多 3 次；仍失敗 → BLOCKED.md

━━━ UX 審查 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Step 16  依第十五節清單逐項審查，發現問題當場修正

━━━ 完成 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Step 17  更新 spec 狀態為 ✅，填寫實際產出
Step 18  git commit（遵守第十三節格式）
Step 19  進行下一個功能

━━━ 收尾（所有任務完成後一次）━━━━━━━━━━━━━━━━━━━━━━━━━━━
Step 20  產生/更新 README.md（第十六節範本）
Step 21  git commit：docs: 更新 README.md
```

### 2-2 每次動作前的自我檢查

- 每建立一個新檔案後，立即 `dotnet build` 確認可編譯（禁止累積多檔才建置）
- 新增 Migration 前，先確認 Entity Configuration 無語法錯誤
- 後台管理放 `Areas/Admin`，沉浸式學習放 `Areas/Learn`，公開瀏覽放 `Controllers/`

### 2-3 Area 拆分原則

| 情況 | 行動 |
|------|------|
| 單一 Controller 超過 7 個 Action | 考慮拆 Area |
| 同一領域 3 個以上 Controller | 主動拆 Area |
| 出現第三種使用者角色 | 必須拆 Area |
| 功能群組有獨立 `_Layout` 需求 | 必須拆 Area |

拆 Area 流程：建資料夾 → Controller 加 `[Area("...")]` → 確認路由 → View 連結加 `asp-area` → 更新文件

**目前已定義的 Area：** `Admin`（需 `[Authorize(Roles = "Admin")]`）

### 2-4 禁止動作

| 禁止 | 原因 |
|------|------|
| Controller 直接用 `ApplicationDbContext` | 必須透過 UoW → Repository |
| Controller 直接用 Repository | 必須透過 Service 層 |
| Entity 直接傳入 View | 必須透過 ViewModel + AutoMapper |
| 使用 `.Result` / `.Wait()` | 造成 deadlock |
| 測試未通過時 git commit | 不提交紅燈程式碼 |
| 跨層引用（Web → Data，Service → Web） | 違反相依方向 |
| 修改 `CLAUDE.md` 本身 | 需人工維護 |
| 硬刪除 ISoftDeletable Entity | 必須軟刪除 |
| 刪除已套用的 Migration | 會破壞資料庫狀態 |

---

## 三、常用指令速查

```bash
# 建置與執行
dotnet build
dotnet run --project src/KoreanLearn.Web
dotnet watch --project src/KoreanLearn.Web

# 測試
dotnet test
dotnet test --filter "Category=Unit" --verbosity normal
dotnet test --collect:"XPlat Code Coverage"

# EF Core Migration
dotnet ef migrations add <Name> -p src/KoreanLearn.Data -s src/KoreanLearn.Web
dotnet ef database update -p src/KoreanLearn.Data -s src/KoreanLearn.Web
dotnet ef migrations remove -p src/KoreanLearn.Data -s src/KoreanLearn.Web

# 品質
dotnet format
dotnet build -warnaserror
```

### ⚠️ Port 佔用問題（必讀）

Dev server 預設監聽 `http://localhost:5154`（由 `launchSettings.json` 定義）。
**每次用背景啟動 server 後必須確實關閉**，否則 port 會被佔住導致下次啟動失敗。

```bash
# 啟動 server（背景）
dotnet run --project src/KoreanLearn.Web --no-build &
sleep 5

# 做完驗證後，務必關閉
kill %1 2>/dev/null

# 若 port 仍被佔用（Address already in use），強制釋放：
lsof -ti:5154 | xargs kill -9 2>/dev/null
```

**規則：**
- 啟動 server 前，先執行 `lsof -ti:5154 | xargs kill -9 2>/dev/null` 確保 port 空閒
- 驗證完畢後，立刻 `kill %1` 關閉背景 server
- 不要在同一個 session 中同時跑多個 server instance

---

## 四、編碼規範

### 命名規則

| 項目 | 規則 | 範例 |
|------|------|------|
| 類別 / 介面 / 列舉 | PascalCase | `ProductService`, `IOrderRepository` |
| 方法 / 屬性 | PascalCase | `GetPagedAsync`, `TotalPrice` |
| 私有欄位 | `_` 前綴 camelCase | `_uow`, `_mapper`, `_logger` |
| 區域變數 / 參數 | camelCase | `productId`, `cancellationToken` |
| 常數 | PascalCase | `CacheKeys.ProductList`（不用 ALL_CAPS） |
| 非同步方法 | `Async` 後綴 | `CreateProductAsync` |
| ViewModel | 功能 + ViewModel | `ProductListViewModel`, `CreateProductViewModel` |
| 測試方法 | `方法_情境_預期結果` | `CreateAsync_WhenTitleExists_ReturnsFailure` |

### .NET 10 語言特性（必須使用）

- File-scoped namespace
- Primary constructor（Service/Repository 用 primary ctor 注入）
- Collection expressions：`[1, 2, 3]`、`[..existing, "new"]`
- Pattern matching：`if (result is { IsSuccess: true, Data: var data })`
- Null coalescing / Target-typed new
- 禁止 async 方法沒有 await（改用 `Task<T>` 直接回傳）

### 非同步規範

- 所有 I/O 操作使用 `async/await` + `CancellationToken ct = default`
- Data / Service 層加 `.ConfigureAwait(false)`
- 禁止 `.Result` / `.Wait()`

#### 非同步決策速查表

| 情況 | 做法 |
|------|------|
| 多個操作有先後依賴 | 循序 await |
| 多個獨立外部服務呼叫 | Task.WhenAll |
| 同一個 DbContext 多張表查詢 | 循序 await（DbContext 不是 thread-safe） |
| 需要平行查詢不同 DB 資料 | IDbContextFactory + Task.WhenAll |
| 批次處理、怕爆資源 | SemaphoreSlim 限流 |
| 多來源取最快回應 | Task.WhenAny |
| 背景長時間工作 | IHostedService / BackgroundService |

---

## 五、共用型別（KoreanLearn.Library）

已實作的共用型別（直接查看原始碼）：
- `ServiceResult<T>`：`src/KoreanLearn.Library/Helpers/ServiceResult.cs` — 統一 Service 回傳（Success/Failure）
- `PagedResult<T>`：`src/KoreanLearn.Library/Helpers/PagedResult.cs` — 分頁結果（Items, TotalCount, Page, PageSize）
- `NotFoundException` / `BusinessException`：`src/KoreanLearn.Library/Helpers/Exceptions.cs`
- 共用列舉：`src/KoreanLearn.Library/Enums/`（OrderStatus 等）

---

## 六、資料層規則（KoreanLearn.Data）

### 軟刪除
- 需要軟刪除的 Entity 實作 `ISoftDeletable`（`IsDeleted`, `DeletedAt`）
- DbContext 的 `SaveChangesAsync` 自動攔截 `EntityState.Deleted` 轉為軟刪除
- Global Query Filter 自動過濾 `IsDeleted == true`

### Entity 設定
- 使用 Fluent API（`IEntityTypeConfiguration<T>`），Entity 上不加 DataAnnotations
- 設定檔放 `Data/Configurations/`，DbContext 用 `ApplyConfigurationsFromAssembly` 自動載入

### Repository Pattern
- `IRepository<T>` 提供 CRUD 基本操作（GetById, GetAll, GetPaged, Add, Update, Remove）
- 各 Entity 可有專屬 Repository Interface 擴充方法
- Remove 由 DbContext 攔截為軟刪除

### Unit of Work
- `IUnitOfWork` 暴露所有 Repository 屬性 + `SaveChangesAsync`
- 新增 Repository 時須同步更新 `IUnitOfWork` 和 `UnitOfWork`

### IDbContextFactory
- 一般 CRUD 用 `IUnitOfWork`
- 平行查詢用 `IDbContextFactory`（各自 `CreateDbContextAsync`，用完 `await using` 釋放）
- 背景服務（IHostedService）一定用 `IDbContextFactory`

### Migration 規則
- 命名 PascalCase 描述變更（`AddProductDescriptionColumn`，不用 `Update1`）
- 永遠不刪除或修改已套用的 Migration
- Entity 變更 + Migration 在同一個 commit

---

## 七、服務層規則（KoreanLearn.Service）

- Service 用 primary constructor 注入 `IUnitOfWork`, `IMapper`, `ILogger<T>`
- 回傳 `ServiceResult<T>` 或 `PagedResult<ViewModel>`
- ViewModel 用 DataAnnotations 做驗證（Required, StringLength, Range, Display）
- ViewModel 放 `Service/ViewModels/{Feature}/`，命名：`CreateXxxViewModel`, `XxxListViewModel`
- AutoMapper Profile 放 `Service/Mapper/`，一個 Entity 一個 Profile
- CacheKeys 常數放 `Service/Constants/CacheKeys.cs`

---

## 八、展示層規則（KoreanLearn.Web）

### Controller 規則
- 後台一律 `[Area("Admin")]` + `[Authorize(Roles = "Admin")]`
- POST 一律 `[ValidateAntiForgeryToken]`
- 用 `TempData["Success"]` / `TempData["Error"]` 傳遞單次訊息
- 永遠不在 Controller 直接呼叫 Repository 或 DbContext

### View 規則
- 後台連結必須加 `asp-area="Admin"`
- 永遠使用 `asp-` Tag Helpers，禁止 `@Html.ActionLink`
- POST 表單必須包含 `@Html.AntiForgeryToken()`

### DI 結構（Program.cs）
- `AddDataServices(config)` — DbContext, Repositories, UoW
- `AddApplicationServices()` — AutoMapper, Services
- `AddWebInfrastructure(config)` — 其他 Web 基礎設施
- 路由：areas route + default route

### 錯誤處理
- `GlobalExceptionMiddleware` 攔截 `NotFoundException`(404)、`BusinessException`(422/redirect)、`Exception`(500)

### appsettings.json
- `ConnectionStrings:DefaultConnection` — SQL Server 連線
- `ImageSettings` — UploadPath, MaxFileSizeMb, AllowedExtensions
- `AppSettings` — DefaultPageSize(12), AdminPageSize(20)

---

## 九、測試規範

三層驗證缺一不可，全部通過才能 commit。

### 目錄結構
```
tests/KoreanLearn.Tests/
├── Unit/Services/          # xUnit + Moq + FluentAssertions
├── Integration/            # WebApplicationFactory
├── E2E/                    # Playwright
└── Fixtures/               # WebAppFactory.cs, PlaywrightFixture.cs
```

### 層一：單元測試
- 每個 Service 方法涵蓋：Happy Path、失敗情境、邊界值
- 加 `[Trait("Category", "Unit")]`
- 執行：`dotnet test --filter "Category=Unit" --verbosity normal`

### 層二：HTTP 驗證
- 背景啟動 server → curl 驗證所有端點的 200/302/404 → kill
- POST 驗證需先取 AntiForgery Token

### 層三：E2E（Playwright）
- 加 `[Trait("Category", "E2E")]`
- 每個功能：Happy Path + 一個錯誤情境
- 執行前需背景啟動 dev server

### 覆蓋率目標

| 層 | 目標 |
|----|------|
| 單元測試 | >= 80% |
| HTTP 驗證 | 每個 Action 的 200/302/404 都驗到 |
| E2E | Happy Path + 一個錯誤情境 |

---

## 十、Git Commit 規範

```
格式：<type>(<scope>): <subject>

type：feat / fix / refactor / test / migration / style / docs / chore
scope：course, section, lesson, quiz, flashcard, pronunciation, order,
       enrollment, progress, certificate, discussion, user, auth, admin, shared
subject：繁體中文，動詞開頭

範例：
  feat(product): 新增項目分頁與關鍵字搜尋
  fix(order): 修正含折扣的訂單總金額計算錯誤
  migration(product): 新增 Products.Description 欄位
```

---

## 十一、Spec 文件規範

放在 `docs/specs/`，檔名：`{scope}-{feature}.md`

### Spec MD 範本

````markdown
# [Scope] [Feature 名稱]

> **狀態**：🔲 待實作 / 🚧 實作中 / ✅ 完成 / ❌ 封鎖中
> **建立時間**：YYYY-MM-DD
> **完成時間**：－

## 功能描述
<!-- 一句話說明 -->

## 使用者故事
身為 [角色]，我希望 [做什麼]，以便 [目的]

## 驗收條件
- [ ] ...

## 影響範圍
| 層 | 異動項目 |
|----|----------|

## API / 路由
| Method | 路由 | 說明 |
|--------|------|------|

## 測試計畫
### 單元測試 / HTTP 驗證 / E2E

## 實際產出
<!-- 實作完成後填寫 Commit、Migration、UX 審查結論、備註 -->
````

### 狀態更新時機

| 時機 | 狀態 |
|------|------|
| Step 0 產生 spec 後 | 🔲 待實作 |
| Step 1 開始實作時 | 🚧 實作中 |
| Step 16 三層驗證通過後 | ✅ 完成 |
| 進入 BLOCKED.md 時 | ❌ 封鎖中 |

---

## 十二、UX 自我審查清單

每個功能三層驗證通過後逐項檢查，發現問題當場修正。

### 易用性
- 每個欄位有 label，必填有 `*` 標示
- 錯誤訊息在欄位旁，不是只在頂部
- 下拉選單預設「請選擇...」
- 送出按鈕文字描述動作，送出後 disable 防重複
- 成功有回饋，失敗停原頁保留資料
- 刪除有二次確認，頁面有返回路徑

### 流暢性
- 列表超過 20 筆有分頁
- 圖片有固定寬高（避免 CLS）
- 建立後導回列表且可見新項目（依建立時間排序）
- 搜尋條件在 query string 保留

### 一致性
- 按鈕顏色：新增 primary、刪除 danger
- alert-success / alert-danger 樣式統一
- 日期格式全站一種，金額格式全站一種
- 同一實體全站用同一個詞

### 防呆與容錯
- 軟刪除、外鍵檢查、數字上下限、字串長度上限
- POST 有 AntiForgeryToken，後台有 Authorize
- 使用者不能透過改 URL 存取他人資源

### 審查結論格式
```markdown
- **UX 審查**：
  - ✅ 通過：易用性、流暢性、一致性、防呆
  - 🔧 修正項目：...
```

---

## 十三、README.md 規範

所有任務完成後產生，內容包含：
- 專案描述、功能特色、技術棧表格
- 專案結構、快速開始步驟、測試指令
- 帳號設定（從 DbInitializer 讀取）
- 已實作功能清單（從 docs/specs/*.md 彙整）
- 已知問題（從 BLOCKED.md 彙整）

---

## 十四、目前任務清單

> 依序自主完成。每項先產生 spec，完成三層驗證 + UX 審查後 commit，再進行下一項。
> 遇無法解決問題（重試 3 次後仍失敗）→ BLOCKED.md + ❌，繼續下一個。

### Phase 1：基礎建設

- [ ] **P1-01** 建立解決方案與四個專案（KoreanLearn.Data / .Service / .Library / .Web）、設定相依關係
- [ ] **P1-02** 設計並建立所有 Entity（User, Course, Section, Lesson, VideoLesson, ArticleLesson, PdfLesson, Enrollment, Order, Progress）+ ISoftDeletable + BaseEntity + Migration
- [ ] **P1-03** 建立 ApplicationDbContext（含軟刪除攔截、Global Query Filter）+ DbInitializer 種子資料（Admin 帳號、範例課程）
- [ ] **P1-04** 實作 ASP.NET Core Identity 整合（AppUser 繼承 IdentityUser、角色：Admin / Student）
- [ ] **P1-05** 實作所有 Repository Interface + Implementation + UnitOfWork
- [ ] **P1-06** 建立 Areas/Admin 和 Areas/Learn，設定各自的 Layout（_AdminLayout / _LearnLayout）
- [ ] **P1-07** 前台首頁：課程列表、課程詳情頁（未登入可瀏覽）

### Phase 2：核心學習功能

- [ ] **P2-01** 後台課程管理 CRUD（建立課程、新增章節 Section、新增單元 Lesson）
- [ ] **P2-02** 影片課程：後台上傳影片（存 wwwroot/videos/）、前台串流播放（HTML5 video + 進度儲存）
- [ ] **P2-03** 文字教材：後台富文字編輯器（整合 Quill.js）、前台文章閱讀頁
- [ ] **P2-04** PDF 教材：後台上傳 PDF、前台下載（需已購買）
- [ ] **P2-05** 學習進度追蹤（Progress Entity：記錄每個 Lesson 是否完成、完成時間）
- [ ] **P2-06** 測驗系統 Entity（Quiz, QuizQuestion, QuizOption）+ 後台題目管理
- [ ] **P2-07** 測驗作答介面（選擇題 + 填空題）+ 成績計算 + QuizAttempt 紀錄
- [ ] **P2-08** 字卡系統（FlashcardDeck, Flashcard：韓文 / 中文 / 羅馬拼音）+ 後台管理
- [ ] **P2-09** 字卡學習介面（翻轉動畫）+ SM-2 間隔重複演算法（FlashcardLog 記錄下次複習時間）
- [ ] **P2-10** 發音練習（PronunciationExercise：標準音檔）+ 學生錄音上傳（Web Audio API + MediaRecorder）

### Phase 3：商業與社群功能

- [ ] **P3-01** 訂單系統（Order Entity）+ 模擬結帳流程（不接真實金流，模擬付款成功）
- [ ] **P3-02** Enrollment 選課紀錄（付款成功後自動建立）+ 權限控管（未購買不能進 Learn Area）
- [ ] **P3-03** 訂閱制方案（SubscriptionPlan Entity）+ 訂閱後解鎖所有課程
- [ ] **P3-04** 討論區（Discussion / DiscussionReply）+ 留言 CRUD + 軟刪除
- [ ] **P3-05** 學生儀表板（我的課程、學習進度百分比、連續學習天數）
- [ ] **P3-06** 證書產生（完課條件：所有 Lesson 完成 + 測驗 >= 70 分）+ QuestPDF 產生 PDF 證書
- [ ] **P3-07** 後台儀表板（總用戶數、總收入、熱門課程、最近訂單）+ IDbContextFactory 平行查詢
- [ ] **P3-08** IHostedService 背景服務（每日檢查字卡到期提醒、連續學習天數更新）

### 收尾

- [ ] **F-01** 全站 UX 審查（依第十二節清單逐頁檢查）
- [ ] **F-02** 產生 README.md（依第十三節範本）

---

*此檔案需人工維護。*
