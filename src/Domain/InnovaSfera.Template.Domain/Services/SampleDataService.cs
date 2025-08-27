using System.Text.Json;
using DomainDrivenDesign.Domain.Entities;
using DomainDrivenDesign.Domain.Interfaces;
using DomainDrivenDesign.Domain.Interfaces.Services;
using InnovaSfera.Template.Domain.Entities;
using InnovaSfera.Template.Domain.Interfaces.Cache;
using InnovaSfera.Template.Domain.Interfaces.External;
using InnovaSfera.Template.Domain.Interfaces.Storage;
using Microsoft.Extensions.Logging;

namespace DomainDrivenDesign.Domain.Services;

public class SampleDataService(
        IUnitOfWork _unitOfWork,
        ILogger<SampleDataService> _logger,
        IStorageContext _storageContext,
        ICacheContext _cacheContext,
        IHarryPotterApiManager _harryPotterApiManager) : ISampleDataService
{
    public async Task AddAsync(SampleData data)
    {
        try
        {
            _unitOfWork.SampleDataRepository.Add(data);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("SampleData added successfully with ID: {Id}", data.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding SampleData");
            throw;
        }
    }

    public async Task<IEnumerable<SampleData>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Starting SampleData search");

            var result = _cacheContext.GetCacheObject<IEnumerable<SampleData>>("key");
            if (result != null && result.Any())
            {
                var sampleDataList = result;
                return sampleDataList ?? new List<SampleData>();
            }
            var resultRepo = await _unitOfWork.SampleDataRepository.GetAllAsync();

            if (resultRepo != null && resultRepo.Any())
                _cacheContext.SetCachedObject("key", resultRepo, 60*60);

            return resultRepo ?? new List<SampleData>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for SampleData");
            throw;
        }
    }

    public async Task<ICollection<Character>> GetAllWizardsAsync()
    {
        return await _harryPotterApiManager.WizardsGetAllAsync();
    }

    public async Task<IEnumerable<string>> GetFilesAsync(string path)
    {
        try
        {
            _logger.LogInformation("Starting file search in path: {Path}", path);

            var files = await _storageContext.GetFilesAsync(path);

            _logger.LogInformation("Found {Count} files", files.Count());

            return files;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for files in path: {Path}", path);
            throw;
        }
    }

    public async Task<byte[]> ReadFileAsync(string filePath)
    {
        try
        {
            _logger.LogInformation("Reading file: {FilePath}", filePath);

            return await _storageContext.ReadFileAsync(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<bool> SaveFileAsync(string filePath, byte[] content)
    {
        try
        {
            _logger.LogInformation("Saving file: {FilePath}", filePath);

            await _storageContext.WriteFileAsync(filePath, content);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file: {FilePath}", filePath);
            return false;
        }
    }
}
