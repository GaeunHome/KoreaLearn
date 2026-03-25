# KoreanLearn 韓文線上學習平台

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4)](https://docs.microsoft.com/aspnet/core/)
[![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4)](https://docs.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-CC2927)](https://www.microsoft.com/sql-server/)

韓文線上學習平台，提供課程瀏覽、後台管理、學習進度追蹤等功能。

## 技術棧

| 項目 | 技術 |
|------|------|
| 框架 | ASP.NET Core MVC (.NET 10) |
| 資料庫 | SQL Server + Entity Framework Core |
| 身份驗證 | ASP.NET Core Identity |
| ORM | Entity Framework Core (Code First) |
| 物件映射 | AutoMapper |
| 日誌 | Serilog (結構化日誌) |
| 前端 | Bootstrap 5 + Bootstrap Icons |
| 測試 | xUnit + Moq + FluentAssertions |

## 專案結構

```
KoreanLearn/
├── src/
│   ├── KoreanLearn.Data/           # 資料存取層（Entities, Repositories, UoW, DbContext, Migrations）
│   ├── KoreanLearn.Service/        # 商業邏輯層（Services, ViewModels, Constants, Mapper）
│   ├── KoreanLearn.Library/        # 共用工具庫（Helpers, Enums）
│   └── KoreanLearn.Web/            # 展示層（Controllers, Areas, Views, wwwroot）
├── tests/KoreanLearn.Tests/        # 測試專案
└── docs/specs/                     # 功能規格文件
```

### 架構分層

```
Web → Service → Data → Library
```

## 快速開始

### 前置需求

- .NET 10 SDK
- SQL Server（開發環境）

### 安裝與執行

```bash
# 還原套件
dotnet restore

# 建置
dotnet build

# 執行資料庫 Migration
dotnet ef database update -p src/KoreanLearn.Data -s src/KoreanLearn.Web

# 啟動
dotnet run --project src/KoreanLearn.Web
```

預設網址：`http://localhost:5154`

### 測試帳號

| 角色 | Email | 密碼 |
|------|-------|------|
| 管理員 | admin@koreanlearn.com | Admin@123 |
| 學生 | student@koreanlearn.com | Student@123 |

## 測試

```bash
# 全部測試
dotnet test

# 單元測試
dotnet test --filter "Category=Unit"

# 覆蓋率
dotnet test --collect:"XPlat Code Coverage"
```

## 已實作功能

### Phase 1：基礎建設

- [x] **P1-01** 四層架構專案建立與相依設定
- [x] **P1-02** 所有 Entity 設計（Course, Section, Lesson, Enrollment, Order, Progress 等）+ ISoftDeletable + BaseEntity + Migration
- [x] **P1-03** ApplicationDbContext（軟刪除攔截、Global Query Filter）+ DbInitializer 種子資料
- [x] **P1-04** ASP.NET Core Identity 整合（AppUser, Admin/Student 角色）+ 登入/註冊 UI
- [x] **P1-05** 所有 Repository Interface + Implementation + UnitOfWork
- [x] **P1-06** Areas/Admin + Areas/Learn + 各自 Layout
- [x] **P1-07** 前台首頁：課程列表、課程詳情頁

### Phase 2：核心學習功能

- [x] **P2-01** 後台課程管理 CRUD（課程 / 章節 / 單元）
- [x] **P2-02** 影片課程上傳與播放（HTML5 video + 進度自動儲存）
- [x] **P2-03** 文字教材（Quill.js 富文字編輯器 + 前台閱讀頁）
- [x] **P2-04** PDF 教材上傳與下載（預覽 + 下載 + 完成標記）
- [x] **P2-05** 學習進度追蹤（課程進度百分比 + 單元完成標記）
- [x] **P2-06** 測驗系統 Entity + 後台題目管理（選擇題 + 填空題 + 選項管理）
- [x] **P2-07** 測驗作答介面 + 自動計分 + QuizAttempt 紀錄
- [x] **P2-08** 字卡系統後台管理（牌組 + 字卡 CRUD）
- [x] **P2-09** 字卡學習介面（翻轉動畫 + SM-2 間隔重複）
- [x] **P2-10** 發音練習（後台管理 + 前台錄音 Web Audio API）

### Phase 3：商業與社群功能

- [x] **P3-01** 訂單系統 + 模擬結帳
- [x] **P3-02** Enrollment 選課紀錄（付款成功自動建立）
- [ ] **P3-03** 訂閱制方案
- [ ] **P3-04** 討論區
- [ ] **P3-05** 學生儀表板
- [ ] **P3-06** 證書產生
- [ ] **P3-07** 後台儀表板
- [ ] **P3-08** 背景服務

## 功能特色

- **四層架構**：Web / Service / Data / Library 清楚分層
- **軟刪除**：實作 ISoftDeletable，DbContext 自動攔截刪除操作
- **Repository + UnitOfWork**：統一資料存取模式
- **AutoMapper**：Entity 與 ViewModel 自動映射
- **Serilog 結構化日誌**：方便監控與除錯
- **ASP.NET Core Identity**：完整的身份驗證與角色授權
