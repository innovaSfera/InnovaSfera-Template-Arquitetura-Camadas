namespace InnovaSfera.Template.Domain.Interfaces.Cache;

public interface ICacheStrategyFactory
{
    ICacheStrategy CreateStrategy(string cacheType);
}
