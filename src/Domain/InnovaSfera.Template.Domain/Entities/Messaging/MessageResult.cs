namespace InnovaSfera.Template.Domain.Entities.Messaging;

public class MessageResult
{
    public bool Success { get; set; }
    public bool IsSuccess => Success;
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public string? Provider { get; set; }

    public static MessageResult Successful(string messageId, string provider) => new()
    {
        Success = true,
        MessageId = messageId,
        Provider = provider
    };

    public static MessageResult Failed(string errorMessage, Exception? exception = null, string? provider = null) => new()
    {
        Success = false,
        ErrorMessage = errorMessage,
        Exception = exception,
        Provider = provider
    };
}