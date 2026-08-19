package com.bursa.bcsms.domain;

import com.bursa.bcsms.common.exception.DomainException;
import com.bursa.bcsms.domain.entity.User;
import com.bursa.bcsms.domain.enums.UserRole;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import java.time.Instant;
import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;

class UserTest {

    @Test
    @DisplayName("Citizen user created successfully without department")
    void createCitizen_Success() {
        User user = User.createCitizen("Ahmet", "Yilmaz", "ahmet@example.com", "5551234567", "hash123", Instant.now());

        assertThat(user.getRole()).isEqualTo(UserRole.CITIZEN);
        assertThat(user.getDepartmentId()).isNull();
        assertThat(user.getFullName()).isEqualTo("Ahmet Yilmaz");
    }

    @Test
    @DisplayName("Employee user requires a department")
    void createEmployee_WithoutDepartment_ThrowsDomainException() {
        assertThatThrownBy(() -> new User(UUID.randomUUID(), "Mehmet", "Kaya", "mehmet@bursa.bel.tr",
                "5559876543", "hash123", UserRole.EMPLOYEE, null, Instant.now()))
                .isInstanceOf(DomainException.class)
                .hasMessageContaining("EMPLOYEE must be assigned to a department");
    }

    @Test
    @DisplayName("Citizen user cannot have a department assigned")
    void createCitizen_WithDepartment_ThrowsDomainException() {
        assertThatThrownBy(() -> new User(UUID.randomUUID(), "Ayse", "Demir", "ayse@example.com",
                "5551112233", "hash123", UserRole.CITIZEN, UUID.randomUUID(), Instant.now()))
                .isInstanceOf(DomainException.class)
                .hasMessageContaining("CITIZEN must not be assigned to a department");
    }
}
