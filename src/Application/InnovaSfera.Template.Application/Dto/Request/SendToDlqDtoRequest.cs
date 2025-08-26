namespace InnovaSfera.Template.Application.Dto.Request;

public class SendToDlqDtoRequest
{
    public string Topic { get; set; } = string.Empty;
    public object Payload { get; set; } = new();
    public string? CorrelationId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
