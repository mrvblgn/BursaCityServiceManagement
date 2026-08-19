using BCSMS.Domain.Common;

namespace BCSMS.Domain.Entities;

/// <summary>
/// Metadata for a file attached to a service request.
/// The actual file is stored by Infrastructure — the domain only tracks metadata.
/// Child entity of the ServiceRequest aggregate — cannot be created externally.
/// </summary>
public class Attachment : BaseEntity
{
    public Guid ServiceRequestId { get; private set; }
    public string FileName { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public long FileSizeInBytes { get; private set; }
    public string StoragePath { get; private set; } = default!;
    public Guid UploadedByUserId { get; private set; }
    public DateTime UploadedAt { get; private set; }

    private Attachment() : base()
    {
        // For EF Core
    }

    internal Attachment(
        Guid id,
        Guid serviceRequestId,
        string fileName,
        string contentType,
        long fileSizeInBytes,
        string storagePath,
        Guid uploadedByUserId,
        DateTime uploadedAt)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainException("Attachment file name is required.");

        if (string.IsNullOrWhiteSpace(contentType))
            throw new DomainException("Attachment content type is required.");

        if (fileSizeInBytes <= 0)
            throw new DomainException("Attachment file size must be greater than zero.");

        if (string.IsNullOrWhiteSpace(storagePath))
            throw new DomainException("Attachment storage path is required.");

        ServiceRequestId = serviceRequestId;
        FileName = fileName.Trim();
        ContentType = contentType.Trim();
        FileSizeInBytes = fileSizeInBytes;
        StoragePath = storagePath.Trim();
        UploadedByUserId = uploadedByUserId;
        UploadedAt = uploadedAt;
    }
}
