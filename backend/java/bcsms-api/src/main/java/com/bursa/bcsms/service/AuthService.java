package com.bursa.bcsms.service;

import com.bursa.bcsms.common.exception.DomainException;
import com.bursa.bcsms.domain.entity.User;
import com.bursa.bcsms.dto.request.LoginRequest;
import com.bursa.bcsms.dto.request.RegisterRequest;
import com.bursa.bcsms.dto.response.AuthResponse;
import com.bursa.bcsms.repository.UserRepository;
import com.bursa.bcsms.security.JwtTokenProvider;
import com.bursa.bcsms.security.UserPrincipal;
import org.springframework.security.authentication.AuthenticationManager;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.Authentication;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.Instant;

@Service
public class AuthService {

    private final UserRepository userRepository;
    private final PasswordEncoder passwordEncoder;
    private final AuthenticationManager authenticationManager;
    private final JwtTokenProvider tokenProvider;

    public AuthService(UserRepository userRepository, PasswordEncoder passwordEncoder,
                       AuthenticationManager authenticationManager, JwtTokenProvider tokenProvider) {
        this.userRepository = userRepository;
        this.passwordEncoder = passwordEncoder;
        this.authenticationManager = authenticationManager;
        this.tokenProvider = tokenProvider;
    }

    @Transactional
    public AuthResponse register(RegisterRequest request) {
        if (userRepository.existsByEmail(request.getEmail().toLowerCase())) {
            throw new DomainException("Email is already registered.");
        }

        String encodedPassword = passwordEncoder.encode(request.getPassword());
        User citizen = User.createCitizen(
                request.getFirstName(),
                request.getLastName(),
                request.getEmail(),
                request.getPhoneNumber(),
                encodedPassword,
                Instant.now()
        );

        User savedUser = userRepository.save(citizen);

        UserPrincipal userPrincipal = UserPrincipal.create(savedUser);
        String token = tokenProvider.generateTokenFromUserPrincipal(userPrincipal);

        return new AuthResponse(
                token,
                savedUser.getId(),
                savedUser.getFirstName(),
                savedUser.getLastName(),
                savedUser.getEmail(),
                savedUser.getRole()
        );
    }

    @Transactional(readOnly = true)
    public AuthResponse login(LoginRequest request) {
        Authentication authentication = authenticationManager.authenticate(
                new UsernamePasswordAuthenticationToken(request.getEmail().toLowerCase(), request.getPassword())
        );

        UserPrincipal userPrincipal = (UserPrincipal) authentication.getPrincipal();
        String token = tokenProvider.generateToken(authentication);

        return new AuthResponse(
                token,
                userPrincipal.getId(),
                userPrincipal.getUsername(), // Email
                "", // Full name can be retrieved if needed
                userPrincipal.getUsername(),
                userPrincipal.getRole()
        );
    }
}
