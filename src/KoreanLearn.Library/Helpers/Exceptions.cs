namespace KoreanLearn.Library.Helpers;

public class NotFoundException(string entityName, object key)
    : Exception($"找不到 {entityName}（ID: {key}）");

public class BusinessException(string message) : Exception(message);
