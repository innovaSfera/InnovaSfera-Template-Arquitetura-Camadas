using AutoMapper;
using DomainDrivenDesign.Application.Entities;
using DomainDrivenDesign.Application.Interfaces;
using DomainDrivenDesign.Domain.Entities;
using DomainDrivenDesign.Domain.Interfaces.Services;
using InnovaSfera.Template.Application.Dto.Response;
using Microsoft.Extensions.Logging;

namespace DomainDrivenDesign.Application.Services;

public class SampleDataAppService(IMapper _mapper, ISampleDataService _service, ILogger<SampleDataAppService> _logger) : ISampleDataAppService
{
    public async Task AddAsync(SampleDataDto sampleDataDto)
    {
        _logger.Log(LogLevel.Information, "AddAsync called in SampleDataAppService");
        var entity = _mapper.Map<SampleData>(sampleDataDto);
        await _service.AddAsync(entity);
    }

    public async Task<IEnumerable<SampleDataDto>> GetAllAsync()
    {
        var entities = await _service.GetAllAsync();
        return _mapper.Map<IEnumerable<SampleDataDto>>(entities);
    }

    public async Task<ICollection<CharacterDtoResponse>> GetAllWizardsAsync()
    {
        var entities = await _service.GetAllWizardsAsync();
        return _mapper.Map<ICollection<CharacterDtoResponse>>(entities);
    }

    public async Task<IEnumerable<string>> GetFilesAsync()
    {
        return await _service.GetFilesAsync(string.Empty);
    }
}
