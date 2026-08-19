package com.bursa.bcsms.controller;

import com.bursa.bcsms.common.model.PagedResult;
import com.bursa.bcsms.domain.enums.Priority;
import com.bursa.bcsms.domain.enums.RequestStatus;
import com.bursa.bcsms.dto.request.AssignRequestApiRequest;
import com.bursa.bcsms.dto.response.ServiceRequestSummaryDto;
import com.bursa.bcsms.security.UserPrincipal;
import com.bursa.bcsms.service.ManagerServiceRequestService;
import jakarta.validation.Valid;
import org.springframework.http.ResponseEntity;
import org.springframework.security.core.annotation.AuthenticationPrincipal;
import org.springframework.web.bind.annotation.*;

import java.util.UUID;

@RestController
@RequestMapping("/api/manager/service-requests")
public class ManagerServiceRequestController {

    private final ManagerServiceRequestService managerService;

    public ManagerServiceRequestController(ManagerServiceRequestService managerService) {
        this.managerService = managerService;
    }

    @GetMapping
    public ResponseEntity<PagedResult<ServiceRequestSummaryDto>> getMunicipalRequests(
            @RequestParam(required = false) RequestStatus status,
            @RequestParam(required = false) UUID categoryId,
            @RequestParam(required = false) UUID departmentId,
            @RequestParam(required = false) Priority priority,
            @RequestParam(defaultValue = "1") int pageNumber,
            @RequestParam(defaultValue = "10") int pageSize,
            @AuthenticationPrincipal UserPrincipal currentUser) {
        PagedResult<ServiceRequestSummaryDto> result = managerService.getMunicipalRequests(
                currentUser, status, categoryId, departmentId, priority, pageNumber, pageSize);
        return ResponseEntity.ok(result);
    }

    @PostMapping("/{id}/review")
    public ResponseEntity<Void> startReview(
            @PathVariable UUID id,
            @AuthenticationPrincipal UserPrincipal currentUser) {
        managerService.startReview(id, currentUser);
        return ResponseEntity.noContent().build();
    }

    @PostMapping("/{id}/assign")
    public ResponseEntity<Void> assign(
            @PathVariable UUID id,
            @Valid @RequestBody AssignRequestApiRequest request,
            @AuthenticationPrincipal UserPrincipal currentUser) {
        managerService.assignRequest(id, request, currentUser);
        return ResponseEntity.noContent().build();
    }
}
