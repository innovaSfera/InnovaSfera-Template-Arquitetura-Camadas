using DomainDrivenDesign.Domain.Interfaces;
using DomainDrivenDesign.Domain.Interfaces.Repositories;
using InnovaSfera.Template.Infrastructure.Data.Context;
using InnovaSfera.Template.Infrastructure.Data.Repositories;
using Microsoft.Extensions.Logging;
using System.Data;

namespace DomainDrivenDesign.Infrastructure.Data.UnitOfWork;

/// <summary>
/// Context for use with Dapper - Sql connection manager
/// For use: uncomment the lines in DomainDrivenDesignModule.cs
/// </summary>
public class UnitOfWorkDapper : IUnitOfWork
{
    private readonly SampleContextDapper _context;
    private readonly ILogger<UnitOfWorkDapper> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private ISampleDataRepository? _sampleDataRepository;
    private IDbTransaction? _transaction;
    private bool _disposed = false;

    public UnitOfWorkDapper(SampleContextDapper context, ILogger<UnitOfWorkDapper> logger, ILoggerFactory loggerFactory)
    {
        _context = context;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Repository SampleData with lazy loading
    /// </summary>
    public ISampleDataRepository SampleDataRepository
    {
        get
        {
            return _sampleDataRepository ??= new SampleDataRepositoryDapper(_context, 
                _loggerFactory.CreateLogger<SampleDataRepositoryDapper>());
        }
    }

    /// <summary>
    /// Commit Operations 
    /// For Dapper, operations individual auto-commit
    /// If a transaction is active, it will also auto-commit.
    /// </summary>
    public async Task<int> CommitAsync()
    {
        return await CommitAsync(CancellationToken.None);
    }

    /// <summary>
    /// Commit operations with cancellation token
    /// </summary>
    public async Task<int> CommitAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Committing changes using Dapper");
            
             if (_transaction != null)
            {
                _transaction.Commit();
                _transaction.Dispose();
                _transaction = null;
                _logger.LogInformation("Transaction committed successfully");
            }
            else
            {
                _logger.LogInformation("No active transaction - operations are auto-committed");
            }
            
            return await Task.FromResult(1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error committing changes with Dapper");
            
             if (_transaction != null)
            {
                try
                {
                    _transaction.Rollback();
                    _logger.LogWarning("Transaction rolled back due to error");
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Error during rollback");
                }
                finally
                {
                    _transaction.Dispose();
                    _transaction = null;
                }
            }
            
            throw;
        }
    }

    /// <summary>
    /// Start transcation
    /// </summary>
    public async Task BeginTransactionAsync()
    {
        await Task.Run(() => BeginTransaction());
    }

    /// <summary>
    /// Commit current transaction
    /// </summary>
    public async Task CommitTransactionAsync()
    {
        try
        {
            if (_transaction != null)
            {
                _transaction.Commit();
                _transaction.Dispose();
                _transaction = null;
                _logger.LogInformation("Transaction committed successfully");
            }
            else
            {
                _logger.LogWarning("No active transaction to commit");
            }
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error committing transaction");
            throw;
        }
    }

    /// <summary>
    /// Rollback current trasaction
    /// </summary>
    public async Task RollbackTransactionAsync()
    {
        await Task.Run(() => RollbackTransaction());
    }

    /// <summary>
    /// Start a new transaction manually
    /// </summary>
    public void BeginTransaction()
    {
        if (_transaction == null)
        {
            _transaction = _context.Connection.BeginTransaction();
            _logger.LogInformation("Transaction started for Dapper operations");
        }
        else
        {
            _logger.LogWarning("Transaction already active");
        }
    }

    /// <summary>
    /// Rollback current trasaction
    /// </summary>
    public void RollbackTransaction()
    {
        if (_transaction != null)
        {
            try
            {
                _transaction.Rollback();
                _logger.LogInformation("Transaction rolled back manually");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual rollback");
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }
        else
        {
            _logger.LogWarning("No active transaction to rollback");
        }
    }

    /// <summary>
    /// Verify current status connection
    /// </summary>
    public bool HasActiveTransaction => _transaction != null;

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            try
            {
                 if (_transaction != null)
                {
                    _logger.LogWarning("Disposing UnitOfWork with active transaction - rolling back");
                    _transaction.Rollback();
                    _transaction.Dispose();
                }

                _context?.Dispose();
                _logger.LogInformation("UnitOfWorkDapper disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing UnitOfWorkDapper");
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}
