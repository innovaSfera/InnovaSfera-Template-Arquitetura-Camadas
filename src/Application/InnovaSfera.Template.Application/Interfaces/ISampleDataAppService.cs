using DomainDrivenDesign.Application.Entities;
using InnovaSfera.Template.Application.Dto.Response;
using InnovaSfera.Template.Domain.Entities.Messaging;

namespace DomainDrivenDesign.Application.Interfaces;

/// <summary>
/// Application service interface for Sample Data operations
/// </summary>
public interface ISampleDataAppService
{
    /// <summary>
    /// Get all sample data
    /// </summary>
    Task<IEnumerable<SampleDataDto>> GetAllAsync();
    
    /// <summary>
    /// Add sample data with automatic event publishing
    /// </summary>
    Task<(SampleDataDto SampleData, MessageResult EventResult)> AddAsync(SampleDataDto sampleDataDto);
    
    /// <summary>
    /// Get files with automatic event publishing
    /// </summary>
    Task<(IEnumerable<string> Files, MessageResult? EventResult)> GetFilesAsync();
    
    /// <summary>
    /// Get all wizards
    /// </summary>
    Task<ICollection<CharacterDtoResponse>> GetAllWizardsAsync();
}
