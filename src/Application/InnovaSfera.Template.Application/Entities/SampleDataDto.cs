namespace DomainDrivenDesign.Application.Entities;

public class SampleDataDto
{
    public Guid Id { get; set; }
    public required string Message { get; set; }
    public DateTime TimeStamp { get; set; }
}
