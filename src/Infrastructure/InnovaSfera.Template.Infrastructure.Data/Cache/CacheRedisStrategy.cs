using InnovaSfera.Template.Domain.Interfaces.Cache;

namespace InnovaSfera.Template.Infrastructure.Data.Cache;

public class CacheRedisStrategy : ICacheStrategy
{

    public string GetCachedString(string key)
    {
        if (!string.IsNullOrEmpty(key))
        {
            string cachedJsonObject = RedisContext.Connection.GetDatabase().StringGet(key);
            return !string.IsNullOrEmpty(cachedJsonObject) ? cachedJsonObject : string.Empty;
        }
        return string.Empty;
    }

    public T GetCacheObject<T>(string key)
    {
        if (!string.IsNullOrEmpty(key))
        {
            string cachedJsonObject = RedisContext.Connection.GetDatabase().StringGet(key);
            if (!string.IsNullOrEmpty(cachedJsonObject))
            {
                return System.Text.Json.JsonSerializer.Deserialize<T>(cachedJsonObject);
            }
        }
        return default(T);
    }

    public string SetCachedObject(string key, object toCache, int timeExpire)
    {
        if (!string.IsNullOrEmpty(key) && toCache != null)
        {
            RedisContext.Connection.GetDatabase().StringSet(key, System.Text.Json.JsonSerializer.Serialize(toCache), TimeSpan.FromMinutes(timeExpire));
        }
        return key;
    }

    public string SetCachedString(string key, string toCache, int timeExpire)
    {
        if (!string.IsNullOrEmpty(key) && toCache != null)
        {
            RedisContext.Connection.GetDatabase().StringSet(key, toCache, TimeSpan.FromMinutes(timeExpire));
        }
        return key;
    }
}
