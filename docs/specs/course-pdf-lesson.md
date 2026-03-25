# [Course] PDF 教材上傳與下載

> **狀態**：✅ 完成
> **建立時間**：2026-03-25
> **完成時間**：2026-03-25

## 功能描述

後台管理員可上傳 PDF 檔案，前台學生可下載 PDF 並標記完成。

## 驗收條件

- [ ] 後台上傳 PDF（已在 Lesson Create/Edit 實作）
- [ ] 前台 PDF 預覽/下載頁
- [ ] 閱讀完畢可標記完成

## API / 路由

| Method | 路由 | 說明 |
|--------|------|------|
| GET | /Learn/Lesson/Pdf/{id} | PDF 預覽/下載頁 |

## 實際產出

- **Service**：ILessonPlayerService 擴充 GetPdfPlayerAsync
- **ViewModel**：PdfPlayerViewModel
- **Controller**：Learn/LessonController 新增 Pdf action
- **View**：Learn/Lesson/Pdf.cshtml（預覽/下載 + 完成標記）
- **HTTP 驗證**：302（未登入）、404（不存在/非 PDF）
- **UX 審查**：✅ 通過
