# [Quiz] 測驗作答介面

> **狀態**：✅ 完成
> **建立時間**：2026-03-25
> **完成時間**：2026-03-25

## 功能描述

前台學生可作答測驗（選擇題 + 填空題），提交後自動計分，記錄 QuizAttempt。

## 驗收條件

- [ ] 前台測驗作答頁面
- [ ] 選擇題：單選/多選
- [ ] 填空題：文字輸入
- [ ] 提交後自動計分
- [ ] QuizAttempt + QuizAnswer 紀錄
- [ ] 成績結果頁面

## API / 路由

| Method | 路由 | 說明 |
|--------|------|------|
| GET | /Learn/Quiz/Take/{id} | 作答頁面 |
| POST | /Learn/Quiz/Submit/{id} | 提交答案 |
| GET | /Learn/Quiz/Result/{attemptId} | 成績結果 |

## 實際產出

- **Data**：IQuizAttemptRepository / QuizAttemptRepository + UoW 更新
- **Service**：IQuizTakeService / QuizTakeService（取得測驗、提交計分、結果查看）
- **ViewModel**：QuizTakeViewModel, QuizResultViewModel, QuizSubmitModel
- **Controller**：Learn/QuizController（Take, Submit, Result）
- **View**：Take.cshtml（選擇題/填空題作答）、Result.cshtml（分數+通過/未通過+詳細對答）
- **HTTP 驗證**：200/302/404 全部通過
- **UX 審查**：✅ 通過
