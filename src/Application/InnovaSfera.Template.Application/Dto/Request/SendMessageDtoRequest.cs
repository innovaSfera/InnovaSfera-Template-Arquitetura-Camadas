namespace InnovaSfera.Template.Application.Dto.Request;

public class SendMessageDtoRequest
{
    public string Topic { get; set; } = string.Empty;
    public object Payload { get; set; } = new();
    public string? CorrelationId { get; set; }
    public Dictionary<string, object>? Headers { get; set; }
}
