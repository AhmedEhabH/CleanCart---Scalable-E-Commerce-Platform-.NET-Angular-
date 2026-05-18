using ECommerce.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Services;

public class CartCleanupService : ICartCleanupService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CartCleanupService> _logger;

    public CartCleanupService(IApplicationDbContext context, ILogger<CartCleanupService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task CleanupAbandonedCartsAsync(CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-7);

        var abandonedCarts = await _context.Carts
            .Where(c => c.UpdatedAt != null && c.UpdatedAt < cutoffDate)
            .ToListAsync(cancellationToken);

        if (abandonedCarts.Count == 0)
        {
            _logger.LogInformation("No abandoned carts found for cleanup.");
            return;
        }

        _context.Carts.RemoveRange(abandonedCarts);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cleaned up {Count} abandoned carts older than {Days} days.", abandonedCarts.Count, 7);
    }
}
