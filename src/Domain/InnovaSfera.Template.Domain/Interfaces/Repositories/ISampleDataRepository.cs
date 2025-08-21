using DomainDrivenDesign.Domain.Entities;
using DomainDrivenDesign.Infrastructure.Core.Interfaces;

namespace DomainDrivenDesign.Domain.Interfaces.Repositories;

public interface ISampleDataRepository : IRepository<SampleData>
{
    Task<IEnumerable<SampleData>> GetAllAsync();
}
