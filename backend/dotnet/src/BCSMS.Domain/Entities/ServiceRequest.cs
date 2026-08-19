using BCSMS.Domain.Common;
using BCSMS.Domain.Enums;
using BCSMS.Domain.ValueObjects;

namespace BCSMS.Domain.Entities;

/// <summary>
/// Represents a citizen's request for a municipal service.
/// Aggregate root containing status history, comments, and attachment metadata.
/// Encapsulates the full request lifecycle state machine.
///
/// All timestamps are supplied by the caller to keep domain behavior deterministic
/// and unit-testable.
/// </summary>
public class ServiceRequest : BaseEntity
{
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public Guid CategoryId { get; private set; }
    public RequestStatus Status { get; private set; }
    public Priority? Priority { get; private set; }
    public Location? Location { get; private set; }

    /// <summary>
    /// The citizen who created this request. Immutable after creation.
    /// </summary>
    public Guid CitizenId { get; private set; }

    public Guid? AssignedDepartmentId { get; private set; }
    public Guid? AssignedEmployeeId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Encapsulated collections — external code uses the IReadOnlyList properties.
    private readonly List<StatusHistoryEntry> _statusHistory = new();
    private readonly List<Comment> _comments = new();
    private readonly List<Attachment> _attachments = new();

    public IReadOnlyList<StatusHistoryEntry> StatusHistory => _statusHistory.AsReadOnly();
    public IReadOnlyList<Comment> Comments => _comments.AsReadOnly();
    public IReadOnlyList<Attachment> Attachments => _attachments.AsReadOnly();

    private ServiceRequest() : base()
    {
        // For EF Core
    }

    public ServiceRequest(
        Guid id,
        string title,
        Guid categoryId,
        Guid citizenId,
        DateTime createdAt,
        string? description = null,
        Location? location = null)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Service request title is required.");

        if (categoryId == Guid.Empty)
            throw new DomainException("Category ID is required.");

        if (citizenId == Guid.Empty)
            throw new DomainException("Citizen ID is required.");

