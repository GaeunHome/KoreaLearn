# [Entity] 設計並建立所有 Entity + Migration

> **狀態**：🚧 實作中
> **建立時間**：2026-03-24
> **完成時間**：－

---

## 功能描述

建立韓文學習平台的所有核心 Entity，包含 BaseEntity、ISoftDeletable 介面，以及初始 Migration。

## 使用者故事

```
身為 開發者
我希望 有完整的資料模型定義
以便 後續功能開發有穩固的資料基礎
```

## 驗收條件

- [ ] BaseEntity 基底類別（Id, CreatedAt, UpdatedAt）
- [ ] ISoftDeletable 介面
- [ ] 所有 Entity 建立完成：AppUser, Course, Section, Lesson, VideoLesson, ArticleLesson, PdfLesson, Enrollment, Order, OrderItem, Progress, Quiz 相關, Flashcard 相關, Discussion 相關
- [ ] Fluent API Configuration 完成
- [ ] 初始 Migration 建立成功
- [ ] `dotnet build` 成功

## 影響範圍

| 層 | 異動項目 |
|----|----------|
| Library | Enums（OrderStatus, PaymentStatus, LessonType 等） |
| Data | Entities/, Configurations/, Migration |

## Entity 清單

### 核心
- AppUser（繼承 IdentityUser）
- Course, Section, Lesson
- VideoLesson, ArticleLesson, PdfLesson（Lesson 子類型）

### 商業
- Order, OrderItem
- Enrollment
- SubscriptionPlan, UserSubscription

### 學習
- Progress
- Quiz, QuizQuestion, QuizOption, QuizAttempt, QuizAnswer
- FlashcardDeck, Flashcard, FlashcardLog

### 社群
- Discussion, DiscussionReply

### 系統
- Announcement

## 實際產出

<!-- 實作完成後填寫 -->
