using InnovaSfera.Template.Domain.Entities.Messaging;

namespace DomainDrivenDesign.Domain.Interfaces.Messaging;

/// <summary>
/// Unified messaging service interface (Port)
/// </summary>
public interface IMessagingService
{
    /// <summary>
    /// Send a message to the specified topic
    /// </summary>
    Task<MessageResult> SendAsync(IMessage message, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send multiple messages in batch
    /// </summary>
    Task<IEnumerable<MessageResult>> SendBatchAsync(IEnumerable<IMessage> messages, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send a message with automatic retry
    /// </summary>
    Task<MessageResult> SendWithRetryAsync(IMessage message, RetryConfiguration? retryConfig = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send a message to Dead Letter Queue
    /// </summary>
    Task<MessageResult> SendToDlqAsync(IMessage message, string reason, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if the messaging provider is healthy
    /// </summary>
    Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Current messaging provider name
    /// </summary>
    string CurrentProvider { get; }
}
