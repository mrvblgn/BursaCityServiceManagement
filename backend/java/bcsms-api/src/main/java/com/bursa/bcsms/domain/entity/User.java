package com.bursa.bcsms.domain.entity;

import com.bursa.bcsms.common.exception.DomainException;
import com.bursa.bcsms.domain.enums.UserRole;
import jakarta.persistence.*;
import java.time.Instant;
import java.util.UUID;

@Entity
@Table(name = "users")
public class User {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID id;

    @Column(name = "first_name", nullable = false, length = 100)
    private String firstName;

    @Column(name = "last_name", nullable = false, length = 100)
    private String lastName;

    @Column(name = "email", nullable = false, unique = true)
    private String email;

    @Column(name = "phone_number", length = 50)
    private String phoneNumber;

    @Column(name = "password_hash", nullable = false)
    private String passwordHash;

    @Enumerated(EnumType.STRING)
    @Column(name = "role", nullable = false, length = 50)
    private UserRole role;

    @Column(name = "department_id")
    private UUID departmentId;

    @Column(name = "is_active", nullable = false)
    private boolean isActive;

    @Column(name = "created_at", nullable = false)
    private Instant createdAt;

    @Column(name = "updated_at")
    private Instant updatedAt;

    protected User() {
    }

    public User(UUID id, String firstName, String lastName, String email, String phoneNumber,
                String passwordHash, UserRole role, UUID departmentId, Instant createdAt) {
        if (firstName == null || firstName.isBlank()) throw new DomainException("First name is required.");
        if (lastName == null || lastName.isBlank()) throw new DomainException("Last name is required.");
        if (email == null || email.isBlank()) throw new DomainException("Email is required.");
        if (passwordHash == null || passwordHash.isBlank()) throw new DomainException("Password hash is required.");
        if (role == null) throw new DomainException("User role is required.");

        validateDepartmentForRole(role, departmentId);

        this.id = id != null ? id : UUID.randomUUID();
        this.firstName = firstName.trim();
        this.lastName = lastName.trim();
        this.email = email.trim().toLowerCase();
        this.phoneNumber = phoneNumber != null ? phoneNumber.trim() : null;
        this.passwordHash = passwordHash.trim();
        this.role = role;
        this.departmentId = departmentId;
        this.isActive = true;
        this.createdAt = createdAt != null ? createdAt : Instant.now();
    }

    public static User createCitizen(String firstName, String lastName, String email, String phoneNumber, String passwordHash, Instant createdAt) {
        return new User(null, firstName, lastName, email, phoneNumber, passwordHash, UserRole.CITIZEN, null, createdAt);
    }

    public static void validateDepartmentForRole(UserRole role, UUID departmentId) {
        boolean requiresDepartment = role == UserRole.EMPLOYEE || role == UserRole.MANAGER;
        if (requiresDepartment && departmentId == null) {
            throw new DomainException("A " + role + " must be assigned to a department.");
        }
        if (!requiresDepartment && departmentId != null) {
            throw new DomainException("A " + role + " must not be assigned to a department.");
        }
    }

    public UUID getId() {
        return id;
    }

    public String getFirstName() {
        return firstName;
    }

    public String getLastName() {
        return lastName;
    }

    public String getFullName() {
        return firstName + " " + lastName;
    }

    public String getEmail() {
        return email;
    }

    public String getPhoneNumber() {
        return phoneNumber;
    }

    public String getPasswordHash() {
        return passwordHash;
    }

    public UserRole getRole() {
        return role;
    }

    public UUID getDepartmentId() {
        return departmentId;
    }

    public boolean isActive() {
        return isActive;
    }

    public Instant getCreatedAt() {
        return createdAt;
    }

    public Instant getUpdatedAt() {
        return updatedAt;
    }
}
