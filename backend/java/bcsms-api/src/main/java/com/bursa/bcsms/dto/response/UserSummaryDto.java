package com.bursa.bcsms.dto.response;

import com.bursa.bcsms.domain.enums.UserRole;
import java.util.UUID;

public class UserSummaryDto {
    private UUID id;
    private String fullName;
    private String email;
    private UserRole role;

    public UserSummaryDto() {
    }

    public UserSummaryDto(UUID id, String fullName, String email, UserRole role) {
        this.id = id;
        this.fullName = fullName;
        this.email = email;
        this.role = role;
    }

    public UUID getId() { return id; }
    public String getFullName() { return fullName; }
    public String getEmail() { return email; }
    public UserRole getRole() { return role; }
}
