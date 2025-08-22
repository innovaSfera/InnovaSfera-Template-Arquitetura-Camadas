using InnovaSfera.Template.Domain.Interfaces.Cache;

namespace InnovaSfera.Template.Infrastructure.Data.Cache;

public class CacheStrategyFactory : ICacheStrategyFactory
{
    public ICacheStrategy CreateStrategy(string cacheType)
    {
        return cacheType.ToLowerInvariant() switch
        {
            "memory" => new CacheMemoryStrategy(),
            "redis" => new CacheRedisStrategy(),
            _ => throw new ArgumentException($"Cache type '{cacheType}' não é suportado.")
        };
    }
}
