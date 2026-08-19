package com.bursa.bcsms.controller;

import com.bursa.bcsms.common.model.PagedResult;
import com.bursa.bcsms.domain.enums.RequestStatus;
import com.bursa.bcsms.dto.request.CreateServiceRequestApiRequest;
import com.bursa.bcsms.dto.response.ServiceRequestDetailDto;
import com.bursa.bcsms.dto.response.ServiceRequestSummaryDto;
import com.bursa.bcsms.security.UserPrincipal;
import com.bursa.bcsms.service.ServiceRequestService;
import jakarta.validation.Valid;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.security.core.annotation.AuthenticationPrincipal;
import org.springframework.web.bind.annotation.*;

import java.util.UUID;

@RestController
@RequestMapping("/api/service-requests")
public class ServiceRequestController {

    private final ServiceRequestService serviceRequestService;

    public ServiceRequestController(ServiceRequestService serviceRequestService) {
        this.serviceRequestService = serviceRequestService;
    }

    @PostMapping
    public ResponseEntity<ServiceRequestDetailDto> create(
            @Valid @RequestBody CreateServiceRequestApiRequest request,
            @AuthenticationPrincipal UserPrincipal currentUser) {
        ServiceRequestDetailDto created = serviceRequestService.createServiceRequest(request, currentUser);
        return ResponseEntity.status(HttpStatus.CREATED).body(created);
    }

    @GetMapping("/my")
    public ResponseEntity<PagedResult<ServiceRequestSummaryDto>> getMyRequests(
            @RequestParam(required = false) RequestStatus status,
            @RequestParam(defaultValue = "1") int pageNumber,
            @RequestParam(defaultValue = "10") int pageSize,
            @AuthenticationPrincipal UserPrincipal currentUser) {
        PagedResult<ServiceRequestSummaryDto> result = serviceRequestService.getMyRequests(currentUser, status, pageNumber, pageSize);
        return ResponseEntity.ok(result);
    }

    @GetMapping("/{id}")
    public ResponseEntity<ServiceRequestDetailDto> getById(
            @PathVariable UUID id,
            @AuthenticationPrincipal UserPrincipal currentUser) {
        ServiceRequestDetailDto detail = serviceRequestService.getById(id, currentUser);
        return ResponseEntity.ok(detail);
    }
}
