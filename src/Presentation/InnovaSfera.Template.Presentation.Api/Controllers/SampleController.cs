using DomainDrivenDesign.Application.Entities;
using DomainDrivenDesign.Application.Interfaces;
using FluentValidation;
using InnovaSfera.Template.Application.Dto.Request;
using InnovaSfera.Template.Application.Dto.Response;
using InnovaSfera.Template.Application.Entities.Base.Handler;
using InnovaSfera.Template.Presentation.Api.Controllers;
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
public class SampleController : MainController
{
    private readonly ISampleDataAppService _sampleDataAppService;
    private readonly IStorageAppService _storageAppService;
    private readonly IMessagingAppService _messagingAppService;
    private readonly ILogger<SampleController> _logger;
    private readonly IValidator<SampleDataDto> _validator;

    public SampleController(
        ISampleDataAppService sampleDataAppService,
        IStorageAppService storageAppService,
        IMessagingAppService messagingAppService,
        ILogger<SampleController> logger,
        IValidator<SampleDataDto> validator)
    {
        _sampleDataAppService = sampleDataAppService;
        _storageAppService = storageAppService;
        _messagingAppService = messagingAppService;
        _logger = logger;
        _validator = validator;
    }

    #region Sample Data Operations

    /// <summary>
    /// Get all sample data
    /// </summary>
    /// <returns></returns>
    [HttpGet("GetSampleData")]
    public async Task<ActionResult<IEnumerable<SampleDataDto>>> GetAsync()
    {
        if (!ModelState.IsValid)
            return StatusCode(StatusCodes.Status400BadRequest, ModelState);

        var result = await _sampleDataAppService.GetAllAsync();

        if (result == null || !result.Any())
            return NoContent();

        return Ok(result);

    }

