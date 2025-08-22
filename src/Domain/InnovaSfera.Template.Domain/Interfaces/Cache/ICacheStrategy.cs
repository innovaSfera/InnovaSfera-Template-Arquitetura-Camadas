namespace InnovaSfera.Template.Domain.Interfaces.Cache;

public interface ICacheStrategy
{
    T GetCacheObject<T>(string key);
    string GetCachedString(string key);
    string SetCachedString(string key, string toCache, int timeExpire);
    string SetCachedObject(string key, object toCache, int timeExpire);
}
