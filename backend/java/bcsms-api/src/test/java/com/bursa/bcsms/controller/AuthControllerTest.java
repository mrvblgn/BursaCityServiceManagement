package com.bursa.bcsms.controller;

import com.bursa.bcsms.domain.enums.UserRole;
import com.bursa.bcsms.dto.request.LoginRequest;
import com.bursa.bcsms.dto.request.RegisterRequest;
import com.bursa.bcsms.dto.response.AuthResponse;
import com.bursa.bcsms.service.AuthService;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.mock.mockito.MockBean;
import org.springframework.http.MediaType;
import org.springframework.test.context.ActiveProfiles;
import org.springframework.test.web.servlet.MockMvc;

import java.util.UUID;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@SpringBootTest
@AutoConfigureMockMvc
@ActiveProfiles("test")
class AuthControllerTest {

    @Autowired
    private MockMvc mockMvc;

    @Autowired
    private ObjectMapper objectMapper;

    @MockBean
    private AuthService authService;

    @Test
    @DisplayName("POST /api/auth/register returns 201 Created and JWT response")
    void register_Success() throws Exception {
        RegisterRequest request = new RegisterRequest("Emre", "Can", "emre@example.com", "5551234567", "password123");
        AuthResponse response = new AuthResponse("mocked-jwt", UUID.randomUUID(), "Emre", "Can", "emre@example.com", UserRole.CITIZEN);

        when(authService.register(any())).thenReturn(response);

        mockMvc.perform(post("/api/auth/register")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(request)))
                .andExpect(status().isCreated())
                .andExpect(jsonPath("$.token").value("mocked-jwt"))
                .andExpect(jsonPath("$.email").value("emre@example.com"))
                .andExpect(jsonPath("$.role").value("CITIZEN"));
    }

    @Test
    @DisplayName("POST /api/auth/login returns 200 OK and token")
    void login_Success() throws Exception {
        LoginRequest request = new LoginRequest("emre@example.com", "password123");
        AuthResponse response = new AuthResponse("mocked-jwt-login", UUID.randomUUID(), "Emre", "Can", "emre@example.com", UserRole.CITIZEN);

        when(authService.login(any())).thenReturn(response);

        mockMvc.perform(post("/api/auth/login")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(request)))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.token").value("mocked-jwt-login"));
    }
}
