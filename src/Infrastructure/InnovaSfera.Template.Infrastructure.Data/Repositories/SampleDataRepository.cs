using DomainDrivenDesign.Domain.Entities;
using DomainDrivenDesign.Domain.Interfaces.Repositories;
using DomainDrivenDesign.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DomainDrivenDesign.Infrastructure.Data.Repositories;

public class SampleDataRepository : RepositoryBase<SampleData>, ISampleDataRepository
{
    public SampleContext _context { get; set; }
    public SampleDataRepository(SampleContext context)
        : base(context)
    {
        _context = context;
    }
    public async Task<IEnumerable<SampleData>> GetAllAsync()
    {
        return await _context.SampleDatas.ToListAsync();
    }
}
