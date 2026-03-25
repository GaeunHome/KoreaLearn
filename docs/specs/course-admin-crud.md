# [Course] 後台課程管理 CRUD

> **狀態**：🚧 實作中
> **建立時間**：2026-03-25
> **完成時間**：－

---

## 功能描述

後台管理員可以建立/編輯/刪除課程、管理章節（Section）和單元（Lesson）。

## 驗收條件

- [ ] 課程列表（分頁）
- [ ] 建立課程（Title, Description, Price, Level, CoverImage）
- [ ] 編輯課程
- [ ] 刪除課程（軟刪除）
- [ ] 章節 CRUD（嵌套在課程下）
- [ ] 單元 CRUD（嵌套在章節下，支援三種類型）
- [ ] 排序功能（SortOrder）

## 影響範圍

| 層 | 異動項目 |
|----|----------|
| Service | ICourseAdminService, CourseAdminService, Admin ViewModels |
| ViewModel | CreateCourseViewModel, EditCourseViewModel, SectionFormViewModel, LessonFormViewModel |
| Mapper | CourseAdminProfile |
| Controller | Admin/CourseController, Admin/SectionController, Admin/LessonController |
| View | Admin/Views/Course/*, Admin/Views/Section/*, Admin/Views/Lesson/* |

## 實際產出

<!-- 實作完成後填寫 -->
