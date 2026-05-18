using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CartEntity = ECommerce.Domain.Entities.Cart;

namespace ECommerce.Application.Tests.Services;

public class CartCleanupServiceTests
{
    private static IApplicationDbContext CreateDbContext(IEnumerable<CartEntity> carts)
    {
        var options = new DbContextOptionsBuilder<Infrastructure.Data.ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new Infrastructure.Data.ApplicationDbContext(options);
        context.Database.EnsureCreated();

        context.Carts.RemoveRange(context.Carts);
        context.SaveChanges();

        context.Carts.AddRange(carts);
        context.SaveChanges();

        return context;
    }

    [Fact]
    public async Task Cleanup_ShouldRemoveCarts_OlderThan7Days()
    {
        var oldCart = CartEntity.CreateForUser(Guid.NewGuid());
        oldCart.GetType().GetProperty("UpdatedAt")?.SetValue(oldCart, DateTime.UtcNow.AddDays(-10));
        oldCart.GetType().GetProperty("CreatedAt")?.SetValue(oldCart, DateTime.UtcNow.AddDays(-10));

        var context = CreateDbContext([oldCart]);
        var logger = new Mock<ILogger<CartCleanupService>>();
        var service = new CartCleanupService(context, logger.Object);

        await service.CleanupAbandonedCartsAsync();

        var remainingCarts = await context.Carts.ToListAsync();
        Assert.Empty(remainingCarts);
    }

    [Fact]
    public async Task Cleanup_ShouldNotRemoveCarts_NewerThan7Days()
    {
        var recentCart = CartEntity.CreateForUser(Guid.NewGuid());
        recentCart.GetType().GetProperty("UpdatedAt")?.SetValue(recentCart, DateTime.UtcNow.AddDays(-1));
        recentCart.GetType().GetProperty("CreatedAt")?.SetValue(recentCart, DateTime.UtcNow.AddDays(-1));

        var context = CreateDbContext([recentCart]);
        var logger = new Mock<ILogger<CartCleanupService>>();
        var service = new CartCleanupService(context, logger.Object);

        await service.CleanupAbandonedCartsAsync();

        var remainingCarts = await context.Carts.ToListAsync();
        Assert.Single(remainingCarts);
    }
}
