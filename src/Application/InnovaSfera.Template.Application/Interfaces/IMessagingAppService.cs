using DomainDrivenDesign.Application.Entities;
using InnovaSfera.Template.Domain.Entities.Messaging;

namespace DomainDrivenDesign.Application.Interfaces;

/// <summary>
/// Application service interface for messaging operations
/// </summary>
public interface IMessagingAppService
{
    /// <summary>
    /// Send a custom message via messaging system
    /// </summary>
    Task<MessageResult> SendCustomMessageAsync(string topic, object payload, string? correlationId = null, Dictionary<string, object>? headers = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send multiple Sample-related messages in batch
    /// </summary>
    Task<IEnumerable<MessageResult>> SendBatchSampleMessagesAsync(IEnumerable<SampleDataDto> sampleDataList, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check messaging system health
    /// </summary>
    Task<(bool IsHealthy, string Provider, DateTime Timestamp)> CheckHealthAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Simulate failure and send to DLQ (Dead Letter Queue) with Sample data
    /// </summary>
    Task<MessageResult> SimulateDlqWithSampleAsync(SampleDataDto sampleData, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send Sample event with automatic retry
    /// </summary>
    Task<MessageResult> SendSampleEventWithRetryAsync(SampleDataDto sampleData, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send Sample creation event
    /// </summary>
    Task<MessageResult> SendSampleCreationEventAsync(SampleDataDto sampleData, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send Sample files retrieved event
    /// </summary>
    Task<MessageResult> SendFilesRetrievedEventAsync(IEnumerable<string> files, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send file deleted event
    /// </summary>
    Task<MessageResult> SendFileDeletedEventAsync(string fileName, string storageType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send empty files event
    /// </summary>
    Task<MessageResult> SendEmptyFilesEventAsync(CancellationToken cancellationToken = default);
}
