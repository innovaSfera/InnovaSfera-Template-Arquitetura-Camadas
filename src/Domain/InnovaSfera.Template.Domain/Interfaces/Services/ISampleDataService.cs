using DomainDrivenDesign.Domain.Entities;
using InnovaSfera.Template.Domain.Entities;

namespace DomainDrivenDesign.Domain.Interfaces.Services;

public interface ISampleDataService
{
    Task AddAsync(SampleData data);
    Task<IEnumerable<SampleData>> GetAllAsync();
    Task<IEnumerable<string>> GetFilesAsync(string path);
    Task<byte[]> ReadFileAsync(string filePath);
    Task<bool> SaveFileAsync(string filePath, byte[] content);
    Task<ICollection<Character>> GetAllWizardsAsync();
}
