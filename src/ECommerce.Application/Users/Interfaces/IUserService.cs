namespace ECommerce.Application.Users.Interfaces;

public interface IUserService
{
    Task<int> GetTotalUsersCountAsync(CancellationToken cancellationToken = default);
}