using AutoMapper;
using DomainDrivenDesign.Application.Entities;
using DomainDrivenDesign.Application.Interfaces;
using DomainDrivenDesign.Domain.Entities;
using DomainDrivenDesign.Domain.Interfaces.Services;
using InnovaSfera.Template.Application.Dto.Response;
using InnovaSfera.Template.Domain.Entities.Messaging;
using Microsoft.Extensions.Logging;

namespace DomainDrivenDesign.Application.Services;

/// <summary>
/// Application service for Sample Data operations with integrated messaging
/// </summary>
public class SampleDataAppService : ISampleDataAppService
{
    private readonly IMapper _mapper;
    private readonly ISampleDataService _service;
    private readonly IMessagingAppService _messagingAppService;
    private readonly ILogger<SampleDataAppService> _logger;

    public SampleDataAppService(
        IMapper mapper,
        ISampleDataService service,
        IMessagingAppService messagingAppService,
        ILogger<SampleDataAppService> logger)
    {
        _mapper = mapper;
        _service = service;
        _messagingAppService = messagingAppService;
        _logger = logger;
    }

    public async Task<(SampleDataDto SampleData, MessageResult EventResult)> AddAsync(SampleDataDto sampleDataDto)
    {
        try
        {
            _logger.LogInformation("AddAsync called in SampleDataAppService for SampleId: {SampleId}", sampleDataDto.Id);
            
            // Add the sample data
            var entity = _mapper.Map<SampleData>(sampleDataDto);
            await _service.AddAsync(entity);

            // Send creation event via messaging
            var eventResult = await _messagingAppService.SendSampleCreationEventAsync(sampleDataDto);

            if (eventResult.IsSuccess)
            {
                _logger.LogInformation("Sample data {Id} created and event sent successfully via {Provider}", 
                    sampleDataDto.Id, eventResult.Provider);
            }
            else
            {
                _logger.LogWarning("Sample data {Id} created but event failed: {Error}", 
                    sampleDataDto.Id, eventResult.ErrorMessage);
            }

            return (sampleDataDto, eventResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AddAsync for SampleId: {SampleId}", sampleDataDto.Id);
            throw;
        }
    }

    public async Task<IEnumerable<SampleDataDto>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("GetAllAsync called in SampleDataAppService");
            var entities = await _service.GetAllAsync();
            return _mapper.Map<IEnumerable<SampleDataDto>>(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllAsync");
            throw;
        }
    }

    public async Task<ICollection<CharacterDtoResponse>> GetAllWizardsAsync()
    {
        try
        {
            _logger.LogInformation("GetAllWizardsAsync called in SampleDataAppService");
            var entities = await _service.GetAllWizardsAsync();
            return _mapper.Map<ICollection<CharacterDtoResponse>>(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllWizardsAsync");
            throw;
        }
    }

    public async Task<(IEnumerable<string> Files, MessageResult? EventResult)> GetFilesAsync()
    {
        try
        {
            _logger.LogInformation("GetFilesAsync called in SampleDataAppService");
            var files = await _service.GetFilesAsync(string.Empty);
            var filesList = files.ToList();

            MessageResult? eventResult = null;

            if (!filesList.Any())
            {
                // Send event when there are no files
                eventResult = await _messagingAppService.SendEmptyFilesEventAsync();
            }
            else
            {
                // Send event with the number of files found
                eventResult = await _messagingAppService.SendFilesRetrievedEventAsync(filesList);
            }

            if (eventResult.IsSuccess)
            {
                _logger.LogInformation("Files retrieved and event sent successfully. Count: {Count}", filesList.Count);
            }
            else
            {
                _logger.LogWarning("Files retrieved but event failed: {Error}", eventResult.ErrorMessage);
            }

            return (filesList, eventResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetFilesAsync");
            throw;
        }
    }
}
