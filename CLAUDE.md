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
├── .claude/
│   └── settings.json                        # Claude Code 權限設定（允許清單）
├── src/
│   ├── KoreanLearn.Data/                # 資料存取層
│   │   ├── Entities/                        # 實體模型（AppUser, Product, Announcement, Order...）
│   │   │   └── ISoftDeletable.cs            # 軟刪除介面
│   │   ├── Repositories/
│   │   │   ├── Interfaces/                  # IRepository<T>, IProductRepository...
│   │   │   └── Implementation/              # Repository 實作
│   │   ├── UnitOfWork/
│   │   │   ├── IUnitOfWork.cs
│   │   │   └── UnitOfWork.cs
│   │   ├── ApplicationDbContext.cs          # DbContext（含軟刪除攔截與 Global Query Filter）
│   │   ├── DbInitializer.cs                 # 種子資料
│   │   └── Migrations/                      # EF Core 遷移檔案
│   │
│   ├── KoreanLearn.Service/             # 商業邏輯層
│   │   ├── Services/
│   │   │   ├── Interfaces/                  # IProductService, IOrderService...
│   │   │   └── Implementation/              # Service 實作
│   │   ├── ViewModels/                      # ViewModel 定義（依功能分資料夾）
│   │   │   ├── Product/
│   │   │   ├── Order/
│   │   │   ├── Admin/
│   │   │   └── Shared/
│   │   ├── Constants/                       # CacheKeys 等常數定義
│   │   └── Mapper/                          # AutoMapper Profile 設定
│   │
│   ├── KoreanLearn.Library/             # 共用工具庫（無框架相依）
│   │   ├── Helpers/                         # 工具類別與擴充方法
│   │   │   ├── PagedResult.cs               # 分頁結果
│   │   │   └── ServiceResult.cs             # 統一回傳型別
│   │   └── Enums/                           # 共用列舉（OrderStatus, PaymentStatus...）
│   │
│   └── KoreanLearn.Web/                 # 展示層
│       ├── Controllers/                     # 前台 MVC 控制器
│       │   └── Api/                         # RESTful API 控制器
│       ├── Areas/
│       │   └── Admin/                       # 後台管理 Area
│       │       ├── Controllers/             # Dashboard, Product, Author, Category, Order, User, Announcement
│       │       └── Views/
│       ├── Infrastructure/                  # Web 基礎設施
│       │   ├── Middleware/                  # 全域例外處理等
│       │   ├── ViewComponents/              # ViewComponent 定義
│       │   ├── Settings/                    # 設定模型（ImageSettings...）
│       │   └── Extensions/                  # IServiceCollection 擴充方法
│       ├── Views/
│       │   ├── Shared/                      # _Layout, _ProductCard 等共用局部視圖
│       │   └── Home/                        # 首頁
│       └── wwwroot/
│           ├── css/
│           ├── js/
│           └── images/
│
├── tests/
│   └── KoreanLearn.Tests/
│       ├── Unit/
│       │   ├── Services/
│       │   └── Repositories/
│       ├── Integration/
│       └── Fixtures/
│
├── .vscode/
├── CLAUDE.md                                # 本檔案
├── README.md
└── KoreanLearn.slnx
```

### 層與層之間的相依方向

```
KoreanLearn.Web
    │  ↓ 引用
KoreanLearn.Service
    │  ↓ 引用
KoreanLearn.Data
    │  ↓ 引用
KoreanLearn.Library   ← 所有層都可以引用，但 Library 不引用其他層
```

---

## 二、自主執行規則（最重要）

### 2-0 權限設定

本專案已在 `.claude/settings.json` 預設允許清單，涵蓋所有日常操作：

```
dotnet * / dotnet ef * / git * / mkdir / cp / mv / rm / grep / Edit / Write
```

**唯一被擋住的操作：**

| 指令 | 原因 |
|------|------|
| `npm install -g*` | 避免污染全域系統環境 |
| `pip install*` | 同上 |
| `sqlcmd*` | 資料庫操作一律透過 EF Migration |
| `osql*` | 同上 |

若執行時仍遇到非以上四項的權限提示（例如 Claude Code 版本更新後行為改變），
**不要停下來等待**，改用以下方式繼續：

1. 確認該操作不在 deny 清單內
2. 改寫成等效指令（例如 `rm` 改用 `git rm`）
3. 若確實無法繞過，將問題記錄至 `BLOCKED.md` 並繼續下一個任務

### 2-1 新增功能標準流程（每次必須照順序執行）

```
━━━ Spec 階段 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Step 0   依照第十五節的範本，在 docs/specs/ 產生本功能的 spec md
         檔名格式：docs/specs/{scope}-{feature}.md
         例：docs/specs/product-create.md
         產生後立刻 git add + git commit：
           docs(product): 新增 product-create spec

━━━ 實作階段 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Step 1   閱讀相關現有程式碼，理解目前架構與命名慣例
Step 2   建立/更新 Entity（KoreanLearn.Data/Entities/）
Step 3   若 Entity 有欄位變更 → 新增 EF Migration（見第八節）
Step 4   更新 Repository Interface（KoreanLearn.Data/Repositories/Interfaces/）
Step 5   實作 Repository（KoreanLearn.Data/Repositories/Implementation/）
Step 6   若 IUnitOfWork 需要新的 Repository 屬性 → 更新 IUnitOfWork + UnitOfWork
Step 7   更新 Service Interface（KoreanLearn.Service/Services/Interfaces/）
Step 8   實作 Service（KoreanLearn.Service/Services/Implementation/）
Step 9   建立/更新 ViewModel（KoreanLearn.Service/ViewModels/{Feature}/）
Step 10  建立/更新 AutoMapper Profile（KoreanLearn.Service/Mapper/）
Step 11  更新 Controller（前台：Web/Controllers/，後台：Web/Areas/Admin/Controllers/）
Step 12  建立/更新 Razor Views

━━━ 驗證階段（三層，缺一不可，每功能寫完立刻執行）━━━━━━━━
Step 13  【層一：單元測試】
         新增/更新單元測試（tests/KoreanLearn.Tests/Unit/）
         執行：dotnet test --filter "Category=Unit"
         失敗 → 修正後重跑，最多 3 次；仍失敗 → 記錄至 BLOCKED.md

Step 14  【層二：HTTP 驗證】
         啟動 dev server（背景）：
           dotnet run --project src/KoreanLearn.Web --no-build &
           sleep 5
         針對本功能所有端點逐一用 curl 驗證（詳見第十二節）
         完成後關閉：kill %1

Step 15  【層三：E2E 瀏覽器驗證】
         新增/更新 Playwright 測試（tests/KoreanLearn.Tests/E2E/）
         執行：dotnet test --filter "Category=E2E"
         失敗 → 修正後重跑，最多 3 次；仍失敗 → 記錄至 BLOCKED.md

━━━ UX 自我審查（三層驗證通過後執行）━━━━━━━━━━━━━━━━━━━━
Step 16  依照第十六節的 UX 檢查清單，逐項審查本功能
         發現問題 → 當場修正 → 重跑受影響的驗證層
         確認無問題 → 繼續

━━━ 完成階段 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Step 17  更新 spec md 狀態為 ✅ 完成，填寫實際產出與 UX 審查結論
Step 18  執行 git commit（遵守第十三節格式）
Step 19  進行下一個功能

━━━ 專案收尾（所有任務清單完成後執行一次）━━━━━━━━━━━━━━━
Step 20  依照第十七節範本自動產生 / 更新 README.md
Step 21  執行 git commit：docs: 更新 README.md
```

### 2-2 每次動作前的自我檢查

- 每建立一個新檔案後，立即執行 `dotnet build` 確認可編譯
- 禁止累積多個檔案後才一次建置（難以定位錯誤）
- 新增 Migration 前，先確認所有 Entity Configuration 正確無語法錯誤
- 後台管理放 `Areas/Admin`，沉浸式學習介面放 `Areas/Learn`，公開瀏覽放 `Controllers/`

### 2-3 Area 拆分原則

當某個領域的 Controller 數量或複雜度增加時，主動評估是否拆成獨立 Area，判斷標準如下：

| 情況 | 行動 |
|------|------|
| 單一 Controller 超過 **7 個 Action** | 考慮拆 Area |
| 同一領域已有 **3 個以上 Controller** | 主動拆 Area |
| 前台 / 後台以外，出現第三種使用者角色（如 `Seller`、`Editor`） | 必須拆 Area |
| 功能群組有自己獨立的 `_Layout`、導覽列需求 | 必須拆 Area |

**拆 Area 標準流程：**
```
1. 在 src/KoreanLearn.Web/Areas/ 下建立新資料夾
   例：Areas/Shop/、Areas/Editor/