    /// <summary>
    /// Add sample data with automatic event publishing and validation Fluent Validation
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("PostSampleData")]
    public async Task<ActionResult<SendMessageDtoResponse>> PostAsync([FromBody] SampleDataDto request)
    {
        var resultValitor = await _validator.ValidateAsync(request);

        if (resultValitor != null && !resultValitor.IsValid)
        {
            List<ErrorHandlerResponse> errors = new();

            foreach (var item in resultValitor.Errors)
                errors.Add(new ErrorHandlerResponse() { Error = item.ErrorMessage });

            return StatusCode(StatusCodes.Status400BadRequest, errors);
        }

        if (!ModelState.IsValid)
            return StatusCode(StatusCodes.Status400BadRequest, ModelState);

        var (addedSampleData, eventResult) = await _sampleDataAppService.AddAsync(request);

        return Ok(new SendMessageDtoResponse
        {
            Message = "Sample data created successfully",
            SampleId = addedSampleData.Id,
            EventSent = eventResult.IsSuccess,
            MessagingProvider = eventResult.Provider,
            EventError = eventResult.IsSuccess ? null : eventResult.ErrorMessage
        });

    }

    /// <summary>
    /// Get files with automatic event publishing and example logging
    /// </summary>
    /// <returns></returns>
    [HttpGet("GetFiles")]
    public async Task<ActionResult<FilesDtoResponse>> GetFilesAsync()
    {
        _logger.LogInformation("Retrieving files from SampleController at {Time}", DateTime.UtcNow);

        if (!ModelState.IsValid)
            return StatusCode(StatusCodes.Status400BadRequest, ModelState);

        var (files, eventResult) = await _sampleDataAppService.GetFilesAsync();

        if (!files.Any())
            return NoContent();

        return Ok(new FilesDtoResponse
        {
            Files = files,
            Count = files.Count(),
            EventSent = eventResult?.IsSuccess ?? false,
            EventError = eventResult?.IsSuccess == false ? eventResult.ErrorMessage : null
        });
    }

    /// <summary>
    /// Get all wizards with integration API Harry Potter
    /// </summary>
    /// <returns></returns>
    [HttpGet("GetWizards")]
    public async Task<ActionResult<ICollection<CharacterDtoResponse>>> GetWizardsAsync()
    {

        if (!ModelState.IsValid)
            return StatusCode(StatusCodes.Status400BadRequest, ModelState);

        var wizards = await _sampleDataAppService.GetAllWizardsAsync();

        if (wizards == null || !wizards.Any())
            return NoContent();

        return Ok(wizards);
    }

    #endregion

    #region Storage Operations

    /// <summary>
    /// Create a test file with the specified content
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    [HttpPost("storage/test-file")]
    public async Task<ActionResult<FileCreateDtoResponse>> CreateTestFileAsync([FromBody] string? content)
    {
        if (!ModelState.IsValid)
            return StatusCode(StatusCodes.Status400BadRequest, ModelState);

        var (fileName, storageType) = await _storageAppService.CreateTestFileAsync(content);

        return Ok(new FileCreateDtoResponse
        {
            Message = "File created successfully",
            FileName = fileName,
            StorageType = storageType
        });
    }

    /// <summary>
    /// Get all files from storage
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    [HttpGet("storage/files")]
    public async Task<ActionResult<FilesDtoResponse>> GetStorageFilesAsync([FromQuery] string path = "")
    {

        if (!ModelState.IsValid)
            return StatusCode(StatusCodes.Status400BadRequest, ModelState);

        var (files, storageType) = await _storageAppService.GetStorageFilesAsync(path);

        return Ok(new FilesDtoResponse
        {
            Files = files,
            StorageType = storageType,
            Path = path,
            Count = files.Count()
        });
    }

    /// <summary>
    /// Get file content by filename
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [HttpGet("storage/file/{fileName}")]
    public async Task<ActionResult<FilesDtoResponse>> GetFileContentAsync(string fileName)
    {
        if (!ModelState.IsValid)
            return StatusCode(StatusCodes.Status400BadRequest, ModelState);

        if (!await _storageAppService.FileExistsAsync(fileName))
            return NotFound($"File '{fileName}' not found");

        var (content, storageType) = await _storageAppService.GetFileContentAsync(fileName);

        return Ok(new FilesDtoResponse
        {
            FileName = fileName,
            Content = content,
            StorageType = storageType
        });
    }

    /// <summary>
    /// Delete a file from storage
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [HttpDelete("storage/file/{fileName}")]
    public async Task<ActionResult> DeleteFileAsync(string fileName)
    {
        if (!ModelState.IsValid)
            return StatusCode(StatusCodes.Status400BadRequest, ModelState);

        if (!await _storageAppService.FileExistsAsync(fileName))
            return NotFound($"File '{fileName}' not found");

        var storageType = await _storageAppService.DeleteFileAsync(fileName);

        return Ok($"File '{fileName}' deleted successfully");
    }

    #endregion

    #region Messaging Operations

    /// <summary>
    /// Send custom message via messaging system
    /// </summary>
    [HttpPost("messaging/send")]
    public async Task<ActionResult<MessageResultDtoResponse>> SendCustomMessageAsync([FromBody] SendMessageDtoRequest request)
    {
        if (!ModelState.IsValid)
            return StatusCode(StatusCodes.Status400BadRequest, ModelState);

        var result = await _messagingAppService.SendCustomMessageAsync(
            request.Topic,
            request.Payload,
            request.CorrelationId,
            request.Headers);

        if (result.IsSuccess)
        {
            return Ok(new MessageResultDtoResponse
            {
                Success = true,
                MessageId = result.MessageId,
                Provider = result.Provider,
                Message = "Message sent successfully"
            });
        }

        return BadRequest(new MessageResultDtoResponse
        {
            Success = false,
            Error = result.ErrorMessage,
            Provider = result.Provider
        });
    }

    /// <summary>
    /// Send multiple Sample-related messages in batch
    /// </summary>
    [HttpPost("messaging/batch")]
    public async Task<ActionResult<MessageResultDtoResponse>> SendBatchSampleMessagesAsync([FromBody] IEnumerable<SampleDataDto> sampleDataList)
    {
        if (!ModelState.IsValid)
            return StatusCode(StatusCodes.Status400BadRequest, ModelState);

        var results = await _messagingAppService.SendBatchSampleMessagesAsync(sampleDataList);
        var resultList = results.ToList();

        var successCount = resultList.Count(r => r.IsSuccess);
        var failureCount = resultList.Count(r => !r.IsSuccess);

        return Ok(new MessageResultDtoResponse
        {
            Success = failureCount == 0,
            TotalMessages = resultList.Count,
            SuccessCount = successCount,
            FailureCount = failureCount,
            Provider = resultList.FirstOrDefault()?.Provider,
            SampleDataProcessed = sampleDataList.Count()
        });
    }

    /// <summary>
    /// Check messaging system health
    /// </summary>
    [HttpGet("messaging/health")]
    public async Task<ActionResult<MessageResultDtoResponse>> CheckMessagingHealthAsync()
    {
        var (isHealthy, provider, timestamp) = await _messagingAppService.CheckHealthAsync();

        return Ok(new MessageResultDtoResponse
        {
            Success = isHealthy,
            Provider = provider,
            Status = isHealthy ? "Healthy" : "Unhealthy",
            Timestamp = timestamp,
            Service = "SampleController Messaging Integration"
        });

    }

    /// <summary>
    /// Simulate failure and send to DLQ (Dead Letter Queue) with Sample data
    /// </summary>
    [HttpPost("messaging/simulate-dlq")]
    public async Task<ActionResult<MessageResultDtoResponse>> SimulateDlqWithSampleAsync([FromBody] SampleDataDto sampleData)
    {
        if (!ModelState.IsValid)
            return StatusCode(StatusCodes.Status400BadRequest, ModelState);

        var result = await _messagingAppService.SimulateDlqWithSampleAsync(sampleData);

        if (result.IsSuccess)
        {
            return Ok(new
            {
                Success = true,
                MessageId = result.MessageId,
                Provider = result.Provider,
                Message = "Sample data sent to DLQ successfully",
                SampleId = sampleData.Id
            });
        }

        return BadRequest(new MessageResultDtoResponse
        {
            Success = false,
            Error = result.ErrorMessage,
            Provider = result.Provider
        });

    }

    /// <summary>
    /// Send Sample event with automatic retry
    /// </summary>
    [HttpPost("messaging/send-with-retry")]
    public async Task<ActionResult<MessageResultDtoResponse>> SendSampleEventWithRetryAsync([FromBody] SampleDataDto sampleData)
    {
        if (!ModelState.IsValid)
            return StatusCode(StatusCodes.Status400BadRequest, ModelState);

        var result = await _messagingAppService.SendSampleEventWithRetryAsync(sampleData);

        return Ok(new MessageResultDtoResponse
        {
            Success = result.IsSuccess,
            MessageId = result.MessageId,
            Provider = result.Provider,
            Message = result.IsSuccess ? "Sample event sent with retry successfully" : "Sample event failed after retries",
            Error = result.ErrorMessage,
            SampleId = sampleData.Id
        });
    }

    #endregion
}

