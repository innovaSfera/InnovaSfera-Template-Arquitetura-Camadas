using DomainDrivenDesign.Domain.Interfaces;
using DomainDrivenDesign.Domain.Interfaces.Repositories;
using DomainDrivenDesign.Infrastructure.Data.Context;
using DomainDrivenDesign.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DomainDrivenDesign.Infrastructure.Data.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly SampleContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed = false;
    
    // Repository instances
    private ISampleDataRepository? _sampleDataRepository;

    public UnitOfWork(SampleContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets the SampleData repository instance (lazy loading)
    /// </summary>
    public ISampleDataRepository SampleDataRepository
    {
        get
        {
            _sampleDataRepository ??= new SampleDataRepository(_context);
            return _sampleDataRepository;
        }
    }

    /// <summary>
    /// Commits all changes made in this unit of work to the database
    /// </summary>
    /// <returns>The number of objects written to the database</returns>
    public async Task<int> CommitAsync()
    {
        return await CommitAsync(CancellationToken.None);
    }

    /// <summary>
    /// Commits all changes made in this unit of work to the database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The number of objects written to the database</returns>
    public async Task<int> CommitAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            // Log the exception if needed
            await RollbackTransactionAsync();
            throw;
        }
    }

    /// <summary>
    /// Begins a database transaction
    /// </summary>
    public async Task BeginTransactionAsync()
    {
        if (_transaction is not null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _transaction = await _context.Database.BeginTransactionAsync();
    }

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    public async Task CommitTransactionAsync()
    {
        if (_transaction is null)
        {
            throw new InvalidOperationException("No transaction is in progress.");
        }

        try
        {
            await _transaction.CommitAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    public async Task RollbackTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// Disposes the unit of work and its resources
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _context?.Dispose();
            _disposed = true;
        }
    }
}
