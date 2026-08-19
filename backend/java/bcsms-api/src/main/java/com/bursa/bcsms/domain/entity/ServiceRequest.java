package com.bursa.bcsms.domain.entity;

import com.bursa.bcsms.common.exception.DomainException;
import com.bursa.bcsms.domain.enums.Priority;
import com.bursa.bcsms.domain.enums.RequestStatus;
import com.bursa.bcsms.domain.valueobject.Location;
import jakarta.persistence.*;

import java.time.Instant;
import java.util.*;

@Entity
@Table(name = "service_requests")
public class ServiceRequest {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID id;

    @Column(name = "title", nullable = false, length = 200)
    private String title;

    @Column(name = "description", columnDefinition = "TEXT")
    private String description;

    @Column(name = "category_id", nullable = false)
    private UUID categoryId;

    @Column(name = "citizen_id", nullable = false)
    private UUID citizenId;

    @Enumerated(EnumType.STRING)
    @Column(name = "status", nullable = false, length = 50)
    private RequestStatus status;

    @Enumerated(EnumType.STRING)
    @Column(name = "priority", length = 50)
    private Priority priority;

    @Column(name = "assigned_department_id")
    private UUID assignedDepartmentId;

    @Column(name = "assigned_employee_id")
    private UUID assignedEmployeeId;

    @Embedded
    private Location location;

    @Column(name = "created_at", nullable = false)
    private Instant createdAt;

    @Column(name = "updated_at")
    private Instant updatedAt;

    @OneToMany(mappedBy = "serviceRequest", cascade = CascadeType.ALL, orphanRemoval = true, fetch = FetchType.LAZY)
    private List<StatusHistoryEntry> statusHistory = new ArrayList<>();

    @OneToMany(mappedBy = "serviceRequest", cascade = CascadeType.ALL, orphanRemoval = true, fetch = FetchType.LAZY)
    private List<Comment> comments = new ArrayList<>();

    @OneToMany(mappedBy = "serviceRequest", cascade = CascadeType.ALL, orphanRemoval = true, fetch = FetchType.LAZY)
    private List<Attachment> attachments = new ArrayList<>();

    protected ServiceRequest() {
    }

    public ServiceRequest(UUID id, String title, UUID categoryId, UUID citizenId,
                          Instant createdAt, String description, Location location) {
        if (title == null || title.isBlank()) {
            throw new DomainException("Service request title is required.");
        }
        if (categoryId == null) {
            throw new DomainException("Category ID is required.");
        }
        if (citizenId == null) {
            throw new DomainException("Citizen ID is required.");
        }

        this.id = id != null ? id : UUID.randomUUID();
        this.title = title.trim();
        this.categoryId = categoryId;
        this.citizenId = citizenId;
        this.description = description != null ? description.trim() : null;
        this.location = location;
        this.status = RequestStatus.NEW;
        this.createdAt = createdAt != null ? createdAt : Instant.now();
    }

    // ──────────────────────────────────────────────
    // State Machine Lifecycle Transitions
    // ──────────────────────────────────────────────

    /**
     * Transitions: NEW -> REVIEWING
     */
    public void startReview(UUID changedByUserId, Instant utcNow) {
        ensureValidTransition(RequestStatus.NEW, RequestStatus.REVIEWING);
        transitionTo(RequestStatus.REVIEWING, changedByUserId, utcNow, null);
    }

    /**
     * Transitions: REVIEWING -> ASSIGNED
     */
    public void assign(UUID departmentId, UUID employeeId, Priority priority, UUID changedByUserId, Instant utcNow) {
        ensureValidTransition(RequestStatus.REVIEWING, RequestStatus.ASSIGNED);

        if (departmentId == null) {
            throw new DomainException("Department ID is required for assignment.");
        }
        if (priority == null) {
            throw new DomainException("Priority is required for assignment.");
        }

        this.assignedDepartmentId = departmentId;
        this.assignedEmployeeId = employeeId;
        this.priority = priority;
        transitionTo(RequestStatus.ASSIGNED, changedByUserId, utcNow, null);
    }

    /**
     * Transitions: ASSIGNED -> IN_PROGRESS
     */
    public void startProgress(UUID changedByUserId, Instant utcNow) {
        ensureValidTransition(RequestStatus.ASSIGNED, RequestStatus.IN_PROGRESS);
        transitionTo(RequestStatus.IN_PROGRESS, changedByUserId, utcNow, null);
    }

