namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>快取服務介面，封裝記憶體快取邏輯</summary>
public interface ICacheService
{
    /// <summary>從快取取得資料，若不存在則透過 factory 建立並快取</summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

    /// <summary>清除指定快取鍵</summary>
    void Remove(string key);

    /// <summary>清除指定前綴的所有快取</summary>
    void RemoveByPrefix(string prefix);
}
