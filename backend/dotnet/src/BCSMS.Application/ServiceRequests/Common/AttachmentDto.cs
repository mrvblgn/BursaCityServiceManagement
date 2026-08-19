namespace BCSMS.Application.ServiceRequests.Common;

/// <summary>
/// DTO representing attachment metadata.
/// </summary>
public record AttachmentDto(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSizeInBytes,
    string StoragePath,
    Guid UploadedByUserId,
    DateTime UploadedAt);
