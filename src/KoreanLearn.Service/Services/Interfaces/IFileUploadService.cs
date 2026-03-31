using Microsoft.AspNetCore.Http;

namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>檔案上傳服務介面，統一處理所有檔案儲存邏輯</summary>
public interface IFileUploadService
{
    /// <summary>儲存上傳檔案至指定分類目錄，回傳相對 URL 路徑</summary>
    /// <param name="file">上傳的檔案</param>
    /// <param name="folder">目標子目錄（covers, videos, pdfs, audio, recordings, banners）</param>
    /// <param name="prefix">檔名前綴（選填）</param>
    Task<string> SaveAsync(IFormFile file, string folder, string? prefix = null);
}
