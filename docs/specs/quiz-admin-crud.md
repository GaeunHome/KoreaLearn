# [Quiz] 後台測驗題目管理

> **狀態**：✅ 完成
> **建立時間**：2026-03-25
> **完成時間**：2026-03-25

## 功能描述

後台管理員可為 Lesson 建立測驗、新增/編輯/刪除題目（選擇題、填空題）與選項。

## 驗收條件

- [ ] 測驗 CRUD（綁定 Lesson）
- [ ] 題目 CRUD（選擇題 + 填空題）
- [ ] 選項管理（新增/編輯/刪除/標記正確答案）
- [ ] 排序功能

## 影響範圍

| 層 | 異動項目 |
|----|----------|
| Service | IQuizAdminService, QuizAdminService |
| ViewModel | QuizFormViewModel, QuestionFormViewModel, OptionFormViewModel |
| Mapper | QuizAdminProfile |
| Controller | Admin/QuizController |
| View | Admin/Views/Quiz/* |

## API / 路由

| Method | 路由 | 說明 |
|--------|------|------|
| GET | /Admin/Quiz/Create?lessonId={id} | 建立測驗 |
| GET | /Admin/Quiz/Edit/{id} | 編輯測驗（含題目管理） |
| POST | /Admin/Quiz/AddQuestion/{quizId} | 新增題目 |
| POST | /Admin/Quiz/DeleteQuestion/{id} | 刪除題目 |
| POST | /Admin/Quiz/Delete/{id} | 刪除測驗 |

## 實際產出

- **Service**：IQuizAdminService / QuizAdminService（Quiz/Question CRUD）
- **ViewModel**：QuizFormViewModel, QuizDetailViewModel, QuestionFormViewModel, QuestionViewModel, OptionFormViewModel, OptionViewModel
- **Controller**：Admin/QuizController（Create, Detail, Edit, Delete, AddQuestion, EditQuestion, DeleteQuestion）
- **View**：Quiz/Create, Detail, Edit, AddQuestion, EditQuestion
- **HTTP 驗證**：200/302/404 全部通過
- **UX 審查**：✅ 通過
