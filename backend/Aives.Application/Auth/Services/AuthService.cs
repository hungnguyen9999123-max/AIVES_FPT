using Aives.Application.Auth.DTOs;
using Aives.Application.Auth.Interfaces;
using Aives.Domain.Entities;
using Aives.Domain.Enums;

namespace Aives.Application.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<UserDto> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepository.ExistsByUsernameAsync(request.Username))
            throw new InvalidOperationException("Username already exists.");

        if (request.Email is not null && await _userRepository.ExistsByEmailAsync(request.Email))
            throw new InvalidOperationException("Email already exists.");

        var user = new User
        {
            Username = request.Username,
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = UserRole.STUDENT,  // public register always creates STUDENT
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);

        return MapToUserDto(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        // Use constant-time-safe message – do not reveal which field was wrong
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password.");

        var token = _jwtService.GenerateToken(user);
        var expiresIn = _jwtService.GetExpirationSeconds();

        return new AuthResponse
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = expiresIn,
            User = MapToUserDto(user)
        };
    }

    public async Task<UserDto> GetCurrentUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new UnauthorizedAccessException("User not found.");

        return MapToUserDto(user);
    }

    private static UserDto MapToUserDto(User user) => new()
    {
        UserId = user.UserId,
        Username = user.Username,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role.ToString()
    };
}
