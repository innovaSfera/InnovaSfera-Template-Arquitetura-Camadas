using Azure.Storage.Blobs;
using InnovaSfera.Template.Domain.Interfaces.Storage;
using Microsoft.Extensions.Configuration;

namespace InnovaSfera.Template.Infrastructure.Data.Storage;

public class StorageStrategyFactory(IConfiguration _configuration) : IStorageStrategyFactory
{

    public IStorageStrategy CreateStrategy(string storageType)
    {
        return storageType.ToLowerInvariant() switch
        {
            "local" => new LocalStorageStrategy(),
            "azureblob" => CreateAzureBlobStrategy(),
            _ => throw new ArgumentException($"Storage type '{storageType}' não é suportado.")
        };
    }

    private AzureBlobStorageStrategy CreateAzureBlobStrategy()
    {
        var connectionString = _configuration.GetConnectionString("AzureBlobStorage");
        var containerName = _configuration["Storage:ContainerName"] ?? "files";

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Connection string do Azure Blob Storage não configurada.");

        var blobServiceClient = new BlobServiceClient(connectionString);
        return new AzureBlobStorageStrategy(blobServiceClient, containerName);
    }
}
