namespace DomainDrivenDesign.Infrastructure.Core.Entity;

public abstract class Entity : IEntity
{
    public Guid Id { get; set; }
}
