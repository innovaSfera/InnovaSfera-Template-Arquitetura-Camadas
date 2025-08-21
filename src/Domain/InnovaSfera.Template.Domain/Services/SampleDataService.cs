using DomainDrivenDesign.Domain.Entities;
using DomainDrivenDesign.Domain.Interfaces.Repositories;
using DomainDrivenDesign.Domain.Interfaces.Services;

namespace DomainDrivenDesign.Domain.Services;

public class SampleDataService(ISampleDataRepository _repository) : ISampleDataService
{
    public async Task Add(SampleData data)
    {
        _repository.Add(data);
        await _repository.SaveChangesAsync();
    }

    public async Task<IEnumerable<SampleData>> GetAll()
    {
        return await _repository.GetAllAsync();
    }
}
