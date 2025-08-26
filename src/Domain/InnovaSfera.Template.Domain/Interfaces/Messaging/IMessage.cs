namespace DomainDrivenDesign.Domain.Interfaces.Messaging;

/// <summary>
/// Representa uma mensagem genérica do sistema
/// </summary>
public interface IMessage
{
    string Id { get; set; }
    string Topic { get; set; }
    object Payload { get; set; }
    Dictionary<string, object> Headers { get; set; }
    DateTime CreatedAt { get; set; }
    int RetryCount { get; set; }
    string? CorrelationId { get; set; }
    
    /// <summary>
    /// Incrementa o contador de retry
    /// </summary>
    void IncrementRetryCount();
}
