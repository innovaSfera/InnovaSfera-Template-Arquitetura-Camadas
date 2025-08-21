using DomainDrivenDesign.Infrastructure.Core.Entity;

namespace DomainDrivenDesign.Infrastructure.Core.Interfaces;

public interface IRepository<in T> : IDisposable where T : IEntity
{
    Task SaveChangesAsync();
    void Add(T entity);
    void Update(T entity);
}