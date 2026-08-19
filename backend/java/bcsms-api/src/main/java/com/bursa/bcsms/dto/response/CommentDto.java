package com.bursa.bcsms.dto.response;

import java.time.Instant;
import java.util.UUID;

public class CommentDto {
    private UUID id;
    private String content;
    private UUID createdByUserId;
    private Instant createdAt;

    public CommentDto() {
    }

    public CommentDto(UUID id, String content, UUID createdByUserId, Instant createdAt) {
        this.id = id;
        this.content = content;
        this.createdByUserId = createdByUserId;
        this.createdAt = createdAt;
    }

    public UUID getId() { return id; }
    public String getContent() { return content; }
    public UUID getCreatedByUserId() { return createdByUserId; }
    public Instant getCreatedAt() { return createdAt; }
}
