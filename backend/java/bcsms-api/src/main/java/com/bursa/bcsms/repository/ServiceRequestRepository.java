package com.bursa.bcsms.repository;

import com.bursa.bcsms.domain.entity.ServiceRequest;
import com.bursa.bcsms.domain.enums.Priority;
import com.bursa.bcsms.domain.enums.RequestStatus;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;

import java.util.UUID;

@Repository
public interface ServiceRequestRepository extends JpaRepository<ServiceRequest, UUID> {

    @Query("SELECT sr FROM ServiceRequest sr WHERE sr.citizenId = :citizenId AND (:status IS NULL OR sr.status = :status)")
    Page<ServiceRequest> findByCitizenIdAndOptionalStatus(@Param("citizenId") UUID citizenId,
                                                          @Param("status") RequestStatus status,
                                                          Pageable pageable);

    @Query("SELECT sr FROM ServiceRequest sr WHERE sr.assignedEmployeeId = :employeeId AND (:status IS NULL OR sr.status = :status)")
    Page<ServiceRequest> findByAssignedEmployeeIdAndOptionalStatus(@Param("employeeId") UUID employeeId,
                                                                   @Param("status") RequestStatus status,
                                                                   Pageable pageable);

    @Query("SELECT sr FROM ServiceRequest sr WHERE " +
           "(:status IS NULL OR sr.status = :status) AND " +
           "(:categoryId IS NULL OR sr.categoryId = :categoryId) AND " +
           "(:departmentId IS NULL OR sr.assignedDepartmentId = :departmentId) AND " +
           "(:priority IS NULL OR sr.priority = :priority)")
    Page<ServiceRequest> findWithFilters(@Param("status") RequestStatus status,
                                         @Param("categoryId") UUID categoryId,
                                         @Param("departmentId") UUID departmentId,
                                         @Param("priority") Priority priority,
                                         Pageable pageable);
}
