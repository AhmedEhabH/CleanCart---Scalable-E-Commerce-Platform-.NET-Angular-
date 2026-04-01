using ECommerce.Application.Auth.DTOs;

namespace ECommerce.Application.Auth.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string email, string role);
    string GenerateRefreshToken();
    DateTime GetTokenExpiration();
}
