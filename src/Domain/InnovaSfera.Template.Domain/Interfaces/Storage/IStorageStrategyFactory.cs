namespace InnovaSfera.Template.Domain.Interfaces.Storage;

public interface IStorageStrategyFactory
{
    IStorageStrategy CreateStrategy(string storageType);
}
