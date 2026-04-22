using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Users.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IApplicationDbContext _context;

    public UserService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetTotalUsersCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users.CountAsync(cancellationToken);
    }
}