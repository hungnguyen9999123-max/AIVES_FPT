using Aives.Application.Auth.DTOs;

namespace Aives.Application.Auth.Interfaces;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<UserDto> GetCurrentUserAsync(int userId);
}
