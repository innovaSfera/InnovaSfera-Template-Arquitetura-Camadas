using InnovaSfera.Template.Domain.Interfaces.Storage;

namespace InnovaSfera.Template.Domain.Interfaces.Cache;

public interface ICacheContext
{
    void SetStrategy(ICacheStrategy strategy);
    T GetCacheObject<T>(string key);
    string GetCachedString(string key);
    string SetCachedString(string key, string toCache, int timeExpire);
    string SetCachedObject(string key, object toCache, int timeExpire);
}