2. 資料夾結構與 Admin 相同：
   Areas/{AreaName}/
   ├── Controllers/
   └── Views/
       └── Shared/   ← 該 Area 專屬 Layout（若需要）

3. Controller 加上 [Area("{AreaName}")] attribute

4. Program.cs 確認已有 areas 路由（通常已存在）：
   app.MapControllerRoute("areas", "{area:exists}/{controller=...}/{action=Index}/{id?}");

5. 所有該 Area 內的 View 連結加上 asp-area="{AreaName}"

6. 更新 CLAUDE.md 目錄結構（第一節）反映新 Area
```

**目前已定義的 Area：**
- `Admin`：後台管理（需 `[Authorize(Roles = "Admin")]`）
- （新 Area 建立後在此補充）

### 2-4 禁止動作

| 禁止 | 原因 |
|------|------|
| 在 Controller 直接使用 `ApplicationDbContext` | 必須透過 UnitOfWork → Repository |
| 在 Controller 直接使用 Repository | 必須透過 Service 層 |
| 直接將 Entity 傳入 View | 必須透過 ViewModel + AutoMapper |
| 使用 `.Result` / `.Wait()` | 造成 deadlock |
| 測試未通過時執行 git commit | 不提交紅燈程式碼 |
| 跨層引用（Web → Data，Service → Web） | 違反相依方向 |
| 修改 `CLAUDE.md` 本身 | 需人工維護 |
| 硬刪除有實作 ISoftDeletable 的 Entity | 必須軟刪除 |
| 刪除 `Migrations/` 中已套用的 Migration | 會破壞資料庫狀態 |

---

## 三、常用指令速查

```bash
# ── 建置與執行 ────────────────────────────────────────────────────
dotnet build                                     # 建置整個方案
dotnet run --project src/KoreanLearn.Web     # 啟動開發伺服器
dotnet watch --project src/KoreanLearn.Web   # 熱重載模式

# ── 測試 ──────────────────────────────────────────────────────────
dotnet test                                      # 執行所有測試
dotnet test --verbosity normal                   # 詳細輸出
dotnet test --collect:"XPlat Code Coverage"      # 含覆蓋率

# ── EF Core Migration ─────────────────────────────────────────────
dotnet ef migrations add <MigrationName> \
    -p src/KoreanLearn.Data \
    -s src/KoreanLearn.Web

dotnet ef database update \
    -p src/KoreanLearn.Data \
    -s src/KoreanLearn.Web

dotnet ef migrations remove \
    -p src/KoreanLearn.Data \
    -s src/KoreanLearn.Web

dotnet ef migrations script \
    -p src/KoreanLearn.Data \
    -s src/KoreanLearn.Web \
    -o ./scripts/migration_$(date +%Y%m%d).sql

# ── 程式碼品質 ────────────────────────────────────────────────────
dotnet format                                    # 自動格式化
dotnet build -warnaserror                        # 警告視為錯誤

# ── 套件管理（常用）──────────────────────────────────────────────
dotnet add src/KoreanLearn.Data    package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src/KoreanLearn.Service package AutoMapper
dotnet add src/KoreanLearn.Tests   package xunit
dotnet add src/KoreanLearn.Tests   package Moq
dotnet add src/KoreanLearn.Tests   package FluentAssertions
dotnet add src/KoreanLearn.Tests   package Microsoft.AspNetCore.Mvc.Testing
```

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

```csharp
// ✅ File-scoped namespace
namespace KoreanLearn.Service.Services.Implementation;

// ✅ Primary constructor
public class ProductService(
    IUnitOfWork uow,
    IMapper mapper,
    ILogger<ProductService> logger) : IProductService
{ }

// ✅ Collection expressions
List<int> ids = [1, 2, 3];
string[] tags = [..existingTags, "新標籤"];

// ✅ Pattern matching
if (result is { IsSuccess: true, Data: var data })
    return View(data);

// ✅ Null coalescing
var title = product?.Title ?? "未命名";

// ✅ Target-typed new
Product product = new() { Title = "測試項目" };

// ❌ 禁止：async 方法但沒有 await
public async Task<Product?> GetAsync(int id) => _uow.Products.GetByIdAsync(id);

// ✅ 正確
public Task<Product?> GetAsync(int id) => _uow.Products.GetByIdAsync(id);
```

### 非同步規範

#### 基本原則

```csharp
// ✅ 所有 I/O 操作使用 async/await + CancellationToken
// ✅ Data / Service 層加 ConfigureAwait(false)
// ❌ 禁止 .Result / .Wait()（造成 deadlock）
// ❌ 禁止 async 方法內沒有 await

public async Task<ServiceResult<ProductViewModel>> CreateAsync(
    CreateProductViewModel vm,
    CancellationToken ct = default)
{
    var exists = await _uow.Products.ExistsByTitleAsync(vm.Title, ct)
        .ConfigureAwait(false);

    if (exists)
        return ServiceResult<ProductViewModel>.Failure("項目標題已存在");

    var product = _mapper.Map<Product>(vm);
    await _uow.Products.AddAsync(product, ct).ConfigureAwait(false);
    await _uow.SaveChangesAsync(ct).ConfigureAwait(false);

    _logger.LogInformation("建立項目 {Id}: {Title}", product.Id, product.Title);
    return ServiceResult<ProductViewModel>.Success(_mapper.Map<ProductViewModel>(product));
}
```

#### Task.WhenAll：平行執行多個獨立 I/O

**適用時機**：多個操作互相獨立、不共用同一個 DbContext instance。

```csharp
// ✅ 適合：呼叫不同外部服務、或使用不同 DbContext 的查詢
public async Task<DashboardViewModel> GetDashboardAsync(CancellationToken ct = default)
{
    // 三個查詢完全獨立，平行跑節省時間
    var productCountTask     = _uow.Products.CountAsync(ct);
    var orderCountTask       = _uow.Orders.CountAsync(ct);
    var announcementListTask = _uow.Announcements.GetActiveAsync(ct);

    await Task.WhenAll(productCountTask, orderCountTask, announcementListTask)
              .ConfigureAwait(false);

    return new DashboardViewModel
    {
        ProductCount      = productCountTask.Result,   // WhenAll 後取 .Result 是安全的
        OrderCount        = orderCountTask.Result,
        Announcements     = announcementListTask.Result
    };
}

// ✅ 適合：平行呼叫多個外部 API
public async Task<(bool EmailSent, bool SmsSent)> SendNotificationsAsync(
    string userId, CancellationToken ct = default)
{
    var emailTask = _emailService.SendAsync(userId, ct);
    var smsTask   = _smsService.SendAsync(userId, ct);

    await Task.WhenAll(emailTask, smsTask).ConfigureAwait(false);

    return (emailTask.Result, smsTask.Result);
}
```

#### ❌ 同一個 DbContext 不能平行

EF Core 的 `DbContext` **不是 thread-safe**，同一個 instance 的操作必須循序執行。

```csharp
// ❌ 錯誤：同一個 _uow（同一個 DbContext）不能平行查詢
var t1 = _uow.Products.GetAllAsync(ct);
var t2 = _uow.Orders.GetAllAsync(ct);
await Task.WhenAll(t1, t2);   // 會拋出 InvalidOperationException

// ✅ 正確做法一：循序執行（同一個 DbContext）
var products = await _uow.Products.GetAllAsync(ct).ConfigureAwait(false);
var orders   = await _uow.Orders.GetAllAsync(ct).ConfigureAwait(false);

