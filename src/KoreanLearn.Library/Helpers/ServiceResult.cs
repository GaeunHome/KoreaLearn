namespace KoreanLearn.Library.Helpers;

/// <summary>統一的 Service 層回傳結果（不含資料），表示操作成功或失敗</summary>
public sealed class ServiceResult
{
    /// <summary>操作是否成功</summary>
    public bool IsSuccess { get; private init; }

    /// <summary>失敗時的錯誤訊息</summary>
    public string? ErrorMessage { get; private init; }

    /// <summary>失敗時的多筆錯誤訊息</summary>
    public IReadOnlyList<string> Errors { get; private init; } = [];

    /// <summary>建立成功結果</summary>
    public static ServiceResult Success() =>
        new() { IsSuccess = true };

    /// <summary>建立含單一錯誤訊息的失敗結果</summary>
    public static ServiceResult Failure(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };

    /// <summary>建立含多筆錯誤訊息的失敗結果</summary>
    public static ServiceResult Failure(IEnumerable<string> errors) =>
        new() { IsSuccess = false, Errors = [..errors] };
}

/// <summary>統一的 Service 層回傳結果（含泛型資料），表示操作成功或失敗</summary>
/// <typeparam name="T">成功時回傳的資料型別</typeparam>
public sealed class ServiceResult<T>
{
    /// <summary>操作是否成功</summary>
    public bool IsSuccess { get; private init; }

    /// <summary>成功時的回傳資料</summary>
    public T? Data { get; private init; }

    /// <summary>失敗時的錯誤訊息</summary>
    public string? ErrorMessage { get; private init; }

    /// <summary>失敗時的多筆錯誤訊息</summary>
    public IReadOnlyList<string> Errors { get; private init; } = [];

    /// <summary>建立含資料的成功結果</summary>
    public static ServiceResult<T> Success(T data) =>
        new() { IsSuccess = true, Data = data };

    /// <summary>建立含單一錯誤訊息的失敗結果</summary>
    public static ServiceResult<T> Failure(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };

    /// <summary>建立含多筆錯誤訊息的失敗結果</summary>
    public static ServiceResult<T> Failure(IEnumerable<string> errors) =>
        new() { IsSuccess = false, Errors = [..errors] };
}
