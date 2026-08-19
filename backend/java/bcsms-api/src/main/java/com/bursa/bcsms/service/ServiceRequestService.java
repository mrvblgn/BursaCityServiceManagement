package com.bursa.bcsms.service;

import com.bursa.bcsms.common.exception.EntityNotFoundException;
import com.bursa.bcsms.common.exception.UnauthorizedAccessException;
import com.bursa.bcsms.common.model.PagedResult;
import com.bursa.bcsms.domain.entity.*;
import com.bursa.bcsms.domain.enums.RequestStatus;
import com.bursa.bcsms.domain.enums.UserRole;
import com.bursa.bcsms.domain.valueobject.Location;
import com.bursa.bcsms.dto.request.CreateServiceRequestApiRequest;
import com.bursa.bcsms.dto.response.*;
import com.bursa.bcsms.repository.CategoryRepository;
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
import java.util.Map;
import java.util.UUID;
import java.util.stream.Collectors;

@Service
public class ServiceRequestService {

    private final ServiceRequestRepository serviceRequestRepository;
    private final CategoryRepository categoryRepository;
    private final UserRepository userRepository;
    private final DepartmentRepository departmentRepository;

    public ServiceRequestService(ServiceRequestRepository serviceRequestRepository,
                                 CategoryRepository categoryRepository,
                                 UserRepository userRepository,
                                 DepartmentRepository departmentRepository) {
        this.serviceRequestRepository = serviceRequestRepository;
        this.categoryRepository = categoryRepository;
        this.userRepository = userRepository;
        this.departmentRepository = departmentRepository;
    }

    @Transactional
    public ServiceRequestDetailDto createServiceRequest(CreateServiceRequestApiRequest request, UserPrincipal currentUser) {
        if (currentUser.getRole() != UserRole.CITIZEN) {
            throw new UnauthorizedAccessException("Only Citizens can submit new service requests.");
        }

        categoryRepository.findById(request.getCategoryId())
                .orElseThrow(() -> new EntityNotFoundException("Category not found with ID: " + request.getCategoryId()));

        Location location = null;
        if (request.getLatitude() != null || request.getLongitude() != null || request.getAddressText() != null) {
            location = new Location(request.getLatitude(), request.getLongitude(), request.getAddressText());
        }

        ServiceRequest serviceRequest = new ServiceRequest(
                null,
                request.getTitle(),
                request.getCategoryId(),
                currentUser.getId(),
                Instant.now(),
                request.getDescription(),
                location
        );

        ServiceRequest saved = serviceRequestRepository.save(serviceRequest);
        return mapToDetailDto(saved);
    }

    @Transactional(readOnly = true)
    public PagedResult<ServiceRequestSummaryDto> getMyRequests(UserPrincipal currentUser, RequestStatus status, int pageNumber, int pageSize) {
        if (currentUser.getRole() != UserRole.CITIZEN) {
            throw new UnauthorizedAccessException("Only Citizens can view their submitted requests.");
        }

        Pageable pageable = PageRequest.of(Math.max(0, pageNumber - 1), pageSize, Sort.by(Sort.Direction.DESC, "createdAt"));
        Page<ServiceRequest> page = serviceRequestRepository.findByCitizenIdAndOptionalStatus(currentUser.getId(), status, pageable);

        List<ServiceRequestSummaryDto> items = mapToSummaryDtos(page.getContent());
        return new PagedResult<>(items, pageNumber, pageSize, page.getTotalElements());
    }

    @Transactional(readOnly = true)
    public ServiceRequestDetailDto getById(UUID id, UserPrincipal currentUser) {
        ServiceRequest request = serviceRequestRepository.findById(id)
                .orElseThrow(() -> new EntityNotFoundException("Service request not found with ID: " + id));

        // Enforce role-based ownership/assignment authorization rules
        if (currentUser.getRole() == UserRole.CITIZEN) {
            if (!request.getCitizenId().equals(currentUser.getId())) {
                throw new UnauthorizedAccessException("You are not authorized to view another citizen's request.");
            }
        } else if (currentUser.getRole() == UserRole.EMPLOYEE) {
            boolean isAssignedToUser = currentUser.getId().equals(request.getAssignedEmployeeId());
            boolean isAssignedToDept = currentUser.getDepartmentId() != null && currentUser.getDepartmentId().equals(request.getAssignedDepartmentId());
            if (!isAssignedToUser && !isAssignedToDept) {
                throw new UnauthorizedAccessException("You are not authorized to view this unassigned service request.");
            }
        }

        return mapToDetailDto(request);
    }

