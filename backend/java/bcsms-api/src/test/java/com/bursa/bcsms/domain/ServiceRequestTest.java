package com.bursa.bcsms.domain;

import com.bursa.bcsms.common.exception.DomainException;
import com.bursa.bcsms.domain.entity.ServiceRequest;
import com.bursa.bcsms.domain.enums.Priority;
import com.bursa.bcsms.domain.enums.RequestStatus;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import java.time.Instant;
import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;

class ServiceRequestTest {

    private UUID categoryId;
    private UUID citizenId;
    private UUID managerId;
    private UUID employeeId;
    private UUID departmentId;
    private Instant now;

    @BeforeEach
    void setUp() {
        categoryId = UUID.randomUUID();
        citizenId = UUID.randomUUID();
        managerId = UUID.randomUUID();
        employeeId = UUID.randomUUID();
        departmentId = UUID.randomUUID();
        now = Instant.now();
    }

    @Test
    @DisplayName("Creation initializes status to NEW")
    void createServiceRequest_InitialStateIsNew() {
        ServiceRequest sr = new ServiceRequest(null, "Pothole fix", categoryId, citizenId, now, "Deep hole on Main street", null);

        assertThat(sr.getStatus()).isEqualTo(RequestStatus.NEW);
        assertThat(sr.getTitle()).isEqualTo("Pothole fix");
        assertThat(sr.getCitizenId()).isEqualTo(citizenId);
    }

    @Test
    @DisplayName("Creation throws DomainException if title is empty")
    void createServiceRequest_EmptyTitle_ThrowsDomainException() {
        assertThatThrownBy(() -> new ServiceRequest(null, "", categoryId, citizenId, now, null, null))
                .isInstanceOf(DomainException.class)
                .hasMessageContaining("title is required");
    }

    @Test
    @DisplayName("Full lifecycle happy path: NEW -> REVIEWING -> ASSIGNED -> IN_PROGRESS -> RESOLVED -> CLOSED")
    void fullLifecycleTransition_HappyPath() {
        ServiceRequest sr = new ServiceRequest(null, "Water leak", categoryId, citizenId, now, "Leaking pipe", null);

        // 1. Start Review
        sr.startReview(managerId, now);
        assertThat(sr.getStatus()).isEqualTo(RequestStatus.REVIEWING);
        assertThat(sr.getStatusHistory()).hasSize(1);
        assertThat(sr.getStatusHistory().get(0).getFromStatus()).isEqualTo(RequestStatus.NEW);
        assertThat(sr.getStatusHistory().get(0).getToStatus()).isEqualTo(RequestStatus.REVIEWING);

        // 2. Assign
        sr.assign(departmentId, employeeId, Priority.HIGH, managerId, now);
        assertThat(sr.getStatus()).isEqualTo(RequestStatus.ASSIGNED);
        assertThat(sr.getAssignedDepartmentId()).isEqualTo(departmentId);
        assertThat(sr.getAssignedEmployeeId()).isEqualTo(employeeId);
        assertThat(sr.getPriority()).isEqualTo(Priority.HIGH);

        // 3. Start Work
        sr.startProgress(employeeId, now);
        assertThat(sr.getStatus()).isEqualTo(RequestStatus.IN_PROGRESS);

        // 4. Resolve
        sr.resolve(employeeId, now, "Fixed pipe coupling");
        assertThat(sr.getStatus()).isEqualTo(RequestStatus.RESOLVED);

        // 5. Close
        sr.close(managerId, now, "Confirmed by citizen");
        assertThat(sr.getStatus()).isEqualTo(RequestStatus.CLOSED);
    }

    @Test
    @DisplayName("Invalid state transition throws DomainException")
    void invalidTransition_ThrowsDomainException() {
        ServiceRequest sr = new ServiceRequest(null, "Street light broken", categoryId, citizenId, now, "Dark street", null);

        // Cannot resolve directly from NEW
        assertThatThrownBy(() -> sr.resolve(employeeId, now, "Done"))
                .isInstanceOf(DomainException.class)
                .hasMessageContaining("Cannot transition to 'RESOLVED' from 'NEW'");
    }

    @Test
    @DisplayName("Reopen transition: RESOLVED -> IN_PROGRESS")
    void reopenTransition_ResolvedToInProgress() {
        ServiceRequest sr = new ServiceRequest(null, "Garbage collection", categoryId, citizenId, now, "Missed bins", null);
        sr.startReview(managerId, now);
        sr.assign(departmentId, employeeId, Priority.MEDIUM, managerId, now);
        sr.startProgress(employeeId, now);
        sr.resolve(employeeId, now, "Collected");

        // Reopen
        sr.reopen(managerId, now, "Bins were missed again");
        assertThat(sr.getStatus()).isEqualTo(RequestStatus.IN_PROGRESS);
    }

    @Test
    @DisplayName("Rejection allowed from NEW or REVIEWING")
    void reject_AllowedFromNewOrReviewing() {
        ServiceRequest sr = new ServiceRequest(null, "Invalid request", categoryId, citizenId, now, "Duplicate", null);
        sr.reject(managerId, now, "Duplicate issue");
        assertThat(sr.getStatus()).isEqualTo(RequestStatus.REJECTED);
    }

    @Test
    @DisplayName("Rejection throws DomainException if request is already IN_PROGRESS")
    void reject_FromInProgress_ThrowsDomainException() {
        ServiceRequest sr = new ServiceRequest(null, "Test request", categoryId, citizenId, now, "Desc", null);
        sr.startReview(managerId, now);
        sr.assign(departmentId, employeeId, Priority.LOW, managerId, now);
        sr.startProgress(employeeId, now);

        assertThatThrownBy(() -> sr.reject(managerId, now, "Not valid"))
                .isInstanceOf(DomainException.class)
                .hasMessageContaining("Cannot reject a request in 'IN_PROGRESS' status");
    }

    @Test
    @DisplayName("Cancellation allowed from NEW, REVIEWING, or ASSIGNED")
    void cancel_AllowedFromEligibleStates() {
        ServiceRequest sr = new ServiceRequest(null, "Cancel test", categoryId, citizenId, now, "Desc", null);
        sr.cancel(citizenId, now);
        assertThat(sr.getStatus()).isEqualTo(RequestStatus.CANCELLED);
    }
}