// ✅ 正確做法二：用 IDbContextFactory 建立獨立 DbContext 再平行
public async Task<(List<Product>, List<Order>)> GetAllParallelAsync(CancellationToken ct)
{
    var t1 = Task.Run(async () =>
    {
        await using var db = _dbFactory.CreateDbContext();
        return await db.Products.AsNoTracking().ToListAsync(ct);
    }, ct);

    var t2 = Task.Run(async () =>
    {
        await using var db = _dbFactory.CreateDbContext();
        return await db.Orders.AsNoTracking().ToListAsync(ct);
    }, ct);

    await Task.WhenAll(t1, t2).ConfigureAwait(false);
    return (t1.Result, t2.Result);
}
```

#### Task.WhenAny：取最快完成的結果

```csharp
// ✅ 適合：多個來源取最快回應（例如快取 vs DB fallback）
public async Task<ProductViewModel?> GetWithFallbackAsync(int id, CancellationToken ct)
{
    var cacheTask = _cache.GetAsync<ProductViewModel>(CacheKeys.ProductDetail, ct);
    var dbTask    = _uow.Products.GetByIdAsync(id, ct)
                        .ContinueWith(t => _mapper.Map<ProductViewModel>(t.Result), ct);

    var winner = await Task.WhenAny(cacheTask, dbTask).ConfigureAwait(false);
    return await winner.ConfigureAwait(false);
}
```

#### SemaphoreSlim：限制平行數量

```csharp
// ✅ 適合：批次處理大量項目，但不能同時全部跑（避免爆資源）
public async Task ProcessBatchAsync(
    IEnumerable<int> ids, CancellationToken ct = default)
{
    using var semaphore = new SemaphoreSlim(initialCount: 5); // 最多同時 5 個

    var tasks = ids.Select(async id =>
    {
        await semaphore.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await ProcessOneAsync(id, ct).ConfigureAwait(false);
        }
        finally
        {
            semaphore.Release();
        }
    });

    await Task.WhenAll(tasks).ConfigureAwait(false);
}
```

#### 決策速查表

| 情況 | 做法 |
|------|------|
| 多個操作有先後依賴 | 循序 await |
| 多個獨立外部服務呼叫 | Task.WhenAll |
| 同一個 DbContext 多張表查詢 | 循序 await（不能平行） |
| 需要平行查詢不同 DB 資料 | IDbContextFactory + Task.WhenAll |
| 批次處理、怕爆資源 | SemaphoreSlim 限流 |
| 多來源取最快回應 | Task.WhenAny |
| 背景長時間工作 | IHostedService / BackgroundService |

---

## 五、共用型別（KoreanLearn.Library）

### ServiceResult

```csharp
// src/KoreanLearn.Library/Helpers/ServiceResult.cs
namespace KoreanLearn.Library.Helpers;

public sealed class ServiceResult<T>
{
    public bool IsSuccess { get; private init; }
    public T? Data { get; private init; }
    public string? ErrorMessage { get; private init; }
    public IReadOnlyList<string> Errors { get; private init; } = [];

    public static ServiceResult<T> Success(T data) =>
        new() { IsSuccess = true, Data = data };

    public static ServiceResult<T> Failure(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };

    public static ServiceResult<T> Failure(IEnumerable<string> errors) =>
        new() { IsSuccess = false, Errors = [..errors] };
}
```

### PagedResult

```csharp
// src/KoreanLearn.Library/Helpers/PagedResult.cs
namespace KoreanLearn.Library.Helpers;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages   => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext     => Page < TotalPages;
}
```

### 共用列舉

```csharp
// src/KoreanLearn.Library/Enums/OrderStatus.cs
namespace KoreanLearn.Library.Enums;

public enum OrderStatus
{
    Pending   = 0,  // 待付款
    Paid      = 1,  // 已付款
    Shipped   = 2,  // 已出貨
    Completed = 3,  // 已完成
    Cancelled = 4   // 已取消
}
```

---

## 六、資料層（KoreanLearn.Data）

### 6-1 軟刪除

所有需要軟刪除的 Entity 實作 `ISoftDeletable`：

```csharp
// src/KoreanLearn.Data/Entities/ISoftDeletable.cs
namespace KoreanLearn.Data.Entities;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}
```

Entity 範例：

```csharp
// src/KoreanLearn.Data/Entities/Product.cs
namespace KoreanLearn.Data.Entities;

public class Product : ISoftDeletable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? MainImageUrl { get; set; }
    public int AuthorId { get; set; }
    public int CategoryId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public Author Author { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}
```

### 6-2 ApplicationDbContext（含軟刪除攔截與 Global Query Filter）

```csharp
// src/KoreanLearn.Data/ApplicationDbContext.cs
namespace KoreanLearn.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Product>  Products  => Set<Product>();
    public DbSet<Author>   Authors   => Set<Author>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order>    Orders    => Set<Order>();
    public DbSet<Announcement> Announcements => Set<Announcement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Global Query Filter：軟刪除（所有 ISoftDeletable 自動過濾）
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType)) continue;

            var param  = Expression.Parameter(entityType.ClrType, "e");
            var filter = Expression.Lambda(
                Expression.Equal(
                    Expression.Property(param, nameof(ISoftDeletable.IsDeleted)),
                    Expression.Constant(false)),
                param);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // 軟刪除攔截
        foreach (var entry in ChangeTracker.Entries<ISoftDeletable>()
                     .Where(e => e.State == EntityState.Deleted))
        {
            entry.State            = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = DateTime.UtcNow;
        }

        // 自動設定 CreatedAt / UpdatedAt
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(ct);
    }
}
```

### 6-3 IDbContextFactory（平行查詢專用）

`DbContext` 不是 thread-safe，同一個 instance 禁止平行操作。
需要平行查詢時，改用 `IDbContextFactory<ApplicationDbContext>` 建立獨立 instance。

**DI 註冊（與 AddDbContext 並存）：**

```csharp
// src/KoreanLearn.Web/Infrastructure/Extensions/DataServiceExtensions.cs
services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlServer(config.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(maxRetryCount: 3)));

// 同時註冊 Factory，讓需要平行的地方可以注入
services.AddDbContextFactory<ApplicationDbContext>(opts =>
    opts.UseSqlServer(config.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(maxRetryCount: 3)),
    lifetime: ServiceLifetime.Scoped);
```

**使用方式：**

```csharp
// src/KoreanLearn.Service/Services/Implementation/DashboardService.cs
public class DashboardService(
    IUnitOfWork uow,
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ILogger<DashboardService> logger) : IDashboardService
{
    // ── 循序查詢（一般情況，用 UoW）────────────────────────────
    public async Task<int> GetProductCountAsync(CancellationToken ct = default)
        => await uow.Products.CountAsync(ct).ConfigureAwait(false);

    // ── 平行查詢（Dashboard 等需要同時撈多張表）────────────────
    public async Task<DashboardViewModel> GetDashboardAsync(CancellationToken ct = default)
    {
        // 每個 Task 各自建立獨立的 DbContext，互不干擾
        var productCountTask = Task.Run(async () =>
        {
            await using var db = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            return await db.Products.CountAsync(p => !p.IsDeleted, ct).ConfigureAwait(false);
        }, ct);

        var orderCountTask = Task.Run(async () =>
        {
            await using var db = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            return await db.Orders.CountAsync(ct).ConfigureAwait(false);
        }, ct);

        var revenueTask = Task.Run(async () =>
        {
            await using var db = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            return await db.Orders
                .Where(o => o.Status == OrderStatus.Completed)
                .SumAsync(o => o.TotalAmount, ct)
                .ConfigureAwait(false);
        }, ct);

        await Task.WhenAll(productCountTask, orderCountTask, revenueTask)
                  .ConfigureAwait(false);

        return new DashboardViewModel
        {
            ProductCount = productCountTask.Result,
            OrderCount   = orderCountTask.Result,
            TotalRevenue = revenueTask.Result
        };
    }
}
```

**使用規則：**

| 情況 | 用什麼 |
|------|--------|
| 一般 CRUD、循序操作 | `IUnitOfWork`（注入 `_uow`） |
| 需要平行查詢不同資料 | `IDbContextFactory`（各自 `CreateDbContextAsync`） |
| 平行查詢後需要寫入 | 各自建立 context 查詢，寫入統一走 `_uow.SaveChangesAsync` |
| 背景服務（`IHostedService`）| 一定用 `IDbContextFactory`（背景服務是 Singleton，不能注入 Scoped DbContext）|

> ⚠️ `CreateDbContextAsync` 建立的 context 用完必須 `await using` 確保釋放，
> 不可以把它存起來跨請求重用。


### 6-4 Repository Pattern

```csharp
// src/KoreanLearn.Data/Repositories/Interfaces/IRepository.cs
namespace KoreanLearn.Data.Repositories.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?>                  GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<T>>    GetAllAsync(CancellationToken ct = default);
    Task<PagedResult<T>>      GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<T>                   AddAsync(T entity, CancellationToken ct = default);
    void                      Update(T entity);
    void                      Remove(T entity);  // ISoftDeletable 由 DbContext 自動攔截為軟刪除
}

