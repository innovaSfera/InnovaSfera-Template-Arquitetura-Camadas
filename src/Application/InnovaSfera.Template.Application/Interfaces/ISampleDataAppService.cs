using DomainDrivenDesign.Application.Entities;

namespace DomainDrivenDesign.Application.Interfaces;

public interface ISampleDataAppService
{
    Task<IEnumerable<SampleDataDto>> GetAll();
    Task Add(SampleDataDto sampleDataDto);
}
