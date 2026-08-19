namespace BCSMS.Application.ServiceRequests.Common;

/// <summary>
/// DTO representing a service request comment.
/// </summary>
public record CommentDto(
    Guid Id,
    string Content,
    Guid CreatedByUserId,
    DateTime CreatedAt);
