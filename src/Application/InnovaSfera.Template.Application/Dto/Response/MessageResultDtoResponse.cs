namespace InnovaSfera.Template.Application.Dto.Response;

public class MessageResultDtoResponse
{
    public bool Success { get; set; }
    public bool IsSuccess => Success;
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public string? Provider { get; set; }
    public string Message { get; set; }
    public string? Error { get; set; }
    public Guid SampleId { get; set; }
    public string Status { get; set; }
    public DateTime Timestamp { get; set; }
    public string Service { get; set; }
    public int TotalMessages { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public int SampleDataProcessed { get; set; }
}
