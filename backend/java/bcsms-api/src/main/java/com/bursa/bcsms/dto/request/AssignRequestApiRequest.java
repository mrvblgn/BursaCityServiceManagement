package com.bursa.bcsms.dto.request;

import com.bursa.bcsms.domain.enums.Priority;
import jakarta.validation.constraints.NotNull;
import java.util.UUID;

public class AssignRequestApiRequest {

    @NotNull(message = "DepartmentId is required")
    private UUID departmentId;

    private UUID employeeId;

    @NotNull(message = "Priority is required")
    private Priority priority;

    public AssignRequestApiRequest() {
    }

    public AssignRequestApiRequest(UUID departmentId, UUID employeeId, Priority priority) {
        this.departmentId = departmentId;
        this.employeeId = employeeId;
        this.priority = priority;
    }

    public UUID getDepartmentId() { return departmentId; }
    public void setDepartmentId(UUID departmentId) { this.departmentId = departmentId; }
    public UUID getEmployeeId() { return employeeId; }
    public void setEmployeeId(UUID employeeId) { this.employeeId = employeeId; }
    public Priority getPriority() { return priority; }
    public void setPriority(Priority priority) { this.priority = priority; }
}
