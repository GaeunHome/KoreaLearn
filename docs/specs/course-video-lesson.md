---
# [Course] 影片課程上傳與播放

> **狀態**：🔲 待實作
> **建立時間**：2026-03-25
> **完成時間**：－

## 功能描述

後台管理員可上傳影片檔案至 wwwroot/uploads/videos/，前台學生可觀看 HTML5 影片播放器並自動儲存播放進度。

## 使用者故事

- 身為管理員，我希望在建立/編輯單元時上傳影片檔案，以便提供影片課程內容。
- 身為學生，我希望在前台播放影片並記錄播放進度，以便下次從上次位置繼續觀看。

## 驗收條件

- [ ] 後台建立 Video 類型 Lesson 時可上傳影片檔案
- [ ] 影片儲存至 wwwroot/uploads/videos/
- [ ] 前台影片播放頁（HTML5 video player）
- [ ] 播放進度自動儲存（透過 Progress Entity）
- [ ] 重新進入時從上次進度繼續播放
- [ ] 影片播放完畢自動標記 Lesson 完成

## 影響範圍

| 層 | 異動項目 |
|----|----------|
| Service | ILessonPlayerService, LessonPlayerService, IProgressService, ProgressService |
| ViewModel | LessonPlayerViewModel, VideoPlayerViewModel |
| Mapper | LessonPlayerProfile |
| Controller | Learn/LessonController |
| View | Learn/Views/Lesson/Video.cshtml |

## API / 路由

| Method | 路由 | 說明 |
|--------|------|------|
| GET | /Learn/Lesson/Video/{id} | 影片播放頁 |
| POST | /Learn/Lesson/SaveProgress | AJAX 儲存播放進度 |
| POST | /Learn/Lesson/Complete/{id} | 標記完成 |

## 測試計畫

### HTTP 驗證
- GET /Learn/Lesson/Video/1 → 200（已登入已購買）或 302（未登入）
- POST /Learn/Lesson/SaveProgress → 200（JSON）
- POST /Learn/Lesson/Complete/1 → 200（JSON）

## 實際產出

<!-- 實作完成後填寫 -->
