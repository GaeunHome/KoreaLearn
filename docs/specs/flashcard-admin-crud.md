# [Flashcard] 字卡系統後台管理

> **狀態**：✅ 完成
> **建立時間**：2026-03-25
> **完成時間**：2026-03-25

## 功能描述

後台管理員可建立字卡牌組（FlashcardDeck）、新增/編輯/刪除字卡（韓文/中文/羅馬拼音/例句）。

## 驗收條件

- [ ] 牌組列表（分頁）
- [ ] 牌組 CRUD
- [ ] 字卡 CRUD（嵌套在牌組下）
- [ ] 排序功能

## API / 路由

| Method | 路由 | 說明 |
|--------|------|------|
| GET | /Admin/FlashcardDeck/Index | 牌組列表 |
| GET | /Admin/FlashcardDeck/Create | 建立牌組 |
| GET | /Admin/FlashcardDeck/Detail/{id} | 牌組詳情（含字卡列表） |
| POST | /Admin/FlashcardDeck/AddCard/{deckId} | 新增字卡 |

## 實際產出

- FlashcardAdminService：牌組 + 字卡 CRUD
- Admin/FlashcardDeckController + 6 個 Views
- HTTP 驗證通過、UX 審查 ✅
