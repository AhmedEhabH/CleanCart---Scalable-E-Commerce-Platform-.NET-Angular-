using ECommerce.Application.Auth.DTOs;

namespace ECommerce.Application.Auth.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
}
