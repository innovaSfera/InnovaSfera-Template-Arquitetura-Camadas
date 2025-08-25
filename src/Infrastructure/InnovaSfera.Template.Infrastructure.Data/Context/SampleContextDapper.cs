using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace InnovaSfera.Template.Infrastructure.Data.Context;

/// <summary>
/// Context for use with Dapper - Sql connection manager
/// For use: uncomment the lines in DomainDrivenDesignModule.cs
/// </summary>
public class SampleContextDapper : IDisposable
{
    private readonly string _connectionString;
    private IDbConnection? _connection;

    public SampleContextDapper(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
    }

    /// <summary>
    /// Open connection with database
    /// </summary>
    public IDbConnection Connection
    {
        get
        {
            if (_connection == null)
            {
                _connection = new SqlConnection(_connectionString);
                _connection.Open();
            }
            return _connection;
        }
    }

    /// <summary>
    /// Dispose connection
    /// </summary>
    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }
}
