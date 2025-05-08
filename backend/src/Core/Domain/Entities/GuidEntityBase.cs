namespace Domain.Entities;

public abstract class GuidEntityBase : IHaveIdentity<Guid>
{
    public Guid Id { get; protected init; }

    protected GuidEntityBase() : this(Guid.CreateVersion7()) {}
    
    protected GuidEntityBase(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Empty guid met.", nameof(id));
        }
        
        Id = id;
    }
}