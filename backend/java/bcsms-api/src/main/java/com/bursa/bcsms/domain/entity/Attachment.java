package com.bursa.bcsms.domain.entity;

import jakarta.persistence.*;
import java.time.Instant;
import java.util.UUID;

@Entity
@Table(name = "attachments")
public class Attachment {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID id;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "service_request_id", nullable = false)
    private ServiceRequest serviceRequest;

    @Column(name = "file_name", nullable = false)
    private String fileName;

    @Column(name = "content_type", nullable = false, length = 100)
    private String contentType;

    @Column(name = "file_size_in_bytes", nullable = false)
    private long fileSizeInBytes;

    @Column(name = "storage_path", nullable = false, length = 500)
    private String storagePath;

    @Column(name = "uploaded_by_user_id", nullable = false)
    private UUID uploadedByUserId;

    @Column(name = "uploaded_at", nullable = false)
    private Instant uploadedAt;

    protected Attachment() {
    }

    public Attachment(UUID id, ServiceRequest serviceRequest, String fileName, String contentType,
                      long fileSizeInBytes, String storagePath, UUID uploadedByUserId, Instant uploadedAt) {
        this.id = id != null ? id : UUID.randomUUID();
        this.serviceRequest = serviceRequest;
        this.fileName = fileName;
        this.contentType = contentType;
        this.fileSizeInBytes = fileSizeInBytes;
        this.storagePath = storagePath;
        this.uploadedByUserId = uploadedByUserId;
        this.uploadedAt = uploadedAt != null ? uploadedAt : Instant.now();
    }

    public UUID getId() {
        return id;
    }

    public ServiceRequest getServiceRequest() {
        return serviceRequest;
    }

    public String getFileName() {
        return fileName;
    }

    public String getContentType() {
        return contentType;
    }

    public long getFileSizeInBytes() {
        return fileSizeInBytes;
    }

    public String getStoragePath() {
        return storagePath;
    }

    public UUID getUploadedByUserId() {
        return uploadedByUserId;
    }

    public Instant getUploadedAt() {
        return uploadedAt;
    }

    public void setServiceRequest(ServiceRequest serviceRequest) {
        this.serviceRequest = serviceRequest;
    }
}
