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
├── tests/KoreanLearn.Tests/         # 測試專案
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

首次啟動會自動執行 Migration 和種子資料。預設網址：`http://localhost:5154`

### 系統預設帳號

| 角色 | Email | 密碼 | 功能範圍 |
|------|-------|------|----------|
| 管理員 (Admin) | admin@koreanlearn.com | Admin@123 | `/Admin` — 系統管理、用戶管理、所有課程管理、訂單管理 |
| 教師 (Teacher) | teacher@koreanlearn.com | Teacher@123 | `/Teacher` — 管理自己的課程、章節、單元、檔案上傳 |
| 學生 (Student) | student@koreanlearn.com | Student@123 | `/Learn` — 課程學習、測驗、字卡、發音練習、討論區 |

> 密碼規則：至少 6 碼，無特殊字元要求。帳號鎖定：連續 5 次錯誤鎖定 5 分鐘。
> 新註冊用戶自動獲得 Student 角色，管理員可在後台將用戶升級為 Teacher。

## 測試

```bash
dotnet test                                    # 全部測試
dotnet test --filter "Category=Unit"           # 單元測試
dotnet test --collect:"XPlat Code Coverage"    # 覆蓋率
```

## 已實作功能

### Phase 1：基礎建設
- [x] 四層架構專案（Data / Service / Library / Web）
- [x] 28 個 Entity + ISoftDeletable + BaseEntity + Fluent API
- [x] ApplicationDbContext（軟刪除攔截、Global Query Filter、自動時戳、RowVersion）
- [x] ASP.NET Core Identity（三角色：Admin / Teacher / Student）
- [x] Repository + UnitOfWork + IDbContextFactory
- [x] Areas（Admin / Teacher / Learn / Identity）+ BaseController 繼承體系

### Phase 2：核心學習功能
- [x] 後台課程管理 CRUD（課程 / 章節 / 單元 + 檔案上傳）
- [x] 影片課程播放（HTML5 video + 進度自動儲存）
- [x] 文字教材（Quill.js 富文字編輯器）
- [x] PDF 教材上傳、預覽與下載
- [x] 學習進度追蹤（課程進度百分比 + 單元完成標記）
- [x] 測驗系統（選擇題 + 填空題 + 自動計分）
- [x] 字卡系統（SM-2 間隔重複演算法 + 翻轉動畫）
- [x] 發音練習（Web Audio API 錄音）

### Phase 3：商業與社群功能
- [x] 訂單系統 + 模擬結帳
- [x] 選課紀錄（付費內容權限管控：Enrollment + Subscription 雙重檢查）
- [x] 訂閱制方案（月/季/年）
- [x] 討論區（發文 + 回覆 + 軟刪除）
- [x] 學生儀表板 + 教師儀表板 + 管理員儀表板
- [x] 證書產生（QuestPDF）
- [x] 背景服務（訂閱到期檢查 + 字卡複習統計）
- [x] 課程附件系統（LessonAttachment）

### 架構與安全
- [x] 三角色系統（Admin / Teacher / Student）+ 用戶管理（升降級教師）
- [x] IAuthService 封裝 Identity（Web 層零 Data 引用）
- [x] Identity 改為 MVC Controller（從 Razor Pages 遷移至 Areas/Identity/Controllers/）
- [x] 付費內容權限管控（LessonPlayerService + ProgressService 檢查 Enrollment）
- [x] Rate Limiting（auth: 10/min, global: 100/min）
- [x] SecurityHeaders Middleware（CSP, X-Frame-Options 等）
- [x] CorrelationId Middleware（請求追蹤）
- [x] MaintenanceMode Middleware（維護模式）
- [x] Cookie 安全設定（HttpOnly, SameSite, 2hr 過期）

### 管理與運維
- [x] 橫幅管理（Banner CRUD）
- [x] 系統設定（SystemSetting）
- [x] 快取服務（ICacheService）
- [x] 密碼歷史紀錄（PasswordHistory）
- [x] 檔案上傳服務（IFileUploadService）
- [x] SMTP 郵件服務（IEmailService）
- [x] 共用 ViewComponent（LearningBadge）
- [x] 共用 Partial View（_CourseCard, _Pagination）
- [x] API 端點（FlashcardApi, ProgressApi）
- [x] LessonType 擴充方法（消除重複的 switch 表達式）
- [x] CourseFormViewModel 合併（Create + Edit 共用單一 ViewModel）
- [x] TeacherCourseService 委派模式（所有權驗證後委派給 CourseAdminService）

## 功能特色

- **嚴謹三層式架構**：Web → Service → Data → Library，Web 層完全不碰 Data 層 Entity
- **三角色系統**：管理員 / 教師 / 學生，各自獨立 Area + BaseController
- **教師委派模式**：TeacherCourseService 驗證所有權後委派給 CourseAdminService，消除重複邏輯
- **付費內容門禁**：未購買無法存取課程內容，支援單課購買和訂閱制
- **軟刪除**：ISoftDeletable + DbContext 自動攔截 + Global Query Filter
- **IDbContextFactory**：平行查詢和背景服務使用獨立 DbContext
- **IAuthService**：所有 Identity 操作封裝在 Service 層
- **Mapster 物件映射**：IRegister Profile，Entity 與 ViewModel 自動轉換
- **N+1 查詢優化**：批次載入取代逐筆查詢
- **Serilog 結構化日誌**：Console + File 輪替（30 天）+ CorrelationId
- **SweetAlert2**：統一的前端提示與確認對話框
