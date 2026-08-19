package com.bursa.bcsms.service;

import com.bursa.bcsms.common.exception.EntityNotFoundException;
import com.bursa.bcsms.common.exception.UnauthorizedAccessException;
import com.bursa.bcsms.common.model.PagedResult;
import com.bursa.bcsms.domain.entity.ServiceRequest;
import com.bursa.bcsms.domain.enums.Priority;
import com.bursa.bcsms.domain.enums.RequestStatus;
import com.bursa.bcsms.domain.enums.UserRole;
import com.bursa.bcsms.dto.request.AssignRequestApiRequest;
import com.bursa.bcsms.dto.response.ServiceRequestDetailDto;
import com.bursa.bcsms.dto.response.ServiceRequestSummaryDto;
import com.bursa.bcsms.repository.DepartmentRepository;
import com.bursa.bcsms.repository.ServiceRequestRepository;
import com.bursa.bcsms.repository.UserRepository;
import com.bursa.bcsms.security.UserPrincipal;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Pageable;
import org.springframework.data.domain.Sort;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.Instant;
import java.util.List;
import java.util.UUID;

@Service
public class ManagerServiceRequestService {

    private final ServiceRequestRepository serviceRequestRepository;
    private final DepartmentRepository departmentRepository;
    private final UserRepository userRepository;
    private final ServiceRequestService serviceRequestService;

    public ManagerServiceRequestService(ServiceRequestRepository serviceRequestRepository,
                                        DepartmentRepository departmentRepository,
                                        UserRepository userRepository,
                                        ServiceRequestService serviceRequestService) {
        this.serviceRequestRepository = serviceRequestRepository;
        this.departmentRepository = departmentRepository;
        this.userRepository = userRepository;
        this.serviceRequestService = serviceRequestService;
    }

    @Transactional(readOnly = true)
    public PagedResult<ServiceRequestSummaryDto> getMunicipalRequests(UserPrincipal managerUser, RequestStatus status,
                                                                      UUID categoryId, UUID departmentId, Priority priority,
                                                                      int pageNumber, int pageSize) {
        verifyManagerRole(managerUser);

        Pageable pageable = PageRequest.of(Math.max(0, pageNumber - 1), pageSize, Sort.by(Sort.Direction.DESC, "createdAt"));
        Page<ServiceRequest> page = serviceRequestRepository.findWithFilters(status, categoryId, departmentId, priority, pageable);

        List<ServiceRequestSummaryDto> items = serviceRequestService.mapToSummaryDtos(page.getContent());
        return new PagedResult<>(items, pageNumber, pageSize, page.getTotalElements());
    }

    @Transactional
    public void startReview(UUID requestId, UserPrincipal managerUser) {
        verifyManagerRole(managerUser);

        ServiceRequest request = serviceRequestRepository.findById(requestId)
                .orElseThrow(() -> new EntityNotFoundException("Service request not found with ID: " + requestId));

        request.startReview(managerUser.getId(), Instant.now());
        serviceRequestRepository.save(request);
    }

    @Transactional
    public void assignRequest(UUID requestId, AssignRequestApiRequest assignRequest, UserPrincipal managerUser) {
        verifyManagerRole(managerUser);

        ServiceRequest request = serviceRequestRepository.findById(requestId)
                .orElseThrow(() -> new EntityNotFoundException("Service request not found with ID: " + requestId));

        departmentRepository.findById(assignRequest.getDepartmentId())
                .orElseThrow(() -> new EntityNotFoundException("Department not found with ID: " + assignRequest.getDepartmentId()));

        if (assignRequest.getEmployeeId() != null) {
            var employee = userRepository.findById(assignRequest.getEmployeeId())
                    .orElseThrow(() -> new EntityNotFoundException("Employee not found with ID: " + assignRequest.getEmployeeId()));
            if (employee.getRole() != UserRole.EMPLOYEE) {
                throw new IllegalArgumentException("Assigned user must have the EMPLOYEE role.");
            }
        }

        request.assign(
                assignRequest.getDepartmentId(),
                assignRequest.getEmployeeId(),
                assignRequest.getPriority(),
                managerUser.getId(),
                Instant.now()
        );

        serviceRequestRepository.save(request);
    }

    private void verifyManagerRole(UserPrincipal managerUser) {
        if (managerUser.getRole() != UserRole.MANAGER && managerUser.getRole() != UserRole.ADMIN) {
            throw new UnauthorizedAccessException("Manager access required.");
        }
    }
}
