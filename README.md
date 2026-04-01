# KoreanLearn 韓文線上學習平台

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4)](https://docs.microsoft.com/aspnet/core/)
[![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4)](https://docs.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927)](https://www.microsoft.com/sql-server/)

韓文線上學習平台，提供課程瀏覽、購買、沉浸式學習、測驗、字卡複習、發音練習等完整學習體驗。支援三種角色（管理員 / 教師 / 學生）與嚴謹的三層式架構。

## 技術

| 項目 | 技術 |
|------|------|
| 框架 | ASP.NET Core MVC (.NET 10) |
| 資料庫 | SQL Server + Entity Framework Core |
| 身份驗證 | ASP.NET Core Identity（IAuthService 封裝） |
| ORM | Entity Framework Core (Code First + Fluent API) |
| 物件映射 | Mapster |
| PDF 產生 | QuestPDF |
| 日誌 | Serilog（結構化日誌 + 檔案輪替） |
| 前端 | Bootstrap 5 + Bootstrap Icons + Quill.js + SweetAlert2 |
| 安全 | Rate Limiting + SecurityHeaders + AntiForgery + CorrelationId |
| 測試 | xUnit + Moq + FluentAssertions |

## 專案結構

```
KoreanLearn/
├── src/
│   ├── KoreanLearn.Data/            # 資料存取層
│   │   ├── Entities/                # Entity + Fluent API Configuration（同檔）
│   │   ├── Repositories/            # Interfaces/ + Implementation/
│   │   ├── UnitOfWork/              # IUnitOfWork + UnitOfWork
│   │   ├── Migrations/              # EF Core Migrations
│   │   ├── ApplicationDbContext.cs  # 軟刪除攔截 + Global Query Filter
│   │   ├── DbInitializer.cs         # 種子資料（角色 + 帳號 + 課程）
│   │   └── DependencyInjection.cs   # Data 層 DI 註冊
│   ├── KoreanLearn.Service/         # 商業邏輯層
│   │   ├── Services/                # Interfaces/ + Implementation/
│   │   ├── ViewModels/              # 按功能分目錄（Admin/, Course/, Learn/, Teacher/, Identity/）
│   │   ├── Mapper/                  # Mapster Profiles（IRegister）
│   │   └── Constants/               # CacheKeys 等常數
│   ├── KoreanLearn.Library/         # 共用工具庫（Helpers, Enums）— 無框架相依
│   └── KoreanLearn.Web/             # 展示層
│       ├── Areas/                   # Admin / Teacher / Learn / Identity
│       ├── Controllers/             # 公開頁面 + Api/
│       ├── Views/                   # Razor Views + Shared Components
│       ├── Infrastructure/          # Middleware, Extensions, Services, ViewComponents
│       └── wwwroot/                 # CSS（components/, pages/）, JS（core/）, uploads
└── docs/specs/                      # 功能 spec 文件
```

### 架構分層

```
Web → Service → Data → Library（嚴格單向相依，Web 層零 Data 引用）
```

- **Web 層**不直接使用 `SignInManager`/`UserManager`，透過 `IAuthService`（Service 層）操作
- **Entity + Configuration 同檔**（`Data/Entities/`），`ApplyConfigurationsFromAssembly` 自動載入
- **IDbContextFactory** 用於平行查詢和背景服務
- **BaseController** 系列（`BaseController`, `AdminBaseController`, `TeacherBaseController`, `LearnBaseController`, `IdentityBaseController`）統一共用邏輯

### 三種角色

| 角色 | Area | 說明 |
|------|------|------|
| 管理員 | `/Admin` | 系統管理、用戶管理、所有課程、橫幅、系統設定 |
| 教師 | `/Teacher` | 管理自己的課程、章節、單元（委派 + 所有權驗證） |
| 學生 | `/Learn` | 學習、進度追蹤、測驗、字卡、發音練習 |

## 快速開始

### 前置需求

- .NET 10 SDK
- SQL Server

### 安裝與執行

```bash
dotnet restore
dotnet build
dotnet run --project src/KoreanLearn.Web
```

首次啟動會自動執行 Migration 和種子資料。預設網址：`httpｓ://localhost:7101`

### 系統預設帳號

| 角色 | Email | 密碼 | 功能範圍 |
|------|-------|------|----------|
| 管理員 (Admin) | admin@koreanlearn.com | Admin@123 | `/Admin` — 系統管理、用戶管理、所有課程管理、訂單管理 |
| 教師 (Teacher) | teacher@koreanlearn.com | Teacher@123 | `/Teacher` — 管理自己的課程、章節、單元、檔案上傳 |
| 學生 (Student) | student@koreanlearn.com | Student@123 | `/Learn` — 課程學習、測驗、字卡、發音練習、討論區 |

> 密碼規則：至少 6 碼，無特殊字元要求。
>
> 帳號鎖定：連續 5 次錯誤鎖定 5 分鐘。
>
> 新註冊用戶自動獲得 Student 角色，管理員可在後台將用戶升級為 Teacher。

## 已實作功能

| 分類 | 功能 | 說明 |
|:----:|------|------|
| **課程與教材** | 課程管理 CRUD | 課程 / 章節 / 單元，管理員與教師雙入口 |
| | 影片課程 | HTML5 video 串流播放 + 進度自動儲存 |
| | 文字教材 | Quill.js 富文字編輯器 |
| | PDF 教材 | 上傳、線上預覽與下載 |
| | 課程附件 | LessonAttachment + IFileUploadService |
| **學習與練習** | 進度追蹤 | 課程進度百分比 + 單元完成標記 |
| | 測驗系統 | 選擇題 + 填空題 + 自動計分 + QuizAttempt 紀錄 |
| | 字卡系統 | SM-2 間隔重複演算法 + 翻轉動畫 + FlashcardLog |
| | 發音練習 | Web Audio API 錄音 + 標準音檔對照 |
| | 完課證書 | 完課 + 測驗 >= 70 分，QuestPDF 輸出 |
| **商業功能** | 訂單系統 | 模擬結帳流程 |
| | 選課紀錄 | Enrollment + Subscription 雙重權限檢查 |
| | 訂閱制 | 月 / 季 / 年方案，訂閱後解鎖所有課程 |
| | 付費門禁 | 未購買 / 未訂閱無法存取課程內容 |
| **社群互動** | 討論區 | 發文 + 回覆 + 軟刪除 |
| | 儀表板 | 學生 / 教師 / 管理員三種專屬儀表板 |
| **架構設計** | 四層架構 | Web → Service → Data → Library，28 個 Entity |
| | Repository + UoW | Unit of Work + IDbContextFactory 平行查詢 |
| | BaseController | Admin / Teacher / Learn / Identity 繼承體系 |
| | 教師委派模式 | TeacherCourseService → CourseAdminService |
| | Mapster 映射 | IRegister Profile，Entity 與 ViewModel 自動轉換 |
| | 軟刪除 | ISoftDeletable + Global Query Filter + RowVersion |
| | 查詢優化 | 批次載入取代逐筆查詢，消除 N+1 |
| **身份驗證與安全** | 三角色系統 | Admin / Teacher / Student + 用戶升降級 |
| | IAuthService | 封裝 Identity，Web 層零 Data 引用 |
| | Identity MVC | 從 Razor Pages 遷移至 MVC Controller |
| | 密碼安全 | PasswordHistory 歷史紀錄 |
| | Rate Limiting | auth: 10/min, global: 100/min |
| | SecurityHeaders | CSP, X-Frame-Options, Cookie HttpOnly/SameSite |
| **管理與運維** | 橫幅管理 | Banner CRUD |
| | 系統設定 | SystemSetting CRUD |
| | 快取 / 郵件 | ICacheService + IEmailService (SMTP) |
| | 背景服務 | 訂閱到期檢查 + 字卡複習統計 |
| | Middleware | CorrelationId 請求追蹤 + MaintenanceMode 維護模式 |
| | 日誌 | Serilog 結構化日誌，Console + File 輪替 30 天 |
| | 共用元件 | _CourseCard、_Pagination、LearningBadge ViewComponent |
| | API 端點 | FlashcardApi、ProgressApi |
| | 前端提示 | SweetAlert2 統一確認與通知 |
