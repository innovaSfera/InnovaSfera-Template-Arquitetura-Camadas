using Dapper;
using DomainDrivenDesign.Domain.Entities;
using DomainDrivenDesign.Domain.Interfaces.Repositories;
using InnovaSfera.Template.Infrastructure.Data.Context;
using Microsoft.Extensions.Logging;

namespace InnovaSfera.Template.Infrastructure.Data.Repositories;

/// <summary>
/// Repository with Dapper (Micro-ORM)
/// To use: uncomment the lines in DomainDrivenDesignModule.cs
/// </summary>
public class SampleDataRepositoryDapper : ISampleDataRepository
{
    private readonly SampleContextDapper _context;
    private readonly ILogger<SampleDataRepositoryDapper> _logger;

    public SampleDataRepositoryDapper(SampleContextDapper context, ILogger<SampleDataRepositoryDapper> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Implementation of IDisposable
    /// </summary>
    public void Dispose()
    {
        _context?.Dispose();
    }

    /// <summary>
    /// Search all records using optimized SQL
    /// </summary>
    public async Task<IEnumerable<SampleData>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Getting all SampleData using Dapper");
            
            const string sql = @"
                SELECT Id, TimeStamp, Message 
                FROM SampleData 
                ORDER BY TimeStamp DESC";
            
            return await _context.Connection.QueryAsync<SampleData>(sql);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all SampleData with Dapper");
            throw;
        }
    }

    /// <summary>
    /// Search records with ID
    /// </summary>
    public async Task<SampleData?> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting SampleData by ID: {Id} using Dapper", id);
            
            const string sql = @"
                SELECT Id, TimeStamp, Message 
                FROM SampleData 
                WHERE Id = @Id";
            
            return await _context.Connection.QueryFirstOrDefaultAsync<SampleData>(sql, new { Id = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SampleData by ID with Dapper");
            throw;
        }
    }

    /// <summary>
    /// Add new records
    /// </summary>
    public void Add(SampleData entity)
    {
        try
        {
            _logger.LogInformation("Adding SampleData using Dapper");
            
            entity.Id = Guid.NewGuid();
            entity.TimeStamp = DateTime.UtcNow;
            
            const string sql = @"
                INSERT INTO SampleData (Id, TimeStamp, Message) 
                VALUES (@Id, @TimeStamp, @Message)";
            
            _context.Connection.Execute(sql, entity);
            
            _logger.LogInformation("SampleData added successfully with ID: {Id}", entity.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding SampleData with Dapper");
            throw;
        }
    }

    /// <summary>
    /// Add new record assync
    /// </summary>
    public async Task<SampleData> AddAsync(SampleData entity)
    {
        try
        {
            _logger.LogInformation("Adding SampleData async using Dapper");
            
            entity.Id = Guid.NewGuid();
            entity.TimeStamp = DateTime.UtcNow;
            
            const string sql = @"
                INSERT INTO SampleData (Id, TimeStamp, Message) 
                VALUES (@Id, @TimeStamp, @Message)";
            
            await _context.Connection.ExecuteAsync(sql, entity);
            
            _logger.LogInformation("SampleData added successfully with ID: {Id}", entity.Id);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding SampleData async with Dapper");
            throw;
        }
    }

    /// <summary>
    /// Update records if existing
    /// </summary>
    public void Update(SampleData entity)
    {
        try
        {
            _logger.LogInformation("Updating SampleData ID: {Id} using Dapper", entity.Id);
            
            const string sql = @"
                UPDATE SampleData 
                SET TimeStamp = @TimeStamp, Message = @Message 
                WHERE Id = @Id";
            
            var rowsAffected = _context.Connection.Execute(sql, entity);
            
            if (rowsAffected == 0)
            {
                _logger.LogWarning("No rows affected when updating SampleData ID: {Id}", entity.Id);
            }
            else
            {
                _logger.LogInformation("SampleData updated successfully with ID: {Id}", entity.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SampleData with Dapper");
            throw;
        }
    }

    /// <summary>
    /// Delete records
    /// </summary>
    public void Delete(SampleData entity)
    {
        try
        {
            _logger.LogInformation("Deleting SampleData ID: {Id} using Dapper", entity.Id);
            
            const string sql = "DELETE FROM SampleData WHERE Id = @Id";
            var rowsAffected = _context.Connection.Execute(sql, new { entity.Id });
            
            if (rowsAffected == 0)
            {
                _logger.LogWarning("No rows affected when deleting SampleData ID: {Id}", entity.Id);
            }
            else
            {
                _logger.LogInformation("SampleData deleted successfully with ID: {Id}", entity.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting SampleData with Dapper");
            throw;
        }
    }

    /// <summary>
    /// Delete records async
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting SampleData ID: {Id} async using Dapper", id);
            
            const string sql = "DELETE FROM SampleData WHERE Id = @Id";
            var rowsAffected = await _context.Connection.ExecuteAsync(sql, new { Id = id });
            
            var success = rowsAffected > 0;
            
            if (success)
            {
                _logger.LogInformation("SampleData deleted successfully with ID: {Id}", id);
            }
            else
            {
                _logger.LogWarning("No rows affected when deleting SampleData ID: {Id}", id);
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting SampleData async with Dapper");
            throw;
        }
    }

    /// <summary>
    /// Get records using like
    /// </summary>
    public async Task<IEnumerable<SampleData>> GetByMessageContainsAsync(string searchTerm)
    {
        _logger.LogInformation("Getting SampleData by message containing: {SearchTerm} using Dapper", searchTerm);
        
        const string sql = @"
            SELECT Id, TimeStamp, Message 
            FROM SampleData 
            WHERE Message LIKE @SearchTerm
            ORDER BY TimeStamp DESC";
        
        return await _context.Connection.QueryAsync<SampleData>(sql, new { SearchTerm = $"%{searchTerm}%" });
    }

    /// <summary>
    /// Get records with date filter
    /// </summary>
    public async Task<IEnumerable<SampleData>> GetCreatedAfterAsync(DateTime date)
    {
        _logger.LogInformation("Getting SampleData created after: {Date} using Dapper", date);
        
        const string sql = @"
            SELECT Id, TimeStamp, Message 
            FROM SampleData 
            WHERE TimeStamp > @Date 
            ORDER BY TimeStamp DESC";
        
        return await _context.Connection.QueryAsync<SampleData>(sql, new { Date = date });
    }

    /// <summary>
    /// Count records
    /// </summary>
    public async Task<int> GetCountAsync()
    {
        _logger.LogInformation("Getting SampleData count using Dapper");
        
        const string sql = "SELECT COUNT(*) FROM SampleData";
        return await _context.Connection.QuerySingleAsync<int>(sql);
    }

    /// <summary>
    /// Get last records inserted in the database
    /// </summary>
    public async Task<IEnumerable<SampleData>> GetLatestAsync(int count = 10)
    {
        _logger.LogInformation("Getting latest {Count} SampleData using Dapper", count);
        
        const string sql = @"
            SELECT TOP (@Count) Id, TimeStamp, Message 
            FROM SampleData 
            ORDER BY TimeStamp DESC";
        
        return await _context.Connection.QueryAsync<SampleData>(sql, new { Count = count });
    }

    /// <summary>
    /// BulkUpdate
    /// </summary>
    public async Task<int> BulkUpdateMessagesAsync(string oldMessage, string newMessage)
    {
        _logger.LogInformation("Bulk updating messages from '{OldMessage}' to '{NewMessage}' using Dapper", 
            oldMessage, newMessage);
        
        const string sql = @"
            UPDATE SampleData 
            SET Message = @NewMessage 
            WHERE Message = @OldMessage";
        
        var rowsAffected = await _context.Connection.ExecuteAsync(sql, new { OldMessage = oldMessage, NewMessage = newMessage });
        
        _logger.LogInformation("Bulk update completed. Rows affected: {RowsAffected}", rowsAffected);
        return rowsAffected;
    }
}