// src/KoreanLearn.Data/Repositories/Interfaces/IProductRepository.cs
public interface IProductRepository : IRepository<Product>
{
    Task<bool>               ExistsByTitleAsync(string title, CancellationToken ct = default);
    Task<PagedResult<Product>> SearchAsync(string keyword, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId, CancellationToken ct = default);
}
```

```csharp
// src/KoreanLearn.Data/Repositories/Implementation/Repository.cs
namespace KoreanLearn.Data.Repositories.Implementation;

public class Repository<T>(ApplicationDbContext db) : IRepository<T> where T : class
{
    protected readonly DbSet<T> DbSet = db.Set<T>();

    public Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => DbSet.FindAsync([id], ct).AsTask();

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await DbSet.AsNoTracking().ToListAsync(ct).ConfigureAwait(false);

    public async Task<PagedResult<T>> GetPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var total = await DbSet.CountAsync(ct).ConfigureAwait(false);
        var items = await DbSet.AsNoTracking()
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct).ConfigureAwait(false);
        return new PagedResult<T>(items, total, page, pageSize);
    }

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await DbSet.AddAsync(entity, ct).ConfigureAwait(false);
        return entity;
    }

    public void Update(T entity) => DbSet.Update(entity);
    public void Remove(T entity) => DbSet.Remove(entity);
}
```

### 6-5 Unit of Work

```csharp
// src/KoreanLearn.Data/UnitOfWork/IUnitOfWork.cs
namespace KoreanLearn.Data.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IProductRepository      Products      { get; }
    IAuthorRepository       Authors       { get; }
    ICategoryRepository     Categories    { get; }
    IOrderRepository        Orders        { get; }
    IAnnouncementRepository Announcements { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

// src/KoreanLearn.Data/UnitOfWork/UnitOfWork.cs
public class UnitOfWork(
    ApplicationDbContext db,
    IProductRepository      products,
    IAuthorRepository       authors,
    ICategoryRepository     categories,
    IOrderRepository        orders,
    IAnnouncementRepository announcements) : IUnitOfWork
{
    public IProductRepository      Products      => products;
    public IAuthorRepository       Authors       => authors;
    public ICategoryRepository     Categories    => categories;
    public IOrderRepository        Orders        => orders;
    public IAnnouncementRepository Announcements => announcements;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);

    public void Dispose() => db.Dispose();
}
```

### 6-6 Entity 型別設定（Fluent API，Entity 上不加 DataAnnotations）

```csharp
// src/KoreanLearn.Data/Configurations/ProductConfiguration.cs
namespace KoreanLearn.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .IsRequired().HasMaxLength(200);

        builder.Property(p => p.Price)
            .HasColumnType("decimal(18,2)").IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.HasIndex(p => p.Title);

        builder.HasOne(p => p.Author)
            .WithMany(a => a.Products)
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

### 6-7 Migration 規則

```bash
# 命名格式：PascalCase 描述實際變更內容
dotnet ef migrations add AddProductDescriptionColumn \
    -p src/KoreanLearn.Data -s src/KoreanLearn.Web

dotnet ef database update \
    -p src/KoreanLearn.Data -s src/KoreanLearn.Web
```

- Migration 名稱必須清楚（`AddProductDescriptionColumn`，不用 `Update1`）
- **永遠不刪除或修改**已套用的 Migration
- Entity 變更 + Migration 必須在**同一個 commit** 內

---

## 七、服務層（KoreanLearn.Service）

### 7-1 Service 規範

```csharp
// src/KoreanLearn.Service/Services/Interfaces/IProductService.cs
namespace KoreanLearn.Service.Services.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductListViewModel>>   GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<ProductDetailViewModel?>             GetByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<ProductListViewModel>> CreateAsync(CreateProductViewModel vm, CancellationToken ct = default);
    Task<ServiceResult<ProductListViewModel>> UpdateAsync(int id, EditProductViewModel vm, CancellationToken ct = default);
    Task<ServiceResult>                       DeleteAsync(int id, CancellationToken ct = default);
}

// src/KoreanLearn.Service/Services/Implementation/ProductService.cs
public class ProductService(
    IUnitOfWork uow,
    IMapper mapper,
    ILogger<ProductService> logger) : IProductService
{
    public async Task<ServiceResult<ProductListViewModel>> CreateAsync(
        CreateProductViewModel vm, CancellationToken ct = default)
    {
        var exists = await uow.Products.ExistsByTitleAsync(vm.Title, ct)
            .ConfigureAwait(false);
        if (exists)
            return ServiceResult<ProductListViewModel>.Failure("項目標題已存在");

        var product = mapper.Map<Product>(vm);
        await uow.Products.AddAsync(product, ct).ConfigureAwait(false);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("建立項目 {Id}: {Title}", product.Id, product.Title);
        return ServiceResult<ProductListViewModel>.Success(mapper.Map<ProductListViewModel>(product));
    }
}
```

### 7-2 AutoMapper Profile

```csharp
// src/KoreanLearn.Service/Mapper/ProductProfile.cs
namespace KoreanLearn.Service.Mapper;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductListViewModel>();
        CreateMap<Product, ProductDetailViewModel>()
            .ForMember(d => d.AuthorName,   o => o.MapFrom(s => s.Author.Name))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name));

        CreateMap<CreateProductViewModel, Product>()
            .ForMember(d => d.MainImageUrl, o => o.Ignore()); // 圖片由 ImageService 處理
        CreateMap<EditProductViewModel, Product>()
            .ForMember(d => d.MainImageUrl, o => o.Ignore());
    }
}
```

### 7-3 ViewModel 規範

```csharp
// src/KoreanLearn.Service/ViewModels/Product/CreateProductViewModel.cs
namespace KoreanLearn.Service.ViewModels.Product;

public class CreateProductViewModel
{
    [Required(ErrorMessage = "標題為必填")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "標題須介於 1–200 字元")]
    [Display(Name = "標題")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "價格為必填")]
    [Range(0, 99999.99, ErrorMessage = "價格須在合理範圍內")]
    [Display(Name = "售價")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [Display(Name = "介紹")]
    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "請選擇作者")]
    [Display(Name = "作者")]
    public int AuthorId { get; set; }

    [Required(ErrorMessage = "請選擇分類")]
    [Display(Name = "分類")]
    public int CategoryId { get; set; }

    [Display(Name = "主圖")]
    public IFormFile? MainImage { get; set; }

    // 供 SelectList 使用
    public IEnumerable<SelectListItem> Authors    { get; set; } = [];
    public IEnumerable<SelectListItem> Categories { get; set; } = [];
}
```

### 7-4 CacheKeys 常數

```csharp
// src/KoreanLearn.Service/Constants/CacheKeys.cs
namespace KoreanLearn.Service.Constants;

public static class CacheKeys
{
    public const string ProductList      = "product:list";
    public const string ProductDetail    = "product:detail:{0}";  // string.Format(ProductDetail, id)
    public const string AnnouncementList = "announcement:list";
    public const string CategoryList     = "category:list";
    public const string AuthorList       = "author:list";
}
```

---

## 八、展示層（KoreanLearn.Web）

### 8-1 前台 Controller

```csharp
// src/KoreanLearn.Web/Controllers/ProductController.cs
namespace KoreanLearn.Web.Controllers;

public class ProductController(IProductService productService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        var result = await productService.GetPagedAsync(page, pageSize: 12, ct);
        return View(result);
    }

    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        var product = await productService.GetByIdAsync(id, ct);
        if (product is null) return NotFound();
        return View(product);
    }
}
```

### 8-2 後台 Area Controller

