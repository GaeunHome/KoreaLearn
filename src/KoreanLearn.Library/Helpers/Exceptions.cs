namespace KoreanLearn.Library.Helpers;

/// <summary>資源找不到例外（由 GlobalExceptionMiddleware 攔截回傳 404）</summary>
public class NotFoundException(string entityName, object key)
    : Exception($"找不到 {entityName}（ID: {key}）");

/// <summary>業務邏輯例外（由 GlobalExceptionMiddleware 攔截回傳 422 或導回頁面）</summary>
public class BusinessException(string message) : Exception(message);
