using InnovaSfera.Template.Domain.Interfaces.Storage;

namespace InnovaSfera.Template.Infrastructure.Data.Storage;

public class LocalStorageStrategy : IStorageStrategy
{
    private readonly string _basePath;

    public LocalStorageStrategy(string basePath = "files")
    {
        _basePath = basePath;
        EnsureDirectoryExists(_basePath);
    }

    public string StorageType => "Local";

    public async Task<IEnumerable<string>> GetFilesAsync(string path)
    {
        var fullPath = Path.Combine(_basePath, path);
        
        if (!Directory.Exists(fullPath))
            return Enumerable.Empty<string>();

        var files = Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories);
        return await Task.FromResult(files.Select(f => Path.GetRelativePath(_basePath, f)));
    }

    public async Task<byte[]> ReadFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found: {filePath}");

        return await File.ReadAllBytesAsync(fullPath);
    }

    public async Task WriteFileAsync(string filePath, byte[] content)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        var directory = Path.GetDirectoryName(fullPath);
        
        if (!string.IsNullOrEmpty(directory))
            EnsureDirectoryExists(directory);

        await File.WriteAllBytesAsync(fullPath, content);
    }

    public Task DeleteFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        return Task.FromResult(File.Exists(fullPath));
    }

    private static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }
}
