using DomainDrivenDesign.Domain.Interfaces.Messaging;
using InnovaSfera.Template.Domain.Entities.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InnovaSfera.Template.Infrastructure.Data.Messaging.Adapters;

/// <summary>
/// Adapter para Apache Kafka
/// Para usar: instale o pacote Confluent.Kafka e descomente as implementações
/// </summary>
public class KafkaMessageSender : IMessageAdapter
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaMessageSender> _logger;
    // private readonly IProducer<string, string> _producer;

    public string ProviderName => "Kafka";

    public KafkaMessageSender(IConfiguration configuration, ILogger<KafkaMessageSender> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // TODO: Implementar configuração Kafka
        // var config = new ProducerConfig
        // {
        //     BootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
        //     ClientId = _configuration["Kafka:ClientId"] ?? "innovasfera-template",
        //     Acks = Acks.All,
        //     MessageTimeoutMs = 10000,
        //     RequestTimeoutMs = 5000,
        //     RetryBackoffMs = 1000,
        //     MessageSendMaxRetries = 3
        // };
        // _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task<MessageResult> SendAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending message {MessageId} to Kafka topic {Topic}", 
                message.Id, message.Topic);

            // TODO: Implementar envio Kafka
            // var kafkaMessage = new Message<string, string>
            // {
            //     Key = message.CorrelationId ?? message.Id,
            //     Value = JsonSerializer.Serialize(message.Payload),
            //     Headers = new Headers()
            // };
            // 
            // // Adicionar headers customizados
            // kafkaMessage.Headers.Add("MessageId", Encoding.UTF8.GetBytes(message.Id));
            // kafkaMessage.Headers.Add("CreatedAt", Encoding.UTF8.GetBytes(message.CreatedAt.ToString("O")));
            // kafkaMessage.Headers.Add("RetryCount", Encoding.UTF8.GetBytes(message.RetryCount.ToString()));
            // 
            // foreach (var header in message.Headers)
            // {
            //     kafkaMessage.Headers.Add(header.Key, Encoding.UTF8.GetBytes(header.Value?.ToString() ?? ""));
            // }
            // 
            // var deliveryResult = await _producer.ProduceAsync(message.Topic, kafkaMessage, cancellationToken);

            await Task.Delay(150, cancellationToken); // Simular envio

            _logger.LogInformation("Message {MessageId} sent successfully to Kafka", message.Id);
            return MessageResult.Successful(message.Id, ProviderName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message {MessageId} to Kafka", message.Id);
            return MessageResult.Failed($"Kafka send failed: {ex.Message}", ex, ProviderName);
        }
    }

    public async Task<IEnumerable<MessageResult>> SendBatchAsync(IEnumerable<IMessage> messages, CancellationToken cancellationToken = default)
    {
        var results = new List<MessageResult>();
        var tasks = new List<Task<MessageResult>>();

        // Kafka pode enviar em paralelo
        foreach (var message in messages)
        {
            tasks.Add(SendAsync(message, cancellationToken));
        }

        var completedResults = await Task.WhenAll(tasks);
        return completedResults;
    }

    public async Task<MessageResult> SendToDlqAsync(IMessage message, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogWarning("Sending message {MessageId} to Kafka DLQ. Reason: {Reason}", 
                message.Id, reason);

            // TODO: Implementar DLQ Kafka
            // var dlqTopic = $"{message.Topic}.dlq";
            // var dlqMessage = new 
            // {
            //     OriginalMessage = message,
            //     Reason = reason,
            //     SentToDlqAt = DateTime.UtcNow,
            //     OriginalTopic = message.Topic
            // };
            // 
            // var kafkaMessage = new Message<string, string>
            // {
            //     Key = message.Id,
            //     Value = JsonSerializer.Serialize(dlqMessage),
            //     Headers = new Headers()
            // };
            // 
            // kafkaMessage.Headers.Add("DLQ-Reason", Encoding.UTF8.GetBytes(reason));
            // kafkaMessage.Headers.Add("Original-Topic", Encoding.UTF8.GetBytes(message.Topic));
            // 
            // await _producer.ProduceAsync(dlqTopic, kafkaMessage, cancellationToken);

            await Task.Delay(75, cancellationToken); // Simular envio

            return MessageResult.Successful(message.Id, $"{ProviderName}-DLQ");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message {MessageId} to Kafka DLQ", message.Id);
            return MessageResult.Failed($"Kafka DLQ send failed: {ex.Message}", ex, ProviderName);
        }
    }

    public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: Implementar health check Kafka
            // var metadata = _producer.GetMetadata(TimeSpan.FromSeconds(5));
            // return metadata.Brokers.Any();
            
            await Task.Delay(20, cancellationToken);
            return true; // Simular sucesso
        }
        catch
        {
            return false;
        }
    }

    // TODO: Implementar IDisposable
    // public void Dispose()
    // {
    //     _producer?.Flush(TimeSpan.FromSeconds(10));
    //     _producer?.Dispose();
    // }
}