    /**
     * Transitions: IN_PROGRESS -> RESOLVED
     */
    public void resolve(UUID changedByUserId, Instant utcNow, String note) {
        ensureValidTransition(RequestStatus.IN_PROGRESS, RequestStatus.RESOLVED);
        transitionTo(RequestStatus.RESOLVED, changedByUserId, utcNow, note);
    }

    /**
     * Transitions: RESOLVED -> CLOSED
     */
    public void close(UUID changedByUserId, Instant utcNow, String note) {
        ensureValidTransition(RequestStatus.RESOLVED, RequestStatus.CLOSED);
        transitionTo(RequestStatus.CLOSED, changedByUserId, utcNow, note);
    }

    /**
     * Transitions: RESOLVED -> IN_PROGRESS
     */
    public void reopen(UUID changedByUserId, Instant utcNow, String note) {
        ensureValidTransition(RequestStatus.RESOLVED, RequestStatus.IN_PROGRESS);
        transitionTo(RequestStatus.IN_PROGRESS, changedByUserId, utcNow, note);
    }

    /**
     * Transitions: NEW or REVIEWING -> REJECTED
     */
    public void reject(UUID changedByUserId, Instant utcNow, String note) {
        if (this.status != RequestStatus.NEW && this.status != RequestStatus.REVIEWING) {
            throw new DomainException("Cannot reject a request in '" + this.status + "' status. Only 'NEW' or 'REVIEWING' requests can be rejected.");
        }
        transitionTo(RequestStatus.REJECTED, changedByUserId, utcNow, note);
    }

    /**
     * Transitions: NEW, REVIEWING, or ASSIGNED -> CANCELLED
     */
    public void cancel(UUID changedByUserId, Instant utcNow) {
        if (this.status != RequestStatus.NEW && this.status != RequestStatus.REVIEWING && this.status != RequestStatus.ASSIGNED) {
            throw new DomainException("Cannot cancel a request in '" + this.status + "' status. Only requests in 'NEW', 'REVIEWING', or 'ASSIGNED' status can be cancelled.");
        }
        transitionTo(RequestStatus.CANCELLED, changedByUserId, utcNow, null);
    }

    // ──────────────────────────────────────────────
    // Private Helpers
    // ──────────────────────────────────────────────

    private void transitionTo(RequestStatus newStatus, UUID changedByUserId, Instant utcNow, String note) {
        StatusHistoryEntry entry = new StatusHistoryEntry(null, this, this.status, newStatus, changedByUserId, utcNow, note);
        this.statusHistory.add(entry);
        this.status = newStatus;
        this.updatedAt = utcNow != null ? utcNow : Instant.now();
    }

    private void ensureValidTransition(RequestStatus expectedCurrent, RequestStatus target) {
        if (this.status != expectedCurrent) {
            throw new DomainException("Cannot transition to '" + target + "' from '" + this.status + "'. Expected current status: '" + expectedCurrent + "'.");
        }
    }

    private void ensureNotInTerminalState(String action) {
        if (this.status == RequestStatus.CLOSED || this.status == RequestStatus.REJECTED || this.status == RequestStatus.CANCELLED) {
            throw new DomainException("Cannot " + action + " on a request in terminal state '" + this.status + "'.");
        }
    }

    // Getters
    public UUID getId() { return id; }
    public String getTitle() { return title; }
    public String getDescription() { return description; }
    public UUID getCategoryId() { return categoryId; }
    public UUID getCitizenId() { return citizenId; }
    public RequestStatus getStatus() { return status; }
    public Priority getPriority() { return priority; }
    public UUID getAssignedDepartmentId() { return assignedDepartmentId; }
    public UUID getAssignedEmployeeId() { return assignedEmployeeId; }
    public Location getLocation() { return location; }
    public Instant getCreatedAt() { return createdAt; }
    public Instant getUpdatedAt() { return updatedAt; }
    public List<StatusHistoryEntry> getStatusHistory() { return Collections.unmodifiableList(statusHistory); }
    public List<Comment> getComments() { return Collections.unmodifiableList(comments); }
    public List<Attachment> getAttachments() { return Collections.unmodifiableList(attachments); }
}
