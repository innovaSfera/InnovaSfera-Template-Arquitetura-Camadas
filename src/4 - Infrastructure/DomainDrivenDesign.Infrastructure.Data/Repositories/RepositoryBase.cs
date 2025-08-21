using DomainDrivenDesign.Infrastructure.Core.Entity;
using DomainDrivenDesign.Infrastructure.Core.Interfaces;
using DomainDrivenDesign.Infrastructure.Data.Context;

namespace DomainDrivenDesign.Infrastructure.Data.Repositories;

public class RepositoryBase<T> : IRepository<T> where T : IEntity
{
    protected readonly SampleContext _sampleContext;

    public RepositoryBase(SampleContext sampleContext)
    {
        _sampleContext = sampleContext;
    }

    public async Task SaveChangesAsync() => await _sampleContext.SaveChangesAsync();

    public void Add(T entity) => _sampleContext.Add(entity);
    public void Update(T entity) => _sampleContext.Update(entity);

    public void Dispose()
    {
        _sampleContext.Dispose();
        GC.SuppressFinalize(this);
    }
}