        Title = title.Trim();
        Description = description?.Trim();
        CategoryId = categoryId;
        CitizenId = citizenId;
        Location = location;
        Status = RequestStatus.New;
        CreatedAt = createdAt;
    }

    // ──────────────────────────────────────────────
    // State Machine Transitions
    // ──────────────────────────────────────────────

    /// <summary>
    /// Transitions: New → Reviewing.
    /// A manager or admin begins reviewing the request.
    /// </summary>
    public void StartReview(Guid changedByUserId, DateTime utcNow)
    {
        EnsureValidTransition(RequestStatus.New, RequestStatus.Reviewing);
        TransitionTo(RequestStatus.Reviewing, changedByUserId, utcNow);
    }

    /// <summary>
    /// Transitions: Reviewing → Assigned.
    /// Assigns the request to a department and optionally to an employee.
    /// Priority must be provided.
    /// </summary>
    public void Assign(Guid departmentId, Guid? employeeId, Priority priority,
        Guid changedByUserId, DateTime utcNow)
    {
        EnsureValidTransition(RequestStatus.Reviewing, RequestStatus.Assigned);

        if (departmentId == Guid.Empty)
            throw new DomainException("Department ID is required for assignment.");

        AssignedDepartmentId = departmentId;
        AssignedEmployeeId = employeeId;
        Priority = priority;
        TransitionTo(RequestStatus.Assigned, changedByUserId, utcNow);
    }

    /// <summary>
    /// Transitions: Assigned → InProgress.
    /// The assigned employee begins working on the request.
    /// </summary>
    public void StartProgress(Guid changedByUserId, DateTime utcNow)
    {
        EnsureValidTransition(RequestStatus.Assigned, RequestStatus.InProgress);
        TransitionTo(RequestStatus.InProgress, changedByUserId, utcNow);
    }

    /// <summary>
    /// Transitions: InProgress → Resolved.
    /// The employee has completed the work.
    /// </summary>
    public void Resolve(Guid changedByUserId, DateTime utcNow, string? note = null)
    {
        EnsureValidTransition(RequestStatus.InProgress, RequestStatus.Resolved);
        TransitionTo(RequestStatus.Resolved, changedByUserId, utcNow, note);
    }

    /// <summary>
    /// Transitions: Resolved → Closed.
    /// A manager or admin confirms the resolution.
    /// </summary>
    public void Close(Guid changedByUserId, DateTime utcNow, string? note = null)
    {
        EnsureValidTransition(RequestStatus.Resolved, RequestStatus.Closed);
        TransitionTo(RequestStatus.Closed, changedByUserId, utcNow, note);
    }

    /// <summary>
    /// Transitions: Resolved → InProgress.
    /// Resolution was not satisfactory — reopen for further work.
    /// </summary>
    public void Reopen(Guid changedByUserId, DateTime utcNow, string? note = null)
    {
        EnsureValidTransition(RequestStatus.Resolved, RequestStatus.InProgress);
        TransitionTo(RequestStatus.InProgress, changedByUserId, utcNow, note);
    }

    /// <summary>
    /// Transitions: New or Reviewing → Rejected.
    /// The request is denied by staff.
    /// </summary>
    public void Reject(Guid changedByUserId, DateTime utcNow, string? note = null)
    {
        if (Status is not (RequestStatus.New or RequestStatus.Reviewing))
            throw new DomainException(
                $"Cannot reject a request in '{Status}' status. " +
                "Only 'New' or 'Reviewing' requests can be rejected.");

        TransitionTo(RequestStatus.Rejected, changedByUserId, utcNow, note);
    }

    /// <summary>
    /// Transitions: New, Reviewing, or Assigned → Cancelled.
    /// The citizen withdraws the request, or a manager cancels it before work starts.
    /// </summary>
    public void Cancel(Guid changedByUserId, DateTime utcNow)
    {
        if (Status is not (RequestStatus.New or RequestStatus.Reviewing or RequestStatus.Assigned))
            throw new DomainException(
                $"Cannot cancel a request in '{Status}' status. " +
                "Only requests in 'New', 'Reviewing', or 'Assigned' status can be cancelled.");

        TransitionTo(RequestStatus.Cancelled, changedByUserId, utcNow);
    }

    // ──────────────────────────────────────────────
    // Child Entity Management
    // ──────────────────────────────────────────────

    /// <summary>
    /// Adds a comment to the request. Not allowed on terminal-state requests.
    /// </summary>
    public Comment AddComment(string content, Guid createdByUserId, DateTime utcNow)
    {
        EnsureNotInTerminalState("add a comment");

        var comment = new Comment(Guid.NewGuid(), Id, content, createdByUserId, utcNow);
        _comments.Add(comment);
        UpdatedAt = utcNow;
        return comment;
    }

    /// <summary>
    /// Adds attachment metadata to the request. Not allowed on terminal-state requests.
    /// </summary>
    public Attachment AddAttachment(
        string fileName,
        string contentType,
        long fileSizeInBytes,
        string storagePath,
        Guid uploadedByUserId,
        DateTime utcNow)
    {
        EnsureNotInTerminalState("add an attachment");

        var attachment = new Attachment(
            Guid.NewGuid(), Id, fileName, contentType,
            fileSizeInBytes, storagePath, uploadedByUserId, utcNow);
        _attachments.Add(attachment);
        UpdatedAt = utcNow;
        return attachment;
    }

    // ──────────────────────────────────────────────
    // Detail Update Methods
    // ──────────────────────────────────────────────

    /// <summary>
    /// Updates the request title. Not allowed on terminal-state requests.
    /// </summary>
    public void UpdateTitle(string title, DateTime utcNow)
    {
        EnsureNotInTerminalState("update title");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Service request title cannot be empty.");

        Title = title.Trim();
        UpdatedAt = utcNow;
    }

    /// <summary>
    /// Updates the request description. Not allowed on terminal-state requests.
    /// </summary>
    public void UpdateDescription(string? description, DateTime utcNow)
    {
        EnsureNotInTerminalState("update description");

        Description = description?.Trim();
        UpdatedAt = utcNow;
    }

    /// <summary>
    /// Changes the request category. Not allowed on terminal-state requests.
    /// </summary>
    public void ChangeCategory(Guid categoryId, DateTime utcNow)
    {
        EnsureNotInTerminalState("change category");

        if (categoryId == Guid.Empty)
            throw new DomainException("Category ID is required.");

        CategoryId = categoryId;
        UpdatedAt = utcNow;
    }

    /// <summary>
    /// Sets or changes the request location. Not allowed on terminal-state requests.
    /// </summary>
    public void ChangeLocation(Location location, DateTime utcNow)
    {
        EnsureNotInTerminalState("change location");

        Location = location ?? throw new DomainException("Location is required. Use RemoveLocation to clear it.");
        UpdatedAt = utcNow;
    }

    /// <summary>
    /// Removes the request location. Not allowed on terminal-state requests.
    /// </summary>
    public void RemoveLocation(DateTime utcNow)
    {
        EnsureNotInTerminalState("remove location");

        Location = null;
        UpdatedAt = utcNow;
    }

    /// <summary>
    /// Sets or updates the priority. Not allowed on terminal-state requests.
    /// </summary>
    public void SetPriority(Priority priority, DateTime utcNow)
    {
        EnsureNotInTerminalState("set priority");

        Priority = priority;
        UpdatedAt = utcNow;
    }

    // ──────────────────────────────────────────────
    // Private Helpers
    // ──────────────────────────────────────────────

    private void TransitionTo(RequestStatus newStatus, Guid changedByUserId,
        DateTime utcNow, string? note = null)
    {
        var entry = new StatusHistoryEntry(
            Guid.NewGuid(), Id, Status, newStatus, changedByUserId, utcNow, note);

        _statusHistory.Add(entry);
        Status = newStatus;
        UpdatedAt = utcNow;
    }

    private void EnsureValidTransition(RequestStatus expectedCurrent, RequestStatus target)
    {
        if (Status != expectedCurrent)
            throw new DomainException(
                $"Cannot transition to '{target}' from '{Status}'. " +
                $"Expected current status: '{expectedCurrent}'.");
    }

    private void EnsureNotInTerminalState(string action)
    {
        if (Status is RequestStatus.Closed or RequestStatus.Rejected or RequestStatus.Cancelled)
            throw new DomainException(
                $"Cannot {action} on a request in terminal state '{Status}'.");
    }
}
