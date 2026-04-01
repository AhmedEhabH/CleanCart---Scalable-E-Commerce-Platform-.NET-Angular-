namespace ECommerce.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsSeller { get; }
    bool IsCustomer { get; }
    IEnumerable<string> Roles { get; }
    bool HasRole(string role);
}
