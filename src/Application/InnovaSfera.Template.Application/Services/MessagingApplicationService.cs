using DomainDrivenDesign.Application.Entities;
using DomainDrivenDesign.Application.Interfaces;
using DomainDrivenDesign.Domain.Entities.Messaging;
using DomainDrivenDesign.Domain.Interfaces.Messaging;
using InnovaSfera.Template.Domain.Entities.Messaging;
using Microsoft.Extensions.Logging;

namespace DomainDrivenDesign.Application.Services;

/// <summary>
/// Application service for messaging operations
/// </summary>
public class MessagingApplicationService : IMessagingAppService
{
    private readonly IMessagingService _messagingService;
    private readonly ILogger<MessagingApplicationService> _logger;

    public MessagingApplicationService(
        IMessagingService messagingService,
        ILogger<MessagingApplicationService> logger)
    {
        _messagingService = messagingService;
        _logger = logger;
    }

    public async Task<MessageResult> SendCustomMessageAsync(string topic, object payload, string? correlationId = null, Dictionary<string, object>? headers = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message(
                topic,
                payload,
                correlationId ?? string.Empty);

            // Add custom headers if provided
            if (headers?.Any() == true)
            {
                foreach (var header in headers)
                {
                    message.AddHeader(header.Key, header.Value);
                }
            }

            var result = await _messagingService.SendAsync(message, cancellationToken);
            
            _logger.LogInformation("Custom message sent successfully to topic {Topic}. MessageId: {MessageId}, Provider: {Provider}", 
                topic, result.MessageId, result.Provider);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending custom message to topic {Topic}", topic);
            throw;
        }
    }

    public async Task<IEnumerable<MessageResult>> SendBatchSampleMessagesAsync(IEnumerable<SampleDataDto> sampleDataList, CancellationToken cancellationToken = default)
    {
        try
        {
            var messages = sampleDataList.Select(sample => new Message(
                "sample.data.batch",
                new { 
                    Id = sample.Id,
                    Message = sample.Message,
                    TimeStamp = sample.TimeStamp,
                    BatchCreatedAt = DateTime.UtcNow
                },
                Guid.NewGuid().ToString()
            )).ToList();

            var results = await _messagingService.SendBatchAsync(messages, cancellationToken);
            
            var resultsList = results.ToList();
            var successCount = resultsList.Count(r => r.IsSuccess);
            var failureCount = resultsList.Count(r => !r.IsSuccess);
            
            _logger.LogInformation("Batch messages sent. Total: {Total}, Success: {Success}, Failures: {Failures}", 
                resultsList.Count, successCount, failureCount);
            
            return resultsList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending batch sample messages");
            throw;
        }
    }

    public async Task<(bool IsHealthy, string Provider, DateTime Timestamp)> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var isHealthy = await _messagingService.HealthCheckAsync(cancellationToken);
            var timestamp = DateTime.UtcNow;
            
            _logger.LogInformation("Messaging health check completed. Provider: {Provider}, Healthy: {IsHealthy}", 
                _messagingService.CurrentProvider, isHealthy);
            
            return (isHealthy, _messagingService.CurrentProvider, timestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during messaging health check");
            return (false, _messagingService.CurrentProvider, DateTime.UtcNow);
        }
    }

    public async Task<MessageResult> SimulateDlqWithSampleAsync(SampleDataDto sampleData, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message(
                "sample.data.failed.processing",
                new { 
                    Id = sampleData.Id,
                    Message = sampleData.Message,
                    TimeStamp = sampleData.TimeStamp,
                    FailedAt = DateTime.UtcNow,
                    Reason = "Simulated failure for testing DLQ"
                },
                Guid.NewGuid().ToString()
            );

            var result = await _messagingService.SendToDlqAsync(message, "Simulated failure for testing purposes", cancellationToken);
            
            _logger.LogInformation("Sample data sent to DLQ successfully. SampleId: {SampleId}, MessageId: {MessageId}", 
                sampleData.Id, result.MessageId);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error simulating DLQ with sample data {SampleId}", sampleData.Id);
            throw;
        }
    }

    public async Task<MessageResult> SendSampleEventWithRetryAsync(SampleDataDto sampleData, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message(
                "sample.data.retry.test",
                new { 
                    Id = sampleData.Id,
                    Message = sampleData.Message,
                    TimeStamp = sampleData.TimeStamp,
                    RetryTestAt = DateTime.UtcNow
                },
                Guid.NewGuid().ToString()
            );

            // Custom retry configuration
            var retryConfig = new RetryConfiguration
            {
                MaxRetries = 5,
                BaseDelayMs = 500,
                MaxDelayMs = 10000,
                UseExponentialBackoff = true,
                SendToDlqOnMaxRetries = true
            };

            var result = await _messagingService.SendWithRetryAsync(message, retryConfig, cancellationToken);
            
            _logger.LogInformation("Sample event sent with retry. SampleId: {SampleId}, Success: {Success}", 
                sampleData.Id, result.IsSuccess);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending sample event with retry for SampleId {SampleId}", sampleData.Id);
            throw;
        }
    }

    public async Task<MessageResult> SendSampleCreationEventAsync(SampleDataDto sampleData, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message(
                "sample.data.created",
                new { 
                    Id = sampleData.Id,
                    Message = sampleData.Message,
                    TimeStamp = sampleData.TimeStamp,
                    CreatedAt = DateTime.UtcNow
                },
                Guid.NewGuid().ToString()
            );

            var result = await _messagingService.SendWithRetryAsync(message, cancellationToken: cancellationToken);
            
            _logger.LogInformation("Sample creation event sent. SampleId: {SampleId}, Success: {Success}", 
                sampleData.Id, result.IsSuccess);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending sample creation event for SampleId {SampleId}", sampleData.Id);
            throw;
        }
    }

    public async Task<MessageResult> SendFilesRetrievedEventAsync(IEnumerable<string> files, CancellationToken cancellationToken = default)
    {
        try
        {
            var filesArray = files.ToArray();
            var message = new Message(
                "sample.files.retrieved",
                new { 
                    Count = filesArray.Length, 
                    Files = filesArray, 
                    Timestamp = DateTime.UtcNow 
                },
                Guid.NewGuid().ToString()
            );

            var result = await _messagingService.SendAsync(message, cancellationToken);
            
            _logger.LogInformation("Files retrieved event sent. Count: {Count}, Success: {Success}", 
                filesArray.Length, result.IsSuccess);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending files retrieved event");
            throw;
        }
    }

    public async Task<MessageResult> SendFileDeletedEventAsync(string fileName, string storageType, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message(
                "sample.file.deleted",
                new { 
                    FileName = fileName, 
                    DeletedAt = DateTime.UtcNow, 
                    StorageType = storageType 
                },
                Guid.NewGuid().ToString()
            );

            var result = await _messagingService.SendAsync(message, cancellationToken);
            
            _logger.LogInformation("File deleted event sent. FileName: {FileName}, Success: {Success}", 
                fileName, result.IsSuccess);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending file deleted event for file {FileName}", fileName);
            throw;
        }
    }

    public async Task<MessageResult> SendEmptyFilesEventAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message(
                "sample.files.empty",
                new { 
                    Timestamp = DateTime.UtcNow, 
                    Action = "GetFiles" 
                },
                Guid.NewGuid().ToString()
            );

            var result = await _messagingService.SendAsync(message, cancellationToken);
            
            _logger.LogInformation("Empty files event sent. Success: {Success}", result.IsSuccess);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending empty files event");
            throw;
        }
    }
}
