# [Infra] 建立解決方案與四個專案

> **狀態**：🚧 實作中
> **建立時間**：2026-03-24
> **完成時間**：－

---

## 功能描述

將現有的單一專案重構為四層架構（Library / Data / Service / Web），建立正確的相依關係。

## 使用者故事

```
身為 開發者
我希望 專案有清楚的分層架構
以便 各層職責分離，降低耦合度
```

## 驗收條件

- [ ] 解決方案包含四個專案：KoreanLearn.Library / .Data / .Service / .Web
- [ ] 相依方向正確：Web → Service → Data → Library
- [ ] `dotnet build` 成功
- [ ] 舊的 KoreaLearning 專案已移除

## 影響範圍

| 層 | 異動項目 |
|----|----------|
| Solution | 重建 `KoreanLearn.slnx` |
| Library | 新建 `src/KoreanLearn.Library/` |
| Data | 新建 `src/KoreanLearn.Data/` |
| Service | 新建 `src/KoreanLearn.Service/` |
| Web | 新建 `src/KoreanLearn.Web/` |

## 實際產出

<!-- 實作完成後填寫 -->
