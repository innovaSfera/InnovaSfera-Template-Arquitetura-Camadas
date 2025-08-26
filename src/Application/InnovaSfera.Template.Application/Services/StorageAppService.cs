using System.Text;
using DomainDrivenDesign.Application.Interfaces;
using InnovaSfera.Template.Domain.Interfaces.Storage;
using Microsoft.Extensions.Logging;

namespace DomainDrivenDesign.Application.Services;

/// <summary>
/// Application service for storage operations
/// </summary>
public class StorageAppService : IStorageAppService
{
    private readonly IStorageContext _storageContext;
    private readonly IMessagingAppService _messagingAppService;
    private readonly ILogger<StorageAppService> _logger;

    public StorageAppService(
        IStorageContext storageContext,
        IMessagingAppService messagingAppService,
        ILogger<StorageAppService> logger)
    {
        _storageContext = storageContext;
        _messagingAppService = messagingAppService;
        _logger = logger;
    }

    public async Task<(string FileName, string StorageType)> CreateTestFileAsync(string? content = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var fileName = $"test-{DateTime.UtcNow:yyyyMMddHHmmss}.txt";
            var fileContent = Encoding.UTF8.GetBytes(content ?? "Default test content");
            
            await _storageContext.WriteFileAsync(fileName, fileContent);
            
            var storageType = _storageContext.GetStorageType();
            
            _logger.LogInformation("Test file created successfully. FileName: {FileName}, StorageType: {StorageType}", 
                fileName, storageType);
            
            return (fileName, storageType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating test file");
            throw;
        }
    }

    public async Task<(IEnumerable<string> Files, string StorageType)> GetStorageFilesAsync(string path = "", CancellationToken cancellationToken = default)
    {
        try
        {
            var files = await _storageContext.GetFilesAsync(path);
            var storageType = _storageContext.GetStorageType();
            
            _logger.LogInformation("Retrieved {Count} files from storage. Path: {Path}, StorageType: {StorageType}", 
                files.Count(), path, storageType);
            
            return (files, storageType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting storage files from path {Path}", path);
            throw;
        }
    }

    public async Task<(string Content, string StorageType)> GetFileContentAsync(string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await _storageContext.FileExistsAsync(fileName))
            {
                throw new FileNotFoundException($"File '{fileName}' not found");
            }

            var fileContent = await _storageContext.ReadFileAsync(fileName);
            var content = Encoding.UTF8.GetString(fileContent);
            var storageType = _storageContext.GetStorageType();
            
            _logger.LogInformation("File content retrieved successfully. FileName: {FileName}, StorageType: {StorageType}", 
                fileName, storageType);
            
            return (content, storageType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file {FileName}", fileName);
            throw;
        }
    }

    public async Task<string> DeleteFileAsync(string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await _storageContext.FileExistsAsync(fileName))
            {
                throw new FileNotFoundException($"File '{fileName}' not found");
            }

            var storageType = _storageContext.GetStorageType();
            
            await _storageContext.DeleteFileAsync(fileName);
            
            // Send file deleted event
            await _messagingAppService.SendFileDeletedEventAsync(fileName, storageType, cancellationToken);
            
            _logger.LogInformation("File deleted successfully. FileName: {FileName}, StorageType: {StorageType}", 
                fileName, storageType);
            
            return storageType;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileName}", fileName);
            throw;
        }
    }

    public async Task<bool> FileExistsAsync(string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await _storageContext.FileExistsAsync(fileName);
            
            _logger.LogDebug("File existence check. FileName: {FileName}, Exists: {Exists}", fileName, exists);
            
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence for {FileName}", fileName);
            throw;
        }
    }
}
