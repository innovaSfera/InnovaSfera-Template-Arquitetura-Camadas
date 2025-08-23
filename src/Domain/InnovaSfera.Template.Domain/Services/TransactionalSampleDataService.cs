using DomainDrivenDesign.Domain.Entities;
using DomainDrivenDesign.Domain.Interfaces;
using DomainDrivenDesign.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace DomainDrivenDesign.Domain.Services;

/// <summary>
/// Exemplo de serviço que demonstra como usar Unit of Work com transações
/// </summary>
public class TransactionalSampleDataService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionalSampleDataService> _logger;

    public TransactionalSampleDataService(
        IUnitOfWork unitOfWork,
        ILogger<TransactionalSampleDataService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Exemplo de operação simples sem transação explícita
    /// </summary>
    public async Task<int> AddSampleDataAsync(SampleData sampleData)
    {
        try
        {
            _unitOfWork.SampleDataRepository.Add(sampleData);
            return await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding sample data");
            throw;
        }
    }

    /// <summary>
    /// Exemplo de operação complexa com transação explícita
    /// </summary>
    public async Task<int> AddMultipleSampleDataAsync(IEnumerable<SampleData> sampleDataList)
    {
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            foreach (var sampleData in sampleDataList)
            {
                _unitOfWork.SampleDataRepository.Add(sampleData);
            }

            var result = await _unitOfWork.CommitAsync();
            await _unitOfWork.CommitTransactionAsync();
            
            _logger.LogInformation("Successfully added {Count} sample data items", sampleDataList.Count());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding multiple sample data items");
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    /// <summary>
    /// Exemplo de operação que combina múltiplas operações em uma transação
    /// </summary>
    public async Task<int> UpdateAndAddSampleDataAsync(SampleData existingData, SampleData newData)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Atualizar dados existentes
            _unitOfWork.SampleDataRepository.Update(existingData);
            
            // Adicionar novos dados
            _unitOfWork.SampleDataRepository.Add(newData);

            var result = await _unitOfWork.CommitAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Successfully updated and added sample data");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating and adding sample data");
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
