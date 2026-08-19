package com.bursa.bcsms.dto.response;

import java.time.Instant;
import java.util.UUID;

public class AttachmentDto {
    private UUID id;
    private String fileName;
    private String contentType;
    private long fileSizeInBytes;
    private UUID uploadedByUserId;
    private Instant uploadedAt;

    public AttachmentDto() {
    }

    public AttachmentDto(UUID id, String fileName, String contentType, long fileSizeInBytes, UUID uploadedByUserId, Instant uploadedAt) {
        this.id = id;
        this.fileName = fileName;
        this.contentType = contentType;
        this.fileSizeInBytes = fileSizeInBytes;
        this.uploadedByUserId = uploadedByUserId;
        this.uploadedAt = uploadedAt;
    }

    public UUID getId() { return id; }
    public String getFileName() { return fileName; }
    public String getContentType() { return contentType; }
    public long getFileSizeInBytes() { return fileSizeInBytes; }
    public UUID getUploadedByUserId() { return uploadedByUserId; }
    public Instant getUploadedAt() { return uploadedAt; }
}
