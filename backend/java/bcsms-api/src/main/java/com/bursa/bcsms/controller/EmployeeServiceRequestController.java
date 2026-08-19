package com.bursa.bcsms.controller;

import com.bursa.bcsms.common.model.PagedResult;
import com.bursa.bcsms.domain.enums.RequestStatus;
import com.bursa.bcsms.dto.request.ResolveRequestApiRequest;
import com.bursa.bcsms.dto.response.ServiceRequestSummaryDto;
import com.bursa.bcsms.security.UserPrincipal;
import com.bursa.bcsms.service.EmployeeServiceRequestService;
import jakarta.validation.Valid;
import org.springframework.http.ResponseEntity;
import org.springframework.security.core.annotation.AuthenticationPrincipal;
import org.springframework.web.bind.annotation.*;

import java.util.UUID;

@RestController
@RequestMapping("/api/employee/service-requests")
public class EmployeeServiceRequestController {

    private final EmployeeServiceRequestService employeeService;

    public EmployeeServiceRequestController(EmployeeServiceRequestService employeeService) {
        this.employeeService = employeeService;
    }

    @GetMapping
    public ResponseEntity<PagedResult<ServiceRequestSummaryDto>> getMyAssignedRequests(
            @RequestParam(required = false) RequestStatus status,
            @RequestParam(defaultValue = "1") int pageNumber,
            @RequestParam(defaultValue = "10") int pageSize,
            @AuthenticationPrincipal UserPrincipal currentUser) {
        PagedResult<ServiceRequestSummaryDto> result = employeeService.getAssignedRequests(
                currentUser, status, pageNumber, pageSize);
        return ResponseEntity.ok(result);
    }

    @PostMapping("/{id}/start")
    public ResponseEntity<Void> startWork(
            @PathVariable UUID id,
            @AuthenticationPrincipal UserPrincipal currentUser) {
        employeeService.startWork(id, currentUser);
        return ResponseEntity.noContent().build();
    }

    @PostMapping("/{id}/resolve")
    public ResponseEntity<Void> resolve(
            @PathVariable UUID id,
            @Valid @RequestBody(required = false) ResolveRequestApiRequest request,
            @AuthenticationPrincipal UserPrincipal currentUser) {
        employeeService.resolveRequest(id, request, currentUser);
        return ResponseEntity.noContent().build();
    }
}
