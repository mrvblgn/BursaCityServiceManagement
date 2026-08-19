namespace BCSMS.Domain.Common;

/// <summary>
/// Minimal base class for all domain entities. Provides identity only.
/// Timestamps and other properties are declared by each entity individually.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; private set; }

    protected BaseEntity()
    {
        // For EF Core
    }

    protected BaseEntity(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Entity Id cannot be empty.", nameof(id));

        Id = id;
    }
}
