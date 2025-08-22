using DomainDrivenDesign.Application.Entities;
using InnovaSfera.Template.Application.Dto.Response;

namespace DomainDrivenDesign.Application.Interfaces;

public interface ISampleDataAppService
{
    Task<IEnumerable<SampleDataDto>> GetAllAsync();
    Task AddAsync(SampleDataDto sampleDataDto);
    Task<IEnumerable<string>> GetFilesAsync();
    Task<ICollection<CharacterDtoResponse>> GetAllWizardsAsync();
}
