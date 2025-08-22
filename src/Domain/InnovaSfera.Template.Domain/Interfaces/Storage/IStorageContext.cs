namespace InnovaSfera.Template.Domain.Interfaces.Storage;

public interface IStorageContext
{
    void SetStrategy(IStorageStrategy strategy);
    Task<IEnumerable<string>> GetFilesAsync(string path);
    Task<byte[]> ReadFileAsync(string filePath);
    Task WriteFileAsync(string filePath, byte[] content);
    Task DeleteFileAsync(string filePath);
    Task<bool> FileExistsAsync(string filePath);
    string GetStorageType();
}
