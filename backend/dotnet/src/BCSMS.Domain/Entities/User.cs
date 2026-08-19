using BCSMS.Domain.Common;
using BCSMS.Domain.Enums;
using BCSMS.Domain.ValueObjects;

namespace BCSMS.Domain.Entities;

/// <summary>
/// Represents any system user: citizen, employee, manager, or admin.
/// Aggregate root. Role-based composition — no subclasses.
/// </summary>
public class User : BaseEntity
{
    public FullName Name { get; private set; } = default!;
    public ContactInfo Contact { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public UserRole Role { get; private set; }

    /// <summary>
    /// The department this user belongs to.
    /// Required for Employee and Manager roles. Must be null for Citizen and Admin roles.
    /// </summary>
    public Guid? DepartmentId { get; private set; }

    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private User() : base()
    {
        // For EF Core
    }

    public User(Guid id, FullName name, ContactInfo contact, string passwordHash, UserRole role,
        Guid? departmentId, DateTime createdAt)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        Name = name ?? throw new DomainException("User name is required.");
        Contact = contact ?? throw new DomainException("User contact information is required.");
        PasswordHash = passwordHash.Trim();
        Role = role;
        IsActive = true;
        CreatedAt = createdAt;

        ValidateDepartmentForRole(role, departmentId);
        DepartmentId = departmentId;
    }

    /// <summary>
    /// Updates the user's name and contact information.
    /// </summary>
    public void UpdateProfile(FullName name, ContactInfo contact, DateTime utcNow)
    {
        Name = name ?? throw new DomainException("User name is required.");
        Contact = contact ?? throw new DomainException("User contact information is required.");
        UpdatedAt = utcNow;
    }

    /// <summary>
    /// Changes the user's role and department assignment.
    /// </summary>
    public void ChangeRole(UserRole newRole, Guid? departmentId, DateTime utcNow)
    {
        ValidateDepartmentForRole(newRole, departmentId);
        Role = newRole;
        DepartmentId = departmentId;
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

    private static void ValidateDepartmentForRole(UserRole role, Guid? departmentId)
    {
        bool requiresDepartment = role is UserRole.Employee or UserRole.Manager;

        if (requiresDepartment && departmentId is null)
            throw new DomainException($"A {role} must be assigned to a department.");

        if (!requiresDepartment && departmentId is not null)
            throw new DomainException($"A {role} must not be assigned to a department.");
    }
}