```csharp
// src/KoreanLearn.Web/Areas/Admin/Controllers/ProductController.cs
namespace KoreanLearn.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductController(IProductService productService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        var result = await productService.GetPagedAsync(page, pageSize: 20, ct);
        return View(result);
    }

    public async Task<IActionResult> Create(CancellationToken ct = default)
    {
        var vm = await productService.BuildCreateViewModelAsync(ct);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateProductViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            await productService.PopulateSelectListsAsync(vm, ct);
            return View(vm);
        }

        var result = await productService.CreateAsync(vm, ct);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            await productService.PopulateSelectListsAsync(vm, ct);
            return View(vm);
        }

        TempData["Success"] = $"「{vm.Title}」建立成功";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct = default)
    {
        var result = await productService.DeleteAsync(id, ct);
        TempData[result.IsSuccess ? "Success" : "Error"] =
            result.IsSuccess ? "項目已刪除" : result.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }
}
```

**Controller 規則：**
- 後台一律加 `[Area("Admin")]` + `[Authorize(Roles = "Admin")]`
- POST 一律加 `[ValidateAntiForgeryToken]`
- 使用 `TempData["Success"]` / `TempData["Error"]` 傳遞單次訊息
- 永遠不在 Controller 直接呼叫 Repository 或 DbContext

### 8-3 Razor View 規範

```cshtml
@* src/KoreanLearn.Web/Areas/Admin/Views/Product/Index.cshtml *@
@model PagedResult<ProductListViewModel>
@{ ViewData["Title"] = "項目管理"; }

@if (TempData["Success"] is string success)
{
    <div class="alert alert-success alert-dismissible fade show" role="alert">
        <i class="bi bi-check-circle-fill me-2"></i>@success
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </div>
}
@if (TempData["Error"] is string error)
{
    <div class="alert alert-danger alert-dismissible fade show" role="alert">
        <i class="bi bi-exclamation-triangle-fill me-2"></i>@error
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </div>
}

<div class="container-fluid py-3">
    <div class="d-flex justify-content-between align-items-center mb-3">
        <h1 class="h3">@ViewData["Title"]</h1>
        <a asp-area="Admin" asp-controller="Product" asp-action="Create" class="btn btn-primary">
            <i class="bi bi-plus-lg"></i> 新增項目
        </a>
    </div>

    <div class="card shadow-sm">
        <div class="card-body p-0">
            <table class="table table-hover mb-0">
                <thead class="table-light">
                    <tr>
                        <th>主圖</th><th>標題</th><th>作者</th>
                        <th class="text-end">售價</th>
                        <th class="text-center" style="width:160px">操作</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var item in Model.Items)
                    {
                        <tr>
                            <td><img src="@item.MainImageUrl" width="48" height="48"
                                     class="rounded object-fit-cover" /></td>
                            <td>@item.Title</td>
                            <td>@item.AuthorName</td>
                            <td class="text-end">@item.Price.ToString("C")</td>
                            <td class="text-center">
                                <a asp-area="Admin" asp-action="Edit" asp-route-id="@item.Id"
                                   class="btn btn-sm btn-outline-primary me-1">編輯</a>
                                <form asp-area="Admin" asp-action="Delete"
                                      asp-route-id="@item.Id" method="post" class="d-inline">
                                    @Html.AntiForgeryToken()
                                    <button type="submit" class="btn btn-sm btn-outline-danger"
                                            onclick="return confirm('確定刪除「@item.Title」？')">
                                        刪除
                                    </button>
                                </form>
                            </td>
                        </tr>
                    }
                    @if (!Model.Items.Any())
                    {
                        <tr><td colspan="5" class="text-center text-muted py-4">目前沒有資料</td></tr>
                    }
                </tbody>
            </table>
        </div>
    </div>

    @if (Model.TotalPages > 1)
    {
        <nav class="mt-3" aria-label="分頁">
            <ul class="pagination justify-content-center">
                @if (Model.HasPrevious)
                {
                    <li class="page-item">
                        <a class="page-link" asp-area="Admin" asp-action="Index"
                           asp-route-page="@(Model.Page - 1)">上一頁</a>
                    </li>
                }
                @for (int i = 1; i <= Model.TotalPages; i++)
                {
                    <li class="page-item @(i == Model.Page ? "active" : "")">
                        <a class="page-link" asp-area="Admin" asp-action="Index"
                           asp-route-page="@i">@i</a>
                    </li>
                }
                @if (Model.HasNext)
                {
                    <li class="page-item">
                        <a class="page-link" asp-area="Admin" asp-action="Index"
                           asp-route-page="@(Model.Page + 1)">下一頁</a>
                    </li>
                }
            </ul>
        </nav>
    }
</div>
```

**View 規則：**
- 後台連結必須加 `asp-area="Admin"`
- 永遠使用 `asp-` Tag Helpers，禁止 `@Html.ActionLink`
- POST 表單必須包含 `@Html.AntiForgeryToken()`

---

## 九、依賴注入（Program.cs）

```csharp
// src/KoreanLearn.Web/Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDataServices(builder.Configuration)
    .AddApplicationServices()
    .AddWebInfrastructure(builder.Configuration);

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapControllerRoute("areas",   "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();

// ─────────────────────────────────────────────────────────────────────────────
// src/KoreanLearn.Web/Infrastructure/Extensions/DataServiceExtensions.cs
public static class DataServiceExtensions
{
    public static IServiceCollection AddDataServices(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ApplicationDbContext>(opts =>
            opts.UseSqlServer(
                config.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure(maxRetryCount: 3)));

        services.AddScoped<IProductRepository,      ProductRepository>();
        services.AddScoped<IAuthorRepository,       AuthorRepository>();
        services.AddScoped<ICategoryRepository,     CategoryRepository>();
        services.AddScoped<IOrderRepository,        OrderRepository>();
        services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
        services.AddScoped<IUnitOfWork,             UnitOfWork>();

        return services;
    }
}

// src/KoreanLearn.Web/Infrastructure/Extensions/ApplicationServiceExtensions.cs
public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(ProductProfile).Assembly);

        services.AddScoped<IProductService,      ProductService>();
        services.AddScoped<IOrderService,        OrderService>();
        services.AddScoped<IAuthorService,       AuthorService>();
        services.AddScoped<ICategoryService,     CategoryService>();
        services.AddScoped<IAnnouncementService, AnnouncementService>();

        return services;
    }
}
```

---

## 十、錯誤處理

```csharp
// src/KoreanLearn.Library/Helpers/Exceptions.cs
namespace KoreanLearn.Library.Helpers;

public class NotFoundException(string entityName, object key)
    : Exception($"找不到 {entityName}（ID: {key}）");

public class BusinessException(string message) : Exception(message);
```

```csharp
// src/KoreanLearn.Web/Infrastructure/Middleware/GlobalExceptionMiddleware.cs
namespace KoreanLearn.Web.Infrastructure.Middleware;

public class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning("找不到資源: {Message}", ex.Message);
            context.Response.StatusCode = 404;
            context.Response.Redirect("/Error/NotFound");
        }
        catch (BusinessException ex)
        {
            logger.LogWarning("業務規則違反: {Message}", ex.Message);
            if (context.Request.Headers.Accept.Contains("application/json"))
            {
                context.Response.StatusCode = 422;
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
                return;
            }
            context.Response.Redirect("/Error");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "未預期的系統錯誤 Path={Path}", context.Request.Path);
            context.Response.StatusCode = 500;
            context.Response.Redirect("/Error");
        }
    }
}
```

---

## 十一、appsettings.json 結構

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=KoreanLearnDb;Trusted_Connection=true;TrustServerCertificate=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
    }
  },
  "ImageSettings": {
    "UploadPath": "wwwroot/images/products",
    "MaxFileSizeMb": 5,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".webp"]
  },
  "AppSettings": {
    "DefaultPageSize": 12,
    "AdminPageSize": 20
  }
}
```

```csharp
// src/KoreanLearn.Web/Infrastructure/Settings/ImageSettings.cs
public class ImageSettings
{
    public const string Section = "ImageSettings";
    public string UploadPath { get; set; } = "wwwroot/images/products";
    public int MaxFileSizeMb { get; set; } = 5;
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp"];
}
```

---

## 十二、測試規範

> 三層驗證缺一不可。每個功能完成後立刻依序執行，全部通過才能 commit。

### 12-1 目錄結構

```
tests/KoreanLearn.Tests/
├── Unit/
│   └── Services/          # xUnit + Moq：Service 業務邏輯
├── Integration/           # WebApplicationFactory：Controller + DB 整合
├── E2E/                   # Playwright：瀏覽器操作驗證
└── Fixtures/
    ├── WebAppFactory.cs   # 整合測試用 InMemory DB
    └── PlaywrightFixture.cs
