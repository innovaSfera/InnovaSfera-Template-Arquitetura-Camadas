using DomainDrivenDesign.Domain.Interfaces.Messaging;
using InnovaSfera.Template.Domain.Entities.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InnovaSfera.Template.Infrastructure.Data.Messaging.Adapters;

/// <summary>
/// Adapter para Azure Service Bus
/// Para usar: instale o pacote Azure.Messaging.ServiceBus e descomente as implementações
/// </summary>
public class AzureServiceBusMessageSender : IMessageAdapter
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureServiceBusMessageSender> _logger;
    // private readonly ServiceBusClient _client;
    // private readonly ServiceBusSender _sender;

    public string ProviderName => "Azure Service Bus";

    public AzureServiceBusMessageSender(IConfiguration configuration, ILogger<AzureServiceBusMessageSender> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // TODO: Implementar configuração Azure Service Bus
        // var connectionString = _configuration["AzureServiceBus:ConnectionString"] 
        //     ?? throw new InvalidOperationException("Azure Service Bus connection string not configured");
        // 
        // _client = new ServiceBusClient(connectionString);
        // var topicName = _configuration["AzureServiceBus:TopicName"] ?? "default-topic";
        // _sender = _client.CreateSender(topicName);
    }

    public async Task<MessageResult> SendAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending message {MessageId} to Azure Service Bus topic {Topic}", 
                message.Id, message.Topic);

            // TODO: Implementar envio Azure Service Bus
            // var serviceBusMessage = new ServiceBusMessage(JsonSerializer.Serialize(message.Payload))
            // {
            //     MessageId = message.Id,
            //     Subject = message.Topic,
            //     ContentType = "application/json",
            //     CorrelationId = message.CorrelationId
            // };
            // 
            // // Adicionar propriedades customizadas
            // serviceBusMessage.ApplicationProperties["CreatedAt"] = message.CreatedAt;
            // serviceBusMessage.ApplicationProperties["RetryCount"] = message.RetryCount;
            // 
            // foreach (var header in message.Headers)
            // {
            //     serviceBusMessage.ApplicationProperties[header.Key] = header.Value;
            // }
            // 
            // await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);

            await Task.Delay(200, cancellationToken); // Simular envio

            _logger.LogInformation("Message {MessageId} sent successfully to Azure Service Bus", message.Id);
            return MessageResult.Successful(message.Id, ProviderName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message {MessageId} to Azure Service Bus", message.Id);
            return MessageResult.Failed($"Azure Service Bus send failed: {ex.Message}", ex, ProviderName);
        }
    }

    public async Task<IEnumerable<MessageResult>> SendBatchAsync(IEnumerable<IMessage> messages, CancellationToken cancellationToken = default)
    {
        try
        {
            var messageList = messages.ToList();
            _logger.LogInformation("Sending batch of {Count} messages to Azure Service Bus", messageList.Count);

            // TODO: Implementar batch send Azure Service Bus
            // var serviceBusMessages = messageList.Select(msg => new ServiceBusMessage(JsonSerializer.Serialize(msg.Payload))
            // {
            //     MessageId = msg.Id,
            //     Subject = msg.Topic,
            //     ContentType = "application/json",
            //     CorrelationId = msg.CorrelationId
            // }).ToList();
            // 
            // await _sender.SendMessagesAsync(serviceBusMessages, cancellationToken);

            await Task.Delay(300, cancellationToken); // Simular envio em lote

            return messageList.Select(m => MessageResult.Successful(m.Id, ProviderName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message batch to Azure Service Bus");
            return messages.Select(m => MessageResult.Failed($"Batch send failed: {ex.Message}", ex, ProviderName));
        }
    }

    public async Task<MessageResult> SendToDlqAsync(IMessage message, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogWarning("Sending message {MessageId} to Azure Service Bus DLQ. Reason: {Reason}", 
                message.Id, reason);

            // TODO: Implementar DLQ Azure Service Bus
            // Azure Service Bus tem DLQ automática, mas podemos enviar para um tópico específico
            // var dlqTopicName = $"{message.Topic}.dlq";
            // var dlqSender = _client.CreateSender(dlqTopicName);
            // 
            // var dlqMessage = new 
            // {
            //     OriginalMessage = message,
            //     Reason = reason,
            //     SentToDlqAt = DateTime.UtcNow,
            //     OriginalTopic = message.Topic
            // };
            // 
            // var serviceBusMessage = new ServiceBusMessage(JsonSerializer.Serialize(dlqMessage))
            // {
            //     MessageId = $"dlq-{message.Id}",
            //     Subject = dlqTopicName,
            //     ContentType = "application/json"
            // };
            // 
            // serviceBusMessage.ApplicationProperties["DLQ-Reason"] = reason;
            // serviceBusMessage.ApplicationProperties["Original-MessageId"] = message.Id;
            // 
            // await dlqSender.SendMessageAsync(serviceBusMessage, cancellationToken);

            await Task.Delay(100, cancellationToken); // Simular envio

            return MessageResult.Successful(message.Id, $"{ProviderName}-DLQ");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message {MessageId} to Azure Service Bus DLQ", message.Id);
            return MessageResult.Failed($"Azure Service Bus DLQ send failed: {ex.Message}", ex, ProviderName);
        }
    }

    public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: Implementar health check Azure Service Bus
            // var processor = _client.CreateProcessor("health-check-topic");
            // return !processor.IsClosed;
            
            await Task.Delay(15, cancellationToken);
            return true; // Simular sucesso
        }
        catch
        {
            return false;
        }
    }

    // TODO: Implementar IDisposable
    // public async ValueTask DisposeAsync()
    // {
    //     if (_sender != null)
    //         await _sender.DisposeAsync();
    //     if (_client != null)
    //         await _client.DisposeAsync();
    // }
}
