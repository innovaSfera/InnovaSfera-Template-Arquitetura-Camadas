namespace InnovaSfera.Template.Application.Dto.Response;

public class FilesDtoResponse
{
    public IEnumerable<string> Files { get; set; } = Enumerable.Empty<string>();
    public int Count { get; set; }
    public bool EventSent { get; set; }
    public string? EventError { get; set; }
    public string? StorageType { get; set; }
    public string? Path { get; set; }
    public string? FileName { get; set; }
    public string? Content { get; set; }
}
