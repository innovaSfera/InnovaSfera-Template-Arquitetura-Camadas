using InnovaSfera.Template.Domain.Interfaces.Cache;

namespace InnovaSfera.Template.Infrastructure.Data.Cache;

public class CacheContext(ICacheStrategy _strategy) : ICacheContext
{
    public string GetCachedString(string key)
    {
        return _strategy.GetCachedString(key);
    }

    public T GetCacheObject<T>(string key)
    {
        return _strategy.GetCacheObject<T>(key);   
    }

    public string SetCachedObject(string key, object toCache, int timeExpire)
    {
        return _strategy.SetCachedObject(key, toCache, timeExpire);
    }

    public string SetCachedString(string key, string toCache, int timeExpire)
    {
        return _strategy.SetCachedObject(key, toCache, timeExpire);
    }

    public void SetStrategy(ICacheStrategy strategy)
    {
        _strategy = strategy;
    }
}
