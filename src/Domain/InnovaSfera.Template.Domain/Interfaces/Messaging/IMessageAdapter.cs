using InnovaSfera.Template.Domain.Entities.Messaging;

namespace DomainDrivenDesign.Domain.Interfaces.Messaging;

/// <summary>
/// Base interface for messaging adapters (Adapter pattern)
/// </summary>
public interface IMessageAdapter
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
    /// Send a message to Dead Letter Queue
    /// </summary>
    Task<MessageResult> SendToDlqAsync(IMessage message, string reason, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if the messaging provider is healthy
    /// </summary>
    Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Messaging provider name
    /// </summary>
    string ProviderName { get; }
}
