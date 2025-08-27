using InnovaSfera.Template.Domain.Interfaces.Cache;
using Microsoft.Extensions.Caching.Memory;

namespace InnovaSfera.Template.Infrastructure.Data.Cache;

public class CacheMemoryStrategy : ICacheStrategy
{
    private static readonly Lazy<MemoryCache> _memoryCache = new Lazy<MemoryCache>(() => 
        new MemoryCache(new MemoryCacheOptions()));

    private static MemoryCache Cache => _memoryCache.Value;

    public string GetCachedString(string key)
    {
        if (!string.IsNullOrEmpty(key))
        {
            var cachedObject = Cache.Get<string>(key);
            return cachedObject ?? string.Empty;
        }
        return string.Empty;
    }

    public T GetCacheObject<T>(string key)
    {
        if (!string.IsNullOrEmpty(key))
        {
            var cachedObject = Cache.Get(key);
            if (cachedObject != null)
            {
                return (T)cachedObject;
            }
        }
        return default(T)!;
    }

    public string SetCachedObject(string key, object toCache, int timeExpire)
    {
        if (!string.IsNullOrEmpty(key) && toCache != null)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(timeExpire)
            };
            Cache.Set(key, toCache, options);
            return key;
        }
        return string.Empty;
    }

    public string SetCachedString(string key, string toCache, int timeExpire)
    {
        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(toCache))
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(timeExpire)
            };
            Cache.Set(key, toCache, options);
            return key;
        }
        return string.Empty;
    }
}
