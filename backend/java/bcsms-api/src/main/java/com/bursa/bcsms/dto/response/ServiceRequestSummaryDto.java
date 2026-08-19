package com.bursa.bcsms.dto.response;

import com.bursa.bcsms.domain.enums.Priority;
import com.bursa.bcsms.domain.enums.RequestStatus;
import com.bursa.bcsms.domain.valueobject.Location;

import java.time.Instant;
import java.util.UUID;

public class ServiceRequestSummaryDto {
    private UUID id;
    private String title;
    private UUID categoryId;
    private String categoryName;
    private RequestStatus status;
    private Priority priority;
    private UUID citizenId;
    private String citizenName;
    private UUID assignedDepartmentId;
    private String assignedDepartmentName;
    private UUID assignedEmployeeId;
    private String assignedEmployeeName;
    private Location location;
    private Instant createdAt;
    private Instant updatedAt;

    public ServiceRequestSummaryDto() {
    }

    public ServiceRequestSummaryDto(UUID id, String title, UUID categoryId, String categoryName,
                                   RequestStatus status, Priority priority, UUID citizenId, String citizenName,
                                   UUID assignedDepartmentId, String assignedDepartmentName,
                                   UUID assignedEmployeeId, String assignedEmployeeName,
                                   Location location, Instant createdAt, Instant updatedAt) {
        this.id = id;
        this.title = title;
        this.categoryId = categoryId;
        this.categoryName = categoryName;
        this.status = status;
        this.priority = priority;
        this.citizenId = citizenId;
        this.citizenName = citizenName;
        this.assignedDepartmentId = assignedDepartmentId;
        this.assignedDepartmentName = assignedDepartmentName;
        this.assignedEmployeeId = assignedEmployeeId;
        this.assignedEmployeeName = assignedEmployeeName;
        this.location = location;
        this.createdAt = createdAt;
        this.updatedAt = updatedAt;
    }

    public UUID getId() { return id; }
    public String getTitle() { return title; }
    public UUID getCategoryId() { return categoryId; }
    public String getCategoryName() { return categoryName; }
    public RequestStatus getStatus() { return status; }
    public Priority getPriority() { return priority; }
    public UUID getCitizenId() { return citizenId; }
    public String getCitizenName() { return citizenName; }
    public UUID getAssignedDepartmentId() { return assignedDepartmentId; }
    public String getAssignedDepartmentName() { return assignedDepartmentName; }
    public UUID getAssignedEmployeeId() { return assignedEmployeeId; }
    public String getAssignedEmployeeName() { return assignedEmployeeName; }
    public Location getLocation() { return location; }
    public Instant getCreatedAt() { return createdAt; }
    public Instant getUpdatedAt() { return updatedAt; }
}
