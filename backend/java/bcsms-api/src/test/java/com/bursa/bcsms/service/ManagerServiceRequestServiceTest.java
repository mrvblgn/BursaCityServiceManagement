package com.bursa.bcsms.service;

import com.bursa.bcsms.domain.entity.Department;
import com.bursa.bcsms.domain.entity.ServiceRequest;
import com.bursa.bcsms.domain.enums.Priority;
import com.bursa.bcsms.domain.enums.RequestStatus;
import com.bursa.bcsms.domain.enums.UserRole;
import com.bursa.bcsms.dto.request.AssignRequestApiRequest;
import com.bursa.bcsms.repository.DepartmentRepository;
import com.bursa.bcsms.repository.ServiceRequestRepository;
import com.bursa.bcsms.repository.UserRepository;
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
class ManagerServiceRequestServiceTest {

    @Mock
    private ServiceRequestRepository serviceRequestRepository;

    @Mock
    private DepartmentRepository departmentRepository;

    @Mock
    private UserRepository userRepository;

    @Mock
    private ServiceRequestService serviceRequestService;

    private ManagerServiceRequestService managerService;

    @BeforeEach
    void setUp() {
        managerService = new ManagerServiceRequestService(serviceRequestRepository, departmentRepository, userRepository, serviceRequestService);
    }

    @Test
    @DisplayName("Manager starts review: NEW -> REVIEWING")
    void startReview_Success() {
        UUID managerId = UUID.randomUUID();
        UUID requestId = UUID.randomUUID();

        UserPrincipal managerUser = new UserPrincipal(managerId, "manager@bursa.bel.tr", "pass", UserRole.MANAGER, UUID.randomUUID(), true);
        ServiceRequest sr = new ServiceRequest(requestId, "Water leak", UUID.randomUUID(), UUID.randomUUID(), Instant.now(), "Desc", null);

        when(serviceRequestRepository.findById(requestId)).thenReturn(Optional.of(sr));

        managerService.startReview(requestId, managerUser);

        assertThat(sr.getStatus()).isEqualTo(RequestStatus.REVIEWING);
        verify(serviceRequestRepository).save(sr);
    }

    @Test
    @DisplayName("Manager assigns request: REVIEWING -> ASSIGNED")
    void assignRequest_Success() {
        UUID managerId = UUID.randomUUID();
        UUID requestId = UUID.randomUUID();
        UUID deptId = UUID.randomUUID();

        UserPrincipal managerUser = new UserPrincipal(managerId, "manager@bursa.bel.tr", "pass", UserRole.MANAGER, deptId, true);
        ServiceRequest sr = new ServiceRequest(requestId, "Water leak", UUID.randomUUID(), UUID.randomUUID(), Instant.now(), "Desc", null);
        sr.startReview(managerId, Instant.now());

        when(serviceRequestRepository.findById(requestId)).thenReturn(Optional.of(sr));
        when(departmentRepository.findById(deptId)).thenReturn(Optional.of(new Department(deptId, "Water Works", "WW", "Desc", Instant.now())));

        AssignRequestApiRequest assignReq = new AssignRequestApiRequest(deptId, null, Priority.URGENT);
        managerService.assignRequest(requestId, assignReq, managerUser);

        assertThat(sr.getStatus()).isEqualTo(RequestStatus.ASSIGNED);
        assertThat(sr.getAssignedDepartmentId()).isEqualTo(deptId);
        assertThat(sr.getPriority()).isEqualTo(Priority.URGENT);
        verify(serviceRequestRepository).save(sr);
    }
}
