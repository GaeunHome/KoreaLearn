namespace KoreanLearn.Library.Helpers;

public sealed class ServiceResult
{
    public bool IsSuccess { get; private init; }
    public string? ErrorMessage { get; private init; }
    public IReadOnlyList<string> Errors { get; private init; } = [];

    public static ServiceResult Success() =>
        new() { IsSuccess = true };

    public static ServiceResult Failure(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };

    public static ServiceResult Failure(IEnumerable<string> errors) =>
        new() { IsSuccess = false, Errors = [..errors] };
}

public sealed class ServiceResult<T>
{
    public bool IsSuccess { get; private init; }
    public T? Data { get; private init; }
    public string? ErrorMessage { get; private init; }
    public IReadOnlyList<string> Errors { get; private init; } = [];

    public static ServiceResult<T> Success(T data) =>
        new() { IsSuccess = true, Data = data };

    public static ServiceResult<T> Failure(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };

    public static ServiceResult<T> Failure(IEnumerable<string> errors) =>
        new() { IsSuccess = false, Errors = [..errors] };
}
