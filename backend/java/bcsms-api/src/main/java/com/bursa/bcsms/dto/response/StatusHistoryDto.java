package com.bursa.bcsms.dto.response;

import com.bursa.bcsms.domain.enums.RequestStatus;
import java.time.Instant;
import java.util.UUID;

public class StatusHistoryDto {
    private UUID id;
    private RequestStatus fromStatus;
    private RequestStatus toStatus;
    private UUID changedByUserId;
    private Instant timestamp;
    private String note;

    public StatusHistoryDto() {
    }

    public StatusHistoryDto(UUID id, RequestStatus fromStatus, RequestStatus toStatus, UUID changedByUserId, Instant timestamp, String note) {
        this.id = id;
        this.fromStatus = fromStatus;
        this.toStatus = toStatus;
        this.changedByUserId = changedByUserId;
        this.timestamp = timestamp;
        this.note = note;
    }

    public UUID getId() { return id; }
    public RequestStatus getFromStatus() { return fromStatus; }
    public RequestStatus getToStatus() { return toStatus; }
    public UUID getChangedByUserId() { return changedByUserId; }
    public Instant getTimestamp() { return timestamp; }
    public String getNote() { return note; }
}
