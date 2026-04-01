using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Service.Services.Implementation;

/// <summary>記憶體快取服務實作，使用 IMemoryCache 搭配鍵值追蹤支援前綴批次清除</summary>
public class CacheService(IMemoryCache cache, ILogger<CacheService> logger) : ICacheService
{
    private readonly ConcurrentDictionary<string, byte> _keys = new();
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);

    /// <inheritdoc />
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (cache.TryGetValue(key, out T? cached) && cached is not null)
        {
            logger.LogDebug("快取命中 | Key={Key}", key);
            return cached;
        }

        logger.LogDebug("快取未命中，執行工廠方法 | Key={Key}", key);
        var value = await factory().ConfigureAwait(false);
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(expiration ?? DefaultExpiration)
            .RegisterPostEvictionCallback((evictedKey, _, _, _) =>
                _keys.TryRemove(evictedKey.ToString()!, out _));

        cache.Set(key, value, options);
        _keys.TryAdd(key, 0);
        logger.LogDebug("快取寫入 | Key={Key} | Expiration={Expiration}", key, expiration ?? DefaultExpiration);
        return value;
    }

    /// <inheritdoc />
    public void Remove(string key)
    {
        cache.Remove(key);
        _keys.TryRemove(key, out _);
        logger.LogDebug("快取移除 | Key={Key}", key);
    }

    /// <inheritdoc />
    public void RemoveByPrefix(string prefix)
    {
        var keysToRemove = _keys.Keys
            .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var key in keysToRemove)
        {
            cache.Remove(key);
            _keys.TryRemove(key, out _);
        }

        logger.LogDebug("快取批次移除 | Prefix={Prefix} | RemovedCount={Count}", prefix, keysToRemove.Count);
    }
}
