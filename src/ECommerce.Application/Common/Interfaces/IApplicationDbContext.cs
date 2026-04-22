using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<ECommerce.Domain.Entities.User> Users { get; }
    DbSet<ECommerce.Domain.Entities.Address> Addresses { get; }
    DbSet<ECommerce.Domain.Entities.Vendor> Vendors { get; }
    DbSet<ECommerce.Domain.Entities.Category> Categories { get; }
    DbSet<ECommerce.Domain.Entities.Product> Products { get; }
    DbSet<ECommerce.Domain.Entities.ProductImage> ProductImages { get; }
    DbSet<ECommerce.Domain.Entities.Cart> Carts { get; }
    DbSet<ECommerce.Domain.Entities.CartItem> CartItems { get; }
    DbSet<ECommerce.Domain.Entities.Wishlist> Wishlists { get; }
    DbSet<ECommerce.Domain.Entities.WishlistItem> WishlistItems { get; }
    DbSet<ECommerce.Domain.Entities.Order> Orders { get; }
    DbSet<ECommerce.Domain.Entities.OrderItem> OrderItems { get; }
    DbSet<ECommerce.Domain.Entities.Payment> Payments { get; }
    DbSet<ECommerce.Domain.Entities.Review> Reviews { get; }
    DbSet<ECommerce.Domain.Entities.RefreshToken> RefreshTokens { get; }
    Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}