using DomainDrivenDesign.Infrastructure.Core.Entity;

namespace DomainDrivenDesign.Infrastructure.Core.Interfaces;

public interface IRepository<in T> : IDisposable where T : class, IEntity
{
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}