```

### 12-2 層一：單元測試（xUnit + Moq + FluentAssertions）

每個 Service 方法都必須涵蓋：Happy Path、失敗情境、邊界值。

```csharp
// tests/KoreanLearn.Tests/Unit/Services/ProductServiceTests.cs
namespace KoreanLearn.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork>             _uowMock    = new();
    private readonly Mock<IMapper>                 _mapperMock = new();
    private readonly Mock<ILogger<ProductService>> _loggerMock = new();
    private readonly ProductService _sut;

    public ProductServiceTests()
        => _sut = new ProductService(_uowMock.Object, _mapperMock.Object, _loggerMock.Object);

    // ── Happy Path ──────────────────────────────────────────────
    [Fact]
    public async Task CreateAsync_WithValidData_SavesAndReturnsSuccess()
    {
        var vm      = new CreateProductViewModel { Title = "新項目", Price = 299m };
        var product = new Product { Id = 1, Title = vm.Title, Price = vm.Price };

        _uowMock.Setup(u => u.Products.ExistsByTitleAsync(vm.Title, default)).ReturnsAsync(false);
        _uowMock.Setup(u => u.Products.AddAsync(It.IsAny<Product>(), default)).ReturnsAsync(product);
        _mapperMock.Setup(m => m.Map<Product>(vm)).Returns(product);
        _mapperMock.Setup(m => m.Map<ProductListViewModel>(product))
                   .Returns(new ProductListViewModel { Id = 1, Title = vm.Title });

        var result = await _sut.CreateAsync(vm);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Title.Should().Be(vm.Title);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    // ── 失敗情境 ────────────────────────────────────────────────
    [Fact]
    public async Task CreateAsync_WhenTitleExists_ReturnsFailureWithoutSaving()
    {
        _uowMock.Setup(u => u.Products.ExistsByTitleAsync(It.IsAny<string>(), default))
                .ReturnsAsync(true);

        var result = await _sut.CreateAsync(new CreateProductViewModel { Title = "重複" });

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("已存在");
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    // ── 邊界值 ──────────────────────────────────────────────────
    [Theory]
    [InlineData("A", 0.01)]         // 最短標題、最低價格
    [InlineData("新項目", 99999.99)] // 一般標題、最高價格
    public async Task CreateAsync_WithBoundaryValues_ReturnsSuccess(string title, decimal price)
    {
        var vm = new CreateProductViewModel { Title = title, Price = price };
        _uowMock.Setup(u => u.Products.ExistsByTitleAsync(title, default)).ReturnsAsync(false);
        _uowMock.Setup(u => u.Products.AddAsync(It.IsAny<Product>(), default))
                .ReturnsAsync(new Product { Id = 1, Title = title, Price = price });
        _mapperMock.Setup(m => m.Map<Product>(vm)).Returns(new Product { Title = title });
        _mapperMock.Setup(m => m.Map<ProductListViewModel>(It.IsAny<Product>()))
                   .Returns(new ProductListViewModel { Title = title });

        var result = await _sut.CreateAsync(vm);

        result.IsSuccess.Should().BeTrue();
    }
}
```

執行指令：
```bash
dotnet test --filter "Category=Unit" --verbosity normal
```

### 12-3 層二：HTTP 驗證（curl）

每個功能完成後，啟動 dev server 並對所有端點逐一驗證。

**啟動 / 關閉 server：**
```bash
# 啟動（背景執行）
dotnet run --project src/KoreanLearn.Web --no-build &
SERVER_PID=$!
sleep 5  # 等待 Kestrel 啟動

# 驗證完畢後關閉
kill $SERVER_PID
```

**驗證腳本範本（每個功能自行調整端點）：**
```bash
BASE="http://localhost:5000"

# ── GET 列表頁：應回 200 ────────────────────────────────────────
STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/Product")
[ "$STATUS" = "200" ] && echo "✅ GET /Product → 200" || echo "❌ GET /Product → $STATUS"

# ── GET 不存在的資源：應回 404 ─────────────────────────────────
STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/Product/Detail/99999")
[ "$STATUS" = "404" ] && echo "✅ GET /Product/Detail/99999 → 404" || echo "❌ → $STATUS"

# ── POST 建立（含 AntiForgery Token，先取 cookie）──────────────
COOKIE_JAR=$(mktemp)
TOKEN=$(curl -s -c "$COOKIE_JAR" "$BASE/Admin/Product/Create"         | grep -o 'name="__RequestVerificationToken" value="[^"]*"'         | grep -o 'value="[^"]*"'         | cut -d'"' -f2)

STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X POST         -b "$COOKIE_JAR"         -d "Title=測試項目HTTP&Price=299&CategoryId=1&AuthorId=1&__RequestVerificationToken=$TOKEN"         "$BASE/Admin/Product/Create")
[ "$STATUS" = "302" ] && echo "✅ POST /Admin/Product/Create → 302 (redirect)" || echo "❌ → $STATUS"
rm "$COOKIE_JAR"

# ── 驗證表單：空白標題應回 200（含 validation error，不 redirect）
STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X POST         -d "Title=&Price=299"         "$BASE/Admin/Product/Create")
[ "$STATUS" = "200" ] && echo "✅ POST 空白標題 → 200 (validation)" || echo "❌ → $STATUS"

# 任何一項 ❌ 就視為驗證失敗，必須修正後重跑
```

### 12-4 層三：E2E 瀏覽器驗證（Playwright）

**安裝（首次）：**
```bash
dotnet add tests/KoreanLearn.Tests package Microsoft.Playwright
dotnet add tests/KoreanLearn.Tests package Microsoft.Playwright.NUnit
dotnet build tests/KoreanLearn.Tests
dotnet run --project tests/KoreanLearn.Tests -- install
```

**Fixture 設定：**
```csharp
// tests/KoreanLearn.Tests/Fixtures/PlaywrightFixture.cs
namespace KoreanLearn.Tests.Fixtures;

public class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser    Browser    { get; private set; } = null!;
    public IPage       Page       { get; private set; } = null!;

    public const string BaseUrl = "http://localhost:5000";

    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser    = await Playwright.Chromium.LaunchAsync(new()
        {
            Headless = true,   // CI 環境改 false 可看到瀏覽器畫面
            SlowMo   = 50      // 稍微放慢，方便 debug
        });
        Page = await Browser.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        Playwright.Dispose();
    }
}
```

**E2E 測試範本：**
```csharp
// tests/KoreanLearn.Tests/E2E/ProductE2ETests.cs
namespace KoreanLearn.Tests.E2E;

[Trait("Category", "E2E")]
public class ProductE2ETests : IClassFixture<PlaywrightFixture>
{
    private readonly IPage   _page;
    private readonly string  _base = PlaywrightFixture.BaseUrl;

    public ProductE2ETests(PlaywrightFixture fixture)
        => _page = fixture.Page;

    // ── 列表頁渲染正確 ──────────────────────────────────────────
    [Fact]
    public async Task IndexPage_ShouldRenderTableWithItems()
    {
        await _page.GotoAsync($"{_base}/Product");

        await Expect(_page).ToHaveTitleAsync(new Regex("項目"));
        await Expect(_page.Locator("table")).ToBeVisibleAsync();
    }

