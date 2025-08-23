using DomainDrivenDesign.Infrastructure.Core.Entity;

namespace DomainDrivenDesign.Infrastructure.Core.Interfaces;

public interface IQueryRepository<T> where T : class, IEntity
{
    Task<T?> GetByIdAsync(object id);
    Task<IEnumerable<T>> GetAllAsync();
}
