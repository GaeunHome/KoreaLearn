using KoreanLearn.Service.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace KoreanLearn.Web.Infrastructure.Services;

/// <summary>檔案上傳服務實作，統一處理檔案儲存至 wwwroot/uploads/ 目錄</summary>
public class FileUploadService(ILogger<FileUploadService> logger) : IFileUploadService
{
    public async Task<string> SaveAsync(IFormFile file, string folder, string? prefix = null)
    {
        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folder);
        Directory.CreateDirectory(uploadsDir);

        var filePrefix = string.IsNullOrEmpty(prefix) ? "" : $"{prefix}-";
        var fileName = $"{filePrefix}{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        logger.LogInformation("檔案上傳成功 | Folder={Folder} | File={FileName}", folder, fileName);
        return $"/uploads/{folder}/{fileName}";
    }
}
