namespace InnovaSfera.Template.Application.Dto.Request;

public class SendBatchMessageDtoRequest
{
    public List<SendMessageDtoRequest> Messages { get; set; } = new();
}
