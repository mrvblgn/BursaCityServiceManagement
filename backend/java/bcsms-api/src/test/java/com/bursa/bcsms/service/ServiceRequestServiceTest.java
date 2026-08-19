package com.bursa.bcsms.service;

import com.bursa.bcsms.common.exception.UnauthorizedAccessException;
import com.bursa.bcsms.domain.entity.Category;
import com.bursa.bcsms.domain.entity.ServiceRequest;
import com.bursa.bcsms.domain.enums.UserRole;
import com.bursa.bcsms.dto.request.CreateServiceRequestApiRequest;
import com.bursa.bcsms.dto.response.ServiceRequestDetailDto;
import com.bursa.bcsms.repository.CategoryRepository;
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
import static org.assertj.core.api.Assertions.assertThatThrownBy;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
class ServiceRequestServiceTest {

    @Mock
    private ServiceRequestRepository serviceRequestRepository;

    @Mock
    private CategoryRepository categoryRepository;

    @Mock
    private UserRepository userRepository;

    @Mock
    private DepartmentRepository departmentRepository;

    private ServiceRequestService serviceRequestService;

    @BeforeEach
    void setUp() {
        serviceRequestService = new ServiceRequestService(serviceRequestRepository, categoryRepository, userRepository, departmentRepository);
    }

    @Test
    @DisplayName("Citizen creates service request successfully")
    void createServiceRequest_CitizenRole_Success() {
        UUID citizenId = UUID.randomUUID();
        UUID categoryId = UUID.randomUUID();

        UserPrincipal citizenUser = new UserPrincipal(citizenId, "citizen@example.com", "pass", UserRole.CITIZEN, null, true);
        CreateServiceRequestApiRequest request = new CreateServiceRequestApiRequest("Fix bench", categoryId, "Broken wooden bench in park", 40.188, 29.061, "Bursa Park");

        when(categoryRepository.findById(categoryId)).thenReturn(Optional.of(new Category(categoryId, "Parks", "Park maintenance", true, Instant.now())));

        ServiceRequest savedSr = new ServiceRequest(UUID.randomUUID(), "Fix bench", categoryId, citizenId, Instant.now(), "Broken wooden bench in park", null);
        when(serviceRequestRepository.save(any(ServiceRequest.class))).thenReturn(savedSr);

        ServiceRequestDetailDto result = serviceRequestService.createServiceRequest(request, citizenUser);

        assertThat(result).isNotNull();
        assertThat(result.getTitle()).isEqualTo("Fix bench");
        verify(serviceRequestRepository).save(any(ServiceRequest.class));
    }

    @Test
    @DisplayName("Employee trying to submit request throws UnauthorizedAccessException")
    void createServiceRequest_EmployeeRole_ThrowsUnauthorizedAccessException() {
        UserPrincipal employeeUser = new UserPrincipal(UUID.randomUUID(), "emp@bursa.bel.tr", "pass", UserRole.EMPLOYEE, UUID.randomUUID(), true);
        CreateServiceRequestApiRequest request = new CreateServiceRequestApiRequest("Fix bench", UUID.randomUUID(), "Desc", null, null, null);

        assertThatThrownBy(() -> serviceRequestService.createServiceRequest(request, employeeUser))
                .isInstanceOf(UnauthorizedAccessException.class)
                .hasMessageContaining("Only Citizens can submit new service requests");

        verify(serviceRequestRepository, never()).save(any());
    }

    @Test
    @DisplayName("Citizen attempting to view another citizen's request throws UnauthorizedAccessException")
    void getById_OtherCitizenRequest_ThrowsUnauthorizedAccessException() {
        UUID citizen1Id = UUID.randomUUID();
        UUID citizen2Id = UUID.randomUUID();
        UUID requestId = UUID.randomUUID();

        UserPrincipal citizen1User = new UserPrincipal(citizen1Id, "citizen1@example.com", "pass", UserRole.CITIZEN, null, true);
        ServiceRequest srBelongingToCitizen2 = new ServiceRequest(requestId, "Title", UUID.randomUUID(), citizen2Id, Instant.now(), "Desc", null);

        when(serviceRequestRepository.findById(requestId)).thenReturn(Optional.of(srBelongingToCitizen2));

        assertThatThrownBy(() -> serviceRequestService.getById(requestId, citizen1User))
                .isInstanceOf(UnauthorizedAccessException.class)
                .hasMessageContaining("You are not authorized to view another citizen's request");
    }
}
