package com.bursa.bcsms.dto.response;

import com.bursa.bcsms.domain.enums.UserRole;
import java.util.UUID;

public class AuthResponse {
    private String token;
    private UUID userId;
    private String firstName;
    private String lastName;
    private String email;
    private UserRole role;

    public AuthResponse() {
    }

    public AuthResponse(String token, UUID userId, String firstName, String lastName, String email, UserRole role) {
        this.token = token;
        this.userId = userId;
        this.firstName = firstName;
        this.lastName = lastName;
        this.email = email;
        this.role = role;
    }

    public String getToken() { return token; }
    public UUID getUserId() { return userId; }
    public String getFirstName() { return firstName; }
    public String getLastName() { return lastName; }
    public String getEmail() { return email; }
    public UserRole getRole() { return role; }
}
