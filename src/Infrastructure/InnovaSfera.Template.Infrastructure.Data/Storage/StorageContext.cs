using InnovaSfera.Template.Domain.Interfaces.Storage;

namespace InnovaSfera.Template.Infrastructure.Data.Storage;

public class StorageContext : IStorageContext
{
    private IStorageStrategy _strategy;

    public StorageContext(IStorageStrategy strategy)
    {
        _strategy = strategy;
    }

    public void SetStrategy(IStorageStrategy strategy)
    {
        _strategy = strategy;
    }

    public async Task<IEnumerable<string>> GetFilesAsync(string path)
    {
        return await _strategy.GetFilesAsync(path);
    }

    public async Task<byte[]> ReadFileAsync(string filePath)
    {
        return await _strategy.ReadFileAsync(filePath);
    }

    public async Task WriteFileAsync(string filePath, byte[] content)
    {
        await _strategy.WriteFileAsync(filePath, content);
    }

    public async Task DeleteFileAsync(string filePath)
    {
        await _strategy.DeleteFileAsync(filePath);
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        return await _strategy.FileExistsAsync(filePath);
    }

    public string GetStorageType() => _strategy.StorageType;
}
