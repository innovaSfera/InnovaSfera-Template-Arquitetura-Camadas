namespace DomainDrivenDesign.Application.Interfaces;

/// <summary>
/// Application service interface for storage operations
/// </summary>
public interface IStorageAppService
{
    /// <summary>
    /// Create a test file with the specified content
    /// </summary>
    Task<(string FileName, string StorageType)> CreateTestFileAsync(string? content = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all files from storage
    /// </summary>
    Task<(IEnumerable<string> Files, string StorageType)> GetStorageFilesAsync(string path = "", CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get file content by filename
    /// </summary>
    Task<(string Content, string StorageType)> GetFileContentAsync(string fileName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete a file from storage
    /// </summary>
    Task<string> DeleteFileAsync(string fileName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if a file exists in storage
    /// </summary>
    Task<bool> FileExistsAsync(string fileName, CancellationToken cancellationToken = default);
}
