using AutoMapper;
using DomainDrivenDesign.Application.Entities;
using DomainDrivenDesign.Application.Interfaces;
using DomainDrivenDesign.Domain.Entities;
using DomainDrivenDesign.Domain.Interfaces.Services;

namespace DomainDrivenDesign.Application.Services;

public class SampleDataAppService(IMapper _mapper, ISampleDataService _service) : ISampleDataAppService
{
    public async Task Add(SampleDataDto sampleDataDto)
    {
        var entity = _mapper.Map<SampleData>(sampleDataDto);
        await _service.Add(entity);
    }

    public async Task<IEnumerable<SampleDataDto>> GetAll()
    {
        var entities = await _service.GetAll();
        return _mapper.Map<IEnumerable<SampleDataDto>>(entities);
    }
}
