# [Progress] 學習進度追蹤

> **狀態**：✅ 完成
> **建立時間**：2026-03-25
> **完成時間**：2026-03-25

## 功能描述

記錄每個 Lesson 的完成狀態與時間，提供課程進度百分比。前台課程詳情頁顯示各單元完成狀態。

## 驗收條件

- [ ] 課程詳情頁顯示各單元完成狀態（已登入時）
- [ ] 課程進度百分比計算
- [ ] ProgressService 已在 P2-02 實作

## API / 路由

| Method | 路由 | 說明 |
|--------|------|------|
| GET | /Course/Detail/{id} | 課程詳情頁（顯示進度） |

## 實際產出

- **Service**：CourseService.GetCourseDetailAsync 擴充 userId 參數，查詢 Progress 填入完成狀態
- **ViewModel**：CourseDetailViewModel 新增 CompletedLessons / ProgressPercent、LessonSummaryViewModel 新增 IsCompleted
- **Controller**：CourseController 傳入 userId
- **View**：Course/Detail.cshtml 顯示進度條 + 完成圖示 + 登入使用者可點擊單元連結
- **HTTP 驗證**：200（匿名/登入）
- **UX 審查**：✅ 通過
