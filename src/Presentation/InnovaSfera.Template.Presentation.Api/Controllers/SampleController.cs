using DomainDrivenDesign.Application.Entities;
using DomainDrivenDesign.Application.Interfaces;
using InnovaSfera.Template.Application.Dto.Response;
using Microsoft.AspNetCore.Mvc;

namespace DomainDriveDesign.Presentation.Api.Controllers;

/// <summary>
/// Sample Controller - follows DDD architecture using only Application Services
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(string), StatusCodes.Status204NoContent)]
[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
public class SampleController : ControllerBase
{
    private readonly ISampleDataAppService _sampleDataAppService;
    private readonly IStorageAppService _storageAppService;
    private readonly IMessagingAppService _messagingAppService;
    private readonly ILogger<SampleController> _logger;

    public SampleController(
        ISampleDataAppService sampleDataAppService,
        IStorageAppService storageAppService,
        IMessagingAppService messagingAppService,
        ILogger<SampleController> logger)
    {
        _sampleDataAppService = sampleDataAppService;
        _storageAppService = storageAppService;
        _messagingAppService = messagingAppService;
        _logger = logger;
    }

    #region Sample Data Operations

    [HttpGet(Name = "GetSampleData")]
    public async Task<ActionResult<IEnumerable<SampleDataDto>>> Get()
    {
        try
        {
            var result = await _sampleDataAppService.GetAllAsync();

            if (result == null || !result.Any())
                return NoContent();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sample data");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpPost(Name = "PostSampleData")]
    public async Task<ActionResult> Post([FromBody] SampleDataDto sampleData)
    {
        try
        {
            // Add the sample data with automatic event sending
            var (addedSampleData, eventResult) = await _sampleDataAppService.AddAsync(sampleData);

            return Ok(new { 
                Message = "Sample data created successfully",
                SampleId = addedSampleData.Id,
                EventSent = eventResult.IsSuccess,
                MessagingProvider = eventResult.Provider,
                EventError = eventResult.IsSuccess ? null : eventResult.ErrorMessage
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sample data");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpGet("files", Name = "GetFiles")]
    public async Task<ActionResult<IEnumerable<string>>> GetFiles()
    {
        try
        {
            var (files, eventResult) = await _sampleDataAppService.GetFilesAsync();

            if (!files.Any())
                return NoContent();

            return Ok(new
            {
                Files = files,
                Count = files.Count(),
                EventSent = eventResult?.IsSuccess ?? false,
                EventError = eventResult?.IsSuccess == false ? eventResult.ErrorMessage : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving files");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpGet("wizards", Name = "GetWizards")]
    public async Task<ActionResult<ICollection<CharacterDtoResponse>>> GetWizards()
    {
        try
        {
            var wizards = await _sampleDataAppService.GetAllWizardsAsync();

            if (wizards == null || !wizards.Any())
                return NoContent();

            return Ok(wizards);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving wizards");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    #endregion

    #region Storage Operations

    [HttpPost("storage/test-file")]
    public async Task<ActionResult<object>> CreateTestFile([FromBody] string? content)
    {
        try
        {
            var (fileName, storageType) = await _storageAppService.CreateTestFileAsync(content);
            
            return Ok(new { 
                Message = "File created successfully", 
                FileName = fileName,
                StorageType = storageType
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating test file");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpGet("storage/files")]
    public async Task<ActionResult<object>> GetStorageFiles([FromQuery] string path = "")
    {
        try
        {
            var (files, storageType) = await _storageAppService.GetStorageFilesAsync(path);
            
            return Ok(new { 
                Files = files,
                StorageType = storageType,
                Path = path,
                Count = files.Count()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting storage files");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpGet("storage/file/{fileName}")]
    public async Task<ActionResult<object>> GetFileContent(string fileName)
    {
        try
        {
            if (!await _storageAppService.FileExistsAsync(fileName))
                return NotFound($"File '{fileName}' not found");

            var (content, storageType) = await _storageAppService.GetFileContentAsync(fileName);
            
            return Ok(new { 
                FileName = fileName,
                Content = content,
                StorageType = storageType
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file {FileName}", fileName);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpDelete("storage/file/{fileName}")]
    public async Task<ActionResult> DeleteFile(string fileName)
    {
        try
        {
            if (!await _storageAppService.FileExistsAsync(fileName))
                return NotFound($"File '{fileName}' not found");

            var storageType = await _storageAppService.DeleteFileAsync(fileName);
            
            return Ok(new { 
                Message = $"File '{fileName}' deleted successfully",
                StorageType = storageType
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileName}", fileName);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    #endregion

    #region Messaging Operations

    /// <summary>
    /// Send custom message via messaging system
    /// </summary>
    [HttpPost("messaging/send")]
    public async Task<ActionResult> SendCustomMessage([FromBody] SendMessageRequest request)
    {
        try
        {
            var result = await _messagingAppService.SendCustomMessageAsync(
                request.Topic,
                request.Payload,
                request.CorrelationId,
                request.Headers);

            if (result.IsSuccess)
            {
                return Ok(new { 
                    Success = true, 
                    MessageId = result.MessageId,
                    Provider = result.Provider,
                    Message = "Message sent successfully" 
                });
            }

            return BadRequest(new { 
                Success = false, 
                Error = result.ErrorMessage,
                Provider = result.Provider 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendCustomMessage endpoint");
            return StatusCode(500, new { Success = false, Error = "Internal server error" });
        }
    }

    /// <summary>
    /// Send multiple Sample-related messages in batch
    /// </summary>
    [HttpPost("messaging/batch")]
    public async Task<ActionResult> SendBatchSampleMessages([FromBody] IEnumerable<SampleDataDto> sampleDataList)
    {
        try
        {
            var results = await _messagingAppService.SendBatchSampleMessagesAsync(sampleDataList);
            var resultList = results.ToList();

            var successCount = resultList.Count(r => r.IsSuccess);
            var failureCount = resultList.Count(r => !r.IsSuccess);

            return Ok(new 
            { 
                Success = failureCount == 0,
                TotalMessages = resultList.Count,
                SuccessCount = successCount,
                FailureCount = failureCount,
                Provider = resultList.FirstOrDefault()?.Provider,
                SampleDataProcessed = sampleDataList.Count(),
                Results = resultList.Select(r => new {
                    MessageId = r.MessageId,
                    Success = r.IsSuccess,
                    Error = r.ErrorMessage
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendBatchSampleMessages endpoint");
            return StatusCode(500, new { Success = false, Error = "Internal server error" });
        }
    }

    /// <summary>
    /// Check messaging system health
    /// </summary>
    [HttpGet("messaging/health")]
    public async Task<ActionResult> CheckMessagingHealth()
    {
        try
        {
            var (isHealthy, provider, timestamp) = await _messagingAppService.CheckHealthAsync();
            
            return Ok(new 
            { 
                Success = isHealthy,
                Provider = provider,
                Status = isHealthy ? "Healthy" : "Unhealthy",
                Timestamp = timestamp,
                Service = "SampleController Messaging Integration"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CheckMessagingHealth endpoint");
            return Ok(new 
            { 
                Success = false,
                Provider = "Unknown",
                Status = "Error",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Simulate failure and send to DLQ (Dead Letter Queue) with Sample data
    /// </summary>
    [HttpPost("messaging/simulate-dlq")]
    public async Task<ActionResult> SimulateDlqWithSample([FromBody] SampleDataDto sampleData)
    {
        try
        {
            var result = await _messagingAppService.SimulateDlqWithSampleAsync(sampleData);

            if (result.IsSuccess)
            {
                return Ok(new { 
                    Success = true, 
                    MessageId = result.MessageId,
                    Provider = result.Provider,
                    Message = "Sample data sent to DLQ successfully",
                    SampleId = sampleData.Id
                });
            }

            return BadRequest(new { 
                Success = false, 
                Error = result.ErrorMessage,
                Provider = result.Provider 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SimulateDlqWithSample endpoint");
            return StatusCode(500, new { Success = false, Error = "Internal server error" });
        }
    }

    /// <summary>
    /// Send Sample event with automatic retry
    /// </summary>
    [HttpPost("messaging/send-with-retry")]
    public async Task<ActionResult> SendSampleEventWithRetry([FromBody] SampleDataDto sampleData)
    {
        try
        {
            var result = await _messagingAppService.SendSampleEventWithRetryAsync(sampleData);

            return Ok(new { 
                Success = result.IsSuccess,
                MessageId = result.MessageId,
                Provider = result.Provider,
                Message = result.IsSuccess ? "Sample event sent with retry successfully" : "Sample event failed after retries",
                Error = result.ErrorMessage,
                SampleId = sampleData.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendSampleEventWithRetry endpoint");
            return StatusCode(500, new { Success = false, Error = "Internal server error" });
        }
    }

    #endregion
}

public class SendMessageRequest
{
    public string Topic { get; set; } = string.Empty;
    public object Payload { get; set; } = new();
    public string? CorrelationId { get; set; }
    public Dictionary<string, object>? Headers { get; set; }
}
