using DomainDrivenDesign.Domain.Interfaces.Repositories;

namespace DomainDrivenDesign.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the SampleData repository instance
    /// </summary>
    ISampleDataRepository SampleDataRepository { get; }
    
    /// <summary>
    /// Commits all changes made in this unit of work to the database
    /// </summary>
    /// <returns>The number of objects written to the database</returns>
    Task<int> CommitAsync();
    
    /// <summary>
    /// Commits all changes made in this unit of work to the database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The number of objects written to the database</returns>
    Task<int> CommitAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Begins a database transaction
    /// </summary>
    Task BeginTransactionAsync();
    
    /// <summary>
    /// Commits the current transaction
    /// </summary>
    Task CommitTransactionAsync();
    
    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    Task RollbackTransactionAsync();
}
