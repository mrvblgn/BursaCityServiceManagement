package com.bursa.bcsms.service;

import com.bursa.bcsms.common.exception.EntityNotFoundException;
import com.bursa.bcsms.common.exception.UnauthorizedAccessException;
import com.bursa.bcsms.common.model.PagedResult;
import com.bursa.bcsms.domain.entity.ServiceRequest;
import com.bursa.bcsms.domain.enums.RequestStatus;
import com.bursa.bcsms.domain.enums.UserRole;
import com.bursa.bcsms.dto.request.ResolveRequestApiRequest;
import com.bursa.bcsms.dto.response.ServiceRequestSummaryDto;
import com.bursa.bcsms.repository.ServiceRequestRepository;
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
public class EmployeeServiceRequestService {

    private final ServiceRequestRepository serviceRequestRepository;
    private final ServiceRequestService serviceRequestService;

    public EmployeeServiceRequestService(ServiceRequestRepository serviceRequestRepository,
                                         ServiceRequestService serviceRequestService) {
        this.serviceRequestRepository = serviceRequestRepository;
        this.serviceRequestService = serviceRequestService;
    }

    @Transactional(readOnly = true)
    public PagedResult<ServiceRequestSummaryDto> getAssignedRequests(UserPrincipal employeeUser, RequestStatus status,
                                                                    int pageNumber, int pageSize) {
        verifyEmployeeRole(employeeUser);

        Pageable pageable = PageRequest.of(Math.max(0, pageNumber - 1), pageSize, Sort.by(Sort.Direction.DESC, "createdAt"));
        Page<ServiceRequest> page = serviceRequestRepository.findByAssignedEmployeeIdAndOptionalStatus(employeeUser.getId(), status, pageable);

        List<ServiceRequestSummaryDto> items = serviceRequestService.mapToSummaryDtos(page.getContent());
        return new PagedResult<>(items, pageNumber, pageSize, page.getTotalElements());
    }

    @Transactional
    public void startWork(UUID requestId, UserPrincipal employeeUser) {
        verifyEmployeeRole(employeeUser);

        ServiceRequest request = serviceRequestRepository.findById(requestId)
                .orElseThrow(() -> new EntityNotFoundException("Service request not found with ID: " + requestId));

        if (request.getAssignedEmployeeId() != null && !request.getAssignedEmployeeId().equals(employeeUser.getId())) {
            throw new UnauthorizedAccessException("This service request is assigned to a different employee.");
        }

        request.startProgress(employeeUser.getId(), Instant.now());
        serviceRequestRepository.save(request);
    }

    @Transactional
    public void resolveRequest(UUID requestId, ResolveRequestApiRequest resolveRequest, UserPrincipal employeeUser) {
        verifyEmployeeRole(employeeUser);

        ServiceRequest request = serviceRequestRepository.findById(requestId)
                .orElseThrow(() -> new EntityNotFoundException("Service request not found with ID: " + requestId));

        if (request.getAssignedEmployeeId() != null && !request.getAssignedEmployeeId().equals(employeeUser.getId())) {
            throw new UnauthorizedAccessException("This service request is assigned to a different employee.");
        }

        String note = resolveRequest != null ? resolveRequest.getNote() : null;
        request.resolve(employeeUser.getId(), Instant.now(), note);
        serviceRequestRepository.save(request);
    }

    private void verifyEmployeeRole(UserPrincipal employeeUser) {
        if (employeeUser.getRole() != UserRole.EMPLOYEE) {
            throw new UnauthorizedAccessException("Employee access required.");
        }
    }
}
