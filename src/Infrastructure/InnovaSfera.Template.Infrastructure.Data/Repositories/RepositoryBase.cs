using DomainDrivenDesign.Infrastructure.Core.Entity;
using DomainDrivenDesign.Infrastructure.Core.Interfaces;
using DomainDrivenDesign.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DomainDrivenDesign.Infrastructure.Data.Repositories;

public class RepositoryBase<T> : IRepository<T>, IQueryRepository<T> where T : class, IEntity
{
    protected readonly SampleContext _context;

    public RepositoryBase(SampleContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public void Add(T entity) 
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        _context.Set<T>().Add(entity);
    }

    public void Update(T entity) 
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        _context.Set<T>().Update(entity);
    }

    public void Delete(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        _context.Set<T>().Remove(entity);
    }

    public async Task<T?> GetByIdAsync(object id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public void Dispose()
    {
        // Context disposal is now handled by UnitOfWork
        GC.SuppressFinalize(this);
    }
}