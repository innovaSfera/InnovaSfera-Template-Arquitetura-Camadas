using DomainDrivenDesign.Domain.Entities;
using DomainDrivenDesign.Domain.Interfaces.Repositories;
using DomainDrivenDesign.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DomainDrivenDesign.Infrastructure.Data.Repositories;

public class SampleDataRepository : RepositoryBase<SampleData>, ISampleDataRepository
{
    public SampleDataRepository(SampleContext context) : base(context)
    {
    }

    // Implementação específica se necessário, caso contrário usa a do RepositoryBase
    public new async Task<IEnumerable<SampleData>> GetAllAsync()
    {
        return await _context.SampleDatas.ToListAsync();
    }
}
