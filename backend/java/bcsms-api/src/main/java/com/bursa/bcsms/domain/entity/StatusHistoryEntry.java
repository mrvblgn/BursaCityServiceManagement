package com.bursa.bcsms.domain.entity;

import com.bursa.bcsms.domain.enums.RequestStatus;
import jakarta.persistence.*;
import java.time.Instant;
import java.util.UUID;

@Entity
@Table(name = "status_history_entries")
public class StatusHistoryEntry {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID id;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "service_request_id", nullable = false)
    private ServiceRequest serviceRequest;

    @Enumerated(EnumType.STRING)
    @Column(name = "from_status", nullable = false, length = 50)
    private RequestStatus fromStatus;

    @Enumerated(EnumType.STRING)
    @Column(name = "to_status", nullable = false, length = 50)
    private RequestStatus toStatus;

    @Column(name = "changed_by_user_id", nullable = false)
    private UUID changedByUserId;

    @Column(name = "timestamp", nullable = false)
    private Instant timestamp;

    @Column(name = "note", columnDefinition = "TEXT")
    private String note;

    protected StatusHistoryEntry() {
    }

    public StatusHistoryEntry(UUID id, ServiceRequest serviceRequest, RequestStatus fromStatus,
                              RequestStatus toStatus, UUID changedByUserId, Instant timestamp, String note) {
        this.id = id != null ? id : UUID.randomUUID();
        this.serviceRequest = serviceRequest;
        this.fromStatus = fromStatus;
        this.toStatus = toStatus;
        this.changedByUserId = changedByUserId;
        this.timestamp = timestamp != null ? timestamp : Instant.now();
        this.note = note != null ? note.trim() : null;
    }

    public UUID getId() {
        return id;
    }

    public ServiceRequest getServiceRequest() {
        return serviceRequest;
    }

    public RequestStatus getFromStatus() {
        return fromStatus;
    }

    public RequestStatus getToStatus() {
        return toStatus;
    }

    public UUID getChangedByUserId() {
        return changedByUserId;
    }

    public Instant getTimestamp() {
        return timestamp;
    }

    public String getNote() {
        return note;
    }

    public void setServiceRequest(ServiceRequest serviceRequest) {
        this.serviceRequest = serviceRequest;
    }
}