    // DTO Mapping Helpers
    public ServiceRequestDetailDto mapToDetailDto(ServiceRequest sr) {
        String categoryName = categoryRepository.findById(sr.getCategoryId())
                .map(Category::getName).orElse(null);

        String citizenName = userRepository.findById(sr.getCitizenId())
                .map(User::getFullName).orElse(null);

        String departmentName = sr.getAssignedDepartmentId() != null ?
                departmentRepository.findById(sr.getAssignedDepartmentId()).map(Department::getName).orElse(null) : null;

        String employeeName = sr.getAssignedEmployeeId() != null ?
                userRepository.findById(sr.getAssignedEmployeeId()).map(User::getFullName).orElse(null) : null;

        List<StatusHistoryDto> history = sr.getStatusHistory().stream()
                .map(h -> new StatusHistoryDto(h.getId(), h.getFromStatus(), h.getToStatus(), h.getChangedByUserId(), h.getTimestamp(), h.getNote()))
                .collect(Collectors.toList());

        List<CommentDto> comments = sr.getComments().stream()
                .map(c -> new CommentDto(c.getId(), c.getContent(), c.getCreatedByUserId(), c.getCreatedAt()))
                .collect(Collectors.toList());

        List<AttachmentDto> attachments = sr.getAttachments().stream()
                .map(a -> new AttachmentDto(a.getId(), a.getFileName(), a.getContentType(), a.getFileSizeInBytes(), a.getUploadedByUserId(), a.getUploadedAt()))
                .collect(Collectors.toList());

        return new ServiceRequestDetailDto(
                sr.getId(), sr.getTitle(), sr.getDescription(), sr.getCategoryId(), categoryName,
                sr.getStatus(), sr.getPriority(), sr.getCitizenId(), citizenName,
                sr.getAssignedDepartmentId(), departmentName, sr.getAssignedEmployeeId(), employeeName,
                sr.getLocation(), sr.getCreatedAt(), sr.getUpdatedAt(),
                history, comments, attachments
        );
    }

    public List<ServiceRequestSummaryDto> mapToSummaryDtos(List<ServiceRequest> requests) {
        if (requests.isEmpty()) return List.of();

        // Batch lookup references to avoid N+1 queries
        Map<UUID, String> categoryNames = categoryRepository.findAllById(
                requests.stream().map(ServiceRequest::getCategoryId).collect(Collectors.toSet())
        ).stream().collect(Collectors.toMap(Category::getId, Category::getName));

        Map<UUID, String> userNames = userRepository.findAllById(
                requests.stream().flatMap(r -> java.util.stream.Stream.of(r.getCitizenId(), r.getAssignedEmployeeId()))
                        .filter(java.util.Objects::nonNull).collect(Collectors.toSet())
        ).stream().collect(Collectors.toMap(User::getId, User::getFullName));

        Map<UUID, String> departmentNames = departmentRepository.findAllById(
                requests.stream().map(ServiceRequest::getAssignedDepartmentId)
                        .filter(java.util.Objects::nonNull).collect(Collectors.toSet())
        ).stream().collect(Collectors.toMap(Department::getId, Department::getName));

        return requests.stream().map(sr -> new ServiceRequestSummaryDto(
                sr.getId(), sr.getTitle(), sr.getCategoryId(), categoryNames.get(sr.getCategoryId()),
                sr.getStatus(), sr.getPriority(), sr.getCitizenId(), userNames.get(sr.getCitizenId()),
                sr.getAssignedDepartmentId(), departmentNames.get(sr.getAssignedDepartmentId()),
                sr.getAssignedEmployeeId(), userNames.get(sr.getAssignedEmployeeId()),
                sr.getLocation(), sr.getCreatedAt(), sr.getUpdatedAt()
        )).collect(Collectors.toList());
    }
}
