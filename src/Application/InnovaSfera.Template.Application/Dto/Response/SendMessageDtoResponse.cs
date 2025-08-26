namespace InnovaSfera.Template.Application.Dto.Response;

public class SendMessageDtoResponse
{
    public string Message { get; set; } = string.Empty;
    public Guid SampleId { get; set; }
    public bool EventSent { get; set; }
    public string? MessagingProvider { get; set; }
    public string? EventError { get; set; }

}
