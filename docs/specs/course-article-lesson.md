# [Course] 文字教材（富文字編輯器 + 閱讀頁）

> **狀態**：✅ 完成
> **建立時間**：2026-03-25
> **完成時間**：2026-03-25

## 功能描述

後台管理員可使用富文字編輯器（Quill.js）編輯文章內容，前台學生可閱讀文章並標記完成。

## 驗收條件

- [ ] 後台 Article 類型 Lesson 使用 Quill.js 富文字編輯器
- [ ] 前台文章閱讀頁（渲染 HTML 內容）
- [ ] 閱讀完畢可標記完成

## 影響範圍

| 層 | 異動項目 |
|----|----------|
| Service | ILessonPlayerService 擴充 GetArticlePlayerAsync |
| ViewModel | ArticlePlayerViewModel |
| Controller | Learn/LessonController 新增 Article action |
| View | Learn/Lesson/Article.cshtml、Admin/Lesson/Create+Edit（Quill） |

## API / 路由

| Method | 路由 | 說明 |
|--------|------|------|
| GET | /Learn/Lesson/Article/{id} | 文章閱讀頁 |

## 實際產出

- **Service**：ILessonPlayerService 擴充 GetArticlePlayerAsync、LessonPlayerService 共用 context 方法
- **ViewModel**：ArticlePlayerViewModel
- **Controller**：Learn/LessonController 新增 Article action
- **View**：Learn/Lesson/Article.cshtml（HTML 渲染 + 完成標記 + 前後導航）
- **後台**：Admin/Lesson Create+Edit 整合 Quill.js 富文字編輯器
- **HTTP 驗證**：200（已登入）、302（未登入）、404（不存在）
- **UX 審查**：✅ 通過
