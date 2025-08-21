using DomainDrivenDesign.Domain.Entities;

namespace DomainDrivenDesign.Domain.Interfaces.Services;

public interface ISampleDataService
{
    Task Add(SampleData data);
    Task<IEnumerable<SampleData>> GetAll();
}
