using DomainDrivenDesign.Domain.Interfaces.Messaging;

namespace DomainDrivenDesign.Domain.Entities.Messaging;

/// <summary>
/// Implementação base de uma mensagem
/// </summary>
public class Message : IMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Topic { get; set; } = string.Empty;
    public object Payload { get; set; } = new();
    public Dictionary<string, object> Headers { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int RetryCount { get; set; } = 0;
    public string? CorrelationId { get; set; }

    public Message() { }

    public Message(string topic, object payload)
    {
        Topic = topic;
        Payload = payload;
    }

    public Message(string topic, object payload, string correlationId) : this(topic, payload)
    {
        CorrelationId = correlationId;
    }

    /// <summary>
    /// Adiciona um header à mensagem
    /// </summary>
    public Message AddHeader(string key, object value)
    {
        Headers[key] = value;
        return this;
    }

    /// <summary>
    /// Incrementa o contador de retry
    /// </summary>
    public void IncrementRetryCount()
    {
        RetryCount++;
    }

    /// <summary>
    /// Incrementa o contador de retry (método alternativo)
    /// </summary>
    public void IncrementRetry()
    {
        RetryCount++;
    }

    /// <summary>
    /// Cria uma cópia da mensagem para retry
    /// </summary>
    public Message CreateRetryMessage()
    {
        var retryMessage = new Message(Topic, Payload, CorrelationId ?? string.Empty)
        {
            Id = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            RetryCount = RetryCount + 1,
            Headers = new Dictionary<string, object>(Headers)
        };

        retryMessage.AddHeader("OriginalMessageId", Id);
        retryMessage.AddHeader("RetryAttempt", retryMessage.RetryCount);

        return retryMessage;
    }
}
