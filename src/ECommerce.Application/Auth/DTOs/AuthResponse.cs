namespace ECommerce.Application.Auth.DTOs;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string Email,
    string FullName,
    string Role
);
