using BCSMS.Application.Abstractions.Persistence;
using BCSMS.Application.Abstractions.Security;
using BCSMS.Application.Abstractions.Time;
using BCSMS.Application.Auth.Login;
using BCSMS.Application.Auth.Register;
using BCSMS.Application.Common.Exceptions;
using BCSMS.Domain.Entities;
using BCSMS.Domain.Enums;
using BCSMS.Domain.ValueObjects;

namespace BCSMS.Application.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IClock _clock;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IClock clock)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _clock = clock;
    }

    public async Task<RegisterResponse> RegisterAsync(
        RegisterCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command == null)
            throw new ValidationException("Command cannot be null.");

        if (string.IsNullOrWhiteSpace(command.FirstName))
            throw new ValidationException("First name is required.");

        if (string.IsNullOrWhiteSpace(command.LastName))
            throw new ValidationException("Last name is required.");

        if (string.IsNullOrWhiteSpace(command.Email))
            throw new ValidationException("Email is required.");

        if (string.IsNullOrWhiteSpace(command.Password))
            throw new ValidationException("Password is required.");

        if (command.Password.Length < 6)
            throw new ValidationException("Password must be at least 6 characters long.");

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();

        var emailExists = await _userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken);
        if (emailExists)
            throw new ApplicationConflictException("A user with this email address already exists.");

        var passwordHash = _passwordHasher.HashPassword(command.Password);

        var user = new User(
            Guid.NewGuid(),
            new FullName(command.FirstName, command.LastName),
            new ContactInfo(normalizedEmail, command.PhoneNumber),
            passwordHash,
            UserRole.Citizen,
            departmentId: null,
            _clock.UtcNow);

        await _userRepository.AddAsync(user, cancellationToken);

        return new RegisterResponse(
            user.Id,
            user.Name.FirstName,
            user.Name.LastName,
            user.Contact.Email,
            user.Role,
            user.CreatedAt);
    }

    public async Task<LoginResponse> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command == null)
            throw new ValidationException("Command cannot be null.");

        if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.Password))
            throw new UnauthorizedException("Invalid email or password.");

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null || !user.IsActive)
            throw new UnauthorizedException("Invalid email or password.");

        var isPasswordValid = _passwordHasher.VerifyPassword(command.Password, user.PasswordHash);
        if (!isPasswordValid)
            throw new UnauthorizedException("Invalid email or password.");

        var (accessToken, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        return new LoginResponse(
            accessToken,
            expiresAt,
            new AuthUserDto(
                user.Id,
                user.Name.FirstName,
                user.Name.LastName,
                user.Contact.Email,
                user.Role));
    }
}
