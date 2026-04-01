using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ECommerce.Application.Common.Interfaces;

namespace ECommerce.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value is { } userId 
        ? Guid.TryParse(userId, out var id) ? id : null 
        : null;

    public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin => HasRole("Admin");

    public bool IsSeller => HasRole("Seller");

    public bool IsCustomer => HasRole("Customer");

    public IEnumerable<string> Roles => User?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();

    public bool HasRole(string role) => User?.HasClaim(ClaimTypes.Role, role) ?? false;
}
