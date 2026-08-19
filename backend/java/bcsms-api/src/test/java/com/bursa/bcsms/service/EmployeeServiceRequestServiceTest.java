package com.bursa.bcsms.service;

import com.bursa.bcsms.domain.entity.ServiceRequest;
import com.bursa.bcsms.domain.enums.Priority;
import com.bursa.bcsms.domain.enums.RequestStatus;
import com.bursa.bcsms.domain.enums.UserRole;
import com.bursa.bcsms.dto.request.ResolveRequestApiRequest;
import com.bursa.bcsms.repository.ServiceRequestRepository;
import com.bursa.bcsms.security.UserPrincipal;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;

import java.time.Instant;
import java.util.Optional;
import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
class EmployeeServiceRequestServiceTest {

    @Mock
    private ServiceRequestRepository serviceRequestRepository;

    @Mock
    private ServiceRequestService serviceRequestService;

    private EmployeeServiceRequestService employeeService;

    @BeforeEach
    void setUp() {
        employeeService = new EmployeeServiceRequestService(serviceRequestRepository, serviceRequestService);
    }

    @Test
    @DisplayName("Employee starts work: ASSIGNED -> IN_PROGRESS")
    void startWork_Success() {
        UUID employeeId = UUID.randomUUID();
        UUID requestId = UUID.randomUUID();

        UserPrincipal employeeUser = new UserPrincipal(employeeId, "emp@bursa.bel.tr", "pass", UserRole.EMPLOYEE, UUID.randomUUID(), true);
        ServiceRequest sr = new ServiceRequest(requestId, "Tree trimming", UUID.randomUUID(), UUID.randomUUID(), Instant.now(), "Desc", null);
        sr.startReview(UUID.randomUUID(), Instant.now());
        sr.assign(UUID.randomUUID(), employeeId, Priority.MEDIUM, UUID.randomUUID(), Instant.now());

        when(serviceRequestRepository.findById(requestId)).thenReturn(Optional.of(sr));

        employeeService.startWork(requestId, employeeUser);

        assertThat(sr.getStatus()).isEqualTo(RequestStatus.IN_PROGRESS);
        verify(serviceRequestRepository).save(sr);
    }

    @Test
    @DisplayName("Employee resolves work: IN_PROGRESS -> RESOLVED")
    void resolveRequest_Success() {
        UUID employeeId = UUID.randomUUID();
        UUID requestId = UUID.randomUUID();

        UserPrincipal employeeUser = new UserPrincipal(employeeId, "emp@bursa.bel.tr", "pass", UserRole.EMPLOYEE, UUID.randomUUID(), true);
        ServiceRequest sr = new ServiceRequest(requestId, "Tree trimming", UUID.randomUUID(), UUID.randomUUID(), Instant.now(), "Desc", null);
        sr.startReview(UUID.randomUUID(), Instant.now());
        sr.assign(UUID.randomUUID(), employeeId, Priority.MEDIUM, UUID.randomUUID(), Instant.now());
        sr.startProgress(employeeId, Instant.now());

        when(serviceRequestRepository.findById(requestId)).thenReturn(Optional.of(sr));

        ResolveRequestApiRequest req = new ResolveRequestApiRequest("Branches trimmed successfully");
        employeeService.resolveRequest(requestId, req, employeeUser);

        assertThat(sr.getStatus()).isEqualTo(RequestStatus.RESOLVED);
        verify(serviceRequestRepository).save(sr);
    }
}