    // ── 後台建立完整流程 ────────────────────────────────────────
    [Fact]
    public async Task AdminCreate_FillFormAndSubmit_ShouldShowSuccessAndAppearInList()
    {
        var title = $"E2E測試項目_{DateTime.Now:HHmmss}"; // 唯一標題避免衝突

        // 前往建立頁
        await _page.GotoAsync($"{_base}/Admin/Product/Create");
        await Expect(_page.Locator("h1")).ToContainTextAsync("新增");

        // 填寫表單
        await _page.FillAsync("#Title",      title);
        await _page.FillAsync("#Price",      "399");
        await _page.FillAsync("#Description","E2E 自動化測試建立");
        await _page.SelectOptionAsync("#CategoryId", new[] { "1" });
        await _page.SelectOptionAsync("#AuthorId",   new[] { "1" });

        // 送出
        await _page.ClickAsync("button[type=submit]");

        // 驗證：導回列表且顯示成功訊息
        await Expect(_page.Locator(".alert-success")).ToBeVisibleAsync();
        await Expect(_page.Locator(".alert-success")).ToContainTextAsync("建立成功");

        // 驗證：列表中出現新項目
        await Expect(_page.Locator("table")).ToContainTextAsync(title);
    }

    // ── 驗證錯誤訊息顯示 ────────────────────────────────────────
    [Fact]
    public async Task AdminCreate_EmptyTitle_ShouldShowValidationError()
    {
        await _page.GotoAsync($"{_base}/Admin/Product/Create");

        // 不填標題直接送出
        await _page.FillAsync("#Price", "299");
        await _page.ClickAsync("button[type=submit]");

        // 應停在同一頁並顯示驗證錯誤
        await Expect(_page).ToHaveURLAsync(new Regex("/Create"));
        await Expect(_page.Locator(".field-validation-error")).ToBeVisibleAsync();
    }

    // ── 刪除流程 ────────────────────────────────────────────────
    [Fact]
    public async Task AdminDelete_ShouldRemoveItemFromList()
    {
        // 先取得列表第一個項目標題
        await _page.GotoAsync($"{_base}/Admin/Product");
        var firstTitle = await _page.Locator("tbody tr:first-child td:nth-child(2)").InnerTextAsync();

        // 點擊刪除並接受 confirm dialog
        _page.Dialog += (_, dialog) => dialog.AcceptAsync();
        await _page.Locator("tbody tr:first-child form button[type=submit]").ClickAsync();

        // 驗證：成功訊息出現，且該項目不在列表中
        await Expect(_page.Locator(".alert-success")).ToBeVisibleAsync();
        await Expect(_page.Locator("table")).Not.ToContainTextAsync(firstTitle);
    }
}
```

執行指令：
```bash
# 需先在背景啟動 dev server
dotnet run --project src/KoreanLearn.Web --no-build &
sleep 5

dotnet test --filter "Category=E2E" --verbosity normal

kill %1
```

### 12-5 覆蓋率目標

| 層 | 目標 |
|----|------|
| 單元測試（Service） | ≥ 80% |
| HTTP 驗證 | 每個 Action 的 200 / 302 / 404 都驗到 |
| E2E | 每個功能的 Happy Path + 一個錯誤情境 |


## 十三、Git Commit 規範

```
格式：<type>(<scope>): <subject>

type：
  feat       新功能
  fix        修正 bug
  refactor   重構（不改功能行為）
  test       新增或修改測試
  migration  新增 EF Migration
  style      UI / 樣式調整
  docs       文件更新
  chore      雜項維護

scope（功能模組）：
  course, section, lesson, quiz, flashcard, pronunciation, order, enrollment, progress, certificate, discussion, user, auth, admin, shared

subject：繁體中文，動詞開頭

範例：
  feat(product): 新增項目分頁與關鍵字搜尋
  fix(order): 修正含折扣的訂單總金額計算錯誤
  feat(admin): 實作後台公告 CRUD
  test(product): 補充 ProductService 邊界值單元測試
  migration(product): 新增 Products.Description 欄位
  refactor(shared): 將 ServiceResult 移至 KoreanLearn.Library
```

---

## 十四、Spec 文件規範

> 每個功能實作前（Step 0）必須先產生 spec md，實作完成後回來更新狀態。
> 所有 spec 統一放在 `docs/specs/`，讓整個專案的功能一目了然。

### 目錄結構

```
docs/
└── specs/
    ├── product-create.md
    ├── product-edit.md
    ├── order-checkout.md
    └── ...
```

### Spec MD 範本

````markdown
# [Scope] [Feature 名稱]

> **狀態**：🔲 待實作 / 🚧 實作中 / ✅ 完成 / ❌ 封鎖中
> **建立時間**：YYYY-MM-DD
> **完成時間**：－

---

## 功能描述

<!-- 一句話說明這個功能做什麼、為什麼需要 -->

## 使用者故事

```
身為 [角色]
我希望 [做什麼]
以便 [達成什麼目的]
```

## 驗收條件

- [ ] （具體可驗證的條件一）
- [ ] （具體可驗證的條件二）
- [ ] （具體可驗證的條件三）

## 影響範圍

| 層 | 異動項目 |
|----|----------|
| Entity | `Product`（新增 `Description` 欄位） |
| Migration | `AddProductDescription` |
| Repository | `IProductRepository.SearchAsync` |
| Service | `IProductService.CreateAsync` |
| ViewModel | `CreateProductViewModel` |
| Controller | `Admin/ProductController.Create` |
| View | `Areas/Admin/Views/Product/Create.cshtml` |

## API / 路由

| Method | 路由 | 說明 |
|--------|------|------|
| GET | `/Admin/Product/Create` | 顯示建立表單 |
| POST | `/Admin/Product/Create` | 送出建立 |

## 測試計畫

### 單元測試
- `CreateAsync_WithValidData_ReturnsSuccess`
- `CreateAsync_WhenTitleExists_ReturnsFailure`
- `CreateAsync_WithBoundaryValues_ReturnsSuccess`

### HTTP 驗證
- GET `/Admin/Product/Create` → 200
- POST 合法資料 → 302
- POST 空白標題 → 200（含 validation error）

### E2E
- 填完表單送出 → 顯示成功訊息、列表出現新項目
- 空白標題送出 → 顯示 validation error、停在同頁

## 實際產出

<!-- 實作完成後填寫 -->

- **Commit**：`feat(product): 新增項目建立功能`
- **Migration**：`AddProductDescription`
- **備註**：（遇到的問題或決策說明）
````

### 命名規則

```
{scope}-{feature}.md

scope：  product / order / user / auth / admin / announcement / shared
feature：動詞 + 名詞，用 kebab-case

範例：
  product-create.md
  product-search.md
  order-checkout.md
  user-login.md
  admin-dashboard.md
```

### 狀態更新時機

| 時機 | 狀態 |
|------|------|
| Step 0 產生 spec 後 | 🔲 待實作 |
| Step 1 開始實作時 | 🚧 實作中 |
| Step 16 三層驗證通過後 | ✅ 完成（填寫實際產出） |
| 進入 BLOCKED.md 時 | ❌ 封鎖中 |

---

## 十五、UX 自我審查清單

> 每個功能三層驗證通過後，AI 必須逐項過這份清單。
> 發現任何問題直接修正，不要只記錄不處理。

---

### 15-1 易用性（Easy to Use）

**表單與輸入**
- [ ] 每個欄位都有清楚的 `label`，不只靠 placeholder 說明
- [ ] 必填欄位有明確標示（`*` 或文字提示）
- [ ] 輸入錯誤時，錯誤訊息顯示在欄位旁邊，不是只在頂部
- [ ] 下拉選單的預設選項是「請選擇...」而不是直接選第一筆資料
- [ ] 送出按鈕的文字清楚描述動作（「建立專輯」而不是「送出」）
- [ ] 送出後按鈕應 disable 防止重複送出

**導覽與流程**
- [ ] 操作成功後有明確的成功回饋（TempData 成功訊息）
- [ ] 操作失敗後停在原頁，且保留使用者已填的資料
- [ ] 刪除等破壞性操作有二次確認（confirm dialog）
- [ ] 每個頁面有麵包屑或明確的返回路徑
- [ ] 列表頁的「新增」按鈕位置固定且顯眼

---

### 15-2 流暢性（Smoothness）

**載入與回應**
- [ ] 列表頁資料超過 20 筆時有分頁，不是一次全撈
- [ ] 圖片有設定固定寬高，不會在載入時造成版面跳動（CLS）
- [ ] 表單送出時有 loading 狀態，讓使用者知道系統正在處理
- [ ] 頁面首次載入不需要等待非必要的資源

**操作連貫性**
- [ ] 成功建立後導回列表，且新增的項目在第一頁可以看到（依建立時間排序）
- [ ] 編輯後導回正確的位置（不是永遠回第一頁）
- [ ] 搜尋 / 篩選條件在重整後不消失（query string 保留狀態）

---

### 15-3 一致性（Consistency）

**視覺與互動**
- [ ] 同樣功能的按鈕顏色一致（新增永遠是 primary、刪除永遠是 danger）
- [ ] 所有表格的欄位順序與其他功能一致（名稱、狀態、建立時間、操作）
- [ ] 成功 / 錯誤訊息的樣式統一（alert-success / alert-danger）
- [ ] 日期格式統一（`yyyy/MM/dd` 或 `yyyy-MM-dd`，全站一種）
- [ ] 金額格式統一（`ToString("C")` 或 `ToString("N0")`，全站一種）

**文字與命名**
- [ ] 同一個實體在所有頁面用同一個詞（不要有的地方叫「專輯」有的叫「Album」）
- [ ] 操作結果的成功訊息用詞一致（「XXX 建立成功」格式）

---

### 15-4 防呆與容錯（Error Prevention）

**資料安全**
- [ ] 刪除的資料走軟刪除（`ISoftDeletable`），不是真的從 DB 刪掉
- [ ] 有外鍵關聯的資料刪除前先檢查，並給出清楚的錯誤訊息
- [ ] 數字欄位有合理的上下限驗證
- [ ] 字串欄位有長度上限，防止超長輸入塞爆 DB

**操作保護**
- [ ] POST 表單都有 `[ValidateAntiForgeryToken]`
- [ ] 後台頁面都有 `[Authorize(Roles = "Admin")]`
- [ ] 使用者不能透過改 URL 存取不屬於自己的資源

---

### 15-5 審查結論格式

審查完成後，將結論更新至 spec md 的「實際產出」區塊：

```markdown
## 實際產出

