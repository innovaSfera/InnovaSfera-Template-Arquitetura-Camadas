using DomainDrivenDesign.Domain.Interfaces.Messaging;
using InnovaSfera.Template.Domain.Entities.Messaging;
using Microsoft.Extensions.Logging;

namespace DomainDrivenDesign.Domain.Services.Messaging;

/// <summary>
/// Unified messaging service with retry and advanced features
/// </summary>
public class MessagingService : IMessagingService
{
    private IMessageAdapter _messageAdapter;
    private readonly ILogger<MessagingService> _logger;

    public string CurrentProvider => _messageAdapter.ProviderName;

    public MessagingService(IMessageAdapter messageAdapter, ILogger<MessagingService> logger)
    {
        _messageAdapter = messageAdapter ?? throw new ArgumentNullException(nameof(messageAdapter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void SetProvider(IMessageAdapter provider)
    {
        _messageAdapter = provider ?? throw new ArgumentNullException(nameof(provider));
        _logger.LogInformation("Message adapter provider changed to: {ProviderName}", provider.ProviderName);
    }

    public async Task<MessageResult> SendAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        _logger.LogDebug("Sending message {MessageId} to topic {Topic}", message.Id, message.Topic);
        return await _messageAdapter.SendAsync(message, cancellationToken);
    }

    public async Task<IEnumerable<MessageResult>> SendBatchAsync(IEnumerable<IMessage> messages, CancellationToken cancellationToken = default)
    {
        if (messages == null)
            throw new ArgumentNullException(nameof(messages));

        var messageList = messages.ToList();
        _logger.LogDebug("Sending batch of {Count} messages", messageList.Count);
        return await _messageAdapter.SendBatchAsync(messageList, cancellationToken);
    }

    public async Task<MessageResult> SendToDlqAsync(IMessage message, string reason, CancellationToken cancellationToken = default)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        _logger.LogWarning("Sending message {MessageId} to DLQ. Reason: {Reason}", message.Id, reason);
        return await _messageAdapter.SendToDlqAsync(message, reason, cancellationToken);
    }

    public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Performing health check for provider {ProviderName}", _messageAdapter.ProviderName);
            return await _messageAdapter.HealthCheckAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed for provider {ProviderName}", _messageAdapter.ProviderName);
            return false;
        }
    }

    public async Task<MessageResult> SendWithRetryAsync(
        IMessage message,
        RetryConfiguration? retryConfig = null,
        CancellationToken cancellationToken = default)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        var config = retryConfig ?? RetryConfiguration.Default;
        
        _logger.LogInformation("Sending message {MessageId} with retry configuration (MaxRetries: {MaxRetries})", 
            message.Id, config.MaxRetries);

        Exception? lastException = null;
        
        for (int attempt = 0; attempt <= config.MaxRetries; attempt++)
        {
            try
            {
                var result = await _messageAdapter.SendAsync(message, cancellationToken);
                
                if (result.IsSuccess)
                {
                    if (attempt > 0)
                    {
                        _logger.LogInformation("Message {MessageId} sent successfully after {Attempts} attempts", 
                            message.Id, attempt + 1);
                    }
                    return result;
                }
                
                lastException = result.Exception ?? new MessageSendException(result.ErrorMessage ?? "Unknown error");
                
                if (attempt < config.MaxRetries)
                {
                    await HandleRetryAttempt(message, config, attempt, lastException, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                
                if (attempt < config.MaxRetries)
                {
                    await HandleRetryAttempt(message, config, attempt, ex, cancellationToken);
                }
            }
        }

        // All attempts failed
        _logger.LogError(lastException, "Message {MessageId} failed after {MaxRetries} retries, sending to DLQ", 
            message.Id, config.MaxRetries);
        
        if (config.SendToDlqOnMaxRetries)
        {
            return await _messageAdapter.SendToDlqAsync(message, 
                $"Failed after {config.MaxRetries} retry attempts: {lastException?.Message}", cancellationToken);
        }

        return MessageResult.Failed(
            $"Failed after {config.MaxRetries} retry attempts: {lastException?.Message}", 
            lastException, 
            _messageAdapter.ProviderName);
    }

    private async Task HandleRetryAttempt(
        IMessage message, 
        RetryConfiguration config, 
        int attempt, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        message.IncrementRetryCount();
        
        var delay = CalculateDelay(config, attempt);
        
        _logger.LogWarning(exception, 
            "Message {MessageId} failed, retrying in {Delay}ms (attempt {Attempt}/{MaxRetries})", 
            message.Id, delay, attempt + 1, config.MaxRetries);
        
        await Task.Delay(TimeSpan.FromMilliseconds(delay), cancellationToken);
    }

    private static double CalculateDelay(RetryConfiguration config, int attempt)
    {
        if (config.UseExponentialBackoff)
        {
            return Math.Min(config.BaseDelayMs * Math.Pow(2, attempt), config.MaxDelayMs);
        }
        
        return config.BaseDelayMs;
    }
}

/// <summary>
/// Custom exception for message sending
/// </summary>
public class MessageSendException : Exception
{
    public MessageSendException(string message) : base(message) { }
    public MessageSendException(string message, Exception? innerException) : base(message, innerException) { }
}
