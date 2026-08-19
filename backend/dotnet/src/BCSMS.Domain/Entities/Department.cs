using BCSMS.Domain.Common;

namespace BCSMS.Domain.Entities;

/// <summary>
/// Represents a municipal department responsible for handling service requests.
/// Aggregate root.
/// </summary>
public class Department : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Department() : base()
    {
        // For EF Core
    }

    public Department(Guid id, string name, string? description, DateTime createdAt)
        : base(id)
    {
        SetName(name);
        Description = description?.Trim();
        IsActive = true;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Updates the department's name and description.
    /// </summary>
    public void Update(string name, string? description, DateTime utcNow)
    {
        SetName(name);
        Description = description?.Trim();
        UpdatedAt = utcNow;
    }

    public void Activate(DateTime utcNow)
    {
        IsActive = true;
        UpdatedAt = utcNow;
    }

    public void Deactivate(DateTime utcNow)
    {
        IsActive = false;
        UpdatedAt = utcNow;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Department name is required.");

        Name = name.Trim();
    }
}
