using InnovaSfera.Template.Domain.Interfaces.Cache;
using Microsoft.Extensions.Caching.Memory;

namespace InnovaSfera.Template.Infrastructure.Data.Cache;

public class CacheMemoryStrategy : ICacheStrategy
{
    public string GetCachedString(string key)
    {
        if (!string.IsNullOrEmpty(key))
        {
            MemoryCache memory = new MemoryCache(new MemoryCacheOptions());
            var cachedObject = memory.Get<string>(key);
            return cachedObject ?? string.Empty;
        }
        return string.Empty;
    }

    public T GetCacheObject<T>(string key)
    {
        if (!string.IsNullOrEmpty(key))
        {
            MemoryCache memory = new MemoryCache(new MemoryCacheOptions());
            var cachedObject = memory.Get(key);
            if (cachedObject != null)
            {
                return (T)cachedObject;
            }
        }
        return default(T);
    }

    public string SetCachedObject(string key, object toCache, int timeExpire)
    {
        if (!string.IsNullOrEmpty(key) && toCache != null)
        {
            MemoryCache memory = new MemoryCache(new MemoryCacheOptions());
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(timeExpire)
            };
            memory.Set(key, toCache, options);
            return key;

        }
        return string.Empty;
    }

    public string SetCachedString(string key, string toCache, int timeExpire)
    {
        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(toCache))
        {
            MemoryCache memory = new MemoryCache(new MemoryCacheOptions());
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(timeExpire)
            };
            memory.Set(key, toCache, options);
            return key;
        }
        return string.Empty;
    }
}
