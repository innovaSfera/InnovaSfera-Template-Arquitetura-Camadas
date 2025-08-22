using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using InnovaSfera.Template.Domain.Interfaces.Storage;

namespace InnovaSfera.Template.Infrastructure.Data.Storage;

public class AzureBlobStorageStrategy : IStorageStrategy
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly BlobContainerClient _containerClient;

    public AzureBlobStorageStrategy(BlobServiceClient blobServiceClient, string containerName)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = containerName;
        _containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        
        // Ensure container exists
        _containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
    }

    public string StorageType => "AzureBlob";

    public async Task<IEnumerable<string>> GetFilesAsync(string path)
    {
        var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";
        var blobs = new List<string>();

        await foreach (var blobItem in _containerClient.GetBlobsAsync(prefix: prefix))
        {
            blobs.Add(blobItem.Name);
        }

        return blobs;
    }

    public async Task<byte[]> ReadFileAsync(string filePath)
    {
        var blobClient = _containerClient.GetBlobClient(filePath);
        
        if (!await blobClient.ExistsAsync())
            throw new FileNotFoundException($"Blob not found: {filePath}");

        var response = await blobClient.DownloadContentAsync();
        return response.Value.Content.ToArray();
    }

    public async Task WriteFileAsync(string filePath, byte[] content)
    {
        var blobClient = _containerClient.GetBlobClient(filePath);
        
        using var stream = new MemoryStream(content);
        await blobClient.UploadAsync(stream, overwrite: true);
    }

    public async Task DeleteFileAsync(string filePath)
    {
        var blobClient = _containerClient.GetBlobClient(filePath);
        await blobClient.DeleteIfExistsAsync();
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        var blobClient = _containerClient.GetBlobClient(filePath);
        var response = await blobClient.ExistsAsync();
        return response.Value;
    }
}
