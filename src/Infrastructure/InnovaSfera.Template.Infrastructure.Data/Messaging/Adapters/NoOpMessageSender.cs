using DomainDrivenDesign.Domain.Interfaces.Messaging;
using InnovaSfera.Template.Domain.Entities.Messaging;
using Microsoft.Extensions.Logging;

namespace InnovaSfera.Template.Infrastructure.Data.Messaging.Adapters;

/// <summary>
/// No-Op implementation for development/testing when no real provider is configured
/// </summary>
public class NoOpMessageSender : IMessageAdapter
{
    private readonly ILogger<NoOpMessageSender> _logger;

    public string ProviderName => "No-Op (Development)";

    public NoOpMessageSender(ILogger<NoOpMessageSender> logger)
    {
        _logger = logger;
    }

    public async Task<MessageResult> SendAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("No-Op: Simulating sending message {MessageId} to topic {Topic}", 
            message.Id, message.Topic);
        
        await Task.Delay(50, cancellationToken); // Simular latência
        
        return MessageResult.Successful(message.Id, ProviderName);
    }

    public async Task<IEnumerable<MessageResult>> SendBatchAsync(IEnumerable<IMessage> messages, CancellationToken cancellationToken = default)
    {
        var messageList = messages.ToList();
        _logger.LogInformation("No-Op: Simulating batch send of {Count} messages", messageList.Count);
        
        await Task.Delay(100, cancellationToken); // Simular latência
        
        return messageList.Select(m => MessageResult.Successful(m.Id, ProviderName));
    }

    public async Task<MessageResult> SendToDlqAsync(IMessage message, string reason, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("No-Op: Simulating DLQ send for message {MessageId}. Reason: {Reason}", 
            message.Id, reason);
        
        await Task.Delay(30, cancellationToken); // Simular latência
        
        return MessageResult.Successful(message.Id, $"{ProviderName}-DLQ");
    }

    public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken);
        return true; // No-Op sempre "saudável"
    }
}
