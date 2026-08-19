using BCSMS.Domain.Common;

namespace BCSMS.Domain.Entities;

/// <summary>
/// A comment or note attached to a service request.
/// Child entity of the ServiceRequest aggregate — cannot be created externally.
/// </summary>
public class Comment : BaseEntity
{
    public Guid ServiceRequestId { get; private set; }
    public string Content { get; private set; } = default!;
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Comment() : base()
    {
        // For EF Core
    }

    internal Comment(Guid id, Guid serviceRequestId, string content,
        Guid createdByUserId, DateTime createdAt)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Comment content is required.");

        ServiceRequestId = serviceRequestId;
        Content = content.Trim();
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
    }
}
