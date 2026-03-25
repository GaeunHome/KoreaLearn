# [Quiz] 測驗作答介面

> **狀態**：🔲 待實作
> **建立時間**：2026-03-25
> **完成時間**：－

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

<!-- 實作完成後填寫 -->