- **Commit**：`feat(product): 新增項目建立功能`
- **UX 審查**：
  - ✅ 通過：易用性、流暢性、一致性、防呆
  - 🔧 修正項目：送出按鈕補 disable 狀態、錯誤訊息移至欄位旁
- **備註**：刪除功能因有訂單關聯，加上提示訊息說明無法刪除原因
```

---

## 十六、README.md 規範

> 所有任務清單完成後，AI 依此範本自動產生 README.md 放在專案根目錄。
> 內容必須根據專案實際狀況填寫，不可留範本文字。

### 範本

````markdown
# KoreanLearn

> 一句話描述這個專案是什麼、解決什麼問題。

---

## 功能特色

- ✅ （列出主要功能，例：商品 CRUD、訂單管理）
- ✅ 
- ✅ 

## 技術棧

| 層 | 技術 |
|----|------|
| 框架 | ASP.NET Core MVC (.NET 10) |
| 資料庫 | SQL Server + Entity Framework Core |
| ORM 模式 | Repository Pattern + Unit of Work |
| 物件映射 | AutoMapper |
| 測試 | xUnit + Moq + FluentAssertions + Playwright |
| 前端 | Bootstrap 5 + Vanilla JS |

## 專案結構

```
src/
├── KoreanLearn.Data/      # 資料存取層（Entity、Repository、UoW）
├── KoreanLearn.Service/   # 業務邏輯層（Service、ViewModel、Mapper）
├── KoreanLearn.Library/   # 共用工具（ServiceResult、PagedResult、Enum）
└── KoreanLearn.Web/       # 展示層（Controller、View、Area）
```

## 快速開始

### 前置需求

- .NET 10 SDK
- SQL Server（或 LocalDB）
- Node.js（Playwright E2E 測試用）

### 啟動步驟

```bash
# 1. 複製專案
git clone <repo-url>
cd KoreanLearn

# 2. 設定連線字串
# 編輯 src/KoreanLearn.Web/appsettings.Development.json
# 將 DefaultConnection 改為你的 SQL Server 連線字串

# 3. 建立資料庫（EF 自動建立）
dotnet ef database update -p src/KoreanLearn.Data -s src/KoreanLearn.Web

# 4. 啟動專案
dotnet run --project src/KoreanLearn.Web
```

瀏覽器開啟 `https://localhost:5001`

### 執行測試

```bash
# 單元測試
dotnet test --filter "Category=Unit"

# E2E 測試（需先啟動 dev server）
dotnet run --project src/KoreanLearn.Web &
dotnet test --filter "Category=E2E"
```

## 帳號設定

| 角色 | 帳號 | 密碼 |
|------|------|------|
| Admin | admin@example.com | （見 DbInitializer） |
| 一般用戶 | user@example.com | （見 DbInitializer） |

## 已實作功能清單

> 依據 docs/specs/ 自動彙整

| 功能 | Spec | 狀態 |
|------|------|------|
| （從 docs/specs/*.md 的標題與狀態自動填入） | | |

## 已知問題 / 待辦

- （從各 spec md 中狀態為 ❌ 的項目彙整）
- （從 BLOCKED.md 彙整）

## License

MIT
````

### 產生規則

- **功能清單**：掃描 `docs/specs/*.md`，取每個檔案的標題和狀態自動填入表格
- **已知問題**：掃描 `BLOCKED.md` 和狀態為 ❌ 的 spec，彙整至此區塊
- **技術棧**：依專案實際使用的套件填寫，不要寫沒用到的
- **帳號設定**：從 `DbInitializer.cs` 讀取實際的種子帳號填入

---

## 十七、目前任務清單

> Claude Code 請依序自主完成以下任務。
> 每項開始前先產生對應的 spec md（第十四節範本）。
> 每項完成（三層驗證全部通過 + UX 審查）後更新 spec 狀態並 git commit，再進行下一項。
> 若遇無法解決的問題（重試 3 次後仍失敗），
> 將問題記錄至 `BLOCKED.md`、spec 狀態改為 ❌，並繼續下一個任務。

---

### 🏗️ Phase 1：基礎建設（先做這些，後面全部依賴）

- [ ] **P1-01** 建立解決方案與四個專案（KoreanLearn.Data / .Service / .Library / .Web）、設定相依關係
- [ ] **P1-02** 設計並建立所有 Entity（User, Course, Section, Lesson, VideoLesson, ArticleLesson, PdfLesson, Enrollment, Order, Progress）+ ISoftDeletable + BaseEntity + Migration
- [ ] **P1-03** 建立 ApplicationDbContext（含軟刪除攔截、Global Query Filter）+ DbInitializer 種子資料（Admin 帳號、範例課程）
- [ ] **P1-04** 實作 ASP.NET Core Identity 整合（AppUser 繼承 IdentityUser、角色：Admin / Student）
- [ ] **P1-05** 實作所有 Repository Interface + Implementation + UnitOfWork
- [ ] **P1-06** 建立 Areas/Admin 和 Areas/Learn，設定各自的 Layout（_AdminLayout / _LearnLayout）
- [ ] **P1-07** 前台首頁：課程列表、課程詳情頁（未登入可瀏覽）

---

### 📚 Phase 2：核心學習功能

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

---

### 💰 Phase 3：商業與社群功能

- [ ] **P3-01** 訂單系統（Order Entity）+ 模擬結帳流程（不接真實金流，模擬付款成功）
- [ ] **P3-02** Enrollment 選課紀錄（付款成功後自動建立）+ 權限控管（未購買不能進 Learn Area）
- [ ] **P3-03** 訂閱制方案（SubscriptionPlan Entity）+ 訂閱後解鎖所有課程
- [ ] **P3-04** 討論區（Discussion / DiscussionReply）+ 留言 CRUD + 軟刪除
- [ ] **P3-05** 學生儀表板（我的課程、學習進度百分比、連續學習天數）
- [ ] **P3-06** 證書產生（完課條件：所有 Lesson 完成 + 測驗 >= 70 分）+ QuestPDF 產生 PDF 證書
- [ ] **P3-07** 後台儀表板（總用戶數、總收入、熱門課程、最近訂單）+ IDbContextFactory 平行查詢
- [ ] **P3-08** IHostedService 背景服務（每日檢查字卡到期提醒、連續學習天數更新）

---

### 🏁 收尾

- [ ] **F-01** 全站 UX 審查（依第十五節清單逐頁檢查）
- [ ] **F-02** 產生 README.md（依第十六節範本）

---

*此檔案需人工維護。Claude Code 不修改本檔案。*
