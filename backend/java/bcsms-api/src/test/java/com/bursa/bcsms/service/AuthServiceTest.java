package com.bursa.bcsms.service;

import com.bursa.bcsms.common.exception.DomainException;
import com.bursa.bcsms.domain.entity.User;
import com.bursa.bcsms.domain.enums.UserRole;
import com.bursa.bcsms.dto.request.RegisterRequest;
import com.bursa.bcsms.dto.response.AuthResponse;
import com.bursa.bcsms.repository.UserRepository;
import com.bursa.bcsms.security.JwtTokenProvider;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.ArgumentCaptor;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.security.authentication.AuthenticationManager;
import org.springframework.security.crypto.password.PasswordEncoder;

import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
class AuthServiceTest {

    @Mock
    private UserRepository userRepository;

    @Mock
    private PasswordEncoder passwordEncoder;

    @Mock
    private AuthenticationManager authenticationManager;

    @Mock
    private JwtTokenProvider tokenProvider;

    private AuthService authService;

    @BeforeEach
    void setUp() {
        authService = new AuthService(userRepository, passwordEncoder, authenticationManager, tokenProvider);
    }

    @Test
    @DisplayName("Registering new citizen saves user and returns token")
    void register_Success() {
        RegisterRequest request = new RegisterRequest("Fatma", "Sahin", "fatma@example.com", "5551112233", "pass123");

        when(userRepository.existsByEmail("fatma@example.com")).thenReturn(false);
        when(passwordEncoder.encode("pass123")).thenReturn("encodedPassword123");
        when(tokenProvider.generateTokenFromUserPrincipal(any())).thenReturn("mocked-jwt-token");

        User savedUser = new User(UUID.randomUUID(), "Fatma", "Sahin", "fatma@example.com", "5551112233", "encodedPassword123", UserRole.CITIZEN, null, null);
        when(userRepository.save(any(User.class))).thenReturn(savedUser);

        AuthResponse response = authService.register(request);

        assertThat(response).isNotNull();
        assertThat(response.getToken()).isEqualTo("mocked-jwt-token");
        assertThat(response.getEmail()).isEqualTo("fatma@example.com");
        assertThat(response.getRole()).isEqualTo(UserRole.CITIZEN);

        ArgumentCaptor<User> userCaptor = ArgumentCaptor.forClass(User.class);
        verify(userRepository).save(userCaptor.capture());
        assertThat(userCaptor.getValue().getEmail()).isEqualTo("fatma@example.com");
        assertThat(userCaptor.getValue().getRole()).isEqualTo(UserRole.CITIZEN);
    }

    @Test
    @DisplayName("Registering existing email throws DomainException")
    void register_DuplicateEmail_ThrowsDomainException() {
        RegisterRequest request = new RegisterRequest("Fatma", "Sahin", "fatma@example.com", "5551112233", "pass123");

        when(userRepository.existsByEmail("fatma@example.com")).thenReturn(true);

        assertThatThrownBy(() -> authService.register(request))
                .isInstanceOf(DomainException.class)
                .hasMessageContaining("Email is already registered");

        verify(userRepository, never()).save(any());
    }
}
