using DomainDrivenDesign.Infrastructure.Core.Entity;

namespace DomainDrivenDesign.Domain.Entities;

public class SampleData : Entity
{
    public DateTime TimeStamp { get; set; }
    public required string Message { get; set; }
}
