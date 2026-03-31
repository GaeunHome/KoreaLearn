namespace KoreanLearn.Library.Helpers;

/// <summary>共用驗證工具類別，提供統一的驗證方法</summary>
public static class ValidationHelper
{
    /// <summary>驗證字串不為空</summary>
    public static void ValidateNotEmpty(string? value, string fieldName, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{fieldName}不能為空", paramName);
    }

    /// <summary>驗證字串長度不超過指定上限</summary>
    public static void ValidateMaxLength(string? value, string fieldName, int maxLength, string paramName)
    {
        if (value is not null && value.Length > maxLength)
            throw new ArgumentException($"{fieldName}不能超過 {maxLength} 個字元", paramName);
    }

    /// <summary>驗證字串不為空且長度不超過指定上限</summary>
    public static void ValidateString(string? value, string fieldName, int maxLength, string paramName)
    {
        ValidateNotEmpty(value, fieldName, paramName);
        ValidateMaxLength(value!, fieldName, maxLength, paramName);
    }

    /// <summary>驗證 decimal 大於零</summary>
    public static void ValidatePositive(decimal value, string fieldName, string paramName)
    {
        if (value <= 0)
            throw new ArgumentException($"{fieldName}必須大於 0", paramName);
    }

    /// <summary>驗證 int 大於零</summary>
    public static void ValidatePositive(int value, string fieldName, string paramName)
    {
        if (value <= 0)
            throw new ArgumentException($"{fieldName}必須大於 0", paramName);
    }

    /// <summary>驗證 ID 有效（大於 0）</summary>
    public static void ValidateId(int id, string fieldName, string paramName)
    {
        if (id <= 0)
            throw new ArgumentException($"{fieldName}必須為有效的 ID（大於 0）", paramName);
    }

    /// <summary>驗證實體存在（不為 null）</summary>
    public static void ValidateEntityExists<T>(T? entity, string entityName, int id) where T : class
    {
        if (entity is null)
            throw new InvalidOperationException($"找不到 ID 為 {id} 的{entityName}");
    }

    /// <summary>驗證條件為 true</summary>
    public static void ValidateCondition(bool condition, string errorMessage)
    {
        if (!condition)
            throw new InvalidOperationException(errorMessage);
    }

    /// <summary>驗證集合不為空</summary>
    public static void ValidateCollectionNotEmpty<T>(IEnumerable<T>? collection, string fieldName)
    {
        if (collection is null || !collection.Any())
            throw new InvalidOperationException($"{fieldName}不能為空");
    }
}
