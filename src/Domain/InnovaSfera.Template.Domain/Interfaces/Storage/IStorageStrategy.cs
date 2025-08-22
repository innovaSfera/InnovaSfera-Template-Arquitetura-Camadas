namespace InnovaSfera.Template.Domain.Interfaces.Storage;

public interface IStorageStrategy
{
    Task<IEnumerable<string>> GetFilesAsync(string path);
    Task<byte[]> ReadFileAsync(string filePath);
    Task WriteFileAsync(string filePath, byte[] content);
    Task DeleteFileAsync(string filePath);
    Task<bool> FileExistsAsync(string filePath);
    string StorageType { get; }
}
